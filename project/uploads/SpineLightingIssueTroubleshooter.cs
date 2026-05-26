#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VFXTool.SpineSkeletonShaderTool
{
    public static class SpineLightingIssueTroubleshooter
    {
        private const string SupportedShaderName = "Spine/Skeleton";
        private const int AllowedLayerMask =
            (1 << 8) | (1 << 9) | (1 << 12) | (1 << 13) | (1 << 18) | (1 << 20);

        public class ResultLine
        {
            public readonly string text;
            public readonly bool isIssue;
            public readonly Object navigationTarget;
            public readonly string navigationLabel;

            public ResultLine(string text, bool isIssue)
                : this(text, isIssue, null, null)
            {
            }

            public ResultLine(string text, bool isIssue, Object navigationTarget, string navigationLabel)
            {
                this.text = text;
                this.isIssue = isIssue;
                this.navigationTarget = navigationTarget;
                this.navigationLabel = navigationLabel;
            }
        }

        public class Result
        {
            public GameObject target;
            public string targetSummary;
            public readonly List<ResultLine> issues = new List<ResultLine>();
            public readonly List<ResultLine> materialSummaries = new List<ResultLine>();
            public readonly List<ResultLine> realtimeLights = new List<ResultLine>();

            public bool HasIssues => issues.Any(i => i.isIssue);
        }

        public static Result Run(GameObject target)
        {
            Result result = new Result { target = target };

            if (target == null)
            {
                result.targetSummary = "未選取 GameObject。";
                AddIssue(result, "請先選取 Hierarchy / Scene 裡的角色 GameObject，不要直接選 Project 裡的 Material。調整方式：回到 Hierarchy，點選角色物件後再按一次排查。");
                return result;
            }

            if (EditorUtility.IsPersistent(target))
            {
                result.targetSummary = $"目前選取的是 Project 資產：{target.name}";
                AddIssue(result, "你現在選到的是 Project 裡的資產，不是場景中的角色物件。調整方式：請到 Hierarchy 選取角色 GameObject，讓工具能讀取 Renderer、Layer 與場景燈光。");
                return result;
            }

            bool isAnimatorObject = target.name == "Animator";
            bool isSupportedSpriteRenderer = !isAnimatorObject && IsSupportedSpriteRendererTarget(target, result);
            if (!isAnimatorObject && !isSupportedSpriteRenderer)
            {
                result.targetSummary = $"目前選取：{target.name}";
                if (target.GetComponent<SpriteRenderer>() == null)
                {
                    AddIssue(result, "請選擇 Animator 物件來開始排查，或選擇使用 Spine/Skeleton Shader 的 Sprite Renderer 物件。調整方式：如果是 Spine 角色，請選 Hierarchy 中名稱為「Animator」的 GameObject；如果是 Sprite 物件，請選有 SpriteRenderer 且材質使用 Spine/Skeleton 的 GameObject。",
                        target, target.name);
                }
                return result;
            }

            int charMask = 1 << target.layer;
            string layerName = LayerMask.LayerToName(target.layer);
            if (string.IsNullOrEmpty(layerName)) layerName = $"Layer {target.layer}";
            result.targetSummary = $"{(isAnimatorObject ? "目標" : "目標(SpriteRenderer)")}：{target.name} / Layer：{target.layer} ({layerName})";

            Renderer renderer = target.GetComponent<Renderer>();
            SpineLightManager manager = FindLightManager();
            Light[] realtimeLights = FindRealtimeLights();

            CheckManager(target, charMask, manager, result);
            CheckMaterialSwitches(renderer, result);
            CheckTargetLayer(target, result);
            CheckRealtimeLights(charMask, manager, realtimeLights, result);
            CheckShaderKeywords(renderer, result);
            CheckDependencies(target, renderer, manager, result);

            return result;
        }

        private static bool IsSupportedSpriteRendererTarget(GameObject target, Result result)
        {
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return false;

            Material[] materials = spriteRenderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                AddIssue(result, $"SpriteRenderer「{spriteRenderer.name}」沒有材質，所以無法用光照問題排查。調整方式：請在 SpriteRenderer 的 Material 放入使用 {SupportedShaderName} 的材質。",
                    spriteRenderer.gameObject, spriteRenderer.name);
                return false;
            }

            bool hasSupportedShader = false;
            List<string> shaderNames = new List<string>();
            foreach (Material mat in materials)
            {
                if (mat == null)
                {
                    shaderNames.Add("空材質");
                    continue;
                }

                string shaderName = mat.shader != null ? mat.shader.name : "無 Shader";
                shaderNames.Add($"{mat.name}({shaderName})");
                if (mat.shader != null && mat.shader.name == SupportedShaderName)
                    hasSupportedShader = true;
            }

            if (!hasSupportedShader)
            {
                AddIssue(result, $"SpriteRenderer「{spriteRenderer.name}」的材質沒有使用 {SupportedShaderName}，所以不開放光照問題排查。目前材質：{string.Join(", ", shaderNames)}。調整方式：請把 SpriteRenderer 的 Material 換成使用 {SupportedShaderName} 的材質。",
                    spriteRenderer.gameObject, spriteRenderer.name);
                return false;
            }

            return true;
        }

        private static void CheckManager(GameObject target, int charMask, SpineLightManager manager, Result result)
        {
            if (manager == null)
            {
                AddIssue(result, "場景裡找不到 SpineLightManager，所以角色可能收不到工具注入的燈光資料。調整方式：開啟任一需要光照的材質選項讓系統自動建立，或在場景中建立掛有 SpineLightManager 的物件。");
                return;
            }

            if ((manager.collectLightMask.value & charMask) == 0)
            {
                AddIssue(result,
                    $"SpineLightManager 沒有收集角色所在的 Layer {target.layer} ({LayerLabel(target.layer)})。調整方式：選取 SpineLightManager，把 collectLightMask 勾上角色這一層。",
                    manager.gameObject, "SpineLightManager");
            }
            else
            {
                AddOk(result, $"SpineLightManager 有包含角色 Layer {target.layer} ({LayerLabel(target.layer)})，角色可以被 Manager 納入光照更新。");
            }
        }

        private static void CheckMaterialSwitches(Renderer renderer, Result result)
        {
            if (renderer == null)
            {
                AddIssue(result, "目前選取的 GameObject 本身沒有 Renderer，所以工具讀不到材質。調整方式：請直接選取有 SkeletonRenderer / MeshRenderer 的那個角色物件；本版不會往子物件掃描。");
                return;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                AddIssue(result, $"Renderer「{renderer.name}」沒有材質。調整方式：確認角色 Renderer 的 Materials 欄位有放入 Spine Shader 的材質球。",
                    renderer.gameObject, renderer.name);
                return;
            }

            bool hasAnyNativeMaterial = false;
            foreach (Material mat in materials)
            {
                if (mat == null)
                {
                    AddIssue(result, $"Renderer「{renderer.name}」有一格材質是空的。調整方式：把正確的角色材質球拖到空的 Material slot。",
                        renderer.gameObject, renderer.name);
                    result.materialSummaries.Add(new ResultLine($"空材質槽：請補上角色材質球。", true));
                    continue;
                }

                bool canJudgeNative = TryGetEnabled(mat, "_EnableNativeLighting", out bool nativeOn);

                result.materialSummaries.Add(new ResultLine(
                    $"{mat.name}：Native={StateLabel(canJudgeNative, nativeOn)}",
                    !canJudgeNative || !nativeOn));

                if (!canJudgeNative)
                {
                    AddIssue(result, $"材質「{mat.name}」看起來不是目前這套 Spine 光照 Shader，找不到 Native Lighting 開關。調整方式：確認材質使用的是 Spine-Skeleton.shader 或相容版本。",
                        mat, mat.name);
                    continue;
                }

                if (!nativeOn)
                {
                    AddIssue(result, $"材質「{mat.name}」沒有開 Native Lighting。調整方式：在材質的 LIGHTING 裡開啟 Enable Native Lighting。",
                        mat, mat.name);
                }
                else
                {
                    hasAnyNativeMaterial = true;
                    AddOk(result, $"材質「{mat.name}」有開 Native Lighting，可以接收 Unity Realtime Light。");
                }
            }

            if (hasAnyNativeMaterial)
            {
                AddOk(result, "材質摘要檢查完成：至少有一個材質的 Native Lighting 是 ON。");
            }
        }

        private static void CheckTargetLayer(GameObject target, Result result)
        {
            if ((AllowedLayerMask & (1 << target.layer)) == 0)
            {
                AddIssue(result,
                    $"角色目前在 Layer {target.layer} ({LayerLabel(target.layer)})，不在建議清單 8, 9, 12, 13, 18, 20 裡。調整方式：把角色 GameObject 的 Layer 改到上述其中一層，或同步把 SpineLightManager 與燈光 Culling Mask 加上這個 Layer。",
                    target, target.name);
            }
            else
            {
                AddOk(result, $"角色 Layer {target.layer} ({LayerLabel(target.layer)}) 在建議清單內。");
            }
        }

        private static void CheckRealtimeLights(int charMask, SpineLightManager manager, Light[] realtimeLights, Result result)
        {
            if (realtimeLights.Length == 0)
            {
                AddIssue(result, "場景裡沒有任何 Realtime Light。調整方式：新增一盞 Light，並把 Mode / Bake Type 設為 Realtime。");
                AddIssue(result, "場景裡沒有 Realtime Directional Light。調整方式：如果角色需要方向光，新增 Directional Light，並把 Bake Type 設為 Realtime。");
                return;
            }

            bool hasRealtimeDirectional = false;
            bool hasUsableRealtimeLight = false;
            foreach (Light light in realtimeLights)
            {
                if (light == null) continue;

                bool includesCharacterLayer = (light.cullingMask & charMask) != 0;
                bool intersectsManagerMask = manager != null && (light.cullingMask & manager.collectLightMask.value) != 0;
                bool lightUsable = light.isActiveAndEnabled && light.intensity > 0f && includesCharacterLayer && (manager == null || intersectsManagerMask);
                if (light.type == LightType.Directional) hasRealtimeDirectional = true;
                if (lightUsable) hasUsableRealtimeLight = true;

                result.realtimeLights.Add(new ResultLine(
                    $"{light.name}：{light.type} / 啟用={YesNo(light.isActiveAndEnabled)} / 強度={light.intensity:0.###} / 有照到角色Layer={YesNo(includesCharacterLayer)} / Manager會收集={UnknownWhenNoManager(manager, intersectsManagerMask)}",
                    !lightUsable));

                if (!includesCharacterLayer)
                {
                    AddIssue(result, $"Realtime Light「{light.name}」沒有照到角色 Layer {MaskLabel(charMask)}。調整方式：選取這盞燈，在 Culling Mask 裡勾選角色所在 Layer。",
                        light.gameObject, light.name);
                }

                if (manager != null && !intersectsManagerMask)
                {
                    AddIssue(result, $"Realtime Light「{light.name}」和 SpineLightManager 的 collectLightMask 沒有交集，所以 Manager 不會收集它。調整方式：讓這盞燈的 Culling Mask 與 SpineLightManager.collectLightMask 至少共同勾到一個角色使用的 Layer。",
                        light.gameObject, light.name);
                }

                if (!light.isActiveAndEnabled)
                {
                    AddIssue(result, $"Realtime Light「{light.name}」目前沒有啟用。調整方式：啟用這個 Light 元件與它所在的 GameObject。",
                        light.gameObject, light.name);
                }

                if (light.intensity <= 0f)
                {
                    AddIssue(result, $"Realtime Light「{light.name}」強度是 0 或更低。調整方式：把 Intensity 調到大於 0，例如先用 1 測試。",
                        light.gameObject, light.name);
                }
            }

            if (!hasRealtimeDirectional)
            {
                AddIssue(result, "場景裡沒有 Realtime Directional Light。調整方式：如果角色依賴方向光，新增或啟用一盞 Directional Light，並把 Bake Type 設為 Realtime。");
            }
            else
            {
                AddOk(result, "場景裡有 Realtime Directional Light。");
            }

            if (hasUsableRealtimeLight)
            {
                AddOk(result, "至少有一盞 Realtime Light 啟用、強度大於 0，且 Culling Mask 能對上角色 Layer。");
            }
        }

        private static void CheckShaderKeywords(Renderer renderer, Result result)
        {
            if (renderer == null || renderer.sharedMaterials == null) return;

            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat == null) continue;

                CheckKeywordForToggle(mat, "_EnableNativeLighting", "_MODULE_NATIVE_LIGHTING", "Native Lighting", result);
                CheckKeywordForToggle(mat, "_EnableLightProbe", "_MODULE_LIGHT_PROBE", "Light Probe", result);
                CheckKeywordForToggle(mat, "_EnableShadowLight", "_MODULE_SHADOW_LIGHT", "Shadow Light", result);

                CheckLitEffectKeyword(mat, "_EnableBackLight", "_BackLightLightingMode", "_MODULE_BACKLIGHT", "BackLight", result);
                CheckLitEffectKeyword(mat, "_EnableInline", "_InlineLightingMode", "_MODULE_INLINE", "Inline", result);
                CheckLitEffectKeyword(mat, "_EnableOutline", "_OutlineLightingMode", "_MODULE_OUTLINE", "Outline", result);
            }
        }

        private static void CheckDependencies(GameObject target, Renderer renderer, SpineLightManager manager, Result result)
        {
            if (renderer != null && target.GetComponent<SpineLightReceiver>() == null)
            {
                AddIssue(result, "這個 GameObject 沒有 SpineLightReceiver。調整方式：在材質 GUI 的自動腳本區確認 SpineLightReceiver 已掛上，或手動替此 GameObject 加上 SpineLightReceiver。",
                    target, target.name);
            }
            else if (renderer != null)
            {
                AddOk(result, "此 GameObject 已有 SpineLightReceiver，可以接收 Manager 注入的光照資料。");
            }
        }

        private static void CheckKeywordForToggle(Material mat, string propName, string keyword, string label, Result result)
        {
            if (!TryGetEnabled(mat, propName, out bool enabled) || !enabled) return;
            if (!mat.IsKeywordEnabled(keyword))
            {
                AddIssue(result, $"材質「{mat.name}」已開啟 {label}，但 Shader Keyword「{keyword}」沒有打開。調整方式：在材質球編輯器右上角按「▤ 全效預覽」。",
                    mat, mat.name);
            }
            else
            {
                AddOk(result, $"材質「{mat.name}」的 {label} Keyword 已打開。");
            }
        }

        private static void CheckLitEffectKeyword(Material mat, string enableProp, string modeProp, string moduleKeyword, string label, Result result)
        {
            if (!TryGetEnabled(mat, enableProp, out bool enabled) || !enabled) return;

            int mode = 0;
            bool hasMode = mat.HasProperty(modeProp);
            if (hasMode) mode = Mathf.RoundToInt(mat.GetFloat(modeProp));

            if (!mat.IsKeywordEnabled(moduleKeyword))
            {
                AddIssue(result, $"材質「{mat.name}」已開啟 {label}，但 Shader Keyword「{moduleKeyword}」沒有打開。調整方式：先按「▤ 全效預覽」測試；若有效，請在平台設定檔啟用 {label} 模組。",
                    mat, mat.name);
            }
            else
            {
                AddOk(result, $"材質「{mat.name}」的 {label} 模組 Keyword 已打開。");
            }

            if (!hasMode)
            {
                AddIssue(result, $"材質「{mat.name}」的 {label} 找不到 Light Source 欄位。調整方式：確認 Shader 與 ShaderGUI 是同一版。",
                    mat, mat.name);
                return;
            }

            if (mode == 0)
            {
                AddIssue(result, $"材質「{mat.name}」的 {label} 已開啟，但 Light Source 是 Disabled。調整方式：在 {label} 的 Light Source 選 Realtime、LightProbe 或 ShadowLight。",
                    mat, mat.name);
                return;
            }

            if ((mode & 2) != 0)
            {
                CheckKeywordForRequiredLightSource(mat, "_EnableLightProbe", "_MODULE_LIGHT_PROBE", label, "LightProbe", result);
            }

            if ((mode & 4) != 0)
            {
                CheckKeywordForRequiredLightSource(mat, "_EnableShadowLight", "_MODULE_SHADOW_LIGHT", label, "ShadowLight", result);
            }
        }

        private static void CheckKeywordForRequiredLightSource(Material mat, string globalToggle, string keyword, string effectLabel, string sourceLabel, Result result)
        {
            if (!mat.IsKeywordEnabled(keyword))
            {
                AddIssue(result, $"材質「{mat.name}」的 {effectLabel} 有選 {sourceLabel}，但 Shader Keyword「{keyword}」沒打開。調整方式：按「▤ 全效預覽」測試；若有效，請在平台設定檔啟用 {sourceLabel} 模組。",
                    mat, mat.name);
            }
            else
            {
                AddOk(result, $"材質「{mat.name}」的 {effectLabel} 所需 {sourceLabel} Keyword 已打開。");
            }

            if (TryGetEnabled(mat, globalToggle, out bool enabled) && !enabled)
            {
                AddIssue(result, $"材質「{mat.name}」的 {effectLabel} 有選 {sourceLabel}，但全域 {globalToggle} 沒開。調整方式：到 LIGHTING 區塊開啟對應的全域光照選項。",
                    mat, mat.name);
            }
        }

        private static void AddIssue(Result result, string text)
        {
            result.issues.Add(new ResultLine(text, true));
        }

        private static void AddIssue(Result result, string text, Object navigationTarget, string navigationLabel)
        {
            result.issues.Add(new ResultLine(text, true, navigationTarget, navigationLabel));
        }

        private static void AddOk(Result result, string text)
        {
            result.issues.Add(new ResultLine(text, false));
        }

        private static bool TryGetEnabled(Material mat, string propName, out bool enabled)
        {
            enabled = false;
            if (mat == null || !mat.HasProperty(propName)) return false;
            enabled = mat.GetFloat(propName) > 0.5f;
            return true;
        }

        private static SpineLightManager FindLightManager()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<SpineLightManager>(FindObjectsInactive.Include);
#else
            return Object.FindObjectOfType<SpineLightManager>(true);
#endif
        }

        private static Light[] FindRealtimeLights()
        {
#if UNITY_2023_1_OR_NEWER
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            Light[] lights = Object.FindObjectsOfType<Light>();
#endif
            return lights.Where(l => l != null && l.lightmapBakeType == LightmapBakeType.Realtime).ToArray();
        }

        private static string LayerLabel(int layer)
        {
            string name = LayerMask.LayerToName(layer);
            return string.IsNullOrEmpty(name) ? $"Layer {layer}" : name;
        }

        private static string MaskLabel(int mask)
        {
            List<string> labels = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                    labels.Add($"{i} ({LayerLabel(i)})");
            }
            return labels.Count == 0 ? "None" : string.Join(", ", labels);
        }

        private static string StateLabel(bool canJudge, bool value)
        {
            if (!canJudge) return "無屬性";
            return value ? "ON" : "OFF";
        }

        private static string YesNo(bool value) => value ? "是" : "否";

        private static string UnknownWhenNoManager(SpineLightManager manager, bool value)
        {
            if (manager == null) return "無法判斷";
            return YesNo(value);
        }
    }
}
#endif
