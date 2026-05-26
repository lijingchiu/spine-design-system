using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using VFXTool.SpineSkeletonShaderTool;

namespace VFXTool.SpineSkeletonShaderTool
{
    public class SpineCustomShaderGUI : ShaderGUI
        {
            public static readonly Color ColorMain      = new Color(0.35f, 0.75f, 0.45f);
            public static readonly Color ColorNoise     = new Color(0.00f, 0.85f, 0.80f);
            public static readonly Color ColorLighting  = new Color(1.00f, 0.75f, 0.10f);
            public static readonly Color ColorEmission  = new Color(1.00f, 0.45f, 0.20f);
            public static readonly Color ColorEffect    = new Color(0.20f, 0.70f, 1.00f);
            public static readonly Color ColorLine      = new Color(0.75f, 0.40f, 1.00f);
    
            [System.Flags]
            public enum LightingMode { Disabled = 0, Realtime = 1 << 0, LightProbe = 1 << 1, ShadowLight = 1 << 2 }
            public enum ZTestMode { LessEqual = 4, Always = 8 }
    
            private static bool showMainCategory = true, showNoiseCategory = true, showLightingCategory = true;
            private static bool showEmissionCategory = true, showEffectCategory = true, showLineCategory = true;
            
            private static bool showMainSettings = true, showPostProcessing = true, showGlobalNoise = true;
            private static bool showLightingSettings = true, showLightProbeSettings = true, showShadowLightSettings = true;
            private static bool showNormalMapSettings = true;
            private static bool showEmissionSettings = true;
            private static bool showEffectOverlay = true, showHitSweepSettings = true, showBackLightSettings = true;
            private static bool showInlineSettings = true, showOutlineSettings = true, showAdvancedOutline = false;
    
            private static bool pShowMainSettings, pShowPostProcessing, pShowGlobalNoise;
            private static bool pShowLightingSettings, pShowLightProbeSettings, pShowShadowLightSettings, pShowNormalMapSettings;
            private static bool pShowEmissionSettings;
            private static bool pShowEffectOverlay, pShowHitSweepSettings, pShowBackLightSettings;
            private static bool pShowInlineSettings, pShowOutlineSettings;
    
            // =========================================================
            // 預掛腳本區塊 UI 狀態與防呆警告
            // =========================================================
            private static bool showScriptSection = false;
            private bool warnAlphaMask = false;
            private bool warnHitSweep = false;
            private bool warnLightReceiver = false;
            private bool warnBloomSync = false;
    
            private SpineModuleProfile profMainSettings, profPostProcessing, profGlobalNoise;
            private SpineModuleProfile profLightingSettings, profLightProbeSettings, profShadowLightSettings, profNormalMapSettings;
            private SpineModuleProfile profEmissionSettings;
            private SpineModuleProfile profEffectOverlay, profHitSweepSettings, profBackLightSettings;
            private SpineModuleProfile profInlineSettings, profOutlineSettings;
    
            public static bool showRawPropertyNames = false;
            private bool _initialized = false;
            private Material _lastTargetMat = null;
    
            private static Dictionary<string, int> profileSaveMasks = new Dictionary<string, int>();
    
            public static readonly Dictionary<string, string> FriendlyNameMap = new Dictionary<string, string>
            {
                {"_Cutoff", "Shadow Alpha Cutoff"}, {"_MainTex", "Main Texture"}, {"_MainTex_ST", "Shared Tiling & Offset (XY:Tiling, ZW:Offset)"}, {"_Color", "Main Color"},
                {"_EffectColor", "Effect Color (HDR)"}, {"_EffectIntensity", "Effect Intensity"},
                {"_EnableBloomSuppression", "Suppress Bloom"}, {"_BloomSuppressThreshold", "Bloom Threshold"},
                {"_EnableGlobalNoise", "Enable Global Noise"}, {"_GlobalNoiseScale", "Noise Scale (X,Y)"}, {"_GlobalNoiseSpeed", "Noise Speed (X,Y)"},
                {"_EnableNativeLighting", "Enable Native Lighting"}, {"_NativeLightIntensity", "Master Light Intensity"},
                {"_DirectionalLightIntensity", "Directional Intensity"}, {"_AdditionalLightIntensity", "Additional Intensity"},
                {"_StraightAlphaInput", "Straight Alpha Texture"}, {"_LightAffectsAdditive", "Light Affects Additive"},
                {"_LightProbeNativeBlend", "Probe → Native Lighting"}, {"_EnableShadowLight", "Enable Shadow Light Base"},
                {"_ShadowLightBaseIntensity", "Base Intensity"},
                {"_SL_AffectsNormalMap", "Affects Normal Map"}, {"_EnableLightProbe", "Enable Light Probe"}, {"_LightProbeIntensity", "Probe Intensity"},
                {"_EnableHitSweep", "Enable Hit Sweep"}, {"_SweepColor", "Sweep Color (HDR)"}, {"_SweepWidth", "Sweep Width"}, {"_SweepSoftness", "Sweep Softness"},
                {"_EnableNormalMap", "Enable Normal Map Module"}, {"_InvertNormalMap", "Invert Normal Height"}, {"_NormalMap", "Normal Map"}, {"_NormalIntensity", "Normal Intensity"},
                {"_EnableEmission", "Enable Emission"}, {"_EmissionTex", "Emission Texture"}, {"_EmissionColor", "Emission Color"},
                {"_EmissionBlend", "Blend Weight"}, {"_EmissionIntensity", "Intensity"}, {"_EmissionMulMain", "Multiply With MainTex"},
                {"_EnableBackLight", "Enable BackLight"}, {"_BackLightLightingMode", "Light Source"}, {"_SpecularTex", "BackLight Texture"},
                {"_BL_SL_IgnoreBehind", "Ignore Behind Fade (Shadow Light)"},
                {"_SpecularColor", "Specular Color"}, {"_BackLightMode", "Blend Mode"}, {"_SpecularBlend", "Blend Intensity"},
                {"_SpecularMulMain", "Multiply With MainTex"}, {"_EnableDirectionalBackLight", "Mask By Light Direction"},
                {"_BackLightTintByLight", "Tint By Light Color"}, {"_BL_EnableNoise", "Enable Noise Mask"}, {"_BL_NoiseIntensity", "Noise Intensity"},
                {"_BL_RT_Power", "Realtime Focus Power"}, {"_BL_RT_Blend", "Realtime Color Blend"}, {"_BL_RT_Intensity", "↳ Intensity"},
                {"_BL_Probe_Power", "Probe Focus Power"}, {"_BL_Probe_Blend", "Probe Color Blend"}, {"_BL_Probe_Intensity", "↳ Intensity"},
                {"_BL_Shadow_Power", "ShadowLight Focus Power"}, {"_BL_Shadow_Blend", "ShadowLight Color Blend"}, {"_BL_Shadow_Intensity", "↳ Intensity"},
                {"_EnableInline", "Enable Inline"}, {"_InlineLightingMode", "Light Source"}, {"_IL_SL_IgnoreBehind", "Ignore Behind Fade (Shadow Light)"}, 
                {"_InlineColor", "Inline Color (HDR)"},
                {"_InlineWidth", "Inline Width"}, {"_InlineFadeSteps", "Fade Steps"}, {"_EnableDirectionalInline", "Mask By Light Direction"},
                {"_InlineTintByLight", "Tint By Light Color"}, {"_InlineZTest", "Inline ZTest"}, {"_IL_EnableNoise", "Enable Noise Mask"},
                {"_IL_NoiseIntensity", "Noise Intensity"}, {"_IL_RT_Power", "Realtime Focus Power"}, {"_IL_RT_Blend", "Realtime Color Blend"},
                {"_IL_RT_Intensity", "↳ Intensity"}, {"_IL_Probe_Power", "Probe Focus Power"}, {"_IL_Probe_Blend", "Probe Color Blend"},
                {"_IL_Probe_Intensity", "↳ Intensity"}, {"_IL_Shadow_Power", "ShadowLight Focus Power"}, {"_IL_Shadow_Blend", "ShadowLight Color Blend"}, {"_IL_Shadow_Intensity", "↳ Intensity"},
                {"_EnableOutline", "Enable Outline"}, {"_OutlineLightingMode", "Light Source"}, {"_OL_SL_IgnoreBehind", "Ignore Behind Fade (Shadow Light)"}, 
                {"_OutlineWidth", "Outline Width"},
                {"_OutlineColor", "Outline Color (HDR)"}, {"_ThresholdEnd", "Outline Smoothness"}, {"_OutlineAlphaCutoff", "Alpha Cutoff"},
                {"_MultiplyEdgeColor", "Multiply Edge Color"}, {"_EnableDirectionalOutline", "Mask By Light Direction"},
                {"_OutlineTintByLight", "Tint By Light Color"}, {"_OutlineZTest", "Outline ZTest"}, {"_OL_EnableNoise", "Enable Noise Mask"},
                {"_OL_NoiseIntensity", "Noise Intensity"}, {"_UseScreenSpaceOutlineWidth", "Screen Space Width"}, {"_Fill", "Fill Inside"},
                {"_OutlineReferenceTexWidth", "Reference Texture Width"}, {"_Use8Neighbourhood", "Sample 8 Neighbours"},
                {"_OutlineOpaqueAlpha", "Opaque Alpha Threshold"}, {"_OutlineMipLevel", "Mip Level"},
                {"_OL_RT_Power", "Realtime Focus Power"}, {"_OL_RT_Blend", "Realtime Color Blend"}, {"_OL_RT_Intensity", "↳ Intensity"},
                {"_OL_Probe_Power", "Probe Focus Power"}, {"_OL_Probe_Blend", "Probe Color Blend"}, {"_OL_Probe_Intensity", "↳ Intensity"},
                {"_OL_Shadow_Power", "ShadowLight Focus Power"}, {"_OL_Shadow_Blend", "ShadowLight Color Blend"}, {"_OL_Shadow_Intensity", "↳ Intensity"}
            };
    
            public static readonly HashSet<string> ProfileIgnoredProperties = new HashSet<string>
            {
                "_EnableInline", "_EnableHitSweep", "_EnableBloomSuppression",
                "_EnableNormalMap", "_EnableBackLight", "_EnableOutline",
                "_EnableLightProbe", "_EnableShadowLight", "_EnableGlobalNoise",
                "_BackLightLightingMode", "_InlineLightingMode", "_OutlineLightingMode"
            };
    
            private class ModuleClipboard
            {
                public Dictionary<string, float> floats = new Dictionary<string, float>();
                public Dictionary<string, Color> colors = new Dictionary<string, Color>();
                public Dictionary<string, Vector4> vectors = new Dictionary<string, Vector4>();
                public Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
            }
            private static ModuleClipboard _clipboard = null;
    
            private const string TIP_LIGHTING_MODE =
                "選擇此模組的光照來源（可多選）：\n" +
                "• Disabled     — 完全不受任何光照影響\n" +
                "• Realtime     — 接收場景即時燈光（Directional / Point / Spot）\n" +
                "• LightProbe   — 接收烘焙後的 Light Probe 環境光\n" +
                "• ShadowLight  — 接收 Spine Shadow Light\n\n" +
                "每個被選中的光源，將會在下方顯示其獨立的參數拉桿。";
            private const string TIP_MASK  = "開啟後，效果會根據光源方向只在受光面顯示。";
            private const string TIP_TINT  = "開啟後，效果的顏色會受到光源顏色影響。";
            private const string TIP_ZTEST = "深度測試模式：\n• LessEqual（預設）— 正常深度測試\n• Always — 永遠渲染，可穿透幾何物件";
            private const string TIP_MAIN_COLOR_ALPHA = "調整 Main Color 的 Alpha 值可控制整體角色透明度，\n包含 BackLight、Inline、Outline 所有模塊。";
    
            public static string GetFormattedModuleName(string rawTitle)
            {
                string s = rawTitle.Replace("❖ ", "").Trim();
                s = s.Replace(" & ", "-");
                s = s.Replace("&", "");
                s = s.Replace(" ", "-");
                return s;
            }
    
            private bool IsFeatureEnabled(MaterialProperty baseProp, SpineModuleProfile profile, string propName) {
                if (profile != null && profile.properties != null) {
                    var prop = profile.properties.Find(p => p.name == propName);
                    if (prop != null) return prop.floatValue > 0.5f;
                }
                return baseProp != null && baseProp.floatValue > 0.5f;
            }
    
            private int GetIntFeature(MaterialProperty baseProp, SpineModuleProfile profile, string propName) {
                if (profile != null && profile.properties != null) {
                    var prop = profile.properties.Find(p => p.name == propName);
                    if (prop != null) return Mathf.RoundToInt(prop.floatValue);
                }
                return baseProp != null ? Mathf.RoundToInt(baseProp.floatValue) : 0;
            }
    
            private void PerformResetCleanup(MaterialEditor editor)
            {
                profMainSettings = null; profPostProcessing = null; profGlobalNoise = null;
                profLightingSettings = null; profLightProbeSettings = null; profShadowLightSettings = null; profNormalMapSettings = null;
                profEmissionSettings = null;
                profEffectOverlay = null; profHitSweepSettings = null; profBackLightSettings = null;
                profInlineSettings = null; profOutlineSettings = null;
    
                _initialized = false;
    
                SyncAutoAnimator(editor);
    
    #if UNITY_2023_1_OR_NEWER
                Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    #else
                Renderer[] allRenderers = Object.FindObjectsOfType<Renderer>();
    #endif
                foreach (Renderer r in allRenderers) {
                    if (r == null || r.gameObject == null) continue;
                    bool usesMat = false;
                    foreach (Material mat in editor.targets) {
                        if (r.sharedMaterials.Contains(mat)) { usesMat = true; break; }
                    }
                    if (usesMat) {
                        string[] autoScripts = { "SpineAlphaMaskRenderer", "SpineHitSweepEffect", "SpineLightReceiver", "SpineBloomThresholdSync" };
                        foreach (string script in autoScripts) {
                            System.Type st = System.Type.GetType($"VFXTool.SpineSkeletonShaderTool.{script}, Assembly-CSharp");
                            if (st != null) {
                                Component comp = r.GetComponent(st);
                                if (comp != null) Undo.DestroyObjectImmediate(comp);
                            }
                        }
                    }
                }
    
                Debug.Log("<b>[SpineShaderGUI]</b> 偵測到 Material 被重置 (Reset)，已清除所有 Preset 與關聯元件。");
            }

            private void ApplySpriteRendererDefaults(MaterialEditor editor)
            {
                if (editor == null || editor.targets == null) return;

    #if UNITY_2023_1_OR_NEWER
                SpriteRenderer[] spriteRenderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    #else
                SpriteRenderer[] spriteRenderers = Object.FindObjectsOfType<SpriteRenderer>();
    #endif
                foreach (Object obj in editor.targets)
                {
                    Material mat = obj as Material;
                    if (mat == null) continue;

                    SpriteRenderer spriteRenderer = FindSpriteRendererUsingMaterial(spriteRenderers, mat);
                    if (spriteRenderer == null || spriteRenderer.sprite == null || spriteRenderer.sprite.texture == null) continue;

                    Undo.RecordObject(mat, "Apply Sprite Renderer Defaults");

                    if (mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", spriteRenderer.sprite.texture);
                    }

                    if (mat.HasProperty("_StraightAlphaInput"))
                    {
                        mat.SetFloat("_StraightAlphaInput", 1f);
                    }

                    EditorUtility.SetDirty(mat);
                    Debug.Log($"[SpineShaderGUI] 偵測到材質「{mat.name}」用於 SpriteRenderer「{spriteRenderer.name}」，已自動套用 Sprite Texture 至 Main Texture 並開啟 Straight Alpha Input。", spriteRenderer);
                }
            }

            private SpriteRenderer FindSpriteRendererUsingMaterial(SpriteRenderer[] spriteRenderers, Material targetMat)
            {
                if (spriteRenderers == null || targetMat == null) return null;

                foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    if (spriteRenderer == null) continue;
                    Material[] materials = spriteRenderer.sharedMaterials;
                    if (materials == null) continue;

                    foreach (Material mat in materials)
                    {
                        if (mat == targetMat) return spriteRenderer;
                    }
                }

                return null;
            }
            
            private void ApplyCurrentPlatformSettings(Material mat)
            {
                // 1. 檢查是否處於「全開預覽模式」
                bool isEditorDefault = SessionState.GetBool("SpineShader_IsEditorDefault", true);
                HashSet<string> enabledKeywords = new HashSet<string>();

                if (isEditorDefault)
                {
                    // 預設模式：全開
                    enabledKeywords = new HashSet<string>(SpineShaderBuildPipeline.AllModuleKeywords);
                }
                else
                {
                    SpineShaderBuildConfig config = null;
                    
                    // 2. 優先讀取「變體管理器」最後套用的 Config 路徑
                    string activeConfigPath = SessionState.GetString("SpineShader_ActiveConfigPath", "");
                    if (!string.IsNullOrEmpty(activeConfigPath)) {
                        config = AssetDatabase.LoadAssetAtPath<SpineShaderBuildConfig>(activeConfigPath);
                    }

                    // 3. 如果管理器沒有記錄 (例如重開 Unity)，才 Fallback 到 Unity 當前的 Build Target
                    if (config == null) {
                        string targetPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
                        string configPath = $"Assets/Scripts/VFXTool/BuildConfigs/SpineSkeletonShaderTool/ShaderConfig_{targetPlatform}.asset";
                        config = AssetDatabase.LoadAssetAtPath<SpineShaderBuildConfig>(configPath);
                    }

                    if (config != null) {
                        enabledKeywords = new HashSet<string>(config.GetEnabledKeywords());
                    } else {
                        // 若真的都找不到設定檔，為了防呆預設全開
                        enabledKeywords = new HashSet<string>(SpineShaderBuildPipeline.AllModuleKeywords);
                    }
                }

                // 4. 僅套用變體 Keyword 至材質球，不再干涉 ShaderGUI 中的 Toggle (Float) 數值
                foreach (string kw in SpineShaderBuildPipeline.AllModuleKeywords)
                {
                    bool isEnabled = enabledKeywords.Contains(kw);
                    
                    // 設定變體 Keyword，移除原先的 mat.SetFloat 行為
                    if (isEnabled) mat.EnableKeyword(kw);
                    else mat.DisableKeyword(kw);
                }

                EditorUtility.SetDirty(mat);
            }
    
            private void ResetPropsToDefault(MaterialProperty[] props, MaterialEditor editor)
            {
                Material targetMat = editor.target as Material;
                if (targetMat == null || targetMat.shader == null) return;
                Shader shader = targetMat.shader;
    
                foreach (MaterialProperty p in props)
                {
                    if (p == null) continue;
                    if (p.name == "_MainTex") continue; 
                    if (p.name == "_MaterialResetCheck") continue; 
    
                    int idx = shader.FindPropertyIndex(p.name);
                    if (idx == -1) continue;
    
                    if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range) {
                        p.floatValue = shader.GetPropertyDefaultFloatValue(idx);
                    } else if (p.type == MaterialProperty.PropType.Color) {
                        p.colorValue = shader.GetPropertyDefaultVectorValue(idx);
                    } else if (p.type == MaterialProperty.PropType.Vector) {
                        p.vectorValue = shader.GetPropertyDefaultVectorValue(idx);
                    } else if (p.type == MaterialProperty.PropType.Texture) {
                        p.textureValue = null;
                    }
                }
            }
            
            private void ApplyPlatformDefaults(Material mat)
            {
                // 1. 偵測當前平台
                string targetPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
                
                // 2. 檢查是否處於「全開預覽模式」
                bool isEditorDefault = SessionState.GetBool("SpineShader_IsEditorDefault", true);
                
                if (isEditorDefault)
                {
                    // 預設模式：將所有關鍵模組屬性設為 1 (ON)
                    foreach (string kw in SpineShaderBuildPipeline.AllModuleKeywords)
                    {
                        mat.EnableKeyword(kw);
                        // 這裡需要一個對應表將 Keyword 轉回 Property Name，例如 _MODULE_OUTLINE -> _EnableOutline
                    }
                    return;
                }

                // 3. 讀取對應平台的 Config 檔案
                string configPath = $"Assets/Scripts/VFXTool/BuildConfigs/SpineSkeletonShaderTool/ShaderConfig_{targetPlatform}.asset";
                var config = AssetDatabase.LoadAssetAtPath<SpineShaderBuildConfig>(configPath);
                
                if (config != null)
                {
                    // 取得該平台啟用的 Keywords
                    var enabledKeywords = new HashSet<string>(config.GetEnabledKeywords());

                    // 同步所有模組的開關屬性
                    // 這裡需要手動對應一次，確保 ShaderGUI 的 Float 數值與變體 Keyword 同步
                    SetModuleState(mat, "_EnableNativeLighting", enabledKeywords.Contains("_MODULE_NATIVE_LIGHTING"), "_MODULE_NATIVE_LIGHTING");
                    SetModuleState(mat, "_EnableShadowLight",    enabledKeywords.Contains("_MODULE_SHADOW_LIGHT"),    "_MODULE_SHADOW_LIGHT");
                    SetModuleState(mat, "_EnableLightProbe",     enabledKeywords.Contains("_MODULE_LIGHT_PROBE"),     "_MODULE_LIGHT_PROBE");
                    SetModuleState(mat, "_EnableNormalMap",      enabledKeywords.Contains("_MODULE_NORMALMAP"),       "_MODULE_NORMALMAP");
                    SetModuleState(mat, "_EnableBackLight",      enabledKeywords.Contains("_MODULE_BACKLIGHT"),       "_MODULE_BACKLIGHT");
                    SetModuleState(mat, "_EnableEmission",       enabledKeywords.Contains("_MODULE_EMISSION"),        "_MODULE_EMISSION");
                    SetModuleState(mat, "_EnableInline",         enabledKeywords.Contains("_MODULE_INLINE"),          "_MODULE_INLINE");
                    SetModuleState(mat, "_EnableOutline",        enabledKeywords.Contains("_MODULE_OUTLINE"),         "_MODULE_OUTLINE");
                    SetModuleState(mat, "_EnableHitSweep",       enabledKeywords.Contains("_MODULE_HITSWEEP"),        "_MODULE_HITSWEEP");
                    SetModuleState(mat, "_EnableGlobalNoise",    enabledKeywords.Contains("_MODULE_GLOBAL_NOISE"),     "_MODULE_GLOBAL_NOISE");
                    SetModuleState(mat, "_EnableBloomSuppression", enabledKeywords.Contains("_MODULE_BLOOM_SUPPRESSION"), "_MODULE_BLOOM_SUPPRESSION");
                    
                    EditorUtility.SetDirty(mat);
                }
            }

        private void SetModuleState(Material mat, string propName, bool state, string keyword)
        {
            if (mat.HasProperty(propName))
            {
                if (state) mat.EnableKeyword(keyword); else mat.DisableKeyword(keyword);
            }
        }
    
            private void SaveProfileToMaterial(Material mat, string moduleName, SpineModuleProfile profile)
            {
                string tagKey = "SpineProfile_" + moduleName.Replace(" ", "").Replace("&", "");
                if (profile == null) {
                    mat.SetOverrideTag(tagKey, "");
                } else {
                    string path = AssetDatabase.GetAssetPath(profile);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    
                    if (string.IsNullOrEmpty(guid) && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(profile, out string outGuid, out long localId)) {
                        guid = outGuid;
                    }
                    mat.SetOverrideTag(tagKey, guid);
                }
            }
    
            private SpineModuleProfile LoadProfileFromMaterial(Material mat, string moduleName)
            {
                string tagKey = "SpineProfile_" + moduleName.Replace(" ", "").Replace("&", "");
                string guid = mat.GetTag(tagKey, false, "");
                if (!string.IsNullOrEmpty(guid)) {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(path)) {
                        return AssetDatabase.LoadAssetAtPath<SpineModuleProfile>(path);
                    }
                }
                return null;
            }
    
            private void LoadProfilesFromMaterial(Material mat)
            {
                profMainSettings = LoadProfileFromMaterial(mat, "Main & Shadow Settings");
                profPostProcessing = LoadProfileFromMaterial(mat, "Post-Processing Control");
                profGlobalNoise = LoadProfileFromMaterial(mat, "Global Noise Settings");
                profLightingSettings = LoadProfileFromMaterial(mat, "Native Lighting Settings");
                profShadowLightSettings = LoadProfileFromMaterial(mat, "Shadow Light Base Settings");
                profNormalMapSettings = LoadProfileFromMaterial(mat, "Normal Map Settings");
                profLightProbeSettings = LoadProfileFromMaterial(mat, "Light Probe Settings");
                profEmissionSettings = LoadProfileFromMaterial(mat, "Emission Settings");
                profEffectOverlay = LoadProfileFromMaterial(mat, "Effect Overlay");
                profHitSweepSettings = LoadProfileFromMaterial(mat, "Hit Sweep Settings");
                profBackLightSettings = LoadProfileFromMaterial(mat, "BackLight Settings");
                profInlineSettings = LoadProfileFromMaterial(mat, "Inline Settings");
                profOutlineSettings = LoadProfileFromMaterial(mat, "Outline Settings");
            }
    
            private void RouteProfileToCorrectModule(SpineModuleProfile newProfile, Object[] targets)
            {
                if (newProfile == null) return;
                string n = newProfile.moduleName;
                
                Undo.RecordObjects(targets, "Assign Preset To Correct Module");
                
                if (n == "Main & Shadow Settings") profMainSettings = newProfile;
                else if (n == "Post-Processing Control") profPostProcessing = newProfile;
                else if (n == "Global Noise Settings") profGlobalNoise = newProfile;
                else if (n == "Native Lighting Settings") profLightingSettings = newProfile;
                else if (n == "Shadow Light Base Settings") profShadowLightSettings = newProfile;
                else if (n == "Normal Map Settings") profNormalMapSettings = newProfile;
                else if (n == "Light Probe Settings") profLightProbeSettings = newProfile;
                else if (n == "Emission Settings") profEmissionSettings = newProfile;
                else if (n == "Effect Overlay") profEffectOverlay = newProfile;
                else if (n == "Hit Sweep Settings") profHitSweepSettings = newProfile;
                else if (n == "BackLight Settings") profBackLightSettings = newProfile;
                else if (n == "Inline Settings") profInlineSettings = newProfile;
                else if (n == "Outline Settings") profOutlineSettings = newProfile;
                else {
                    Debug.LogWarning($"[SpineShaderGUI] 無法自動導向：未知的 Preset 模塊名稱 '{n}'");
                    return;
                }
                
                foreach (Material mat in targets) {
                    SaveProfileToMaterial(mat, n, newProfile);
                    EditorUtility.SetDirty(mat);
                }
                Debug.Log($"[SpineShaderGUI] 防呆機制啟動：已將 Preset 自動導向至正確的模塊【{n}】。");
            }
    
            private void DrawProperty(MaterialEditor editor, MaterialProperty prop, GUIContent friendlyContent, SpineModuleProfile activeProfile = null)
            {
                if (prop == null) return;
                
                SpineModuleProfile.ProfileProperty profileProp = null;
                if (activeProfile != null && activeProfile.properties != null) {
                    profileProp = activeProfile.properties.Find(p => p.name == prop.name);
                }
    
                GUIContent content = showRawPropertyNames ? new GUIContent(prop.name, friendlyContent.tooltip) : new GUIContent(friendlyContent.text, friendlyContent.tooltip);
                
                if (profileProp != null) {
                    content.text = "✦ " + content.text; 
                    content.tooltip = "[受 Preset 驅動] " + content.tooltip;
    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    
                    if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range) {
                        bool isToggle = prop.name.Contains("Enable") || prop.name.Contains("Use") || 
                                        prop.name.Contains("Fill") || prop.name.Contains("Invert") || 
                                        prop.name.Contains("Affects") || prop.name.Contains("Mul") || 
                                        prop.name.Contains("Tint");
    
                        if (isToggle) {
                            prop.floatValue = EditorGUILayout.Toggle(content, prop.floatValue > 0.5f) ? 1f : 0f;
                        } else if (prop.name == "_InlineFadeSteps") {
                            prop.floatValue = EditorGUILayout.IntSlider(content, Mathf.RoundToInt(prop.floatValue), 2, 8);
                        } else if (prop.type == MaterialProperty.PropType.Range) {
                            prop.floatValue = EditorGUILayout.Slider(content, prop.floatValue, prop.rangeLimits.x, prop.rangeLimits.y);
                        } else {
                            prop.floatValue = EditorGUILayout.FloatField(content, prop.floatValue);
                        }
                    }
                    else if (prop.type == MaterialProperty.PropType.Color) {
                        bool isHDR = (prop.flags & MaterialProperty.PropFlags.HDR) != 0;
                        prop.colorValue = EditorGUILayout.ColorField(content, prop.colorValue, true, true, isHDR);
                    }
                    else if (prop.type == MaterialProperty.PropType.Vector) {
                        prop.vectorValue = EditorGUILayout.Vector4Field(content, prop.vectorValue);
                    }
                    
                    bool prevCurve = profileProp.useCurve;
                    bool nextCurve = GUILayout.Toggle(profileProp.useCurve, new GUIContent("∿", "開啟曲線動畫"), "Button", GUILayout.Width(26), GUILayout.Height(18));
                    
                    if (nextCurve != prevCurve) {
                        Undo.RecordObject(activeProfile, "Toggle Profile Curve");
                        profileProp.useCurve = nextCurve;
                        if (nextCurve) {
                            if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range) {
                                if (profileProp.curveX.length == 0) { profileProp.curveX.AddKey(0f, prop.floatValue); profileProp.curveX.AddKey(1f, prop.floatValue); }
                            } else if (prop.type == MaterialProperty.PropType.Color) {
                                if (profileProp.curveX.length == 0) { profileProp.curveX.AddKey(0f, prop.colorValue.r); profileProp.curveX.AddKey(1f, prop.colorValue.r); }
                                if (profileProp.curveY.length == 0) { profileProp.curveY.AddKey(0f, prop.colorValue.g); profileProp.curveY.AddKey(1f, prop.colorValue.g); }
                                if (profileProp.curveZ.length == 0) { profileProp.curveZ.AddKey(0f, prop.colorValue.b); profileProp.curveZ.AddKey(1f, prop.colorValue.b); }
                                if (profileProp.curveW.length == 0) { profileProp.curveW.AddKey(0f, prop.colorValue.a); profileProp.curveW.AddKey(1f, prop.colorValue.a); }
                            } else if (prop.type == MaterialProperty.PropType.Vector) {
                                if (profileProp.curveX.length == 0) { profileProp.curveX.AddKey(0f, prop.vectorValue.x); profileProp.curveX.AddKey(1f, prop.vectorValue.x); }
                                if (profileProp.curveY.length == 0) { profileProp.curveY.AddKey(0f, prop.vectorValue.y); profileProp.curveY.AddKey(1f, prop.vectorValue.y); }
                                if (profileProp.curveZ.length == 0) { profileProp.curveZ.AddKey(0f, prop.vectorValue.z); profileProp.curveZ.AddKey(1f, prop.vectorValue.z); }
                                if (profileProp.curveW.length == 0) { profileProp.curveW.AddKey(0f, prop.vectorValue.w); profileProp.curveW.AddKey(1f, prop.vectorValue.w); }
                            }
                        }
                        EditorUtility.SetDirty(activeProfile);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(activeProfile, "Modify Profile Property");
                        if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range) profileProp.floatValue = prop.floatValue;
                        else if (prop.type == MaterialProperty.PropType.Color) profileProp.colorValue = prop.colorValue;
                        else if (prop.type == MaterialProperty.PropType.Vector) profileProp.vectorValue = prop.vectorValue;
                        EditorUtility.SetDirty(activeProfile);
                    }
    
                    if (profileProp.useCurve) {
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.indentLevel++;
                        if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range) {
                            profileProp.curveX = EditorGUILayout.CurveField("↳ Value Curve", profileProp.curveX);
                        } else if (prop.type == MaterialProperty.PropType.Color) {
                            profileProp.curveX = EditorGUILayout.CurveField("↳ R Curve", profileProp.curveX);
                            profileProp.curveY = EditorGUILayout.CurveField("↳ G Curve", profileProp.curveY);
                            profileProp.curveZ = EditorGUILayout.CurveField("↳ B Curve", profileProp.curveZ);
                            profileProp.curveW = EditorGUILayout.CurveField("↳ A Curve", profileProp.curveW);
                        } else if (prop.type == MaterialProperty.PropType.Vector) {
                            profileProp.curveX = EditorGUILayout.CurveField("↳ X Curve", profileProp.curveX);
                            profileProp.curveY = EditorGUILayout.CurveField("↳ Y Curve", profileProp.curveY);
                            profileProp.curveZ = EditorGUILayout.CurveField("↳ Z Curve", profileProp.curveZ);
                            profileProp.curveW = EditorGUILayout.CurveField("↳ W Curve", profileProp.curveW);
                        }
                        EditorGUI.indentLevel--;
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(activeProfile, "Modify Profile Curve");
                            EditorUtility.SetDirty(activeProfile);
                        }
                    }
                } 
                else 
                {
                    editor.ShaderProperty(prop, content);
                }
                
                HandleContextMenu(prop.name);
            }
    
            private void DrawTextureSingleLine(MaterialEditor editor, GUIContent friendlyContent, MaterialProperty texProp, MaterialProperty extraProp = null, SpineModuleProfile activeProfile = null)
            {
                if (texProp == null) return;
    
                SpineModuleProfile.ProfileProperty profileTexProp = null;
                if (activeProfile != null && activeProfile.properties != null) {
                    profileTexProp = activeProfile.properties.Find(p => p.name == texProp.name);
                }
                
                GUIContent content = showRawPropertyNames ? new GUIContent(texProp.name, friendlyContent.tooltip) : new GUIContent(friendlyContent.text, friendlyContent.tooltip);
                if (profileTexProp != null) content.text = "✦ " + content.text;
    
                EditorGUI.BeginChangeCheck();
                if (extraProp != null) editor.TexturePropertySingleLine(content, texProp, extraProp);
                else editor.TexturePropertySingleLine(content, texProp);
                
                if (EditorGUI.EndChangeCheck()) {
                    if (profileTexProp != null) {
                        Undo.RecordObject(activeProfile, "Modify Profile Texture");
                        profileTexProp.textureValue = texProp.textureValue;
                        EditorUtility.SetDirty(activeProfile);
                    }
                    if (extraProp != null && activeProfile != null) {
                        var pExtra = activeProfile.properties.Find(p => p.name == extraProp.name);
                        if (pExtra != null) {
                            Undo.RecordObject(activeProfile, "Modify Profile Color");
                            pExtra.colorValue = extraProp.colorValue;
                            EditorUtility.SetDirty(activeProfile);
                        }
                    }
                }
    
                if (extraProp != null && activeProfile != null) {
                    var pExtra = activeProfile.properties.Find(p => p.name == extraProp.name);
                    if (pExtra != null) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(EditorGUIUtility.labelWidth);
                        bool prevCurve = pExtra.useCurve;
                        GUIContent curveBtnContent = new GUIContent("∿ " + extraProp.name + " 曲線", "開啟顏色曲線");
                        bool nextCurve = GUILayout.Toggle(pExtra.useCurve, curveBtnContent, "Button");
                        
                        if (nextCurve != prevCurve) {
                            Undo.RecordObject(activeProfile, "Toggle Profile Curve");
                            pExtra.useCurve = nextCurve;
                            if (nextCurve) {
                                if (pExtra.curveX.length == 0) { pExtra.curveX.AddKey(0f, pExtra.colorValue.r); pExtra.curveX.AddKey(1f, pExtra.colorValue.r); }
                                if (pExtra.curveY.length == 0) { pExtra.curveY.AddKey(0f, pExtra.colorValue.g); pExtra.curveY.AddKey(1f, pExtra.colorValue.g); }
                                if (pExtra.curveZ.length == 0) { pExtra.curveZ.AddKey(0f, pExtra.colorValue.b); pExtra.curveZ.AddKey(1f, pExtra.colorValue.b); }
                                if (pExtra.curveW.length == 0) { pExtra.curveW.AddKey(0f, pExtra.colorValue.a); pExtra.curveW.AddKey(1f, pExtra.colorValue.a); }
                            }
                            EditorUtility.SetDirty(activeProfile);
                        }
                        EditorGUILayout.EndHorizontal();
    
                        if (pExtra.useCurve) {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.indentLevel++;
                            pExtra.curveX = EditorGUILayout.CurveField("↳ R Curve", pExtra.curveX);
                            pExtra.curveY = EditorGUILayout.CurveField("↳ G Curve", pExtra.curveY);
                            pExtra.curveZ = EditorGUILayout.CurveField("↳ B Curve", pExtra.curveZ);
                            pExtra.curveW = EditorGUILayout.CurveField("↳ A Curve", pExtra.curveW);
                            EditorGUI.indentLevel--;
                            if (EditorGUI.EndChangeCheck()) {
                                Undo.RecordObject(activeProfile, "Modify Profile Curve");
                                EditorUtility.SetDirty(activeProfile);
                            }
                        }
                    }
                }
                
                HandleContextMenu(texProp.name);
            }
    
            private void DrawZTestField(MaterialProperty prop, string label, string tooltip, SpineModuleProfile activeProfile = null)
            {
                if (prop == null) return;
                SpineModuleProfile.ProfileProperty profileProp = activeProfile?.properties?.Find(p => p.name == prop.name);
                
                ZTestMode cur = (ZTestMode)Mathf.RoundToInt(prop.floatValue);
                EditorGUI.BeginChangeCheck();
                
                GUIContent content = showRawPropertyNames ? new GUIContent(prop.name, tooltip) : new GUIContent(label, tooltip);
                if (profileProp != null) { content.text = "✦ " + content.text; content.tooltip = "[受 Preset 驅動] " + content.tooltip; }
    
                ZTestMode next = (ZTestMode)EditorGUILayout.EnumPopup(content, cur);
                
                if (EditorGUI.EndChangeCheck()) {
                    prop.floatValue = (float)(int)next;
                    if (profileProp != null) {
                        Undo.RecordObject(activeProfile, "Modify Profile ZTest");
                        profileProp.floatValue = prop.floatValue;
                        EditorUtility.SetDirty(activeProfile);
                    }
                }
                HandleContextMenu(prop.name);
            }
    
            private void DrawLightingModeField(MaterialProperty prop, MaterialProperty enableLightProbe, SpineModuleProfile activeProfile, System.Action onSyncRequired)
            {
                if (prop == null) return;
                SpineModuleProfile.ProfileProperty profileProp = activeProfile?.properties?.Find(p => p.name == prop.name);
    
                LightingMode current = (LightingMode)(int)prop.floatValue;
                EditorGUI.BeginChangeCheck();
                
                GUIContent content = showRawPropertyNames ? new GUIContent(prop.name, TIP_LIGHTING_MODE) : new GUIContent("Light Source", TIP_LIGHTING_MODE);
                if (profileProp != null) { content.text = "✦ " + content.text; content.tooltip = "[受 Preset 驅動] " + content.tooltip; }
    
                LightingMode next = (LightingMode)EditorGUILayout.EnumFlagsField(content, current);
                
                if (EditorGUI.EndChangeCheck()) {
                    next &= LightingMode.Realtime | LightingMode.LightProbe | LightingMode.ShadowLight;
                    prop.floatValue = (float)(int)next;
                    if (profileProp != null) {
                        Undo.RecordObject(activeProfile, "Modify Profile LightMode");
                        profileProp.floatValue = prop.floatValue;
                        EditorUtility.SetDirty(activeProfile);
                    }
                }
                HandleContextMenu(prop.name);
    
                if ((next & LightingMode.LightProbe) != 0 && (enableLightProbe == null || enableLightProbe.floatValue < 0.5f)) {
                    EditorGUILayout.HelpBox("已選擇 LightProbe，但全域「Enable Light Probe」尚未開啟。\n請至 LIGHTING > Light Probe Settings 開啟，或點下方按鈕。", MessageType.Warning);
                    if (enableLightProbe != null && GUILayout.Button("點此自動開啟 Enable Light Probe")) {
                        ApplyPropertyChange(enableLightProbe, 1f);
                        onSyncRequired?.Invoke();
                        GUIUtility.ExitGUI();
                    }
                }
            }
    
            private void HandleContextMenu(string propName)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent($"複製參數名稱: {propName}"), false, () => {
                        EditorGUIUtility.systemCopyBuffer = propName;
                        Debug.Log($"[SpineShaderGUI] 已複製屬性名稱到剪貼簿：{propName}");
                    });
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("📖 查看使用說明"), false, () => {
                        SpineArtHelpWindow.ShowWindowAndScrollTo(propName);
                    });
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
    
            private MaterialProperty[] ConcatProps(params MaterialProperty[][] arrays)
            {
                return arrays.SelectMany(x => x).Where(p => p != null).ToArray();
            }
    
            private static Texture2D TryFindTextureBySuffix(MaterialProperty mainTex, string suffix)
            {
                if (mainTex == null || mainTex.textureValue == null) return null;
                string path = AssetDatabase.GetAssetPath(mainTex.textureValue);
                if (string.IsNullOrEmpty(path)) return null;
                string dir  = Path.GetDirectoryName(path);
                string name = Path.GetFileNameWithoutExtension(path);
                string ext  = Path.GetExtension(path);
                foreach (string e in new[] { ext, ".png", ".tga", ".jpg", ".psd" })
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(dir, name + suffix + e).Replace('\\', '/'));
                    if (tex != null) return tex;
                }
                return null;
            }
    
            // =========================================================
            // 核心 OnGUI
            // =========================================================
            public override void OnGUI(MaterialEditor editor, MaterialProperty[] props)
            {
                MaterialProperty resetCheck = FindProperty("_MaterialResetCheck", props, false);
                if (resetCheck != null && resetCheck.floatValue < 0.5f) {
        
                    // 1. 先在當前幀把檢查碼設為 1，防止下一幀重複觸發
                    resetCheck.floatValue = 1f;
    
                    // 2. 將大幅度修改材質球的邏輯，推遲到當前 GUI 繪製週期結束後執行
                    EditorApplication.delayCall += () =>
                    {
                        // 防呆檢查，確保編輯器還活著
                        if (editor == null || editor.targets == null) return;
    
                        // 執行原本的清理邏輯
                        PerformResetCleanup(editor);
                        ApplySpriteRendererDefaults(editor);
    
                        // 套用當前平台預設值
                        foreach (Material mat in editor.targets) {
                            if (mat != null) {
                                ApplyCurrentPlatformSettings(mat);
                                // 雙重保險，確保真實的材質球屬性被正確寫入
                                mat.SetFloat("_MaterialResetCheck", 1f); 
                            }
                        }
            
                        // 強制 Inspector 重新整理，確保畫面正確更新
                        ActiveEditorTracker.sharedTracker.ForceRebuild();
                    };
    
                    // 注意：這裡【不要】加 GUIUtility.ExitGUI()，讓它順順地把這幀畫完
                }
    
                Material targetMat = editor.target as Material;
                if (!_initialized || _lastTargetMat != targetMat) {
                    _initialized = true;
                    _lastTargetMat = targetMat;
                    if (targetMat != null) LoadProfilesFromMaterial(targetMat); 
                }
    
                MaterialProperty cutoff            = FindProperty("_Cutoff",              props, false);
                MaterialProperty mainTex           = FindProperty("_MainTex",             props, false);
                MaterialProperty mainTex_ST        = FindProperty("_MainTex_ST",          props, false);
                MaterialProperty mainColor         = FindProperty("_Color",               props, false);
                MaterialProperty effectColor       = FindProperty("_EffectColor",         props, false);
                MaterialProperty effectIntensity   = FindProperty("_EffectIntensity",     props, false);
                MaterialProperty enableBloomSup    = FindProperty("_EnableBloomSuppression", props, false);
                MaterialProperty bloomThreshold    = FindProperty("_BloomSuppressThreshold", props, false);
                MaterialProperty enableGlobalNoise = FindProperty("_EnableGlobalNoise",   props, false);
                MaterialProperty globalNoiseScale  = FindProperty("_GlobalNoiseScale",    props, false);
                MaterialProperty globalNoiseSpeed  = FindProperty("_GlobalNoiseSpeed",    props, false);
                
                MaterialProperty enableNativeLight = FindProperty("_EnableNativeLighting",props, false);
                MaterialProperty nativeLightInt    = FindProperty("_NativeLightIntensity",props, false);
                MaterialProperty dirLightInt       = FindProperty("_DirectionalLightIntensity", props, false);
                MaterialProperty addLightInt       = FindProperty("_AdditionalLightIntensity",  props, false);
                MaterialProperty straightAlpha     = FindProperty("_StraightAlphaInput",  props, false);
                MaterialProperty lightAffectsAdd   = FindProperty("_LightAffectsAdditive",props, false);
                MaterialProperty probeNativeBlend  = FindProperty("_LightProbeNativeBlend",props, false);
                MaterialProperty enableShadowLight = FindProperty("_EnableShadowLight",   props, false);
                MaterialProperty shadowLightBaseInt = FindProperty("_ShadowLightBaseIntensity", props, false);
                MaterialProperty slAffectsNormal   = FindProperty("_SL_AffectsNormalMap", props, false);
                MaterialProperty enableLightProbe  = FindProperty("_EnableLightProbe",    props, false);
                MaterialProperty lightProbeInt     = FindProperty("_LightProbeIntensity", props, false);
                MaterialProperty enableHitSweep    = FindProperty("_EnableHitSweep",      props, false);
                MaterialProperty sweepColor        = FindProperty("_SweepColor",          props, false);
                MaterialProperty sweepWidth        = FindProperty("_SweepWidth",          props, false);
                MaterialProperty sweepSoftness     = FindProperty("_SweepSoftness",       props, false);
    
                MaterialProperty enableNormalMap = FindProperty("_EnableNormalMap", props, false);
                MaterialProperty invertNormal    = FindProperty("_InvertNormalMap", props, false);
                MaterialProperty normalMap       = FindProperty("_NormalMap",       props, false);
                MaterialProperty normalIntensity = FindProperty("_NormalIntensity", props, false);
    
                MaterialProperty enableEmission  = FindProperty("_EnableEmission",   props, false);
                MaterialProperty emissionTex     = FindProperty("_EmissionTex",      props, false);
                MaterialProperty emissionColor   = FindProperty("_EmissionColor",    props, false);
                MaterialProperty emissionBlend   = FindProperty("_EmissionBlend",    props, false);
                MaterialProperty emissionInt     = FindProperty("_EmissionIntensity",props, false);
                MaterialProperty emissionMulMain = FindProperty("_EmissionMulMain",  props, false);
    
                MaterialProperty enableBackLight    = FindProperty("_EnableBackLight",            props, false);
                MaterialProperty backLightLightMode = FindProperty("_BackLightLightingMode",      props, false);
                MaterialProperty specularTex        = FindProperty("_SpecularTex",                props, false);
                MaterialProperty specularColor      = FindProperty("_SpecularColor",              props, false);
                MaterialProperty backLightMode      = FindProperty("_BackLightMode",              props, false);
                MaterialProperty specularBlend      = FindProperty("_SpecularBlend",              props, false);
                MaterialProperty specularMulMain    = FindProperty("_SpecularMulMain",            props, false);
                MaterialProperty enableDirBL        = FindProperty("_EnableDirectionalBackLight", props, false);
                MaterialProperty blTintByLight      = FindProperty("_BackLightTintByLight",       props, false);
                MaterialProperty blEnableNoise      = FindProperty("_BL_EnableNoise",             props, false);
                MaterialProperty blNoiseIntensity   = FindProperty("_BL_NoiseIntensity",          props, false);
                MaterialProperty blRtPower          = FindProperty("_BL_RT_Power",    props, false);
                MaterialProperty blRtBlend          = FindProperty("_BL_RT_Blend",    props, false);
                MaterialProperty blRtIntensity      = FindProperty("_BL_RT_Intensity",props, false);
                MaterialProperty blProbePower       = FindProperty("_BL_Probe_Power", props, false);
                MaterialProperty blProbeBlend       = FindProperty("_BL_Probe_Blend", props, false);
                MaterialProperty blProbeIntensity   = FindProperty("_BL_Probe_Intensity", props, false);
                MaterialProperty blShadowPower      = FindProperty("_BL_Shadow_Power", props, false);
                MaterialProperty blShadowBlend      = FindProperty("_BL_Shadow_Blend", props, false);
                MaterialProperty blShadowIntensity  = FindProperty("_BL_Shadow_Intensity", props, false);
                MaterialProperty blSLIgnoreBehind   = FindProperty("_BL_SL_IgnoreBehind",  props, false);
    
                MaterialProperty enableInline       = FindProperty("_EnableInline",           props, false);
                MaterialProperty inlineLightMode    = FindProperty("_InlineLightingMode",     props, false);
                MaterialProperty inlineColor        = FindProperty("_InlineColor",            props, false);
                MaterialProperty inlineWidth        = FindProperty("_InlineWidth",            props, false);
                MaterialProperty inlineFadeSteps    = FindProperty("_InlineFadeSteps",        props, false);
                MaterialProperty enableDirIL        = FindProperty("_EnableDirectionalInline",props, false);
                MaterialProperty ilTintByLight      = FindProperty("_InlineTintByLight",      props, false);
                MaterialProperty inlineZTest        = FindProperty("_InlineZTest",            props, false);
                MaterialProperty ilEnableNoise      = FindProperty("_IL_EnableNoise",         props, false);
                MaterialProperty ilNoiseIntensity   = FindProperty("_IL_NoiseIntensity",      props, false);
                MaterialProperty ilRtPower          = FindProperty("_IL_RT_Power",    props, false);
                MaterialProperty ilRtBlend          = FindProperty("_IL_RT_Blend",    props, false);
                MaterialProperty ilRtIntensity      = FindProperty("_IL_RT_Intensity",props, false);
                MaterialProperty ilProbePower       = FindProperty("_IL_Probe_Power", props, false);
                MaterialProperty ilProbeBlend       = FindProperty("_IL_Probe_Blend", props, false);
                MaterialProperty ilProbeIntensity   = FindProperty("_IL_Probe_Intensity", props, false);
                MaterialProperty ilShadowPower      = FindProperty("_IL_Shadow_Power", props, false);
                MaterialProperty ilShadowBlend      = FindProperty("_IL_Shadow_Blend", props, false);
                MaterialProperty ilShadowIntensity  = FindProperty("_IL_Shadow_Intensity", props, false);
                MaterialProperty ilSLIgnoreBehind   = FindProperty("_IL_SL_IgnoreBehind",  props, false);
    
                MaterialProperty enableOutline      = FindProperty("_EnableOutline",             props, false);
                MaterialProperty outlineLightMode   = FindProperty("_OutlineLightingMode",       props, false);
                MaterialProperty outlineWidth       = FindProperty("_OutlineWidth",              props, false);
                MaterialProperty outlineColor       = FindProperty("_OutlineColor",              props, false);
                MaterialProperty outlineSmoothness  = FindProperty("_ThresholdEnd",              props, false);
                MaterialProperty outlineAlphaCutoff = FindProperty("_OutlineAlphaCutoff",        props, false);
                MaterialProperty multiplyEdgeColor  = FindProperty("_MultiplyEdgeColor",         props, false);
                MaterialProperty enableDirOL        = FindProperty("_EnableDirectionalOutline",  props, false);
                MaterialProperty olTintByLight      = FindProperty("_OutlineTintByLight",        props, false);
                MaterialProperty outlineZTest       = FindProperty("_OutlineZTest",              props, false);
                MaterialProperty olEnableNoise      = FindProperty("_OL_EnableNoise",            props, false);
                MaterialProperty olNoiseIntensity   = FindProperty("_OL_NoiseIntensity",         props, false);
                MaterialProperty useScreenSpace     = FindProperty("_UseScreenSpaceOutlineWidth",props, false);
                MaterialProperty fillInside         = FindProperty("_Fill",                      props, false);
                MaterialProperty refTexWidth        = FindProperty("_OutlineReferenceTexWidth",  props, false);
                MaterialProperty use8N              = FindProperty("_Use8Neighbourhood",         props, false);
                MaterialProperty opaqueAlpha        = FindProperty("_OutlineOpaqueAlpha",        props, false);
                MaterialProperty mipLevel           = FindProperty("_OutlineMipLevel",           props, false);
                MaterialProperty olRtPower          = FindProperty("_OL_RT_Power",    props, false);
                MaterialProperty olRtBlend          = FindProperty("_OL_RT_Blend",    props, false);
                MaterialProperty olRtIntensity      = FindProperty("_OL_RT_Intensity",props, false);
                MaterialProperty olProbePower       = FindProperty("_OL_Probe_Power", props, false);
                MaterialProperty olProbeBlend       = FindProperty("_OL_Probe_Blend", props, false);
                MaterialProperty olProbeIntensity   = FindProperty("_OL_Probe_Intensity", props, false);
                MaterialProperty olShadowPower      = FindProperty("_OL_Shadow_Power", props, false);
                MaterialProperty olShadowBlend      = FindProperty("_OL_Shadow_Blend", props, false);
                MaterialProperty olShadowIntensity  = FindProperty("_OL_Shadow_Intensity", props, false);
                MaterialProperty olSLIgnoreBehind   = FindProperty("_OL_SL_IgnoreBehind",  props, false);
    
                MaterialProperty[] mainShadowProps   = { mainTex, mainTex_ST, mainColor, cutoff, straightAlpha };
                MaterialProperty[] postProcProps     = { enableBloomSup, bloomThreshold };
                MaterialProperty[] globalNoiseProps  = { enableGlobalNoise, globalNoiseScale, globalNoiseSpeed };
                MaterialProperty[] nativeLightProps  = { enableNativeLight, nativeLightInt, dirLightInt, addLightInt, lightAffectsAdd, probeNativeBlend };
                MaterialProperty[] shadowLightProps  = { enableShadowLight, shadowLightBaseInt, slAffectsNormal };
                MaterialProperty[] normalMapProps    = { enableNormalMap, invertNormal, normalMap, normalIntensity };
                MaterialProperty[] lightProbeProps   = { enableLightProbe, lightProbeInt };
                MaterialProperty[] emissionProps     = { enableEmission, emissionTex, emissionColor, emissionBlend, emissionInt, emissionMulMain };
                MaterialProperty[] effectOverlayProps= { effectColor, effectIntensity };
                MaterialProperty[] hitSweepProps     = { enableHitSweep, sweepColor, sweepWidth, sweepSoftness };
                MaterialProperty[] backLightProps    = { enableBackLight, backLightLightMode, blSLIgnoreBehind, specularTex, specularColor, backLightMode, specularBlend, specularMulMain, enableDirBL, blTintByLight, blEnableNoise, blNoiseIntensity, blRtPower, blRtBlend, blRtIntensity, blProbePower, blProbeBlend, blProbeIntensity, blShadowPower, blShadowBlend, blShadowIntensity };
                MaterialProperty[] inlineProps       = { enableInline, inlineLightMode, ilSLIgnoreBehind, inlineColor, inlineWidth, inlineFadeSteps, enableDirIL, ilTintByLight, inlineZTest, ilEnableNoise, ilNoiseIntensity, ilRtPower, ilRtBlend, ilRtIntensity, ilProbePower, ilProbeBlend, ilProbeIntensity, ilShadowPower, ilShadowBlend, ilShadowIntensity };
                MaterialProperty[] outlineProps      = { enableOutline, outlineLightMode, olSLIgnoreBehind, outlineWidth, outlineColor, outlineSmoothness, outlineAlphaCutoff, multiplyEdgeColor, enableDirOL, olTintByLight, outlineZTest, olEnableNoise, olNoiseIntensity, useScreenSpace, fillInside, refTexWidth, use8N, opaqueAlpha, mipLevel, olRtPower, olRtBlend, olRtIntensity, olProbePower, olProbeBlend, olProbeIntensity, olShadowPower, olShadowBlend, olShadowIntensity };
    
                MaterialProperty[] mainCatProps   = ConcatProps(mainShadowProps, postProcProps);
                MaterialProperty[] noiseCatProps  = ConcatProps(globalNoiseProps);
                MaterialProperty[] lightCatProps  = ConcatProps(nativeLightProps, shadowLightProps, normalMapProps, lightProbeProps);
                MaterialProperty[] emisCatProps   = ConcatProps(emissionProps);
                MaterialProperty[] effectCatProps = ConcatProps(effectOverlayProps, hitSweepProps, backLightProps);
                MaterialProperty[] lineCatProps   = ConcatProps(inlineProps, outlineProps);
    
                if (normalMap != null && normalMap.textureValue == null) {
                    Texture2D found = TryFindTextureBySuffix(mainTex, "_Normal");
                    if (found != null) { normalMap.textureValue = found; if (enableNormalMap != null) enableNormalMap.floatValue = 1f; EditorUtility.SetDirty(editor.target); }
                }
                if (specularTex != null && specularTex.textureValue == null) {
                    Texture2D found = TryFindTextureBySuffix(mainTex, "_BackLight");
                    if (found != null) { specularTex.textureValue = found; if (enableBackLight != null) enableBackLight.floatValue = 1f; EditorUtility.SetDirty(editor.target); }
                }
                if (emissionTex != null && emissionTex.textureValue == null) {
                    Texture2D found = TryFindTextureBySuffix(mainTex, "_Emission");
                    if (found != null) { emissionTex.textureValue = found; if (enableEmission != null) enableEmission.floatValue = 1f; EditorUtility.SetDirty(editor.target); }
                }
    
                DrawTopButtons(editor);
    
                // =========================================================
                // 實時預掛腳本管理區塊 (Auto Components UI)
                // =========================================================
                int blModeCheck = GetIntFeature(enableBackLight, profBackLightSettings, "_BackLightLightingMode");
                int ilModeCheck = GetIntFeature(enableInline, profInlineSettings, "_InlineLightingMode");
                int olModeCheck = GetIntFeature(enableOutline, profOutlineSettings, "_OutlineLightingMode");
    
                bool reqAlphaMask = IsFeatureEnabled(enableInline, profInlineSettings, "_EnableInline");
                bool reqHitSweep  = IsFeatureEnabled(enableHitSweep, profHitSweepSettings, "_EnableHitSweep");
                bool reqBloomSync = IsFeatureEnabled(enableBloomSup, profPostProcessing, "_EnableBloomSuppression");
                bool reqLightReceiver = 
                    IsFeatureEnabled(enableNormalMap, profNormalMapSettings, "_EnableNormalMap") ||
                    (IsFeatureEnabled(enableBackLight, profBackLightSettings, "_EnableBackLight") && blModeCheck != 0) ||
                    (reqAlphaMask && ilModeCheck != 0) ||
                    (IsFeatureEnabled(enableOutline, profOutlineSettings, "_EnableOutline") && olModeCheck != 0) ||
                    IsFeatureEnabled(enableLightProbe, profLightProbeSettings, "_EnableLightProbe") ||
                    IsFeatureEnabled(enableShadowLight, profShadowLightSettings, "_EnableShadowLight");
    
                DrawScriptAttachmentSection(editor, reqAlphaMask, reqHitSweep, reqLightReceiver, reqBloomSync);
    
                // =========================================================
                // 同步委派邏輯 (pendingComponentSync)
                // =========================================================
                System.Action pendingComponentSync = () => {
                    bool needInlineMask = IsFeatureEnabled(enableInline, profInlineSettings, "_EnableInline");
                    bool needHitSweep   = IsFeatureEnabled(enableHitSweep, profHitSweepSettings, "_EnableHitSweep");
                    bool needBloomSync  = IsFeatureEnabled(enableBloomSup, profPostProcessing, "_EnableBloomSuppression");
    
                    int blMode = GetIntFeature(backLightLightMode, profBackLightSettings, "_BackLightLightingMode");
                    int ilMode = GetIntFeature(inlineLightMode, profInlineSettings, "_InlineLightingMode");
                    int olMode = GetIntFeature(outlineLightMode, profOutlineSettings, "_OutlineLightingMode");
    
                    bool needLightData =
                        IsFeatureEnabled(enableNormalMap, profNormalMapSettings, "_EnableNormalMap") ||
                        (IsFeatureEnabled(enableBackLight, profBackLightSettings, "_EnableBackLight") && blMode != 0) ||
                        (IsFeatureEnabled(enableInline, profInlineSettings, "_EnableInline") && ilMode != 0) ||
                        (IsFeatureEnabled(enableOutline, profOutlineSettings, "_EnableOutline") && olMode != 0) ||
                        IsFeatureEnabled(enableLightProbe, profLightProbeSettings, "_EnableLightProbe") ||
                        IsFeatureEnabled(enableShadowLight, profShadowLightSettings, "_EnableShadowLight");
    
                    SyncAutoComponent(editor.targets, "SpineAlphaMaskRenderer", needInlineMask);
                    SyncAutoComponent(editor.targets, "SpineHitSweepEffect", needHitSweep);
                    SyncAutoComponent(editor.targets, "SpineLightReceiver", needLightData);
                    if (needLightData) EditorApplication.delayCall += EnsureSpineLightManager;
                    SyncAutoComponent(editor.targets, "SpineBloomThresholdSync", needBloomSync);
                };
    
                EditorGUI.BeginChangeCheck();
    
                bool w_il = IsFeatureEnabled(enableInline, profInlineSettings, "_EnableInline");
                bool w_bl = IsFeatureEnabled(enableBackLight, profBackLightSettings, "_EnableBackLight");
                LightingMode w_blM = (LightingMode)GetIntFeature(backLightLightMode, profBackLightSettings, "_BackLightLightingMode");
                bool w_ol = IsFeatureEnabled(enableOutline, profOutlineSettings, "_EnableOutline");
                LightingMode w_olM = (LightingMode)GetIntFeature(outlineLightMode, profOutlineSettings, "_OutlineLightingMode");
                bool w_lp = IsFeatureEnabled(enableLightProbe, profLightProbeSettings, "_EnableLightProbe");
                bool w_sl = IsFeatureEnabled(enableShadowLight, profShadowLightSettings, "_EnableShadowLight");
                bool w_bs = IsFeatureEnabled(enableBloomSup, profPostProcessing, "_EnableBloomSuppression");
                bool w_hs = IsFeatureEnabled(enableHitSweep, profHitSweepSettings, "_EnableHitSweep");
                
                bool needWarning = w_il || (w_bl && w_blM != LightingMode.Disabled) || (w_ol && w_olM != LightingMode.Disabled) || w_lp || w_sl || w_bs || w_hs;
                CheckAndShowLayerWarning(needWarning);
    
                bool isMainOpen = BeginCategoryBox(ref showMainCategory, "❖ MAIN", ColorMain, mainCatProps, editor, new string[] { "Main & Shadow Settings", "Post-Processing Control" }, pendingComponentSync);
                if (isMainOpen)
                {
                    BeginHelpBox();
                    if (DrawSubHeader(ref showMainSettings, ref pShowMainSettings, "Main & Shadow Settings", ColorMain, mainShadowProps, editor, ref profMainSettings, pendingComponentSync))
                    {
                        GUILayout.Space(4); EditorGUI.indentLevel++;
                        if (mainTex != null) {
                            DrawTextureSingleLine(editor, new GUIContent("Main Texture"), mainTex, null, profMainSettings);
                            if (mainTex_ST != null) {
                                EditorGUI.indentLevel++;
                                DrawProperty(editor, mainTex_ST, new GUIContent("Tiling & Offset (XY:Tiling ZW:Offset)"), profMainSettings);
                                EditorGUI.indentLevel--;
                            }
                        }
                        if (mainColor != null)
                        {
                            DrawProperty(editor, mainColor, new GUIContent("Main Color", TIP_MAIN_COLOR_ALPHA), profMainSettings);
                            if (mainColor.colorValue.a < 0.99f)
                                EditorGUILayout.HelpBox($"Main Color Alpha = {mainColor.colorValue.a:F2}，所有模組透明度均受此值影響。", MessageType.Info);
                        }
                        if (cutoff         != null) DrawProperty(editor, cutoff,        new GUIContent("Shadow Alpha Cutoff"), profMainSettings);
                        if (straightAlpha!= null) DrawProperty(editor, straightAlpha,new GUIContent("Straight Alpha Texture"), profMainSettings);
                        EditorGUI.indentLevel--; GUILayout.Space(4);
                    }
                    EndHelpBox(); GUILayout.Space(3);
    
                    if (enableBloomSup != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showPostProcessing, ref pShowPostProcessing, "Post-Processing Control", ColorMain, postProcProps, editor, ref profPostProcessing, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableBloomSup, new GUIContent("Suppress Bloom"), profPostProcessing);
                            if (enableBloomSup.floatValue > 0.5f && bloomThreshold != null)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUI.BeginDisabledGroup(true);
                                DrawProperty(editor, bloomThreshold, new GUIContent("Bloom Threshold", "由 SpineBloomThresholdSync 自動同步。"), profPostProcessing);
                                EditorGUI.EndDisabledGroup();
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox();
                    }
                }
                EndCategoryBox(isMainOpen);
    
                bool isNoiseOpen = BeginCategoryBox(ref showNoiseCategory, "❖ NOISE", ColorNoise, noiseCatProps, editor, new string[] { "Global Noise Settings" }, pendingComponentSync);
                if (isNoiseOpen)
                {
                    BeginHelpBox();
                    if (DrawSubHeader(ref showGlobalNoise, ref pShowGlobalNoise, "Global Noise Settings", ColorNoise, globalNoiseProps, editor, ref profGlobalNoise, pendingComponentSync))
                    {
                        GUILayout.Space(4); EditorGUI.indentLevel++;
                        if (enableGlobalNoise != null) DrawProperty(editor, enableGlobalNoise, new GUIContent("Enable Global Noise"), profGlobalNoise);
                        if (enableGlobalNoise != null && enableGlobalNoise.floatValue > 0.5f)
                        {
                            EditorGUI.indentLevel++;
                            if (globalNoiseScale != null) DrawProperty(editor, globalNoiseScale, new GUIContent("Noise Scale (X,Y)"), profGlobalNoise);
                            if (globalNoiseSpeed != null) DrawProperty(editor, globalNoiseSpeed, new GUIContent("Noise Speed (X,Y)"), profGlobalNoise);
                            EditorGUI.indentLevel--;
                        }
                        
                        bool anyNoise = IsFeatureEnabled(blEnableNoise, profBackLightSettings, "_BL_EnableNoise") ||
                                        IsFeatureEnabled(ilEnableNoise, profInlineSettings, "_IL_EnableNoise") ||
                                        IsFeatureEnabled(olEnableNoise, profOutlineSettings, "_OL_EnableNoise");
                                        
                        if (anyNoise && !IsFeatureEnabled(enableGlobalNoise, profGlobalNoise, "_EnableGlobalNoise"))
                        {
                            EditorGUILayout.HelpBox("BackLight / Inline / Outline 中已啟用 Noise Mask，Global Noise 需被開啟。系統將在變更後自動開啟。", MessageType.Info);
                        }
                        EditorGUI.indentLevel--; GUILayout.Space(4);
                    }
                    EndHelpBox();
                }
                EndCategoryBox(isNoiseOpen);
    
                bool isLightOpen = BeginCategoryBox(ref showLightingCategory, "❖ LIGHTING", ColorLighting, lightCatProps, editor, new string[] { "Native Lighting Settings", "Shadow Light Base Settings", "Normal Map Settings", "Light Probe Settings" }, pendingComponentSync);
                if (isLightOpen)
                {
                    if (enableNativeLight != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showLightingSettings, ref pShowLightingSettings, "Native Lighting Settings", ColorLighting, nativeLightProps, editor, ref profLightingSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
    
                            if (enableNativeLight != null) {
                                DrawProperty(editor, enableNativeLight, new GUIContent("Enable Native Lighting"), profLightingSettings);
                            }
    
                            if (enableNativeLight != null && enableNativeLight.floatValue > 0.5f)
                            {
                                EditorGUI.indentLevel++;
                                if (nativeLightInt != null) DrawProperty(editor, nativeLightInt, new GUIContent("Master Light Intensity"), profLightingSettings);
                                if (dirLightInt    != null) DrawProperty(editor, dirLightInt,    new GUIContent("Directional Intensity"), profLightingSettings);
                                
                                if (addLightInt    != null) DrawProperty(editor, addLightInt,    new GUIContent("Additional Intensity"), profLightingSettings);
                                if (lightAffectsAdd!= null) DrawProperty(editor, lightAffectsAdd,new GUIContent("Light Affects Additive"), profLightingSettings);
    
                                DrawInnerHeader("Light Probe Blend", ColorLighting);
                                if (probeNativeBlend != null)
                                {
                                    bool probeOn = IsFeatureEnabled(enableLightProbe, profLightProbeSettings, "_EnableLightProbe");
                                    using (new EditorGUI.DisabledScope(!probeOn))
                                        DrawProperty(editor, probeNativeBlend, new GUIContent("Probe → Native Lighting", "需先開啟全域 Light Probe"), profLightingSettings);
                                    if (!probeOn && probeNativeBlend.floatValue > 0.01f)
                                    {
                                        EditorGUILayout.HelpBox("「Light Probe Blend」需先開啟全域 Enable Light Probe。", MessageType.Warning);
                                        if (enableLightProbe != null && GUILayout.Button("點此自動開啟 Enable Light Probe")) {
                                            ApplyPropertyChange(enableLightProbe, 1f);
                                            pendingComponentSync?.Invoke();
                                            GUIUtility.ExitGUI();
                                        }
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox(); GUILayout.Space(3);
                    }
    
                    if (enableShadowLight != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showShadowLightSettings, ref pShowShadowLightSettings, "Shadow Light Base Settings", ColorLighting, shadowLightProps, editor, ref profShadowLightSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableShadowLight, new GUIContent("Enable Shadow Light Base"), profShadowLightSettings);
                            if (enableShadowLight.floatValue > 0.5f)
                            {
                                EditorGUI.indentLevel++;
                                if (shadowLightBaseInt != null) DrawProperty(editor, shadowLightBaseInt, new GUIContent("Base Intensity"), profShadowLightSettings);
                                if (slAffectsNormal != null) DrawProperty(editor, slAffectsNormal, new GUIContent("Affects Normal Map"), profShadowLightSettings);
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox(); GUILayout.Space(3);
                    }
    
                    if (enableNormalMap != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showNormalMapSettings, ref pShowNormalMapSettings, "Normal Map Settings", ColorLighting, normalMapProps, editor, ref profNormalMapSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableNormalMap, new GUIContent("Enable Normal Map Module"), profNormalMapSettings);
                            if (enableNormalMap.floatValue > 0.5f)
                            {
                                EditorGUI.indentLevel++;
                                if (normalMap != null) DrawTextureSingleLine(editor, new GUIContent("Normal Map"), normalMap, null, profNormalMapSettings);
                                if (invertNormal != null) DrawProperty(editor, invertNormal, new GUIContent("Invert Normal Height"), profNormalMapSettings);
                                if (normalIntensity != null) DrawProperty(editor, normalIntensity, new GUIContent("Normal Intensity"), profNormalMapSettings);
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox(); GUILayout.Space(3);
                    }
    
                    if (enableLightProbe != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showLightProbeSettings, ref pShowLightProbeSettings, "Light Probe Settings", ColorLighting, lightProbeProps, editor, ref profLightProbeSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableLightProbe, new GUIContent("Enable Light Probe"), profLightProbeSettings);
                            if (enableLightProbe.floatValue > 0.5f)
                            {
                                EditorGUI.indentLevel++;
                                if (lightProbeInt != null) DrawProperty(editor, lightProbeInt, new GUIContent("Probe Intensity"), profLightProbeSettings);
                                DrawProbePreview(ColorLighting);
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox();
                    }
                }
                EndCategoryBox(isLightOpen);
    
                bool isEmissionOpen = BeginCategoryBox(ref showEmissionCategory, "❖ EMISSION", ColorEmission, emisCatProps, editor, new string[] { "Emission Settings" }, pendingComponentSync);
                if (isEmissionOpen)
                {
                    if (enableEmission != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showEmissionSettings, ref pShowEmissionSettings, "Emission Settings", ColorEmission, emissionProps, editor, ref profEmissionSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableEmission, new GUIContent("Enable Emission"), profEmissionSettings);
                            if (enableEmission.floatValue > 0.5f)
                            {
                                DrawInnerHeader("Base Settings", ColorEmission);
                                if (emissionTex != null) DrawTextureSingleLine(editor, new GUIContent("Emission Texture"), emissionTex, emissionColor, profEmissionSettings);
                                if (emissionBlend != null) DrawProperty(editor, emissionBlend, new GUIContent("Blend Weight"), profEmissionSettings);
                                if (emissionInt != null) DrawProperty(editor, emissionInt, new GUIContent("Intensity"), profEmissionSettings);
                                if (emissionMulMain != null) DrawProperty(editor, emissionMulMain, new GUIContent("Multiply With MainTex"), profEmissionSettings);
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox();
                    }
                }
                EndCategoryBox(isEmissionOpen);
    
                bool isEffectOpen = BeginCategoryBox(ref showEffectCategory, "❖ EFFECT", ColorEffect, effectCatProps, editor, new string[] { "Effect Overlay", "Hit Sweep Settings", "BackLight Settings" }, pendingComponentSync);
                if (isEffectOpen)
                {
                    if (effectColor != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showEffectOverlay, ref pShowEffectOverlay, "Effect Overlay", ColorEffect, effectOverlayProps, editor, ref profEffectOverlay, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, effectColor,      new GUIContent("Effect Color (HDR)"), profEffectOverlay);
                            DrawProperty(editor, effectIntensity, new GUIContent("Effect Intensity"), profEffectOverlay);
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox(); GUILayout.Space(3);
                    }
    
                    if (enableHitSweep != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showHitSweepSettings, ref pShowHitSweepSettings, "Hit Sweep Settings", ColorEffect, hitSweepProps, editor, ref profHitSweepSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableHitSweep, new GUIContent("Enable Hit Sweep"), profHitSweepSettings);
                            if (enableHitSweep.floatValue > 0.5f)
                            {
                                if (sweepColor   != null) DrawProperty(editor, sweepColor,   new GUIContent("Sweep Color (HDR)"), profHitSweepSettings);
                                if (sweepWidth   != null) DrawProperty(editor, sweepWidth,   new GUIContent("Sweep Width"), profHitSweepSettings);
                                if (sweepSoftness!= null) DrawProperty(editor, sweepSoftness,new GUIContent("Sweep Softness"), profHitSweepSettings);
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox(); GUILayout.Space(3);
                    }
    
                    if (enableBackLight != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showBackLightSettings, ref pShowBackLightSettings, "BackLight Settings", ColorEffect, backLightProps, editor, ref profBackLightSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableBackLight, new GUIContent("Enable BackLight"), profBackLightSettings);
                            if (enableBackLight.floatValue > 0.5f)
                            {
                                DrawInnerHeader("Lighting Mode", ColorEffect);
                                DrawLightingModeField(backLightLightMode, enableLightProbe, profBackLightSettings, pendingComponentSync);
                                LightingMode blMode = backLightLightMode != null ? (LightingMode)(int)backLightLightMode.floatValue : LightingMode.Disabled;
    
                                if ((blMode & LightingMode.ShadowLight) != 0 && blSLIgnoreBehind != null) {
                                    EditorGUI.indentLevel++;
                                    DrawProperty(editor, blSLIgnoreBehind, new GUIContent("Ignore Behind Fade (Shadow Light)", "勾選後，當 Shadow Light 位於角色背後時將忽略漸隱效果"), profBackLightSettings);
                                    EditorGUI.indentLevel--;
                                }
    
                                DrawInnerHeader("Base Settings", ColorEffect);
                                if (specularTex  != null) DrawTextureSingleLine(editor, new GUIContent("BackLight Texture"), specularTex, specularColor, profBackLightSettings);
                                if (specularBlend != null) DrawProperty(editor, specularBlend, new GUIContent("Blend Intensity"), profBackLightSettings);
                                if (backLightMode != null) DrawProperty(editor, backLightMode, new GUIContent("Blend Mode"), profBackLightSettings);
                                if (specularMulMain!=null) DrawProperty(editor, specularMulMain,new GUIContent("Multiply With MainTex"), profBackLightSettings);
    
                                if (blMode != LightingMode.Disabled)
                                {
                                    if (enableDirBL != null) {
                                        DrawInnerHeader("Directional Mask", ColorEffect);
                                        DrawProperty(editor, enableDirBL, new GUIContent("Mask By Light Direction", TIP_MASK), profBackLightSettings);
                                        if (enableDirBL.floatValue > 0.5f) {
                                            EditorGUI.indentLevel++;
                                            if ((blMode & LightingMode.Realtime)    != 0) DrawProperty(editor, blRtPower,    new GUIContent("Realtime Focus Power"), profBackLightSettings);
                                            if ((blMode & LightingMode.LightProbe)  != 0) DrawProperty(editor, blProbePower, new GUIContent("Probe Focus Power"), profBackLightSettings);
                                            if ((blMode & LightingMode.ShadowLight) != 0) DrawProperty(editor, blShadowPower,new GUIContent("ShadowLight Focus Power"), profBackLightSettings);
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                    DrawInnerHeader("Light Color Influence", ColorEffect);
                                    if (blTintByLight != null) {
                                        DrawProperty(editor, blTintByLight, new GUIContent("Tint By Light Color", TIP_TINT), profBackLightSettings);
                                        if (blTintByLight.floatValue > 0.5f) {
                                            EditorGUI.indentLevel++;
                                            if ((blMode & LightingMode.Realtime) != 0)    { DrawProperty(editor, blRtBlend,    new GUIContent("Realtime Color Blend"), profBackLightSettings);   EditorGUI.indentLevel++; DrawProperty(editor, blRtIntensity,   new GUIContent("↳ Intensity"), profBackLightSettings); EditorGUI.indentLevel--; }
                                            if ((blMode & LightingMode.LightProbe) != 0)  { DrawProperty(editor, blProbeBlend, new GUIContent("Probe Color Blend"), profBackLightSettings);        EditorGUI.indentLevel++; DrawProperty(editor, blProbeIntensity,new GUIContent("↳ Intensity"), profBackLightSettings); EditorGUI.indentLevel--; }
                                            if ((blMode & LightingMode.ShadowLight) != 0) { DrawProperty(editor, blShadowBlend,new GUIContent("ShadowLight Color Blend"), profBackLightSettings);  EditorGUI.indentLevel++; DrawProperty(editor, blShadowIntensity,new GUIContent("↳ Intensity"), profBackLightSettings); EditorGUI.indentLevel--; }
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                }
                                
                                DrawInnerHeader("Noise Setting", ColorEffect);
                                if (blEnableNoise != null) {
                                    DrawProperty(editor, blEnableNoise, new GUIContent("Enable Noise Mask"), profBackLightSettings);
                                    if (blEnableNoise.floatValue > 0.5f) {
                                        if (blNoiseIntensity != null) { EditorGUI.indentLevel++; DrawProperty(editor, blNoiseIntensity, new GUIContent("Noise Intensity"), profBackLightSettings); EditorGUI.indentLevel--; }
                                    }
                                }
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox();
                    }
                }
                EndCategoryBox(isEffectOpen);
    
                bool isLineOpen = BeginCategoryBox(ref showLineCategory, "❖ LINE", ColorLine, lineCatProps, editor, new string[] { "Inline Settings", "Outline Settings" }, pendingComponentSync);
                if (isLineOpen)
                {
                    if (enableInline != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showInlineSettings, ref pShowInlineSettings, "Inline Settings", ColorLine, inlineProps, editor, ref profInlineSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableInline, new GUIContent("Enable Inline"), profInlineSettings);
                            if (enableInline.floatValue > 0.5f)
                            {
                                DrawInnerHeader("Lighting Mode", ColorLine);
                                DrawLightingModeField(inlineLightMode, enableLightProbe, profInlineSettings, pendingComponentSync);
                                LightingMode ilMode = inlineLightMode != null ? (LightingMode)(int)inlineLightMode.floatValue : LightingMode.Disabled;
    
                                if ((ilMode & LightingMode.ShadowLight) != 0 && ilSLIgnoreBehind != null) {
                                    EditorGUI.indentLevel++;
                                    DrawProperty(editor, ilSLIgnoreBehind, new GUIContent("Ignore Behind Fade (Shadow Light)", "勾選後，當 Shadow Light 位於角色背後時將忽略漸隱效果"), profInlineSettings);
                                    EditorGUI.indentLevel--;
                                }
    
                                DrawInnerHeader("Base Settings", ColorLine);
                                if (inlineColor != null) DrawProperty(editor, inlineColor, new GUIContent("Inline Color (HDR)"), profInlineSettings);
                                if (inlineWidth != null) DrawProperty(editor, inlineWidth, new GUIContent("Inline Width"), profInlineSettings);
                                if (inlineFadeSteps != null) DrawProperty(editor, inlineFadeSteps, new GUIContent("Fade Steps"), profInlineSettings);
                                if (inlineZTest != null) DrawZTestField(inlineZTest, "Inline ZTest", TIP_ZTEST, profInlineSettings);
    
                                if (ilMode != LightingMode.Disabled) {
                                    if (enableDirIL != null) {
                                        DrawInnerHeader("Directional Mask", ColorLine);
                                        DrawProperty(editor, enableDirIL, new GUIContent("Mask By Light Direction", TIP_MASK), profInlineSettings);
                                        if (enableDirIL.floatValue > 0.5f) {
                                            EditorGUI.indentLevel++;
                                            if ((ilMode & LightingMode.Realtime)    != 0) DrawProperty(editor, ilRtPower,    new GUIContent("Realtime Focus Power"), profInlineSettings);
                                            if ((ilMode & LightingMode.LightProbe)  != 0) DrawProperty(editor, ilProbePower, new GUIContent("Probe Focus Power"), profInlineSettings);
                                            if ((ilMode & LightingMode.ShadowLight) != 0) DrawProperty(editor, ilShadowPower,new GUIContent("ShadowLight Focus Power"), profInlineSettings);
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                    DrawInnerHeader("Light Color Influence", ColorLine);
                                    if (ilTintByLight != null) {
                                        DrawProperty(editor, ilTintByLight, new GUIContent("Tint By Light Color", TIP_TINT), profInlineSettings);
                                        if (ilTintByLight.floatValue > 0.5f) {
                                            EditorGUI.indentLevel++;
                                            if ((ilMode & LightingMode.Realtime) != 0)    { DrawProperty(editor, ilRtBlend,    new GUIContent("Realtime Color Blend"), profInlineSettings);   EditorGUI.indentLevel++; DrawProperty(editor, ilRtIntensity,   new GUIContent("↳ Intensity"), profInlineSettings); EditorGUI.indentLevel--; }
                                            if ((ilMode & LightingMode.LightProbe) != 0)  { DrawProperty(editor, ilProbeBlend, new GUIContent("Probe Color Blend"), profInlineSettings);        EditorGUI.indentLevel++; DrawProperty(editor, ilProbeIntensity,new GUIContent("↳ Intensity"), profInlineSettings); EditorGUI.indentLevel--; }
                                            if ((ilMode & LightingMode.ShadowLight) != 0) { DrawProperty(editor, ilShadowBlend,new GUIContent("ShadowLight Color Blend"), profInlineSettings); EditorGUI.indentLevel++; DrawProperty(editor, ilShadowIntensity,new GUIContent("↳ Intensity"), profInlineSettings); EditorGUI.indentLevel--; }
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                }
                                
                                DrawInnerHeader("Noise Setting", ColorLine);
                                if (ilEnableNoise != null) {
                                    DrawProperty(editor, ilEnableNoise, new GUIContent("Enable Noise Mask"), profInlineSettings);
                                    if (ilEnableNoise.floatValue > 0.5f) {
                                        if (ilNoiseIntensity != null) { EditorGUI.indentLevel++; DrawProperty(editor, ilNoiseIntensity, new GUIContent("Noise Intensity"), profInlineSettings); EditorGUI.indentLevel--; }
                                    }
                                }
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox(); GUILayout.Space(3);
                    }
    
                    if (enableOutline != null)
                    {
                        BeginHelpBox();
                        if (DrawSubHeader(ref showOutlineSettings, ref pShowOutlineSettings, "Outline Settings", ColorLine, outlineProps, editor, ref profOutlineSettings, pendingComponentSync))
                        {
                            GUILayout.Space(4); EditorGUI.indentLevel++;
                            DrawProperty(editor, enableOutline, new GUIContent("Enable Outline"), profOutlineSettings);
                            if (enableOutline.floatValue > 0.5f)
                            {
                                DrawInnerHeader("Lighting Mode", ColorLine);
                                DrawLightingModeField(outlineLightMode, enableLightProbe, profOutlineSettings, pendingComponentSync);
                                LightingMode olMode = outlineLightMode != null ? (LightingMode)(int)outlineLightMode.floatValue : LightingMode.Disabled;
    
                                if ((olMode & LightingMode.ShadowLight) != 0 && olSLIgnoreBehind != null) {
                                    EditorGUI.indentLevel++;
                                    DrawProperty(editor, olSLIgnoreBehind, new GUIContent("Ignore Behind Fade (Shadow Light)", "勾選後，當 Shadow Light 位於角色背後時將忽略漸隱效果"), profOutlineSettings);
                                    EditorGUI.indentLevel--;
                                }
    
                                DrawInnerHeader("Base Settings", ColorLine);
                                if (outlineColor      != null) DrawProperty(editor, outlineColor,      new GUIContent("Outline Color (HDR)"), profOutlineSettings);
                                if (outlineWidth      != null) DrawProperty(editor, outlineWidth,      new GUIContent("Outline Width"), profOutlineSettings);
                                if (multiplyEdgeColor != null) DrawProperty(editor, multiplyEdgeColor,new GUIContent("Multiply Edge Color"), profOutlineSettings);
                                if (outlineSmoothness != null) DrawProperty(editor, outlineSmoothness,new GUIContent("Outline Smoothness"), profOutlineSettings);
                                if (outlineAlphaCutoff!= null) DrawProperty(editor, outlineAlphaCutoff,new GUIContent("Alpha Cutoff"), profOutlineSettings);
                                if (outlineZTest      != null) DrawZTestField(outlineZTest, "Outline ZTest", TIP_ZTEST, profOutlineSettings);
    
                                if (olMode != LightingMode.Disabled) {
                                    if (enableDirOL != null) {
                                        DrawInnerHeader("Directional Mask", ColorLine);
                                        DrawProperty(editor, enableDirOL, new GUIContent("Mask By Light Direction", TIP_MASK), profOutlineSettings);
                                        if (enableDirOL.floatValue > 0.5f) {
                                            EditorGUI.indentLevel++;
                                            if ((olMode & LightingMode.Realtime)    != 0) DrawProperty(editor, olRtPower,    new GUIContent("Realtime Focus Power"), profOutlineSettings);
                                            if ((olMode & LightingMode.LightProbe)  != 0) DrawProperty(editor, olProbePower, new GUIContent("Probe Focus Power"), profOutlineSettings);
                                            if ((olMode & LightingMode.ShadowLight) != 0) DrawProperty(editor, olShadowPower,new GUIContent("ShadowLight Focus Power"), profOutlineSettings);
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                    DrawInnerHeader("Light Color Influence", ColorLine);
                                    if (olTintByLight != null) {
                                        DrawProperty(editor, olTintByLight, new GUIContent("Tint By Light Color", TIP_TINT), profOutlineSettings);
                                        if (olTintByLight.floatValue > 0.5f) {
                                            EditorGUI.indentLevel++;
                                            if ((olMode & LightingMode.Realtime) != 0)    { DrawProperty(editor, olRtBlend,    new GUIContent("Realtime Color Blend"), profOutlineSettings);   EditorGUI.indentLevel++; DrawProperty(editor, olRtIntensity,   new GUIContent("↳ Intensity"), profOutlineSettings); EditorGUI.indentLevel--; }
                                            if ((olMode & LightingMode.LightProbe) != 0)  { DrawProperty(editor, olProbeBlend, new GUIContent("Probe Color Blend"), profOutlineSettings);        EditorGUI.indentLevel++; DrawProperty(editor, olProbeIntensity,new GUIContent("↳ Intensity"), profOutlineSettings); EditorGUI.indentLevel--; }
                                            if ((olMode & LightingMode.ShadowLight) != 0) { DrawProperty(editor, olShadowBlend,new GUIContent("ShadowLight Color Blend"), profOutlineSettings); EditorGUI.indentLevel++; DrawProperty(editor, olShadowIntensity,new GUIContent("↳ Intensity"), profOutlineSettings); EditorGUI.indentLevel--; }
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                }
                                
                                DrawInnerHeader("Noise Setting", ColorLine);
                                if (olEnableNoise != null) {
                                    DrawProperty(editor, olEnableNoise, new GUIContent("Enable Noise Mask"), profOutlineSettings);
                                    if (olEnableNoise.floatValue > 0.5f) {
                                        if (olNoiseIntensity != null) { EditorGUI.indentLevel++; DrawProperty(editor, olNoiseIntensity, new GUIContent("Noise Intensity"), profOutlineSettings); EditorGUI.indentLevel--; }
                                    }
                                }
    
                                GUILayout.Space(5);
                                if (DrawAdvancedFoldout(ref showAdvancedOutline, "Advanced Outline Options")) {
                                    EditorGUI.indentLevel++;
                                    if (useScreenSpace != null) DrawProperty(editor, useScreenSpace, new GUIContent("Screen Space Width"), profOutlineSettings);
                                    if (fillInside     != null) DrawProperty(editor, fillInside,     new GUIContent("Fill Inside"), profOutlineSettings);
                                    if (use8N          != null) DrawProperty(editor, use8N,          new GUIContent("Sample 8 Neighbours"), profOutlineSettings);
                                    if (refTexWidth    != null) DrawProperty(editor, refTexWidth,    new GUIContent("Reference Texture Width"), profOutlineSettings);
                                    if (opaqueAlpha    != null) DrawProperty(editor, opaqueAlpha,    new GUIContent("Opaque Alpha Threshold"), profOutlineSettings);
                                    if (mipLevel       != null) DrawProperty(editor, mipLevel,       new GUIContent("Mip Level"), profOutlineSettings);
                                    EditorGUI.indentLevel--;
                                }
                            }
                            EditorGUI.indentLevel--; GUILayout.Space(4);
                        }
                        EndHelpBox();
                    }
                }
                EndCategoryBox(isLineOpen);
    
                if (EditorGUI.EndChangeCheck())
                {
                    bool blNoise = IsFeatureEnabled(blEnableNoise, profBackLightSettings, "_BL_EnableNoise");
                    bool ilNoise = IsFeatureEnabled(ilEnableNoise, profInlineSettings, "_IL_EnableNoise");
                    bool olNoise = IsFeatureEnabled(olEnableNoise, profOutlineSettings, "_OL_EnableNoise");
                    bool anyNoiseUsed = blNoise || ilNoise || olNoise;
    
                    if (enableGlobalNoise != null)
                    {
                        bool curGlobal = IsFeatureEnabled(enableGlobalNoise, profGlobalNoise, "_EnableGlobalNoise");
                        if (anyNoiseUsed && !curGlobal) {
                            ApplyPropertyChange(enableGlobalNoise, 1f);
                            if (profGlobalNoise != null) {
                                var p = profGlobalNoise.properties?.Find(x => x.name == "_EnableGlobalNoise");
                                if (p != null) p.floatValue = 1f;
                                EditorUtility.SetDirty(profGlobalNoise);
                            }
                            Debug.Log("[SpineShaderGUI] 自動開啟 Global Noise，因為已有模塊啟用 Noise Mask。");
                        }
                        else if (!anyNoiseUsed && curGlobal) {
                            ApplyPropertyChange(enableGlobalNoise, 0f);
                            if (profGlobalNoise != null) {
                                var p = profGlobalNoise.properties?.Find(x => x.name == "_EnableGlobalNoise");
                                if (p != null) p.floatValue = 0f;
                                EditorUtility.SetDirty(profGlobalNoise);
                            }
                            Debug.Log("[SpineShaderGUI] 自動關閉 Global Noise 以節省效能，因為沒有任何模塊啟用 Noise Mask。");
                        }
                    }
    
                    foreach (var obj in editor.targets) EditorUtility.SetDirty(obj);
    
                    pendingComponentSync?.Invoke();
                }
            }
            
            private void DrawTopButtons(MaterialEditor editor)
            {
                GUILayout.Space(5);
                
                // ==========================================
                // === 第一排：顯示模式 與 全效預覽 ============
                // ==========================================
                GUILayout.BeginHorizontal();
                
                // --- 1. 顯示模式標籤 (動態延展寬度) ---
                Rect r1 = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
                Event e1 = Event.current; 
                bool hover1 = r1.Contains(e1.mousePosition);
                Color accent1 = showRawPropertyNames ? new Color(1f, 0.7f, 0.4f) : new Color(0.6f, 0.6f, 0.6f);
            
                EditorGUI.DrawRect(r1, EditorGUIUtility.isProSkin
                    ? (hover1 ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                    : (hover1 ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(r1.x, r1.yMax-1f, r1.width, 1f),
                    EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
                EditorGUI.DrawRect(new Rect(r1.x, r1.y, 4f, r1.height), accent1);
            
                if (e1.type == EventType.MouseDown && e1.button == 0 && hover1) { showRawPropertyNames = !showRawPropertyNames; e1.Use(); }
            
                var ts1 = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
                ts1.normal.textColor = showRawPropertyNames 
                    ? (EditorGUIUtility.isProSkin ? accent1 : Color.Lerp(accent1, Color.black, 0.4f)) 
                    : (EditorGUIUtility.isProSkin ? new Color(.8f,.8f,.8f) : new Color(.3f,.3f,.3f));
                
                string title = showRawPropertyNames ? "▤ 顯示模式：Shader 變數名稱" : "▤ 顯示模式：美術易讀名稱";
                EditorGUI.LabelField(new Rect(r1.x+12f, r1.y, r1.width-60f, r1.height), title, ts1);
            
                var is1 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight, fontSize = 11 };
                is1.normal.textColor = ts1.normal.textColor;
                EditorGUI.LabelField(new Rect(r1.xMax-60f, r1.y, 50f, r1.height), showRawPropertyNames ? "[ON]" : "[OFF]", is1);
            
                GUILayout.Space(2); // 兩塊標籤之間的水平間距
            
                // --- 2. 全效預覽標籤 (固定寬度 90px，風格同其他工具按鈕) ---
                Rect rAll = GUILayoutUtility.GetRect(90f, 26f, GUILayout.ExpandWidth(false));
                Event eAll = Event.current;
                bool hoverAll = rAll.Contains(eAll.mousePosition);
                Color accentAll = new Color(1.0f, 0.35f, 0.35f); // 顯眼的亮紅色，代表具有全局覆蓋性的行為
                
                EditorGUI.DrawRect(rAll, EditorGUIUtility.isProSkin 
                    ? (hoverAll ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f)) 
                    : (hoverAll ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(rAll.x, rAll.yMax-1f, rAll.width, 1f), 
                    EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
                EditorGUI.DrawRect(new Rect(rAll.x, rAll.y, 4f, rAll.height), accentAll);
            
                if (eAll.type == EventType.MouseDown && eAll.button == 0 && hoverAll)
                {
                    Undo.RecordObjects(editor.targets, "Enable All Shader Keywords");
                    foreach (Material mat in editor.targets)
                    {
                        if (mat != null)
                        {
                            foreach (string kw in SpineShaderBuildPipeline.AllModuleKeywords)
                            {
                                mat.EnableKeyword(kw);
                            }
                            EditorUtility.SetDirty(mat);
                        }
                    }
                    Debug.Log("[SpineShaderGUI] 已切換至全效預覽模式 (Shader Keywords 已全部開啟)。");
                    eAll.Use();
                }
            
                var tsAll = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
                tsAll.normal.textColor = EditorGUIUtility.isProSkin ? accentAll : Color.Lerp(accentAll, Color.black, 0.4f);
                
                // 定義按鈕的顯示文字與詳細 Tooltip 提示
                GUIContent previewBtnContent = new GUIContent(
                    "▤ 全效預覽", 
                    "【本地預覽專用】\n強制開啟'當前選擇的材質'的所有 Shader 變體以供完整預覽。\n\n⚠️ 注意：\n1. 此操作「不會」改變模塊的打勾狀態，該勾還是要勾。\n2. 此操作「不會」影響最終打包 (Build) 效能，最終包體仍由『變體管理』嚴格控管。"
                );
                
                // 畫出帶有 Tooltip 的文字
                GUI.Label(new Rect(rAll.x, rAll.y, rAll.width, rAll.height), previewBtnContent, tsAll);
            
                GUILayout.EndHorizontal();
                
                GUILayout.Space(2); // 上下兩排之間的垂直間距
                
                // ==========================================
                // === 第二排：子工具列 (美術、程式、除錯等) =======
                // ==========================================
                GUILayout.BeginHorizontal();
            
                Rect r2 = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
                Event e2 = Event.current; bool hover2 = r2.Contains(e2.mousePosition);
                Color accent2 = new Color(0.9f, 0.4f, 0.4f); 
            
                EditorGUI.DrawRect(r2, EditorGUIUtility.isProSkin ? (hover2 ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f)) : (hover2 ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(r2.x, r2.yMax-1f, r2.width, 1f), EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
                EditorGUI.DrawRect(new Rect(r2.x, r2.y, 4f, r2.height), accent2);
                if (e2.type == EventType.MouseDown && e2.button == 0 && hover2) { SpineArtHelpWindow.ShowWindow(); e2.Use(); }
                var ts2 = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
                ts2.normal.textColor = EditorGUIUtility.isProSkin ? accent2 : Color.Lerp(accent2, Color.black, 0.4f);
                EditorGUI.LabelField(new Rect(r2.x+12f, r2.y, r2.width-12f, r2.height), "▤ 美術", ts2);
            
                Rect r3 = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
                Event e3 = Event.current; bool hover3 = r3.Contains(e3.mousePosition);
                Color accent3 = new Color(0.3f, 0.6f, 0.9f); 
            
                EditorGUI.DrawRect(r3, EditorGUIUtility.isProSkin ? (hover3 ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f)) : (hover3 ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(r3.x, r3.yMax-1f, r3.width, 1f), EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
                EditorGUI.DrawRect(new Rect(r3.x, r3.y, 4f, r3.height), accent3);
                if (e3.type == EventType.MouseDown && e3.button == 0 && hover3) { SpineProfileHelpWindow.ShowWindow(); e3.Use(); }
                var ts3 = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
                ts3.normal.textColor = EditorGUIUtility.isProSkin ? accent3 : Color.Lerp(accent3, Color.black, 0.4f);
                EditorGUI.LabelField(new Rect(r3.x+12f, r3.y, r3.width-12f, r3.height), "▤ 程式", ts3);
            
                Rect r4 = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
                Event e4 = Event.current; bool hover4 = r4.Contains(e4.mousePosition);
                Color accent4 = new Color(0.9f, 0.7f, 0.2f); 
            
                EditorGUI.DrawRect(r4, EditorGUIUtility.isProSkin ? (hover4 ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f)) : (hover4 ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(r4.x, r4.yMax-1f, r4.width, 1f), EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
                EditorGUI.DrawRect(new Rect(r4.x, r4.y, 4f, r4.height), accent4);
                if (e4.type == EventType.MouseDown && e4.button == 0 && hover4) { SpineDebugHelpWindow.ShowWindow(); e4.Use(); }
                var ts4 = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
                ts4.normal.textColor = EditorGUIUtility.isProSkin ? accent4 : Color.Lerp(accent4, Color.black, 0.4f);
                EditorGUI.LabelField(new Rect(r4.x+12f, r4.y, r4.width-12f, r4.height), "▤ 除錯", ts4);
                
                Rect r5 = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
                Event e5 = Event.current; bool hover5 = r5.Contains(e5.mousePosition);
                Color accent5 = new Color(0.2f, 0.8f, 0.6f); 
            
                EditorGUI.DrawRect(r5, EditorGUIUtility.isProSkin ? (hover5 ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f)) : (hover5 ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(r5.x, r5.yMax-1f, r5.width, 1f), EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
                EditorGUI.DrawRect(new Rect(r5.x, r5.y, 4f, r5.height), accent5);
                if (e5.type == EventType.MouseDown && e5.button == 0 && hover5) { SpineNamingRuleWindow.ShowWindow(); e5.Use(); }
                var ts5 = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
                ts5.normal.textColor = EditorGUIUtility.isProSkin ? accent5 : Color.Lerp(accent5, Color.black, 0.4f);
                EditorGUI.LabelField(new Rect(r5.x+12f, r5.y, r5.width-12f, r5.height), "▤ 命名", ts5);
            
                Rect r6 = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
                Event e6 = Event.current; bool hover6 = r6.Contains(e6.mousePosition);
                Color accent6 = new Color(0.7f, 0.4f, 0.9f); 
            
                EditorGUI.DrawRect(r6, EditorGUIUtility.isProSkin ? (hover6 ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f)) : (hover6 ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(r6.x, r6.yMax-1f, r6.width, 1f), EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
                EditorGUI.DrawRect(new Rect(r6.x, r6.y, 4f, r6.height), accent6);
                if (e6.type == EventType.MouseDown && e6.button == 0 && hover6) { 
                    System.Type t = System.Type.GetType("SpinePlatformManagerWindow, Assembly-CSharp-Editor");
                    if (t == null) t = System.Type.GetType("SpinePlatformManagerWindow, Assembly-CSharp");
                    if (t != null) t.GetMethod("ShowWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
                    else Debug.LogWarning("[SpineShaderGUI] 找不到 SpinePlatformManagerWindow，請確認腳本已正確編譯。");
                    e6.Use(); 
                }
                var ts6 = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
                ts6.normal.textColor = EditorGUIUtility.isProSkin ? accent6 : Color.Lerp(accent6, Color.black, 0.4f);
                EditorGUI.LabelField(new Rect(r6.x+12f, r6.y, r6.width-12f, r6.height), "▤ 變體管理", ts6);
            
                GUILayout.EndHorizontal();
            }
    
            // =========================================================
            // 預掛腳本 UI 繪製與狀態控制
            // =========================================================
            private void DrawScriptAttachmentSection(MaterialEditor editor, bool reqAlphaMask, bool reqHitSweep, bool reqLightReceiver, bool reqBloomSync)
            {
                Material targetMat = editor.target as Material;
                bool hasAlphaMask = false, hasHitSweep = false, hasLightReceiver = false, hasBloomSync = false;
                bool hasValidRenderer = false;
    
                GameObject[] selectedGOs = Selection.gameObjects;
                List<GameObject> targetGOs = new List<GameObject>();
    
                foreach (var go in selectedGOs) {
                    Renderer rend = go.GetComponent<Renderer>();
                    if (rend != null) {
                        bool usesMat = false;
                        foreach(var mat in rend.sharedMaterials) {
                            if (mat == targetMat) { usesMat = true; break; }
                        }
                        if (usesMat) {
                            hasValidRenderer = true;
                            targetGOs.Add(go);
                            if (go.GetComponent(System.Type.GetType("VFXTool.SpineSkeletonShaderTool.SpineAlphaMaskRenderer, Assembly-CSharp")) != null) hasAlphaMask = true;
                            if (go.GetComponent(System.Type.GetType("VFXTool.SpineSkeletonShaderTool.SpineHitSweepEffect, Assembly-CSharp")) != null) hasHitSweep = true;
                            if (go.GetComponent(System.Type.GetType("VFXTool.SpineSkeletonShaderTool.SpineLightReceiver, Assembly-CSharp")) != null) hasLightReceiver = true;
                            if (go.GetComponent(System.Type.GetType("VFXTool.SpineSkeletonShaderTool.SpineBloomThresholdSync, Assembly-CSharp")) != null) hasBloomSync = true;
                        }
                    }
                }
    
                GUILayout.Space(10);
                Rect headerRect = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
                Event e = Event.current; bool hover = headerRect.Contains(e.mousePosition);
                
                if (e.type == EventType.MouseDown && e.button == 0 && hover) { showScriptSection = !showScriptSection; e.Use(); }
    
                Color accent = new Color(0.65f, 0.65f, 0.65f);
                Color bg = EditorGUIUtility.isProSkin
                    ? (hover ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                    : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f));
                EditorGUI.DrawRect(headerRect, bg);
                EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, headerRect.width, 3f), accent);
    
                var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
                ts.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                EditorGUI.LabelField(new Rect(headerRect.x+12f, headerRect.y+3f, headerRect.width-30f, headerRect.height-3f), "❖ 預掛腳本 (Auto Components)", ts);
                
                var is2 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
                is2.normal.textColor = ts.normal.textColor;
                EditorGUI.LabelField(new Rect(headerRect.xMax-24f, headerRect.y+4f, 20f, headerRect.height-3f), showScriptSection ? "−" : "＋", is2);
    
                if (showScriptSection)
                {
                    EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
                    GUILayout.Space(5);
    
                    if (!hasValidRenderer) {
                        DrawModernInfoBox("請先在 Hierarchy 點選「掛載有此材質球」的角色物件，即可實時管理其附屬的腳本。", new Color(0.3f, 0.6f, 0.9f));
                    } else {
                        if (!reqAlphaMask) warnAlphaMask = false;
                        if (!reqHitSweep) warnHitSweep = false;
                        if (!reqLightReceiver) warnLightReceiver = false;
                        if (!reqBloomSync) warnBloomSync = false;
    
                        warnAlphaMask = DrawScriptToggle("SpineAlphaMaskRenderer", "Inline (SpineAlphaMaskRenderer)", hasAlphaMask, reqAlphaMask, warnAlphaMask, targetGOs, accent);
                        warnHitSweep = DrawScriptToggle("SpineHitSweepEffect", "Hit Sweep (SpineHitSweepEffect)", hasHitSweep, reqHitSweep, warnHitSweep, targetGOs, accent);
                        warnLightReceiver = DrawScriptToggle("SpineLightReceiver", "Normal Map, BackLight, Inline, Outline, Light Probe, Shadow Light (SpineLightReceiver)", hasLightReceiver, reqLightReceiver, warnLightReceiver, targetGOs, accent);
                        warnBloomSync = DrawScriptToggle("SpineBloomThresholdSync", "Suppress Bloom (SpineBloomThresholdSync)", hasBloomSync, reqBloomSync, warnBloomSync, targetGOs, accent);
                    }
    
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                }
            }
    
            private bool DrawScriptToggle(string scriptName, string displayName, bool isAttached, bool isRequired, bool showWarning, List<GameObject> targetGOs, Color accentColor)
            {
                Rect r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(26));
                Event e = Event.current;
                bool hover = r.Contains(e.mousePosition);
    
                if (e.type == EventType.MouseDown && e.button == 0 && hover) {
                    if (isAttached) {
                        if (isRequired) { showWarning = true; }
                        else {
                            showWarning = false;
                            ToggleScriptOnGameObjects(scriptName, false, targetGOs);
                            isAttached = false;
                        }
                    } else {
                        showWarning = false;
                        ToggleScriptOnGameObjects(scriptName, true, targetGOs);
                        isAttached = true;
                    }
                    e.Use();
                    GUI.changed = true;
                }
    
                if (e.type == EventType.Repaint) {
                    Color bgColor = isAttached 
                        ? (EditorGUIUtility.isProSkin ? new Color(accentColor.r, accentColor.g, accentColor.b, 0.25f) : new Color(accentColor.r, accentColor.g, accentColor.b, 0.15f))
                        : (EditorGUIUtility.isProSkin ? new Color(0, 0, 0, 0.2f) : new Color(1, 1, 1, 0.4f));
                    
                    if (hover) bgColor.a += 0.1f;
                    EditorGUI.DrawRect(r, bgColor);
                    EditorGUI.DrawRect(new Rect(r.x, r.yMax - 2f, r.width, 2f), isAttached ? accentColor : new Color(0.2f, 0.2f, 0.2f));
                    
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11, alignment = TextAnchor.MiddleLeft };
                    labelStyle.normal.textColor = isAttached ? (EditorGUIUtility.isProSkin ? Color.white : new Color(0.1f, 0.1f, 0.1f)) : new Color(0.5f, 0.5f, 0.5f);
                    string prefix = isAttached ? "■ " : "□ ";
                    GUI.Label(new Rect(r.x + 12, r.y, r.width - 12, r.height - 2), prefix + displayName, labelStyle);
                }
    
                if (showWarning) {
                    DrawModernInfoBox($"無法移除 {scriptName}，因為對應的 Material 特效模塊尚未關閉。", new Color(0.9f, 0.6f, 0.2f));
                }
                return showWarning;
            }
    
            private void ToggleScriptOnGameObjects(string scriptName, bool add, List<GameObject> gos)
            {
                System.Type st = System.Type.GetType($"VFXTool.SpineSkeletonShaderTool.{scriptName}, Assembly-CSharp");
                if (st == null) return;
                foreach(var go in gos) {
                    Component comp = go.GetComponent(st);
                    if (add && comp == null) Undo.AddComponent(go, st);
                    else if (!add && comp != null) Undo.DestroyObjectImmediate(comp);
                }
            }
    
            private void DrawModernInfoBox(string message, Color accentColor)
            {
                GUILayout.Space(4);
                GUIStyle style = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 12, padding = new RectOffset(14, 8, 8, 8) };
                style.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.2f, 0.2f, 0.2f);
                Rect r = GUILayoutUtility.GetRect(new GUIContent(message), style);
                if (Event.current.type == EventType.Repaint) {
                    Color bgColor = EditorGUIUtility.isProSkin ? new Color(0.14f, 0.14f, 0.14f) : new Color(0.92f, 0.92f, 0.92f);
                    EditorGUI.DrawRect(r, bgColor);
                    EditorGUI.DrawRect(new Rect(r.x, r.y, 4f, r.height), accentColor);
                    GUI.Label(r, message, style);
                }
                GUILayout.Space(4);
            }
    
            // =========================================================
            // 場景與選取檢查自動同步
            // =========================================================
            [InitializeOnLoadMethod]
            private static void RegisterSceneLoadValidation()
            {
                UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (scene, mode) => { ValidateAllSceneComponents(); };
                Selection.selectionChanged += OnSelectionChanged;
            }
    
            private static void OnSelectionChanged()
            {
                if (Selection.gameObjects == null) return;
                foreach (GameObject go in Selection.gameObjects) { if (go != null) ValidateGameObjectScripts(go); }
            }
    
            public static void ValidateAllSceneComponents()
            {
    #if UNITY_2023_1_OR_NEWER
                Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    #else
                Renderer[] allRenderers = Object.FindObjectsOfType<Renderer>();
    #endif
                foreach (Renderer r in allRenderers) { if (r != null && r.gameObject != null) ValidateGameObjectScripts(r.gameObject); }
            }
    
            private static void ValidateGameObjectScripts(GameObject go)
            {
                Renderer rend = go.GetComponent<Renderer>();
                if (rend == null) return;
            
                int layer = go.layer;
                bool isValidLayer = (layer == 8 || layer == 9 || layer == 12 || layer == 13 || layer == 18 || layer == 20);
                bool hasSpineCustomMaterial = false;
                Material activeMat = null;
            
                // ─────────────────────────────────────────────────────────────
                //  ✅ Ferr2D 豁免判定
                //  Ferr2D 的 Pixel Lit 系列 shader 自行整合了 SpineShadowLightCommon，
                //  其 SpineLightReceiver 由 Ferr2DT_TerrainShaderGUI 獨立管理，
                //  不應被 SpineCustomShaderGUI 的驗證流程清除。
                // ─────────────────────────────────────────────────────────────
                bool isFerr2DTerrain = false;
                foreach (Material m in rend.sharedMaterials)
                {
                    if (m == null || m.shader == null) continue;
                    if (m.shader.name.Contains("Ferr/2D Terrain/Pixel Lit"))
                    {
                        isFerr2DTerrain = true;
                        break;
                    }
                }
                // 若為 Ferr2D 地形物件，直接略過，交由 Ferr2DT_TerrainShaderGUI 管理
                if (isFerr2DTerrain) return;
            
                foreach (Material m in rend.sharedMaterials) {
                    if (m != null && m.HasProperty("_MaterialResetCheck")) {
                        hasSpineCustomMaterial = true; activeMat = m; break;
                    }
                }
            
                if (!isValidLayer || !hasSpineCustomMaterial) {
                    RemoveScriptIfExist(go, "SpineAlphaMaskRenderer");
                    RemoveScriptIfExist(go, "SpineHitSweepEffect");
                    RemoveScriptIfExist(go, "SpineLightReceiver");
                    RemoveScriptIfExist(go, "SpineBloomThresholdSync");
                    return;
                }
            
                SpineProfileAnimator anim = go.GetComponent<SpineProfileAnimator>();
                bool IsFeatureEnabledLocally(string propName) {
                    if (anim != null && anim.profiles != null) {
                        foreach (var p in anim.profiles) {
                            if (p == null || p.properties == null) continue;
                            var prop = p.properties.Find(x => x.name == propName);
                            if (prop != null) return prop.floatValue > 0.5f;
                        }
                    }
                    return activeMat.HasProperty(propName) && activeMat.GetFloat(propName) > 0.5f;
                }
                int GetIntFeatureLocally(string propName) {
                    if (anim != null && anim.profiles != null) {
                        foreach (var p in anim.profiles) {
                            if (p == null || p.properties == null) continue;
                            var prop = p.properties.Find(x => x.name == propName);
                            if (prop != null) return Mathf.RoundToInt(prop.floatValue);
                        }
                    }
                    return activeMat.HasProperty(propName) ? Mathf.RoundToInt(activeMat.GetFloat(propName)) : 0;
                }
            
                bool needInlineMask = IsFeatureEnabledLocally("_EnableInline");
                bool needHitSweep   = IsFeatureEnabledLocally("_EnableHitSweep");
                bool needBloomSync  = IsFeatureEnabledLocally("_EnableBloomSuppression");
                int blMode = GetIntFeatureLocally("_BackLightLightingMode");
                int ilMode = GetIntFeatureLocally("_InlineLightingMode");
                int olMode = GetIntFeatureLocally("_OutlineLightingMode");
            
                bool needLightData = IsFeatureEnabledLocally("_EnableNormalMap") || (IsFeatureEnabledLocally("_EnableBackLight") && blMode != 0) || 
                                     (IsFeatureEnabledLocally("_EnableInline") && ilMode != 0) || (IsFeatureEnabledLocally("_EnableOutline") && olMode != 0) || 
                                     IsFeatureEnabledLocally("_EnableLightProbe") || IsFeatureEnabledLocally("_EnableShadowLight");
            
                StaticSyncAutoComponent(go, "SpineAlphaMaskRenderer", needInlineMask);
                StaticSyncAutoComponent(go, "SpineHitSweepEffect", needHitSweep);
                StaticSyncAutoComponent(go, "SpineLightReceiver", needLightData);
                StaticSyncAutoComponent(go, "SpineBloomThresholdSync", needBloomSync);
                if (needLightData) EnsureSpineLightManager();
            }
    
            private static void RemoveScriptIfExist(GameObject go, string scriptName)
            {
                System.Type st = System.Type.GetType($"VFXTool.SpineSkeletonShaderTool.{scriptName}, Assembly-CSharp");
                if (st == null) return;
                Component comp = go.GetComponent(st);
                if (comp != null) { Undo.DestroyObjectImmediate(comp); }
            }
    
            private static void StaticSyncAutoComponent(GameObject go, string scriptName, bool isEnabled)
            {
                System.Type st = System.Type.GetType($"VFXTool.SpineSkeletonShaderTool.{scriptName}, Assembly-CSharp");
                if (st == null) return;
                Component comp = go.GetComponent(st);
                if (isEnabled && comp == null) { Undo.AddComponent(go, st); }
            }
    
            private void SyncAutoComponent(Object[] materials, string scriptName, bool isEnabled)
            {
                System.Type st = System.Type.GetType($"VFXTool.SpineSkeletonShaderTool.{scriptName}, Assembly-CSharp"); 
                if (st == null) return;
    #if UNITY_2023_1_OR_NEWER
                Renderer[] all = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    #else
                Renderer[] all = Object.FindObjectsOfType<Renderer>();
    #endif
                foreach (Renderer r in all) {
                    if (r == null || r.gameObject == null) continue;
                    int layer = r.gameObject.layer;
                    if (layer != 8 && layer != 9 && layer != 12 && layer != 13 && layer != 18 && layer != 20) continue;
                    bool uses = false;
                    foreach (Material m in r.sharedMaterials) {
                        if (m == null) continue;
                        foreach (Material tm in materials)
                            if (tm != null && (m == tm || m.name.Replace(" (Instance)","") == tm.name.Replace(" (Instance)",""))) { uses = true; break; }
                        if (uses) break;
                    }
                    if (!uses) continue;
                    GameObject go = r.gameObject;
                    Component ex = go.GetComponent(st);
                    if (isEnabled && ex == null) {
                        if (Application.isPlaying) go.AddComponent(st);
                        else Undo.AddComponent(go, st);
                    }
                }
            }
    
            // =========================================================
            // UI 工具與選單功能
            // =========================================================
            private void ShowCopyPasteMenu(string title, MaterialProperty[] props, MaterialEditor editor, string[] profileTitlesToClear, System.Action onSyncRequired)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent($"Copy [ {title.Replace("❖ ", "")} ] Settings"), false, () => CopyProps(props));
                if (_clipboard != null)
                    menu.AddItem(new GUIContent("Paste Settings"), false, () => PasteProps(props, editor));
                else
                    menu.AddDisabledItem(new GUIContent("Paste Settings"));
                    
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Reset Settings to Default"), false, () => {
                    Undo.RecordObjects(editor.targets, "Reset " + title);
                    ResetPropsToDefault(props, editor);
                    
                    // 【新增】重置後再次套用平台預設狀態
                    foreach (Material mat in editor.targets) {
                        ApplyCurrentPlatformSettings(mat);
                    }
                    
                    if (profileTitlesToClear != null && profileTitlesToClear.Length > 0) {
                        foreach (string pTitle in profileTitlesToClear) {
                            foreach (Material mat in editor.targets) SaveProfileToMaterial(mat, pTitle, null);
                        }
                    }
                    foreach (Material mat in editor.targets) { MaterialEditor.ApplyMaterialPropertyDrawers(mat); EditorUtility.SetDirty(mat); }
                    _initialized = false;
                    if (editor.target is Material targetMat) LoadProfilesFromMaterial(targetMat);
                    SyncAutoAnimator(editor); 
                    onSyncRequired?.Invoke();
                    editor.Repaint();
                    Debug.Log($"[SpineShaderGUI] 已將 {title} 模塊重置為預設值，並安全解除其 Preset 綁定。");
                });
                menu.ShowAsContext();
            }
    
            private void CopyProps(MaterialProperty[] props)
            {
                _clipboard = new ModuleClipboard();
                foreach (var p in props) {
                    if (p == null) continue;
                    if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range) _clipboard.floats[p.name] = p.floatValue;
                    if (p.type == MaterialProperty.PropType.Color) _clipboard.colors[p.name] = p.colorValue;
                    if (p.type == MaterialProperty.PropType.Vector) _clipboard.vectors[p.name] = p.vectorValue;
                    if (p.type == MaterialProperty.PropType.Texture) _clipboard.textures[p.name] = p.textureValue;
                }
                Debug.Log($"[SpineShaderGUI] 已複製 {props.Length} 個參數設定。");
            }
    
            private void PasteProps(MaterialProperty[] props, MaterialEditor editor)
            {
                if (_clipboard == null) return;
                Undo.RecordObjects(editor.targets, "Paste Material Settings");
                foreach (var p in props) {
                    if (p == null) continue;
                    if ((p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range) && _clipboard.floats.TryGetValue(p.name, out float f)) p.floatValue = f;
                    else if (p.type == MaterialProperty.PropType.Color && _clipboard.colors.TryGetValue(p.name, out Color c)) p.colorValue = c;
                    else if (p.type == MaterialProperty.PropType.Vector && _clipboard.vectors.TryGetValue(p.name, out Vector4 v)) p.vectorValue = v;
                    else if (p.type == MaterialProperty.PropType.Texture && _clipboard.textures.TryGetValue(p.name, out Texture t)) p.textureValue = t;
                }
                foreach (var obj in editor.targets) {
                    if (obj is Material mat) { MaterialEditor.ApplyMaterialPropertyDrawers(mat); EditorUtility.SetDirty(mat); }
                }
                Debug.Log("[SpineShaderGUI] 已成功貼上參數設定。");
            }
    
            private bool SaveProfile(string title, MaterialProperty[] props, ref SpineModuleProfile profile, int saveMask)
            {
                string cleanTitle = title.Replace("❖ ", "").Trim();
                string formattedModule = GetFormattedModuleName(title);
    
                if (profile == null) {
                    string defaultName = $"General_{formattedModule}_Default_Index";
                    string path = EditorUtility.SaveFilePanelInProject($"Save {cleanTitle} Profile", defaultName, "asset", "選擇儲存預設檔的位置");
                    if (string.IsNullOrEmpty(path)) return false; 
                    profile = ScriptableObject.CreateInstance<SpineModuleProfile>();
                    profile.moduleName = cleanTitle;
                    AssetDatabase.CreateAsset(profile, path);
                }
    
                var oldProps = profile.properties != null ? profile.properties.ToDictionary(p => p.name, p => p) : new Dictionary<string, SpineModuleProfile.ProfileProperty>();
                List<SpineModuleProfile.ProfileProperty> newProperties = new List<SpineModuleProfile.ProfileProperty>();
    
                for (int i = 0; i < props.Length; i++) {
                    if ((saveMask & (1 << i)) == 0) continue;
                    var p = props[i];
                    if (p == null) continue;
                    if (oldProps.TryGetValue(p.name, out var existingProp) && existingProp.useCurve) {
                        newProperties.Add(existingProp);
                        continue;
                    }
    
                    string dName = FriendlyNameMap.ContainsKey(p.name) ? FriendlyNameMap[p.name] : p.displayName;
                    var propData = new SpineModuleProfile.ProfileProperty { name = p.name, displayName = dName };
                    if (existingProp != null) {
                        propData.curveX = existingProp.curveX; propData.curveY = existingProp.curveY;
                        propData.curveZ = existingProp.curveZ; propData.curveW = existingProp.curveW;
                    }
                    if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range) { propData.type = SpineModuleProfile.ProfilePropType.Float; propData.floatValue = p.floatValue; }
                    else if (p.type == MaterialProperty.PropType.Color) { propData.type = SpineModuleProfile.ProfilePropType.Color; propData.colorValue = p.colorValue; }
                    else if (p.type == MaterialProperty.PropType.Vector) { propData.type = SpineModuleProfile.ProfilePropType.Vector; propData.vectorValue = p.vectorValue; }
                    else if (p.type == MaterialProperty.PropType.Texture) { propData.type = SpineModuleProfile.ProfilePropType.Texture; propData.textureValue = p.textureValue; }
                    newProperties.Add(propData);
                }
                profile.properties = newProperties;
                EditorUtility.SetDirty(profile);
                Debug.Log($"[SpineShaderGUI] 已成功儲存 {cleanTitle} Profile (包含 {newProperties.Count} 個選擇的參數)。");
                return true;
            }
    
            private void LoadProfile(SpineModuleProfile profile, MaterialProperty[] props, MaterialEditor editor)
            {
                if (profile == null) return;
                Undo.RecordObjects(editor.targets, $"Load {profile.moduleName} Profile Properties");
                var propDict = profile.properties.ToDictionary(x => x.name, x => x);
                foreach (var p in props) {
                    if (p == null) continue;
                    if (propDict.TryGetValue(p.name, out var data)) {
                        if ((p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range) && data.type == SpineModuleProfile.ProfilePropType.Float) p.floatValue = data.floatValue;
                        else if (p.type == MaterialProperty.PropType.Color && data.type == SpineModuleProfile.ProfilePropType.Color) p.colorValue = data.colorValue;
                        else if (p.type == MaterialProperty.PropType.Vector && data.type == SpineModuleProfile.ProfilePropType.Vector) p.vectorValue = data.vectorValue;
                        else if (p.type == MaterialProperty.PropType.Texture && data.type == SpineModuleProfile.ProfilePropType.Texture) p.textureValue = data.textureValue;
                    }
                }
                foreach (var obj in editor.targets) {
                    if (obj is Material mat) { MaterialEditor.ApplyMaterialPropertyDrawers(mat); EditorUtility.SetDirty(mat); }
                }
                Debug.Log($"[SpineShaderGUI] 已成功載入 {profile.moduleName} Profile 的數值至 Material。");
            }
    
            private void SyncAutoAnimator(MaterialEditor editor)
            {
                List<SpineModuleProfile> activeProfiles = new List<SpineModuleProfile>();
                if (profMainSettings != null) activeProfiles.Add(profMainSettings);
                if (profPostProcessing != null) activeProfiles.Add(profPostProcessing);
                if (profGlobalNoise != null) activeProfiles.Add(profGlobalNoise);
                if (profLightingSettings != null) activeProfiles.Add(profLightingSettings);
                if (profShadowLightSettings != null) activeProfiles.Add(profShadowLightSettings);
                if (profNormalMapSettings != null) activeProfiles.Add(profNormalMapSettings);
                if (profLightProbeSettings != null) activeProfiles.Add(profLightProbeSettings);
                if (profEmissionSettings != null) activeProfiles.Add(profEmissionSettings);
                if (profEffectOverlay != null) activeProfiles.Add(profEffectOverlay);
                if (profHitSweepSettings != null) activeProfiles.Add(profHitSweepSettings);
                if (profBackLightSettings != null) activeProfiles.Add(profBackLightSettings);
                if (profInlineSettings != null) activeProfiles.Add(profInlineSettings);
                if (profOutlineSettings != null) activeProfiles.Add(profOutlineSettings);
    
    #if UNITY_2023_1_OR_NEWER
                Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    #else
                Renderer[] allRenderers = Object.FindObjectsOfType<Renderer>();
    #endif
                foreach (Renderer r in allRenderers) {
                    if (r == null || r.gameObject == null) continue;
                    bool usesMat = false;
                    foreach (var t in editor.targets) {
                        if (r.sharedMaterials.Contains((Material)t)) { usesMat = true; break; }
                    }
                    if (!usesMat) continue;
                    SpineProfileAnimator anim = r.GetComponent<SpineProfileAnimator>();
                    if (activeProfiles.Count > 0) {
                        if (anim == null) anim = Undo.AddComponent<SpineProfileAnimator>(r.gameObject);
                        Undo.RecordObject(anim, "Update Profile Animator");
                        anim.profiles = new List<SpineModuleProfile>(activeProfiles);
                        anim.Restart();
                    } else {
                        if (anim != null) { anim.ClearAllMPB(); Undo.DestroyObjectImmediate(anim); }
                    }
                }
            }
    
            private bool BeginCategoryBox(ref bool foldout, string title, Color accent, MaterialProperty[] props, MaterialEditor editor, string[] profileTitles, System.Action onSyncRequired)
            {
                GUILayout.Space(12);
                Rect r = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
                Event e = Event.current; bool hover = r.Contains(e.mousePosition);
                
                if (e.type == EventType.ContextClick && hover) { ShowCopyPasteMenu(title, props, editor, profileTitles, onSyncRequired); e.Use(); }
                if (e.type == EventType.MouseDown && e.button == 0 && hover) { foldout = !foldout; e.Use(); }
    
                Color bg = EditorGUIUtility.isProSkin
                    ? (hover ? new Color(.22f,.22f,.22f) : new Color(.18f,.18f,.18f))
                    : (hover ? new Color(.85f,.85f,.85f) : new Color(.80f,.80f,.80f));
                EditorGUI.DrawRect(r, bg);
                EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), accent);
    
                var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
                ts.normal.textColor = foldout
                    ? (EditorGUIUtility.isProSkin ? Color.Lerp(accent, Color.white, .3f) : Color.Lerp(accent, Color.black, .4f))
                    : (EditorGUIUtility.isProSkin ? new Color(.5f,.5f,.5f) : new Color(.4f,.4f,.4f));
                EditorGUI.LabelField(new Rect(r.x+12f, r.y+3f, r.width-30f, r.height-3f), title, ts);
                var is2 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
                is2.normal.textColor = ts.normal.textColor;
                EditorGUI.LabelField(new Rect(r.xMax-24f, r.y+4f, 20f, r.height-3f), foldout ? "−" : "＋", is2);
    
                if (!foldout) return false;
                var box = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10,10,8,12), margin = new RectOffset(0,0,0,0) };
                EditorGUILayout.BeginVertical(box); 
                return true;
            }
            private void EndCategoryBox(bool foldout) { if (foldout) { EditorGUILayout.EndVertical(); GUILayout.Space(5); } }
    
            private bool DrawSubHeader(ref bool foldout, ref bool showPreset, string title, Color accent, MaterialProperty[] props, MaterialEditor editor, ref SpineModuleProfile profile, System.Action onSyncRequired)
            {
                Rect r = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
                Event e = Event.current; 
                bool hover = r.Contains(e.mousePosition);
    
                Rect presetRect = new Rect(r.xMax - 52f, r.y + 4f, 24f, 18f); 
                bool hoverPreset = presetRect.Contains(e.mousePosition);
    
                if (e.type == EventType.ContextClick && hover) { 
                    if (hoverPreset) {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("📖 查看 Preset 儲存說明"), false, () => {
                            SpineArtHelpWindow.ShowWindowAndScrollTo("Preset_Guide");
                        });
                        menu.ShowAsContext();
                        e.Use();
                    } else {
                        ShowCopyPasteMenu(title, props, editor, new string[] { title }, onSyncRequired); 
                        e.Use(); 
                    }
                }
                
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (hoverPreset) { showPreset = !showPreset; e.Use(); }
                    else if (hover) { foldout = !foldout; e.Use(); }
                }
    
                EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin
                    ? (hover ? new Color(.28f,.28f,.28f) : new Color(.22f,.22f,.22f))
                    : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f)));
                EditorGUI.DrawRect(new Rect(r.x, r.yMax-1f, r.width, 1f),
                    EditorGUIUtility.isProSkin ? new Color(.15f,.15f,.15f) : new Color(.6f,.6f,.6f));
                if (foldout) EditorGUI.DrawRect(new Rect(r.x, r.y, 3f, r.height), accent);
                
                var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
                ts.normal.textColor = foldout
                    ? (EditorGUIUtility.isProSkin ? new Color(.9f,.9f,.9f) : new Color(.1f,.1f,.1f))
                    : (EditorGUIUtility.isProSkin ? new Color(.6f,.6f,.6f) : new Color(.4f,.4f,.4f));
                EditorGUI.LabelField(new Rect(r.x+12f, r.y, r.width-60f, r.height), title, ts);
    
                if (showPreset || hoverPreset)
                {
                    Color btnBgColor;
                    if (showPreset)
                    {
                        btnBgColor = Color.white;
                        if (hoverPreset) btnBgColor = Color.Lerp(Color.white, accent, 0.15f);
                    }
                    else
                    {
                        float gray = EditorGUIUtility.isProSkin ? 0.95f : 1.0f; 
                        btnBgColor = new Color(gray, gray, gray, 1f);
                    }
                    
                    GUIStyle roundedStyle = new GUIStyle(EditorStyles.textArea); 
                    Color oldBgColor = GUI.backgroundColor;
                    GUI.backgroundColor = btnBgColor;
                    GUI.Box(presetRect, GUIContent.none, roundedStyle); 
                    GUI.backgroundColor = oldBgColor;
                }
                
                GUIContent presetIcon = EditorGUIUtility.IconContent("ScriptableObject Icon");
                if (presetIcon == null || presetIcon.image == null) presetIcon = new GUIContent("P");
    
                GUI.color = showPreset ? new Color(0.4f, 0.4f, 0.4f, 1f) : new Color(0.8f, 0.8f, 0.8f, 0.9f);
                if (presetIcon.image != null)
                {
                    float iconSize = 16f * 0.9f;
                    Rect iconRect = new Rect(presetRect.center.x - iconSize / 2f, presetRect.center.y - iconSize / 2f, iconSize, iconSize);
                    GUI.DrawTexture(iconRect, presetIcon.image, ScaleMode.ScaleToFit);
                }
                else
                {
                    var pStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 10, fontStyle = FontStyle.Bold };
                    EditorGUI.LabelField(presetRect, "P", pStyle);
                }
                GUI.color = Color.white; 
    
                var is2 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 15 };
                is2.normal.textColor = ts.normal.textColor;
                EditorGUI.LabelField(new Rect(r.xMax-24f, r.y+1f, 20f, r.height), foldout ? "−" : "＋", is2);
    
                if (foldout && showPreset)
                {
                    GUILayout.Space(2);
                    Rect pRect = GUILayoutUtility.GetRect(10f, 52f, GUILayout.ExpandWidth(true));
                    
                    if (Event.current.type == EventType.ContextClick && pRect.Contains(Event.current.mousePosition)) {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("📖 查看 Preset 使用說明"), false, () => {
                            SpineArtHelpWindow.ShowWindowAndScrollTo("Preset_Guide");
                        });
                        menu.ShowAsContext();
                        Event.current.Use();
                    }
    
                    EditorGUI.DrawRect(pRect, EditorGUIUtility.isProSkin ? new Color(0,0,0, 0.2f) : new Color(1,1,1, 0.5f));
                    EditorGUI.DrawRect(new Rect(pRect.x, pRect.y, 3f, pRect.height), accent * 0.7f);
                    
                    Rect row1 = new Rect(pRect.x + 8f, pRect.y + 6f, pRect.width - 16f, 18f);
                    Rect row2 = new Rect(pRect.x + 8f, pRect.y + 28f, pRect.width - 16f, 18f);
                    
                    Rect labelRect    = new Rect(row1.x, row1.y, 45f, row1.height);
                    Rect objRect      = new Rect(row1.x + 50f, row1.y, row1.width - 150f, row1.height);
                    Rect btnLoadRect  = new Rect(row1.xMax - 93f, row1.y, 45f, row1.height);
                    Rect btnClearRect = new Rect(row1.xMax - 46f, row1.y, 45f, row1.height);
                    
                    Rect maskLabelRect = new Rect(row2.x, row2.y, 100f, row2.height);
                    Rect maskFieldRect = new Rect(row2.x + 105f, row2.y, row2.width - 155f, row2.height);
                    Rect btnSaveRect   = new Rect(row2.xMax - 46f, row2.y, 45f, row2.height);
                    
                    GUIStyle lblStyle = new GUIStyle(EditorStyles.label) { fontSize = 11 };
                    lblStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(accent, Color.white, 0.3f) : Color.Lerp(accent, Color.black, 0.4f);
                    
                    EditorGUI.LabelField(labelRect, "Preset", lblStyle);
                    
                    EditorGUI.BeginChangeCheck();
                    SpineModuleProfile newProfile = (SpineModuleProfile)EditorGUI.ObjectField(objRect, profile, typeof(SpineModuleProfile), false);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObjects(editor.targets, "Assign Preset Profile"); 
                        
                        if (newProfile != null && newProfile.moduleName != title) {
                            RouteProfileToCorrectModule(newProfile, editor.targets);
                        } else {
                            profile = newProfile;
                            foreach (Material mat in editor.targets) {
                                SaveProfileToMaterial(mat, title, profile);
                                EditorUtility.SetDirty(mat);
                            }
                        }
                        SyncAutoAnimator(editor); 
                        onSyncRequired?.Invoke(); 
                    }
                    
                    EditorGUI.BeginDisabledGroup(profile == null);
                    if (GUI.Button(btnLoadRect, "Load", EditorStyles.miniButtonLeft)) {
                        LoadProfile(profile, props, editor);
                        SyncAutoAnimator(editor); 
                        onSyncRequired?.Invoke(); 
                        GUIUtility.ExitGUI(); 
                    }
                    EditorGUI.EndDisabledGroup();
    
                    if (GUI.Button(btnClearRect, "Clear", EditorStyles.miniButtonRight)) {
                        Undo.RecordObjects(editor.targets, "Clear Preset Profile");
                        profile = null; 
                        foreach(Material mat in editor.targets) {
                            SaveProfileToMaterial(mat, title, null);
                            EditorUtility.SetDirty(mat);
                        }
                        GUI.FocusControl(null); 
                        SyncAutoAnimator(editor); 
                        onSyncRequired?.Invoke();
                        GUIUtility.ExitGUI();
                    }
    
                    List<MaterialProperty> saveablePropsList = new List<MaterialProperty>();
                    foreach(var p in props) {
                        if (p != null && !ProfileIgnoredProperties.Contains(p.name)) {
                            saveablePropsList.Add(p);
                        }
                    }
                    MaterialProperty[] saveableProps = saveablePropsList.ToArray();
    
                    string[] propNames = new string[saveableProps.Length];
                    for (int i = 0; i < saveableProps.Length; i++) {
                        propNames[i] = FriendlyNameMap.ContainsKey(saveableProps[i].name) ? FriendlyNameMap[saveableProps[i].name] : (string.IsNullOrEmpty(saveableProps[i].displayName) ? saveableProps[i].name : saveableProps[i].displayName);
                    }
                    if (!profileSaveMasks.ContainsKey(title)) profileSaveMasks[title] = -1;
    
                    EditorGUI.LabelField(maskLabelRect, "Save Properties", lblStyle);
                    profileSaveMasks[title] = EditorGUI.MaskField(maskFieldRect, profileSaveMasks[title], propNames);
    
                    if (GUI.Button(btnSaveRect, "Save", EditorStyles.miniButton)) {
                        if (SaveProfile(title, saveableProps, ref profile, profileSaveMasks[title])) {
                            foreach (Material mat in editor.targets) {
                                SaveProfileToMaterial(mat, title, profile);
                                EditorUtility.SetDirty(mat);
                            }
                            SyncAutoAnimator(editor);
                            onSyncRequired?.Invoke();
                            AssetDatabase.SaveAssets(); 
                            GUIUtility.ExitGUI(); 
                        }
                    }
    
                    GUILayout.Space(2);
                    EditorGUILayout.HelpBox("如果某個參數需要程式Runtime動態控制，請將該Property剔除後再Save，以避免 MPB 控制權衝突。", MessageType.Info);
                    GUILayout.Space(4);
                }
    
                return foldout;
            }
    
            private void DrawInnerHeader(string title, Color accent)
            {
                GUILayout.Space(8);
                var s = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11 };
                s.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(accent, Color.white, .2f) : Color.Lerp(accent, Color.black, .3f);
                EditorGUILayout.LabelField(title, s);
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(10f, 1f, GUILayout.ExpandWidth(true)), EditorGUIUtility.isProSkin ? new Color(1f,1f,1f,.08f) : new Color(0f,0f,0f,.08f));
                GUILayout.Space(4);
            }
    
            private bool DrawAdvancedFoldout(ref bool foldout, string title)
            {
                GUILayout.Space(5);
                Rect r = GUILayoutUtility.GetRect(10f, 22f, GUILayout.ExpandWidth(true));
                Event e = Event.current; bool hover = r.Contains(e.mousePosition);
                if (e.type == EventType.MouseDown && hover) { foldout = !foldout; e.Use(); }
                EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin
                    ? (hover ? new Color(1f,1f,1f,.05f) : new Color(0f,0f,0f,.15f))
                    : (hover ? new Color(0f,0f,0f,.05f) : new Color(1f,1f,1f,.5f)));
                var s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, fontStyle = FontStyle.Bold };
                s.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.5f,.5f,.5f) : new Color(.4f,.4f,.4f);
                EditorGUI.LabelField(r, title + (foldout ? "  ▼" : "  ▶"), s);
                return foldout;
            }
    
            private static void BeginHelpBox() => EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            private static void EndHelpBox()   => EditorGUILayout.EndVertical();
    
            private void CheckAndShowLayerWarning(bool needWarning)
            {
                if (!needWarning) return;
                if (Selection.gameObjects.Length == 0) return;
                foreach (GameObject go in Selection.gameObjects) {
                    if (go.layer != 8 && go.layer != 9 && go.layer != 12 && go.layer != 13 && go.layer != 18 && go.layer != 20) {
                        EditorGUILayout.HelpBox("注意：目前選取的物件圖層 (Layer) 並非 8、9、12、13、18、20。\n依賴腳本的特效將不會自動掛載，請切換到對應 Layer 後再操作。", MessageType.Warning);
                        EditorGUILayout.Space(3);
                        break;
                    }
                }
            }
    
            private void DrawProbePreview(Color accent)
            {
                EditorGUILayout.Space(4); DrawInnerHeader("Runtime Probe Preview", accent);
                System.Type t = System.Type.GetType("VFXTool.SpineSkeletonShaderTool.SpineLightManager, Assembly-CSharp"); 
                if (t == null) return;
            
                object mgr = t.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null);
                if (mgr == null) return;
                var ef = t.GetField("enableLightProbe"); if (ef == null || !(bool)ef.GetValue(mgr)) return;
                var cl = t.GetField("targetCharacters")?.GetValue(mgr) as System.Collections.IList;
                if (cl == null || cl.Count == 0 || cl[0] == null) return;
                var rend = cl[0] as Renderer; if (rend == null) return;
                var mpb = new MaterialPropertyBlock(); rend.GetPropertyBlock(mpb);
                Vector4 ar = mpb.GetVector("_SpineSHAr"), ag = mpb.GetVector("_SpineSHAg"), ab = mpb.GetVector("_SpineSHAb");
                Color c = new Color(Mathf.Max(ar.w,0f), Mathf.Max(ag.w,0f), Mathf.Max(ab.w,0f), 1f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Approx. Probe Color", "SH L0 近似環境顏色。"));
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(18)), c);
                EditorGUILayout.EndHorizontal();
            }
    
            private void ApplyPropertyChange(MaterialProperty prop, float value)
            {
                if (prop == null) return;
                prop.floatValue = value;
                foreach (var t in prop.targets) {
                    if (t is Material mat) { MaterialEditor.ApplyMaterialPropertyDrawers(mat); EditorUtility.SetDirty(mat); }
                }
            }
    
            private static void EnsureSpineLightManager()
            {
                System.Type t = System.Type.GetType("VFXTool.SpineSkeletonShaderTool.SpineLightManager, Assembly-CSharp"); 
                if (t == null) return;
                try { t.GetMethod("EnsureInstance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null); }
                catch (System.Exception ex) { Debug.LogWarning($"[SpineCustomShaderGUI] EnsureSpineLightManager: {ex.Message}"); }
            }
        }

    // ─────────────────────────────────────────────────────────────
    // 獨立點擊 Profile 檔案時，專屬的現代化 Inspector 介面 (包含場景同步)
    // ─────────────────────────────────────────────────────────────
    [CustomEditor(typeof(SpineModuleProfile))]
    public class SpineModuleProfileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SpineModuleProfile profile = (SpineModuleProfile)target;
            
            Undo.RecordObject(profile, "Modify Profile Settings");

            GUILayout.Space(10);
            Rect r = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            
            Color accent = Color.gray;
            if (profile.moduleName != null) {
                if (profile.moduleName.Contains("Main")) accent = SpineCustomShaderGUI.ColorMain;
                else if (profile.moduleName.Contains("Noise")) accent = SpineCustomShaderGUI.ColorNoise;
                else if (profile.moduleName.Contains("Lighting") || profile.moduleName.Contains("Light Probe") || profile.moduleName.Contains("Shadow Light") || profile.moduleName.Contains("Normal Map")) accent = SpineCustomShaderGUI.ColorLighting;
                else if (profile.moduleName.Contains("Emission")) accent = SpineCustomShaderGUI.ColorEmission;
                else if (profile.moduleName.Contains("Effect") || profile.moduleName.Contains("Hit Sweep") || profile.moduleName.Contains("BackLight")) accent = SpineCustomShaderGUI.ColorEffect;
                else if (profile.moduleName.Contains("Inline") || profile.moduleName.Contains("Outline")) accent = SpineCustomShaderGUI.ColorLine;
            }

            EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin ? new Color(.22f,.22f,.22f) : new Color(.85f,.85f,.85f));
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), accent);

            var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
            ts.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            EditorGUI.LabelField(new Rect(r.x+12f, r.y+3f, r.width-30f, r.height-3f), $"❖ {profile.moduleName} Settings", ts);

            GUILayout.Space(5);
            SpineCustomShaderGUI.showRawPropertyNames = EditorGUILayout.ToggleLeft("顯示 Shader 變數名稱", SpineCustomShaderGUI.showRawPropertyNames);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
            GUILayout.Space(5);
            EditorGUI.indentLevel++;

            if (profile.properties == null) {
                profile.properties = new List<SpineModuleProfile.ProfileProperty>();
            }

            foreach (var prop in profile.properties)
            {
                GUIContent content = SpineCustomShaderGUI.showRawPropertyNames ? new GUIContent(prop.name) : new GUIContent(prop.displayName);
                
                switch (prop.type)
                {
                    case SpineModuleProfile.ProfilePropType.Float:
                        EditorGUILayout.BeginHorizontal();
                        if (prop.name.StartsWith("_Enable") || prop.name.StartsWith("_Use") || prop.name.StartsWith("_Fill") || prop.name.StartsWith("_Invert") || prop.name.Contains("Affects")) {
                            bool b = EditorGUILayout.Toggle(content, prop.floatValue > 0.5f);
                            prop.floatValue = b ? 1f : 0f;
                        }
                        else if (prop.name.Contains("LightingMode")) {
                            prop.floatValue = (float)(int)(SpineCustomShaderGUI.LightingMode)EditorGUILayout.EnumFlagsField(content, (SpineCustomShaderGUI.LightingMode)(int)prop.floatValue);
                        }
                        else if (prop.name.Contains("ZTest")) {
                            prop.floatValue = (float)(int)(SpineCustomShaderGUI.ZTestMode)EditorGUILayout.EnumPopup(content, (SpineCustomShaderGUI.ZTestMode)(int)prop.floatValue);
                        }
                        else {
                            prop.floatValue = EditorGUILayout.FloatField(content, prop.floatValue);
                        }
                        
                        bool prevFloatCurve = prop.useCurve;
                        prop.useCurve = GUILayout.Toggle(prop.useCurve, new GUIContent("∿", "開啟曲線動畫"), "Button", GUILayout.Width(26), GUILayout.Height(18));
                        if (prop.useCurve && !prevFloatCurve && prop.curveX.length == 0) {
                            prop.curveX.AddKey(new Keyframe(0f, prop.floatValue));
                            prop.curveX.AddKey(new Keyframe(1f, prop.floatValue));
                        }
                        EditorGUILayout.EndHorizontal();

                        if (prop.useCurve) {
                            EditorGUI.indentLevel++;
                            prop.curveX = EditorGUILayout.CurveField("↳ Value Curve", prop.curveX);
                            EditorGUI.indentLevel--;
                        }
                        break;
                    
                    case SpineModuleProfile.ProfilePropType.Color:
                        EditorGUILayout.BeginHorizontal();
                        prop.colorValue = EditorGUILayout.ColorField(content, prop.colorValue, true, true, true);
                        bool prevColCurve = prop.useCurve;
                        prop.useCurve = GUILayout.Toggle(prop.useCurve, new GUIContent("∿", "開啟曲線動畫"), "Button", GUILayout.Width(26), GUILayout.Height(18));
                        if (prop.useCurve && !prevColCurve) {
                            if (prop.curveX.length == 0) { prop.curveX.AddKey(0f, prop.colorValue.r); prop.curveX.AddKey(1f, prop.colorValue.r); }
                            if (prop.curveY.length == 0) { prop.curveY.AddKey(0f, prop.colorValue.g); prop.curveY.AddKey(1f, prop.colorValue.g); }
                            if (prop.curveZ.length == 0) { prop.curveZ.AddKey(0f, prop.colorValue.b); prop.curveZ.AddKey(1f, prop.colorValue.b); }
                            if (prop.curveW.length == 0) { prop.curveW.AddKey(0f, prop.colorValue.a); prop.curveW.AddKey(1f, prop.colorValue.a); }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (prop.useCurve) {
                            EditorGUI.indentLevel++;
                            prop.curveX = EditorGUILayout.CurveField("↳ R Curve", prop.curveX);
                            prop.curveY = EditorGUILayout.CurveField("↳ G Curve", prop.curveY);
                            prop.curveZ = EditorGUILayout.CurveField("↳ B Curve", prop.curveZ);
                            prop.curveW = EditorGUILayout.CurveField("↳ A Curve", prop.curveW);
                            EditorGUI.indentLevel--;
                        }
                        break;
                    
                    case SpineModuleProfile.ProfilePropType.Vector:
                        EditorGUILayout.BeginHorizontal();
                        prop.vectorValue = EditorGUILayout.Vector4Field(content, prop.vectorValue);
                        bool prevVecCurve = prop.useCurve;
                        prop.useCurve = GUILayout.Toggle(prop.useCurve, new GUIContent("∿", "開啟曲線動畫"), "Button", GUILayout.Width(26), GUILayout.Height(18));
                        if (prop.useCurve && !prevVecCurve) {
                            if (prop.curveX.length == 0) { prop.curveX.AddKey(0f, prop.vectorValue.x); prop.curveX.AddKey(1f, prop.vectorValue.x); }
                            if (prop.curveY.length == 0) { prop.curveY.AddKey(0f, prop.vectorValue.y); prop.curveY.AddKey(1f, prop.vectorValue.y); }
                            if (prop.curveZ.length == 0) { prop.curveZ.AddKey(0f, prop.vectorValue.z); prop.curveZ.AddKey(1f, prop.vectorValue.z); }
                            if (prop.curveW.length == 0) { prop.curveW.AddKey(0f, prop.vectorValue.w); prop.curveW.AddKey(1f, prop.vectorValue.w); }
                        }
                        EditorGUILayout.EndHorizontal();

                        if (prop.useCurve) {
                            EditorGUI.indentLevel++;
                            prop.curveX = EditorGUILayout.CurveField("↳ X Curve", prop.curveX);
                            prop.curveY = EditorGUILayout.CurveField("↳ Y Curve", prop.curveY);
                            prop.curveZ = EditorGUILayout.CurveField("↳ Z Curve", prop.curveZ);
                            prop.curveW = EditorGUILayout.CurveField("↳ W Curve", prop.curveW);
                            EditorGUI.indentLevel--;
                        }
                        break;
                    
                    case SpineModuleProfile.ProfilePropType.Texture:
                        prop.textureValue = (Texture)EditorGUILayout.ObjectField(content, prop.textureValue, typeof(Texture), false);
                        break;
                }
                GUILayout.Space(2);
            }

            EditorGUI.indentLevel--;
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(profile);
                SyncAllRenderersUsingThisProfile(profile);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 以下為全場景 C# 腳本同步邏輯
        // ─────────────────────────────────────────────────────────────
        private void SyncAllRenderersUsingThisProfile(SpineModuleProfile modifiedProfile)
        {
            if (modifiedProfile == null) return;

#if UNITY_2023_1_OR_NEWER
            Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            Renderer[] allRenderers = FindObjectsOfType<Renderer>();
#endif
            foreach (Renderer r in allRenderers)
            {
                if (r == null || r.gameObject == null) continue;

                int layer = r.gameObject.layer;
                if (layer != 8 && layer != 9 && layer != 12 && layer != 13 && layer != 18 && layer != 20) continue;

                SpineProfileAnimator anim = r.GetComponent<SpineProfileAnimator>();
                if (anim == null || anim.profiles == null || !anim.profiles.Contains(modifiedProfile))
                {
                    continue; // 該物件沒有使用這個被修改的 Profile
                }

                Material mat = r.sharedMaterials.FirstOrDefault(m => m != null);

                // Profile > Material 雙重驗證
                bool needInlineMask = IsFeatureEnabled(mat, anim, "_EnableInline");
                bool needHitSweep   = IsFeatureEnabled(mat, anim, "_EnableHitSweep");
                bool needBloomSync  = IsFeatureEnabled(mat, anim, "_EnableBloomSuppression");

                int blMode = GetIntFeature(mat, anim, "_BackLightLightingMode");
                int ilMode = GetIntFeature(mat, anim, "_InlineLightingMode");
                int olMode = GetIntFeature(mat, anim, "_OutlineLightingMode");

                bool needLightData =
                    IsFeatureEnabled(mat, anim, "_EnableNormalMap") ||
                    (IsFeatureEnabled(mat, anim, "_EnableBackLight") && blMode != 0) ||
                    (IsFeatureEnabled(mat, anim, "_EnableInline") && ilMode != 0) ||
                    (IsFeatureEnabled(mat, anim, "_EnableOutline") && olMode != 0) ||
                    IsFeatureEnabled(mat, anim, "_EnableLightProbe") ||
                    IsFeatureEnabled(mat, anim, "_EnableShadowLight");

                SyncAutoComponent(r.gameObject, "SpineAlphaMaskRenderer", needInlineMask);
                SyncAutoComponent(r.gameObject, "SpineHitSweepEffect", needHitSweep);
                SyncAutoComponent(r.gameObject, "SpineLightReceiver", needLightData);
                SyncAutoComponent(r.gameObject, "SpineBloomThresholdSync", needBloomSync);

                if (needLightData) EnsureSpineLightManager();
            }
        }

        private bool IsFeatureEnabled(Material mat, SpineProfileAnimator anim, string propName)
        {
            foreach (var p in anim.profiles) {
                if (p == null || p.properties == null) continue;
                var prop = p.properties.Find(x => x.name == propName);
                if (prop != null) return prop.floatValue > 0.5f;
            }
            if (mat != null && mat.HasProperty(propName)) {
                return mat.GetFloat(propName) > 0.5f;
            }
            return false;
        }

        private int GetIntFeature(Material mat, SpineProfileAnimator anim, string propName)
        {
            foreach (var p in anim.profiles) {
                if (p == null || p.properties == null) continue;
                var prop = p.properties.Find(x => x.name == propName);
                if (prop != null) return Mathf.RoundToInt(prop.floatValue);
            }
            if (mat != null && mat.HasProperty(propName)) {
                return Mathf.RoundToInt(mat.GetFloat(propName));
            }
            return 0;
        }

        private void SyncAutoComponent(GameObject go, string scriptName, bool isEnabled)
        {
            System.Type st = System.Type.GetType($"VFXTool.SpineSkeletonShaderTool.{scriptName}, Assembly-CSharp");
            if (st == null) return;

            Component comp = go.GetComponent(st);
            // 僅負責自動加載，不干涉使用者手動點亮的腳本
            if (isEnabled && comp == null)
            {
                Undo.AddComponent(go, st);
                Debug.Log($"[Profile 同步] 已為 {go.name} 自動加載 {scriptName}");
            }
        }

        private void EnsureSpineLightManager()
        {
            System.Type t = System.Type.GetType("VFXTool.SpineSkeletonShaderTool.SpineLightManager, Assembly-CSharp");
            if (t == null) return;
            try { t.GetMethod("EnsureInstance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null); }
            catch { /* ignore */ }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 現代化程式調用指南視窗 (支援折疊、搜尋)
    // ─────────────────────────────────────────────────────────────
    public class SpineProfileHelpWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private string searchQuery = "";

        private class ProgDocTopic {
            public string title;
            public string desc;
            public string warning;
            public string code;
        }
        
        private class ProgDocModule {
            public string moduleName;
            public Color accentColor;
            public bool isExpanded;
            public List<ProgDocTopic> topics = new List<ProgDocTopic>();
        }

        private List<ProgDocModule> docModules;

        public static void ShowWindow()
        {
            var window = GetWindow<SpineProfileHelpWindow>("程式調用與 MPB 說明");
            window.minSize = new Vector2(600, 750);
            window.Show();
        }

        private void OnEnable()
        {
            InitDocs();
        }

        private void OnGUI()
        {
            // === 現代化頂部搜尋列 (Flat Design) ===
            GUILayout.Space(10);
            
            Rect searchRect = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            
            Color bg = EditorGUIUtility.isProSkin ? new Color(.20f, .20f, .20f) : new Color(.85f, .85f, .85f);
            Color searchAccent = new Color(0.3f, 0.6f, 0.9f); // 程式指南的專屬科技藍
            
            EditorGUI.DrawRect(searchRect, bg);
            EditorGUI.DrawRect(new Rect(searchRect.x, searchRect.y, searchRect.width, 3f), searchAccent);

            GUIContent searchIcon = EditorGUIUtility.IconContent("Search Icon");
            if (searchIcon != null && searchIcon.image != null) {
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.3f, 0.3f, 0.3f);
                GUI.DrawTexture(new Rect(searchRect.x + 12, searchRect.y + 8, 16, 16), searchIcon.image);
                GUI.color = Color.white;
            } else {
                GUI.Label(new Rect(searchRect.x + 12, searchRect.y + 6, 20, 20), "🔍");
            }

            GUIStyle tfStyle = new GUIStyle(GUIStyle.none) {
                margin = new RectOffset(0,0,0,0), padding = new RectOffset(0,0,0,0),
                fontSize = 13, alignment = TextAnchor.MiddleLeft
            };
            tfStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            tfStyle.focused.textColor = tfStyle.normal.textColor;
            tfStyle.active.textColor = tfStyle.normal.textColor;
            tfStyle.hover.textColor = tfStyle.normal.textColor;
            
            Rect textRect = new Rect(searchRect.x + 36, searchRect.y + 3, searchRect.width - 65, searchRect.height - 3);
            
            if (string.IsNullOrEmpty(searchQuery)) {
                GUIStyle placeholderStyle = new GUIStyle(tfStyle) { fontStyle = FontStyle.Italic };
                placeholderStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(textRect, "輸入 API 名稱、關鍵字或報錯原因搜尋...", placeholderStyle);
            }

            searchQuery = GUI.TextField(textRect, searchQuery, tfStyle);
            
            if (!string.IsNullOrEmpty(searchQuery)) {
                if (GUI.Button(new Rect(searchRect.xMax - 26, searchRect.y + 8, 16, 16), EditorGUIUtility.IconContent("Clear"), GUIStyle.none)) {
                    searchQuery = "";
                    GUI.FocusControl(null); 
                }
            }
            
            GUILayout.Space(15);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold, margin = new RectOffset(10, 10, 10, 15) };
            GUILayout.Label("Spine Profile 系統：程式調用與防衝突指南", titleStyle);

            bool hasSearch = !string.IsNullOrEmpty(searchQuery);
            string queryLower = hasSearch ? searchQuery.ToLower() : "";

            foreach (var mod in docModules)
            {
                List<ProgDocTopic> visibleTopics = new List<ProgDocTopic>();
                if (hasSearch) {
                    foreach (var t in mod.topics) {
                        if (t.title.ToLower().Contains(queryLower) || 
                            t.desc.ToLower().Contains(queryLower) || 
                            (t.code != null && t.code.ToLower().Contains(queryLower))) {
                            visibleTopics.Add(t);
                        }
                    }
                } else {
                    visibleTopics = mod.topics;
                }

                if (hasSearch && visibleTopics.Count > 0) mod.isExpanded = true;
                if (hasSearch && visibleTopics.Count == 0 && !mod.moduleName.ToLower().Contains(queryLower)) continue;

                DrawModuleHeader(mod);

                if (mod.isExpanded)
                {
                    EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
                    GUILayout.Space(5);

                    foreach (var topic in visibleTopics)
                    {
                        DrawTopicDoc(topic, mod.accentColor);
                    }

                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                }
            }

            GUILayout.Space(30);
            GUILayout.EndScrollView();
        }

        private void DrawModuleHeader(ProgDocModule mod)
        {
            Rect r = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            Event e = Event.current; bool hover = r.Contains(e.mousePosition);
            if (e.type == EventType.MouseDown && e.button == 0 && hover) { mod.isExpanded = !mod.isExpanded; e.Use(); }

            Color bg = EditorGUIUtility.isProSkin
                ? (hover ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f));
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), mod.accentColor);

            var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
            ts.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            EditorGUI.LabelField(new Rect(r.x+12f, r.y+3f, r.width-30f, r.height-3f), "❖ " + mod.moduleName, ts);
            
            var is2 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            is2.normal.textColor = ts.normal.textColor;
            EditorGUI.LabelField(new Rect(r.xMax-24f, r.y+4f, 20f, r.height-3f), mod.isExpanded ? "−" : "＋", is2);
        }

        private void DrawTopicDoc(ProgDocTopic topic, Color accentColor)
        {
            EditorGUILayout.BeginVertical();
            
            GUIStyle propTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, richText = true, margin = new RectOffset(5, 5, 10, 5) };
            propTitle.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(accentColor, Color.white, 0.5f) : Color.Lerp(accentColor, Color.black, 0.4f);
            GUILayout.Label($"🔹 {topic.title}", propTitle);
            
            GUIStyle contentStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, margin = new RectOffset(15, 10, 2, 5) };
            
            if (!string.IsNullOrEmpty(topic.warning)) {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(topic.warning, MessageType.Warning);
                EditorGUI.indentLevel--;
                GUILayout.Space(2);
            }

            GUILayout.Label(topic.desc, contentStyle);

            if (!string.IsNullOrEmpty(topic.code)) {
                GUIStyle codeBox = new GUIStyle(EditorStyles.textArea) { wordWrap = true, margin = new RectOffset(15, 10, 5, 10), padding = new RectOffset(10, 10, 10, 10) };
                GUILayout.TextArea(topic.code, codeBox);
            }
            
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        private void InitDocs()
        {
            docModules = new List<ProgDocModule>();

            var modCore = new ProgDocModule { moduleName = "概念與防衝突機制", accentColor = new Color(0.9f, 0.4f, 0.4f), isExpanded = true };
            modCore.topics.Add(new ProgDocTopic { 
                title = "渲染優先權與衝突機制", 
                warning = "優先權：MaterialPropertyBlock (最高) > Material Instance > Shared Material",
                desc = "SpineProfileAnimator 每一幀都會透過 MaterialPropertyBlock (MPB) 套用預設數值。\n若在腳本中直接呼叫 <color=#D4A017>material.SetFloat()</color>，將會因為優先權較低而被 MPB 覆寫，導致修改無效，且會產生實體化材質打斷 SRP Batching。"
            });
            modCore.topics.Add(new ProgDocTopic { 
                title = "參數遮罩 (Save Mask) — 每幀高頻修改的最佳解", 
                desc = "如果某個參數需要由程式在 Update 中「每幀高頻控制」（例如閃爍），請請美術在 ShaderGUI 各模塊的「Save Properties」下拉選單中，將該參數<b>【取消勾選】</b>。\n如此一來，Profile 就不會綁定該參數，程式即可安全使用 SpineProfileHelper.SetFloat 直接修改 MPB。"
            });
            docModules.Add(modCore);

            var modAPI = new ProgDocModule { moduleName = "Profile 動態控制與加載 API", accentColor = new Color(0.3f, 0.6f, 0.9f), isExpanded = true };
            modAPI.topics.Add(new ProgDocTopic { 
                title = "動態加載 / 替換 / 移除 Profile", 
                desc = "強烈建議使用專屬的 <color=#66B2FF>SpineProfileHelper</color> 靜態類別來進行控制，它能確保生命週期防錯與效能的自動回收：",
                code = 
@"// 1. 動態加載或替換 Profile (自動掛載 Animator、防重複、強制刷新)
SpineProfileHelper.ApplyProfileRuntime(myRenderer, newProfile);

// 2. 移除指定模塊的 Profile (使用 Enum，完全避免拼寫錯誤！)
SpineProfileHelper.RemoveProfileRuntime(myRenderer, SpineProfileModule.Outline);

// 3. 一鍵清除角色身上所有 Spine Profile 與緩存
SpineProfileHelper.ClearAllProfiles(myRenderer);"
            });
            docModules.Add(modAPI);

            var modTemp = new ProgDocModule { moduleName = "進階：Temp Profile 實體竄改 (推薦)", accentColor = new Color(0.8f, 0.4f, 0.8f), isExpanded = true };
            modTemp.topics.Add(new ProgDocTopic {
                title = "臨時副本竄改法 (狀態切換專用)",
                warning = "警告：回傳的 TempProfile 實體必須手陪 Destroy，否則會引發 Memory Leak！",
                desc = "當角色進入特殊狀態時，不建議直接去改 MPB（因為會被曲線蓋掉）。最佳解法是呼叫 ApplyTempProfile，系統會自動複製一份 Profile 並將指定的參數強制覆寫，同時關閉該參數的曲線動畫。",
                code = 
@"// 1. 準備參數字典
var overrides = new Dictionary<string, object> {
    { ""_OutlineColor"", Color.green },
    { ""_OutlineWidth"", 5.0f }
};

// 2. 套用副本，並將回傳的實體存起來
SpineModuleProfile activeTempProfile = SpineProfileHelper.ApplyTempProfile(myRenderer, baseProfile, overrides);

// 3. 狀態結束時，移除效果並銷毀副本
SpineProfileHelper.RemoveProfileRuntime(myRenderer, activeTempProfile.moduleName);
Destroy(activeTempProfile);"
            });
            docModules.Add(modTemp);

            var modMPB = new ProgDocModule { moduleName = "底層 MPB 安全控制 API", accentColor = new Color(0.2f, 0.8f, 0.4f), isExpanded = false };
            modMPB.topics.Add(new ProgDocTopic { 
                title = "安全的數值修改方法", 
                warning = "前提：請確保該參數在 ShaderGUI 儲存 Preset 時【沒有】被勾選！",
                desc = "請統一使用 Helper 封裝好的靜態方法。這將會無縫融合系統底層的 MPB 狀態，不會互相打架，且能保持最極致的合批效能：",
                code = 
@"// 正確寫法：安全地設定 Float、Color 等數值，保持高效能合批
SpineProfileHelper.SetFloat(myRenderer, ""_OutlineWidth"", 0.1f);
SpineProfileHelper.SetColor(myRenderer, ""_EffectColor"", Color.red);
SpineProfileHelper.SetVector(myRenderer, ""_GlobalNoiseSpeed"", new Vector4(1, 1, 0, 0));

// 錯誤寫法：會產生材質實例，且可能被 Animator 覆寫
// myRenderer.material.SetFloat(""_OutlineWidth"", 0.1f);"
            });
            docModules.Add(modMPB);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 現代化除錯指南視窗 (支援搜尋)
    // ─────────────────────────────────────────────────────────────
    public class SpineDebugHelpWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private string searchQuery = "";
        private SpineLightingIssueTroubleshooter.Result lightingTroubleshootResult;

        private class DebugDocTopic {
            public string title;
            public string desc;
            public string[] solutions;
        }

        private class DebugDocModule {
            public string moduleName;
            public Color accentColor;
            public bool isExpanded;
            public List<DebugDocTopic> topics = new List<DebugDocTopic>();
        }

        private List<DebugDocModule> docModules;

        public static void ShowWindow()
        {
            var window = GetWindow<SpineDebugHelpWindow>("除錯指南");
            window.minSize = new Vector2(600, 750);
            window.Show();
        }

        private void OnEnable()
        {
            InitDocs();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            
            Rect searchRect = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            Color bg = EditorGUIUtility.isProSkin ? new Color(.20f, .20f, .20f) : new Color(.85f, .85f, .85f);
            Color searchAccent = new Color(0.9f, 0.7f, 0.2f);
            
            EditorGUI.DrawRect(searchRect, bg);
            EditorGUI.DrawRect(new Rect(searchRect.x, searchRect.y, searchRect.width, 3f), searchAccent);

            GUIContent searchIcon = EditorGUIUtility.IconContent("Search Icon");
            if (searchIcon != null && searchIcon.image != null) {
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.3f, 0.3f, 0.3f);
                GUI.DrawTexture(new Rect(searchRect.x + 12, searchRect.y + 8, 16, 16), searchIcon.image);
                GUI.color = Color.white;
            } else {
                GUI.Label(new Rect(searchRect.x + 12, searchRect.y + 6, 20, 20), "🔍");
            }

            GUIStyle tfStyle = new GUIStyle(GUIStyle.none) {
                margin = new RectOffset(0,0,0,0), padding = new RectOffset(0,0,0,0),
                fontSize = 13, alignment = TextAnchor.MiddleLeft
            };
            tfStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            tfStyle.focused.textColor = tfStyle.normal.textColor;
            tfStyle.active.textColor = tfStyle.normal.textColor;
            tfStyle.hover.textColor = tfStyle.normal.textColor;
            
            Rect textRect = new Rect(searchRect.x + 36, searchRect.y + 3, searchRect.width - 65, searchRect.height - 3);
            
            if (string.IsNullOrEmpty(searchQuery)) {
                GUIStyle placeholderStyle = new GUIStyle(tfStyle) { fontStyle = FontStyle.Italic };
                placeholderStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(textRect, "輸入問題關鍵字進行搜尋...", placeholderStyle);
            }

            searchQuery = GUI.TextField(textRect, searchQuery, tfStyle);
            
            if (!string.IsNullOrEmpty(searchQuery)) {
                if (GUI.Button(new Rect(searchRect.xMax - 26, searchRect.y + 8, 16, 16), EditorGUIUtility.IconContent("Clear"), GUIStyle.none)) {
                    searchQuery = "";
                    GUI.FocusControl(null); 
                }
            }
            
            GUILayout.Space(15);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold, margin = new RectOffset(10, 10, 10, 15) };
            GUILayout.Label("Spine Shader 常見除錯指南", titleStyle);

            bool hasSearch = !string.IsNullOrEmpty(searchQuery);
            string queryLower = hasSearch ? searchQuery.ToLower() : "";

            foreach (var mod in docModules)
            {
                List<DebugDocTopic> visibleTopics = new List<DebugDocTopic>();
                if (hasSearch) {
                    foreach (var t in mod.topics) {
                        if (t.title.ToLower().Contains(queryLower) || 
                            t.desc.ToLower().Contains(queryLower) || 
                            t.solutions.Any(s => s.ToLower().Contains(queryLower))) {
                            visibleTopics.Add(t);
                        }
                    }
                } else {
                    visibleTopics = mod.topics;
                }

                bool troubleshootSearchHit = hasSearch && mod.moduleName == "光照與陰影問題" && TroubleshootResultContains(queryLower);
                if (hasSearch && (visibleTopics.Count > 0 || troubleshootSearchHit)) mod.isExpanded = true;
                if (hasSearch && visibleTopics.Count == 0 && !mod.moduleName.ToLower().Contains(queryLower) && !troubleshootSearchHit) continue;

                DrawModuleHeader(mod);

                if (mod.isExpanded)
                {
                    EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
                    GUILayout.Space(5);

                    foreach (var topic in visibleTopics)
                    {
                        DrawTopicDoc(topic, mod.accentColor);
                    }

                    if (mod.moduleName == "光照與陰影問題")
                    {
                        DrawLightingTroubleshooterPanel(mod.accentColor, hasSearch, queryLower);
                    }

                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                }
            }

            GUILayout.Space(30);
            GUILayout.EndScrollView();
        }

        private void DrawModuleHeader(DebugDocModule mod)
        {
            Rect r = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            Event e = Event.current; bool hover = r.Contains(e.mousePosition);
            if (e.type == EventType.MouseDown && e.button == 0 && hover) { mod.isExpanded = !mod.isExpanded; e.Use(); }

            Color bg = EditorGUIUtility.isProSkin
                ? (hover ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f));
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), mod.accentColor);

            var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
            ts.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            EditorGUI.LabelField(new Rect(r.x+12f, r.y+3f, r.width-30f, r.height-3f), "❖ " + mod.moduleName, ts);
            
            var is2 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            is2.normal.textColor = ts.normal.textColor;
            EditorGUI.LabelField(new Rect(r.xMax-24f, r.y+4f, 20f, r.height-3f), mod.isExpanded ? "−" : "＋", is2);
        }

        private void DrawTopicDoc(DebugDocTopic topic, Color accentColor)
        {
            EditorGUILayout.BeginVertical();
            
            GUIStyle propTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, richText = true, margin = new RectOffset(5, 5, 5, 5) };
            propTitle.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(accentColor, Color.white, 0.5f) : Color.Lerp(accentColor, Color.black, 0.4f);
            GUILayout.Label($"🔹 Q：{topic.title}", propTitle);
            
            GUIStyle contentStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, margin = new RectOffset(15, 10, 2, 5) };

            if (!string.IsNullOrEmpty(topic.desc)) {
                GUILayout.Label(topic.desc, contentStyle);
            }

            if (topic.solutions != null && topic.solutions.Length > 0) {
                GUIStyle solutionBox = new GUIStyle(EditorStyles.helpBox) { margin = new RectOffset(15, 10, 5, 10), padding = new RectOffset(10, 10, 8, 8) };
                EditorGUILayout.BeginVertical(solutionBox);
                
                GUIStyle solTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, richText = true };
                solTitle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.9f, 0.7f) : new Color(0.2f, 0.6f, 0.2f);
                GUILayout.Label("💡 解決方案 / 檢查步驟：", solTitle);
                GUILayout.Space(2);

                for(int i = 0; i < topic.solutions.Length; i++) {
                    GUILayout.Label($"<b>{i+1}.</b> {topic.solutions[i]}", contentStyle);
                }
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        private void DrawLightingTroubleshooterPanel(Color accentColor, bool hasSearch, string queryLower)
        {
            GUIStyle box = new GUIStyle(EditorStyles.helpBox) {
                margin = new RectOffset(15, 10, 8, 12),
                padding = new RectOffset(10, 10, 8, 10)
            };
            EditorGUILayout.BeginVertical(box);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, richText = true };
            titleStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(accentColor, Color.white, 0.35f) : Color.Lerp(accentColor, Color.black, 0.35f);
            GUILayout.Label("光照問題排查", titleStyle);

            GUIStyle contentStyle = new GUIStyle(EditorStyles.label) {
                wordWrap = true,
                richText = true,
                margin = new RectOffset(2, 2, 2, 4)
            };

            GameObject selectedGO = Selection.activeGameObject;
            bool validSceneObject = selectedGO != null && !EditorUtility.IsPersistent(selectedGO);

            if (!validSceneObject)
            {
                Object selectedObject = Selection.activeObject;
                if (selectedObject is Material)
                {
                    EditorGUILayout.HelpBox("請先選取 Hierarchy / Scene 中的角色 GameObject，透過 GameObject 的 Renderer/材質進行排查；不能直接選 Project 中的 Material。", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("請先選取 Hierarchy / Scene 中的角色 GameObject，才能啟動光照問題排查。", MessageType.Info);
                }
            }
            else
            {
                GUILayout.Label($"目前目標：{selectedGO.name}", contentStyle);
            }

            EditorGUI.BeginDisabledGroup(!validSceneObject);
            if (GUILayout.Button("光照問題排查", GUILayout.Height(28)))
            {
                lightingTroubleshootResult = SpineLightingIssueTroubleshooter.Run(selectedGO);
            }
            EditorGUI.EndDisabledGroup();

            if (lightingTroubleshootResult != null)
            {
                GUILayout.Space(8);
                DrawTroubleshootTargetSummary(lightingTroubleshootResult, contentStyle);

                bool filterTroubleshootLines = hasSearch && !TroubleshootHeaderOrSummaryContains(queryLower);
                DrawTroubleshootList("檢查結果", FilterTroubleshootLines(lightingTroubleshootResult.issues, filterTroubleshootLines, queryLower), lightingTroubleshootResult.HasIssues ? MessageType.Warning : MessageType.Info, contentStyle, lightingTroubleshootResult.issues);
                DrawTroubleshootList("Realtime 燈光列表", FilterTroubleshootLines(lightingTroubleshootResult.realtimeLights, filterTroubleshootLines, queryLower), MessageType.Info, contentStyle);
                DrawTroubleshootList("材質摘要", FilterTroubleshootLines(lightingTroubleshootResult.materialSummaries, filterTroubleshootLines, queryLower), MessageType.Info, contentStyle);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTroubleshootTargetSummary(SpineLightingIssueTroubleshooter.Result result, GUIStyle contentStyle)
        {
            if (result == null || string.IsNullOrEmpty(result.targetSummary))
                return;

            GameObject target = result.target;
            bool canNavigate = target != null && !EditorUtility.IsPersistent(target);
            string displayText = result.targetSummary;
            if (canNavigate)
            {
                displayText = displayText.Replace(target.name, ToUnderlinedText(target.name));
            }

            GUIStyle lineStyle = new GUIStyle(contentStyle) { richText = true };
            GUIContent content = new GUIContent(displayText);
            float width = Mathf.Max(120f, EditorGUIUtility.currentViewWidth - 70f);
            float height = lineStyle.CalcHeight(content, width);
            Rect lineRect = GUILayoutUtility.GetRect(content, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            GUI.Label(lineRect, content, lineStyle);

            if (!canNavigate) return;

            Rect linkRect = GetInlineLinkRect(lineRect, result.targetSummary, target.name, lineStyle, "");
            if (linkRect.width <= 0f) return;

            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && linkRect.Contains(e.mousePosition))
            {
                FocusTroubleshootTarget(target);
                e.Use();
            }
        }

        private bool TroubleshootResultContains(string queryLower)
        {
            if (lightingTroubleshootResult == null || string.IsNullOrEmpty(queryLower)) return false;
            if (TroubleshootHeaderOrSummaryContains(queryLower)) return true;
            return TroubleshootLinesContain(lightingTroubleshootResult.issues, queryLower) ||
                   TroubleshootLinesContain(lightingTroubleshootResult.realtimeLights, queryLower) ||
                   TroubleshootLinesContain(lightingTroubleshootResult.materialSummaries, queryLower);
        }

        private bool TroubleshootHeaderOrSummaryContains(string queryLower)
        {
            if (string.IsNullOrEmpty(queryLower)) return false;
            if ("光照問題排查".ToLower().Contains(queryLower)) return true;
            return lightingTroubleshootResult != null &&
                   !string.IsNullOrEmpty(lightingTroubleshootResult.targetSummary) &&
                   lightingTroubleshootResult.targetSummary.ToLower().Contains(queryLower);
        }

        private bool TroubleshootLinesContain(List<SpineLightingIssueTroubleshooter.ResultLine> lines, string queryLower)
        {
            if (lines == null) return false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] != null && !string.IsNullOrEmpty(lines[i].text) && lines[i].text.ToLower().Contains(queryLower))
                    return true;
            }
            return false;
        }

        private List<SpineLightingIssueTroubleshooter.ResultLine> FilterTroubleshootLines(List<SpineLightingIssueTroubleshooter.ResultLine> lines, bool hasSearch, string queryLower)
        {
            if (!hasSearch || string.IsNullOrEmpty(queryLower)) return lines;

            List<SpineLightingIssueTroubleshooter.ResultLine> filtered = new List<SpineLightingIssueTroubleshooter.ResultLine>();
            if (lines == null) return filtered;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] != null && !string.IsNullOrEmpty(lines[i].text) && lines[i].text.ToLower().Contains(queryLower))
                    filtered.Add(lines[i]);
            }
            return filtered;
        }

        private void DrawTroubleshootList(string title, List<SpineLightingIssueTroubleshooter.ResultLine> items, MessageType emptyMessageType, GUIStyle contentStyle)
        {
            DrawTroubleshootList(title, items, emptyMessageType, contentStyle, null);
        }

        private void DrawTroubleshootList(string title, List<SpineLightingIssueTroubleshooter.ResultLine> items, MessageType emptyMessageType, GUIStyle contentStyle, List<SpineLightingIssueTroubleshooter.ResultLine> allItems)
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, richText = true };
            GUILayout.Space(4);
            GUILayout.Label(title, headerStyle);

            GUIStyle lineStyle = new GUIStyle(contentStyle) { richText = true };
            if (title == "檢查結果")
            {
                if (items == null) items = new List<SpineLightingIssueTroubleshooter.ResultLine>();
                DrawTroubleshootSection("有問題", items, true, lineStyle, allItems);
                DrawTroubleshootSection("沒問題", items, false, lineStyle, allItems);
                return;
            }

            if (items == null || items.Count == 0)
            {
                string message = title == "檢查結果" ? "未發現明確問題。" : "沒有資料。";
                EditorGUILayout.HelpBox(message, emptyMessageType);
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                DrawTroubleshootLine(items[i], lineStyle);
            }
        }

        private void DrawTroubleshootSection(string sectionTitle, List<SpineLightingIssueTroubleshooter.ResultLine> items, bool issueSection, GUIStyle lineStyle, List<SpineLightingIssueTroubleshooter.ResultLine> allItems)
        {
            bool hasAny = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].isIssue == issueSection) { hasAny = true; break; }
            }

            GUILayout.Space(2);
            GUILayout.Label(sectionTitle, EditorStyles.miniBoldLabel);

            if (!hasAny)
            {
                string emptyText;
                if (issueSection && HasTroubleshootLineOfType(allItems, true))
                    emptyText = "尚有其他問題。";
                else
                    emptyText = issueSection ? "目前未發現問題，若視覺效果仍有錯誤，請找立璟或傳送小花。" : "目前無可列出的正常項目。";
                GUILayout.Label($"• {HighlightTroubleshootKeywords(emptyText, false)}", lineStyle);
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].isIssue == issueSection)
                    DrawTroubleshootLine(items[i], lineStyle);
            }
        }

        private bool HasTroubleshootLineOfType(List<SpineLightingIssueTroubleshooter.ResultLine> items, bool issueType)
        {
            if (items == null) return false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].isIssue == issueType)
                    return true;
            }
            return false;
        }

        private void DrawTroubleshootLine(SpineLightingIssueTroubleshooter.ResultLine item, GUIStyle lineStyle)
        {
            string text = FormatTroubleshootText(item.text);
            string displayText;
            if (item.isIssue)
            {
                displayText = $"• <color=#ff5555>{ApplyNavigationUnderline(text, item)}</color>";
            }
            else
            {
                displayText = $"• {HighlightTroubleshootKeywords(text, false)}";
            }

            GUIContent content = new GUIContent(displayText);
            float width = Mathf.Max(120f, EditorGUIUtility.currentViewWidth - 70f);
            float height = lineStyle.CalcHeight(content, width);
            Rect lineRect = GUILayoutUtility.GetRect(content, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            GUI.Label(lineRect, content, lineStyle);

            if (CanNavigate(item))
            {
                Rect linkRect = GetTroubleshootLinkRect(lineRect, item, lineStyle);
                if (linkRect.width <= 0f) return;

                EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
                Event e = Event.current;
                if (e.type == EventType.MouseUp && e.button == 0 && linkRect.Contains(e.mousePosition))
                {
                    FocusTroubleshootTarget(item.navigationTarget);
                    e.Use();
                }
            }
        }

        private Rect GetTroubleshootLinkRect(Rect lineRect, SpineLightingIssueTroubleshooter.ResultLine item, GUIStyle lineStyle)
        {
            if (!CanNavigate(item)) return Rect.zero;
            return GetInlineLinkRect(lineRect, FormatTroubleshootText(item.text), item.navigationLabel, lineStyle, "• ");
        }

        private Rect GetInlineLinkRect(Rect lineRect, string fullText, string linkLabel, GUIStyle lineStyle, string prefixText)
        {
            if (string.IsNullOrEmpty(fullText) || string.IsNullOrEmpty(linkLabel)) return Rect.zero;

            int labelIndex = fullText.IndexOf(linkLabel, System.StringComparison.Ordinal);
            if (labelIndex < 0) return Rect.zero;

            string prefix = prefixText + fullText.Substring(0, labelIndex);
            string linkText = ToUnderlinedText(linkLabel);
            float xOffset = lineStyle.CalcSize(new GUIContent(prefix)).x;
            float linkWidth = lineStyle.CalcSize(new GUIContent(linkText)).x;
            float linkHeight = Mathf.Min(lineRect.height, EditorGUIUtility.singleLineHeight + 4f);

            Rect linkRect = new Rect(lineRect.x + xOffset, lineRect.y, linkWidth, linkHeight);
            if (linkRect.xMax > lineRect.xMax) return Rect.zero;
            return linkRect;
        }

        private string ApplyNavigationUnderline(string text, SpineLightingIssueTroubleshooter.ResultLine item)
        {
            if (!CanNavigate(item)) return text;
            if (string.IsNullOrEmpty(item.navigationLabel)) return text;
            return text.Replace(item.navigationLabel, ToUnderlinedText(item.navigationLabel));
        }

        private string ToUnderlinedText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length * 2);
            for (int i = 0; i < text.Length; i++)
            {
                sb.Append(text[i]);
                if (!char.IsWhiteSpace(text[i])) sb.Append('\u0332');
            }
            return sb.ToString();
        }

        private bool CanNavigate(SpineLightingIssueTroubleshooter.ResultLine item)
        {
            return item != null && item.navigationTarget != null && !string.IsNullOrEmpty(item.navigationLabel);
        }

        private void FocusTroubleshootTarget(Object target)
        {
            if (target == null) return;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);

            GameObject go = target as GameObject;
            Component component = target as Component;
            if (go == null && component != null) go = component.gameObject;

            if (go != null && !EditorUtility.IsPersistent(go) && SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        private string FormatTroubleshootText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("。調整方式：", "。\n    調整方式：");
        }

        private string HighlightTroubleshootKeywords(string text, bool isIssue)
        {
            if (string.IsNullOrEmpty(text)) return text;

            if (isIssue)
            {
                string[] redWords = {
                    "沒有", "找不到", "未", "不是", "不在", "不能", "不會", "無法",
                    "否", "OFF", "Disabled", "空", "無屬性", "0 或更低"
                };
                return HighlightWords(text, redWords, "#ff5555");
            }

            string[] greenWords = {
                "有", "是", "已", "可以", "正確", "完成", "ON"
            };
            return HighlightWords(text, greenWords, "#55cc66");
        }

        private string HighlightWords(string text, string[] words, string color)
        {
            string result = text;
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                result = result.Replace(word, $"<color={color}>{word}</color>");
            }
            return result;
        }

        private void InitDocs()
        {
            docModules = new List<DebugDocModule>();
        
            // ==========================================
            // === 光照與陰影問題模塊 ===================
            // ==========================================
            var modLighting = new DebugDocModule { moduleName = "光照與陰影問題", accentColor = SpineCustomShaderGUI.ColorLighting, isExpanded = true };

            // 新增：角色全黑、開燈無效的問題
            modLighting.topics.Add(new DebugDocTopic {
                title = "為何我的角色看起來黑黑的，明明有開燈但卻感覺沒受到光照影響？",
                desc = "當角色未能正常接收光照時，通常是材質開關、層級設定或編譯變體被限制所導致，請依序檢查：",
                solutions = new string[] {
                    "<b>SpineLightManager 層級規範設定</b> → 需考慮那些層級需使用特殊光照效果，該層級才需要被加入 SpineLightManager 物件的名單中。",
                    "<b>確認材質球開關</b> → 請先確認角色的材質球是否有勾選 <b>Enable Native Lighting</b> 等光照選項。",
                    "<b>確認物件 Layer</b> → 確認該材質所在的物件 Layer 是否被設定在 <b>8, 9, 12, 13, 18, 20</b> 中。",
                    "<b>確認燈光 Culling Mask</b> → 檢查場景中以有沒有打光，以及照射該角色的燈光，其 Culling Mask 是否有包含上述的 <b>8, 9, 12, 13, 18, 20</b> 層。",
                    "<b>使用預覽功能排除變體問題</b> → 請按下材質球編輯器右上方的 <color=#ff5555><b>「▤ 全效預覽」</b></color> 按鈕。若按下後恢復正常，代表是目前選擇的「平台設定檔」禁用了該光照模塊。"
                }
            });
        
            docModules.Add(modLighting);
        
            // ==========================================
            // === 特效與模組缺失問題模塊 ===============
            // ==========================================
            var modEffect = new DebugDocModule { moduleName = "特效與模組缺失問題", accentColor = SpineCustomShaderGUI.ColorEffect, isExpanded = true };
        
            // 新增：勾選效果無效、材質球看似壞掉的問題
            modEffect.topics.Add(new DebugDocTopic {
                title = "為何我勾選這個角色的特殊效果 (內外框、背光、掃光等) 都不起作用？材質球選項好像壞掉了",
                desc = "當您發現材質面板上的開關 (Toggle) 怎麼勾選都沒反應時，通常是因為底層的 Shader Keyword 已經被「平台變體管理」給剔除了：",
                solutions = new string[] {
                    "<b>確認腳本輔助層級</b> → 再次確認物件的 Layer 是否在 <b>8, 9, 12, 13, 18, 20</b>，因為許多特效依賴腳本抓取資料，若層級不對，特效依然不會顯示。",
                    "<b>使用本地全開預覽</b> → 請按下材質球編輯器右上方的 <color=#ff5555><b>「▤ 全效預覽」</b></color> 按鈕！這會強制開啟此材質的所有 Shader 變體供本地完整預覽，且不會影響最終打包效能。"
                }
            });
        
            docModules.Add(modEffect);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 現代化美術使用指南視窗 (支援折疊、搜尋、右鍵跳轉)
    // ─────────────────────────────────────────────────────────────
    public class SpineArtHelpWindow : EditorWindow
        {
            private Vector2 scrollPos;
            private string searchQuery = "";
            private static string scrollToTarget = null; 
            private static string highlightedTarget = null;
            private double highlightStartTime = 0;
    
            private class HelpDocProp {
                public string propName;
                public string friendlyName;
                public string desc;
                public string dependency;
                public string bugFix;
            }
            
            private class HelpDocModule {
                public string moduleName;
                public Color accentColor;
                public bool isExpanded;
                public List<HelpDocProp> props = new List<HelpDocProp>();
            }
    
            private List<HelpDocModule> docModules;
    
            public static void ShowWindow()
            {
                var window = GetWindow<SpineArtHelpWindow>("美術使用指南");
                window.minSize = new Vector2(600, 750);
                window.Show();
            }
    
            public static void ShowWindowAndScrollTo(string propertyName)
            {
                scrollToTarget = propertyName;
                ShowWindow();
            }
    
            private void OnEnable()
            {
                InitDocs();
                EditorApplication.update += RepaintUpdate;
            }
    
            private void OnDisable()
            {
                EditorApplication.update -= RepaintUpdate;
            }
    
            private void RepaintUpdate()
            {
                if (highlightedTarget != null && EditorApplication.timeSinceStartup - highlightStartTime < 2.0) {
                    Repaint();
                }
            }
    
            private void OnGUI()
            {
                GUILayout.Space(10);
                
                Rect searchRect = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
                
                Color bg = EditorGUIUtility.isProSkin ? new Color(.20f, .20f, .20f) : new Color(.85f, .85f, .85f);
                Color accentColor = new Color(0.4f, 0.6f, 0.9f);
                
                EditorGUI.DrawRect(searchRect, bg);
                EditorGUI.DrawRect(new Rect(searchRect.x, searchRect.y, searchRect.width, 3f), accentColor);
    
                GUIContent searchIcon = EditorGUIUtility.IconContent("Search Icon");
                if (searchIcon != null && searchIcon.image != null) {
                    GUI.color = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.3f, 0.3f, 0.3f);
                    GUI.DrawTexture(new Rect(searchRect.x + 12, searchRect.y + 8, 16, 16), searchIcon.image);
                    GUI.color = Color.white;
                } else {
                    GUI.Label(new Rect(searchRect.x + 12, searchRect.y + 6, 20, 20), "🔍");
                }
    
                GUIStyle tfStyle = new GUIStyle(GUIStyle.none) {
                    margin = new RectOffset(0,0,0,0),
                    padding = new RectOffset(0,0,0,0),
                    fontSize = 13, 
                    alignment = TextAnchor.MiddleLeft
                };
                tfStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                tfStyle.focused.textColor = tfStyle.normal.textColor;
                tfStyle.active.textColor = tfStyle.normal.textColor;
                tfStyle.hover.textColor = tfStyle.normal.textColor;
                
                Rect textRect = new Rect(searchRect.x + 36, searchRect.y + 3, searchRect.width - 65, searchRect.height - 3);
                
                if (string.IsNullOrEmpty(searchQuery)) {
                    GUIStyle placeholderStyle = new GUIStyle(tfStyle) { fontStyle = FontStyle.Italic };
                    placeholderStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                    GUI.Label(textRect, "輸入關鍵字搜尋效果或屬性...", placeholderStyle);
                }
    
                searchQuery = GUI.TextField(textRect, searchQuery, tfStyle);
                
                if (!string.IsNullOrEmpty(searchQuery)) {
                    if (GUI.Button(new Rect(searchRect.xMax - 26, searchRect.y + 8, 16, 16), EditorGUIUtility.IconContent("Clear"), GUIStyle.none)) {
                        searchQuery = "";
                        GUI.FocusControl(null); 
                    }
                }
                
                GUILayout.Space(15);
    
                scrollPos = GUILayout.BeginScrollView(scrollPos);
    
                GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold, margin = new RectOffset(10, 10, 10, 15) };
                GUILayout.Label("Spine Shader 美術視覺開發指南", titleStyle);
    
                EditorGUILayout.HelpBox("【注意事項】\n角色物件必須位於 Layer 8、9、12、13、18、20，否則各類特效依賴腳本(光照/掃光/內框)將無法自動掛載與運作！", MessageType.Warning);
                GUILayout.Space(10);
    
                bool hasSearch = !string.IsNullOrEmpty(searchQuery);
                string queryLower = hasSearch ? searchQuery.ToLower() : "";
    
                if (scrollToTarget != null) {
                    foreach (var mod in docModules) {
                        if (mod.props.Any(p => p.propName == scrollToTarget || p.friendlyName == scrollToTarget)) {
                            mod.isExpanded = true;
                        }
                    }
                }
    
                foreach (var mod in docModules)
                {
                    List<HelpDocProp> visibleProps = new List<HelpDocProp>();
                    if (hasSearch) {
                        foreach (var p in mod.props) {
                            if (p.propName.ToLower().Contains(queryLower) || 
                                p.friendlyName.ToLower().Contains(queryLower) || 
                                p.desc.ToLower().Contains(queryLower)) {
                                visibleProps.Add(p);
                            }
                        }
                    } else {
                        visibleProps = mod.props;
                    }
    
                    if (hasSearch && visibleProps.Count > 0) mod.isExpanded = true;
                    if (hasSearch && visibleProps.Count == 0 && !mod.moduleName.ToLower().Contains(queryLower)) continue;
    
                    DrawModuleHeader(mod);
    
                    if (mod.isExpanded)
                    {
                        EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
                        GUILayout.Space(5);
    
                        foreach (var prop in visibleProps)
                        {
                            DrawPropertyDoc(prop, mod.accentColor);
                        }
    
                        GUILayout.Space(5);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10);
                    }
                }
    
                GUILayout.Space(30);
                GUILayout.EndScrollView();
            }
    
            private void DrawModuleHeader(HelpDocModule mod)
            {
                Rect r = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
                Event e = Event.current; bool hover = r.Contains(e.mousePosition);
                if (e.type == EventType.MouseDown && e.button == 0 && hover) { mod.isExpanded = !mod.isExpanded; e.Use(); }
    
                Color bg = EditorGUIUtility.isProSkin
                    ? (hover ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                    : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f));
                EditorGUI.DrawRect(r, bg);
                EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), mod.accentColor);
    
                var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
                ts.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                EditorGUI.LabelField(new Rect(r.x+12f, r.y+3f, r.width-30f, r.height-3f), "❖ " + mod.moduleName, ts);
                
                var is2 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
                is2.normal.textColor = ts.normal.textColor;
                EditorGUI.LabelField(new Rect(r.xMax-24f, r.y+4f, 20f, r.height-3f), mod.isExpanded ? "−" : "＋", is2);
            }
    
            private void DrawPropertyDoc(HelpDocProp prop, Color accentColor)
            {
                EditorGUILayout.BeginVertical();
                
                Rect titleRect = GUILayoutUtility.GetRect(new GUIContent(prop.friendlyName), EditorStyles.boldLabel);
                
                if (Event.current.type == EventType.Repaint) {
                    if (scrollToTarget == prop.propName || scrollToTarget == prop.friendlyName) {
                        scrollPos.y = titleRect.y - 50f; 
                        highlightedTarget = prop.propName;
                        highlightStartTime = EditorApplication.timeSinceStartup;
                        scrollToTarget = null;
                        Repaint();
                    }
    
                    if (highlightedTarget == prop.propName) {
                        float t = (float)(EditorApplication.timeSinceStartup - highlightStartTime);
                        if (t < 2.0f) {
                            float alpha = Mathf.Lerp(0.6f, 0f, t / 2.0f);
                            Color pingColor = new Color(1f, 0.8f, 0.2f, alpha);
                            Rect pingRect = new Rect(titleRect.x - 5, titleRect.y - 2, titleRect.width + 10, titleRect.height + 4);
                            EditorGUI.DrawRect(pingRect, pingColor);
                        } else {
                            highlightedTarget = null;
                        }
                    }
                }
    
                GUIStyle propTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, richText = true };
                propTitle.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(accentColor, Color.white, 0.4f) : Color.Lerp(accentColor, Color.black, 0.4f);
                
                GUI.Label(titleRect, $"🔹 {prop.friendlyName} <color=#888888>({prop.propName})</color>", propTitle);
                
                GUIStyle contentStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, margin = new RectOffset(5,0,2,2) };
                
                EditorGUI.indentLevel++;
                GUILayout.Label(prop.desc, contentStyle);
                
                if (!string.IsNullOrEmpty(prop.dependency)) {
                    GUILayout.Label($"<b><color=#D4A017>[依賴 / 共同啟用]</color></b> {prop.dependency}", contentStyle);
                }
                
                if (!string.IsNullOrEmpty(prop.bugFix)) {
                    GUILayout.Label($"<b><color=#D45050>[除錯 / 注意事項]</color></b> {prop.bugFix}", contentStyle);
                }
                EditorGUI.indentLevel--;
                
                GUILayout.Space(10);
                EditorGUILayout.EndVertical();
            }
    
            private void InitDocs()
            {
                docModules = new List<HelpDocModule>();
    
                var modPreset = new HelpDocModule { moduleName = "Preset Save Properties (預設檔儲存與使用)", accentColor = new Color(0.9f, 0.9f, 0.9f), isExpanded = false };
                modPreset.props.Add(new HelpDocProp { 
                    propName = "Preset_Guide", 
                    friendlyName = "Preset Save Properties 使用規範", 
                    desc = "<b>【功能說明】</b>\n允許將單一模塊(例如 Outline 或 Emission)的設定參數獨立儲存成 Profile，以便在不同角色或狀態切換之間共用與覆用。\n\n" +
                           "<b>【使用步驟】</b>\n" +
                           "1. 點擊模塊右上方的資產圖示或「P」，展開 Preset 介面。\n" +
                           "2. 在 <b>Save Properties</b> 下拉選單中，勾選你想存入這個 Preset 的參數（未勾選的參數將不會被此 Preset 影響）。\n" +
                           "3. 點擊 Save，選擇存檔路徑即可建立 資產檔。\n" +
                           "4. 之後在其他材質球上，將該 資產檔 拖曳到 Preset 欄位並點擊 Load 即可套用。\n\n" +
                           "<b>【注意事項】</b>\n" +
                           "• <b>程式控制衝突：</b>如果某個參數（如外框顏色）需要由程式在 Runtime 動態漸變變化，請務必在儲存 Preset 時<b>【取消勾選】</b>該參數，否則 MPB 的控制權會被 Profile 強制覆蓋，導致程式控制無效！\n" +
                           "• <b>Clear 按鈕：</b>點擊 Clear 會解除目前綁定的 Preset，並清除相對應的 MPB 緩存，恢復材質球預設狀態。" +
                           "• <b>模塊限制：</b>如果要開啟某項功能，請在調整材質球時，就必須先將該功能設定為Enable。",
                    dependency = "", bugFix = "" });
                docModules.Add(modPreset);
    
                var modMain = new HelpDocModule { moduleName = "Main & Shadow Settings (基礎與陰影)", accentColor = SpineCustomShaderGUI.ColorMain, isExpanded = false };
                modMain.props.Add(new HelpDocProp { propName = "_MainTex", friendlyName = "Main Texture", 
                    desc = "角色的主貼圖，通常由 Spine 自動指派。", dependency = "", bugFix = "" });
                modMain.props.Add(new HelpDocProp { propName = "_MainTex_ST", friendlyName = "Shared Tiling & Offset", 
                    desc = "共用的紋理縮放與偏移控制器。\n透過修改此數值 (X,Y 為縮放，Z,W 為偏移)，將會同步影響 MainTex (主貼圖)、Emission (自發光)、BackLight (背光遮罩) 以及 Normal Map (法線貼圖)，確保所有疊加效果完美對齊。", dependency = "", bugFix = "" });
                modMain.props.Add(new HelpDocProp { propName = "_Color", friendlyName = "Main Color", 
                    desc = "控制角色的基礎顏色與全域透明度。若 Alpha 值小於 1，會連同外框、內框等所有特效一起變透明。",
                    dependency = "", bugFix = "若發現外框或特效不自然變暗，請檢查此處的 Alpha 是否被誤調小於 1。\n如果發現 Alpha 調整為 0 時出現鬼影，為以下兩個原因造成\n1. Beautify的壓暗功能問題。\n2. 沒有勾選下方Straight Alpha Texture選項。" });
                modMain.props.Add(new HelpDocProp { propName = "_Cutoff", friendlyName = "Shadow Alpha Cutoff", 
                    desc = "決定角色投射陰影的 Alpha 閾值。小於此值的像素不會產生陰影。", dependency = "", bugFix = "" });
                modMain.props.Add(new HelpDocProp { propName = "_StraightAlphaInput", friendlyName = "Straight Alpha Texture", 
                    desc = "切換貼圖透明度計算模式。Spine 預設為預乘 Alpha (PMA)。",
                    dependency = "", bugFix = "如果角色邊緣出現異常黑邊或白邊，請嘗試勾選或取消此項以符合匯出的 Spine 貼圖格式。" });
                modMain.props.Add(new HelpDocProp { propName = "_EnableBloomSuppression", friendlyName = "Suppress Bloom (抑制 Bloom 泛光)", 
                    desc = "防止角色本身的貼圖過亮而觸發後製的 Bloom 泛光效果。",
                    dependency = "需要系統自動掛載的 SpineBloomThresholdSync 腳本。", bugFix = "若功能無效，請檢查角色是否位於正確的 Layer (8, 9, 12, 13, 20)。" });
                modMain.props.Add(new HelpDocProp { propName = "_BloomSuppressThreshold", friendlyName = "Bloom Threshold", 
                    desc = "由系統自動同步的全域閾值，高於此值的亮度會被壓制。", dependency = "開啟 Suppress Bloom 後由腳本自動填入，通常無需手動修改。", bugFix = "" });
                docModules.Add(modMain);
    
                var modNoise = new HelpDocModule { moduleName = "Global Noise Settings (全域噪點遮罩)", accentColor = SpineCustomShaderGUI.ColorNoise, isExpanded = false };
                modNoise.props.Add(new HelpDocProp { propName = "_EnableGlobalNoise", friendlyName = "Enable Global Noise", 
                    desc = "啟用 Shader 全域的噪點運算，可用於製作消融 (Dissolve) 或動態火焰外框等效果。",
                    dependency = "開啟此項後，必須前往 BackLight, Inline 或 Outline 模塊中開啟對應的 Enable Noise Mask 才會看到效果。", bugFix = "" });
                modNoise.props.Add(new HelpDocProp { propName = "_GlobalNoiseScale", friendlyName = "Noise Scale (X,Y)", 
                    desc = "設定噪點的 XY 縮放大小，數值越小噪點越大。", dependency = "", bugFix = "" });
                modNoise.props.Add(new HelpDocProp { propName = "_GlobalNoiseSpeed", friendlyName = "Noise Speed (X,Y)", 
                    desc = "設定噪點的 XY 流動速度。", dependency = "", bugFix = "若感覺特效沒有在流動，請將此數值調大。" });
                docModules.Add(modNoise);
    
                var modLight = new HelpDocModule { moduleName = "Lighting Settings (光照系統)", accentColor = SpineCustomShaderGUI.ColorLighting, isExpanded = false };
                modLight.props.Add(new HelpDocProp { propName = "_EnableNativeLighting", friendlyName = "Enable Native Lighting (原生光照)", 
                    desc = "讓角色接收 Unity 場景中原生的 Directional / Point / Spot 燈光影響。",
                    dependency = "", bugFix = "若角色全黑，請確認場景中是否有打燈，或是 Master Light Intensity 被調為 0。" });
                modLight.props.Add(new HelpDocProp { propName = "_NativeLightIntensity", friendlyName = "Master Light Intensity", 
                    desc = "全域控制所有 Unity 燈光對角色造成的影響強度。", dependency = "需開啟 Enable Native Lighting。", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_DirectionalLightIntensity", friendlyName = "Directional Intensity", 
                    desc = "單獨控制「方向光 (太陽光)」的受光強度。", dependency = "需開啟 Enable Native Lighting。", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_AdditionalLightIntensity", friendlyName = "Additional Intensity", 
                    desc = "單獨控制「點光源 / 聚光燈」的受光強度。", dependency = "需開啟 Enable Native Lighting。", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_LightAffectsAdditive", friendlyName = "Light Affects Additive", 
                    desc = "勾選後，Addtive 疊加模式的部位也會受到光照變暗/變亮的影響。", dependency = "", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_LightProbeNativeBlend", friendlyName = "Probe → Native Lighting", 
                    desc = "將 Light Probe 收集到的環境光，融合進原生的受光計算中，使暗部不至於死黑。", dependency = "需先開啟全域的 Enable Light Probe。", bugFix = "" });
                
                modLight.props.Add(new HelpDocProp { propName = "_EnableShadowLight", friendlyName = "Enable Shadow Light Base", 
                    desc = "讓角色接收自定義的 Spine Shadow Light 特效燈。",
                    dependency = "場景中必須存在掛載了 SpineShadowLight 腳本的光源物件。", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_ShadowLightBaseIntensity", friendlyName = "Base Intensity", 
                    desc = "單獨控制人物本體 (Base Body) 受到陰影燈影響的強度倍率。", dependency = "需開啟 Enable Shadow Light Base。", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_SL_AffectsNormalMap", friendlyName = "Affects Normal Map", 
                    desc = "讓 Shadow Light 也會根據法線凹凸圖產生立體陰影。", dependency = "需開啟 Enable Shadow Light 與 Enable Normal Map Module。", bugFix = "" });
                
                modLight.props.Add(new HelpDocProp { propName = "_EnableNormalMap", friendlyName = "Enable Normal Map Module", 
                    desc = "賦予角色法線凹凸細節，讓光照更有立體感。",
                    dependency = "需提供法線貼圖，並啟用對應光源。", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_InvertNormalMap", friendlyName = "Invert Normal Height", 
                    desc = "反轉法線的凹凸方向。", dependency = "需開啟 Normal Map Module。", bugFix = "若凹凸方向與預期相反(凸變凹)，請勾選此項。" });
                modLight.props.Add(new HelpDocProp { propName = "_NormalMap", friendlyName = "Normal Map", 
                    desc = "角色的法線貼圖檔案。\n\n<b>【自動填入規範】</b>\n若法線貼圖與 MainTex 放置在相同資料夾，且命名格式為「<b>主貼圖名稱_Normal</b>」(例如主貼圖為 <i>Char_A.png</i>，此貼圖為 <i>Char_A_Normal.png</i>)，系統將會在材質選取時自動帶入此欄位中。", dependency = "", bugFix = "" });
                modLight.props.Add(new HelpDocProp { propName = "_NormalIntensity", friendlyName = "Normal Intensity", 
                    desc = "法線凹凸的強度倍率。", dependency = "", bugFix = "" });
    
                modLight.props.Add(new HelpDocProp { propName = "_EnableLightProbe", friendlyName = "Enable Light Probe", 
                    desc = "讓角色受到烘焙環境光 (Light Probes) 的影響，若當前場景開啟LightMap及LightProbe時，此選項將會自動開啟。",
                    dependency = "需要場景有烘焙 Light Probe，並由 SpineLightManager 採集。", bugFix = "如果按鈕一直跳回關閉，代表場景中缺乏 Lightmap 或 Light Probe 資料。" });
                modLight.props.Add(new HelpDocProp { propName = "_LightProbeIntensity", friendlyName = "Probe Intensity", 
                    desc = "控制 Light Probe 環境光的影響強度。", dependency = "需開啟 Enable Light Probe。", bugFix = "" });
                docModules.Add(modLight);
    
                var modEmission = new HelpDocModule { moduleName = "Emission Settings (自發光)", accentColor = SpineCustomShaderGUI.ColorEmission, isExpanded = false };
                modEmission.props.Add(new HelpDocProp { propName = "_EnableEmission", friendlyName = "Enable Emission", desc = "開啟角色局部的自發光效果。", dependency = "", bugFix = "" });
                modEmission.props.Add(new HelpDocProp { propName = "_EmissionTex", friendlyName = "Emission Texture", 
                    desc = "指定要發光的部位遮罩 (黑白或彩色圖)。\n\n<b>【自動填入規範】</b>\n若發光圖與 MainTex 放置在相同資料夾，且命名格式為「<b>主貼圖名稱_Emission</b>」(例如主貼圖為 <i>Char_A.png</i>，此貼圖為 <i>Char_A_Emission.png</i>)，系統將自動帶入此欄位。", dependency = "", bugFix = "" });
                modEmission.props.Add(new HelpDocProp { propName = "_EmissionColor", friendlyName = "Emission Color", desc = "疊加的發光顏色 (支援 HDR)。", dependency = "", bugFix = "" });
                modEmission.props.Add(new HelpDocProp { propName = "_EmissionBlend", friendlyName = "Blend Weight", desc = "發光效果與原本顏色的混合權重。", dependency = "", bugFix = "" });
                modEmission.props.Add(new HelpDocProp { propName = "_EmissionIntensity", friendlyName = "Intensity", desc = "自發光的強度倍率。", dependency = "", bugFix = "" });
                modEmission.props.Add(new HelpDocProp { propName = "_EmissionMulMain", friendlyName = "Multiply With MainTex", desc = "發光顏色是否要與角色原圖顏色相乘。", dependency = "", bugFix = "" });
                docModules.Add(modEmission);
    
                var modEffect = new HelpDocModule { moduleName = "Effect Overlay & BackLight (覆蓋特效與背光)", accentColor = SpineCustomShaderGUI.ColorEffect, isExpanded = false };
                modEffect.props.Add(new HelpDocProp { propName = "_EffectColor", friendlyName = "Effect Color (HDR)", desc = "全域疊加在角色上的純色，常用於受擊閃白或閃紅。", dependency = "", bugFix = "【程式依賴】若是供程式 Runtime 呼叫，請在 Save Profile 時剔除此項。" });
                modEffect.props.Add(new HelpDocProp { propName = "_EffectIntensity", friendlyName = "Effect Intensity", desc = "全域覆蓋色的強度，0 為不顯示，1 為完全覆蓋。", dependency = "", bugFix = "" });
                
                modEffect.props.Add(new HelpDocProp { propName = "_EnableHitSweep", friendlyName = "Enable Hit Sweep (掃光特效)", 
                    desc = "在角色身上產生一條動態掃過的光帶，常用於出場、升級或受擊提示。",
                    dependency = "需由 SpineHitSweepEffect 腳本自動提供座標，並由程式控制 Sweep Progress (-1 到 1)。", 
                    bugFix = "若掃光位置異常，請確保角色有對應的 Layer 讓腳本自動掛載。" });
                modEffect.props.Add(new HelpDocProp { propName = "_SweepColor", friendlyName = "Sweep Color (HDR)", desc = "掃光光帶的顏色。", dependency = "需開啟 Enable Hit Sweep。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_SweepWidth", friendlyName = "Sweep Width", desc = "光帶的寬度比例。", dependency = "需開啟 Enable Hit Sweep。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_SweepSoftness", friendlyName = "Sweep Softness", desc = "光帶邊緣的柔和漸層程度。", dependency = "需開啟 Enable Hit Sweep。", bugFix = "" });
    
                modEffect.props.Add(new HelpDocProp { propName = "_EnableBackLight", friendlyName = "Enable BackLight (邊緣背光/高光)", 
                    desc = "為角色繪製側面的環境反光或金屬質感光澤。", dependency = "可於 Light Source 欄位選擇依賴的光源類型。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BackLightLightingMode", friendlyName = "Light Source (光照來源)", 
                    desc = "決定哪種光源(Realtime / Probe / ShadowLight) 可以觸發背光效果。", dependency = "需開啟對應的全局光照設定。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BL_SL_IgnoreBehind", friendlyName = "Ignore Behind Fade (Shadow Light)", 
                    desc = "勾選後，當 Shadow Light 位於角色背後時將忽略漸隱衰減，強制在背面也產生背光效果。", dependency = "Light Source 需包含 ShadowLight。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_SpecularTex", friendlyName = "BackLight Texture", 
                    desc = "背光遮罩圖，決定角色哪裡會有高光。\n\n<b>【自動填入規範】</b>\n若遮罩圖與 MainTex 放置在相同資料夾，且命名格式為「<b>主貼圖名稱_BackLight</b>」(例如主貼圖為 <i>Char_A.png</i>，此貼圖為 <i>Char_A_BackLight.png</i>)，系統將自動帶入此欄位。", dependency = "", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_SpecularColor", friendlyName = "Specular Color", desc = "背光的基礎顏色。", dependency = "", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BackLightMode", friendlyName = "Blend Mode", desc = "背光的混合模式，可選 Replace (覆蓋) 或 Additive (疊加)。", dependency = "", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_SpecularBlend", friendlyName = "Blend Intensity", desc = "背光的整體顯示強度。", dependency = "", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_SpecularMulMain", friendlyName = "Multiply With MainTex", desc = "勾選後，背光顏色會混入角色的原圖顏色。", dependency = "", bugFix = "" });
                
                modEffect.props.Add(new HelpDocProp { propName = "_EnableDirectionalBackLight", friendlyName = "Mask By Light Direction (方向遮罩)", 
                    desc = "勾選後，只有迎向光源的受光面才會顯示 BackLight，背光面會被自動隱藏。", dependency = "", bugFix = "若勾選後看不到背光，請確認所選光源的方向是否打在角色正前方，導致側面背光面被遮蔽。" });
                modEffect.props.Add(new HelpDocProp { propName = "_BL_RT_Power", friendlyName = "Realtime Focus Power", desc = "控制 Realtime 光源在模型邊緣的光暈集中度 (數值越高越窄)。", dependency = "需啟用 Mask By Light Direction 與 Realtime 光源。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BL_Probe_Power", friendlyName = "Probe Focus Power", desc = "控制 Light Probe 在模型邊緣的光暈集中度。", dependency = "需啟用 Mask By Light Direction 與 Probe 光源。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BL_Shadow_Power", friendlyName = "ShadowLight Focus Power", desc = "控制 Shadow Light 在模型邊緣的光暈集中度。", dependency = "需啟用 Mask By Light Direction 與 ShadowLight 光源。", bugFix = "" });
    
                modEffect.props.Add(new HelpDocProp { propName = "_BackLightTintByLight", friendlyName = "Tint By Light Color (光源染色)", 
                    desc = "勾選後，背光的顏色會與場景中打過來的燈光顏色混合。", dependency = "", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BL_RT_Blend", friendlyName = "Realtime Color Blend (染色權重)", desc = "控制 Realtime 光源顏色的染入程度。", dependency = "需啟用 Tint By Light Color。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BL_RT_Intensity", friendlyName = "↳ Intensity", desc = "單獨放大 Realtime 染色的亮度倍率。", dependency = "需啟用 Tint By Light Color。", bugFix = "" });
                
                modEffect.props.Add(new HelpDocProp { propName = "_BL_EnableNoise", friendlyName = "Enable Noise Mask", desc = "在背光上疊加噪點遮罩，產生殘破或流動感。", dependency = "需先開啟 Global Noise Settings。", bugFix = "" });
                modEffect.props.Add(new HelpDocProp { propName = "_BL_NoiseIntensity", friendlyName = "Noise Intensity", desc = "噪點侵蝕背光的強度。", dependency = "需開啟 Enable Noise Mask。", bugFix = "" });
                docModules.Add(modEffect);
    
                var modLine = new HelpDocModule { moduleName = "Line Settings (內外框線)", accentColor = SpineCustomShaderGUI.ColorLine, isExpanded = false };
                
                modLine.props.Add(new HelpDocProp { propName = "_EnableInline", friendlyName = "Enable Inline (內邊光 / RimLight)", 
                    desc = "在角色邊緣向「內」繪製的柔和邊緣光。",
                    dependency = "【極度重要】需要 SpineAlphaMaskRenderer 腳本即時擷取輪廓遮罩才能運作。", 
                    bugFix = "如果內框沒有出現，或形狀異常，請 100% 確認角色在 Layer 8, 9, 12, 13, 20！" });
                modLine.props.Add(new HelpDocProp { propName = "_InlineLightingMode", friendlyName = "Light Source (光照來源)", desc = "決定哪些光源會影響內邊光的產生。", dependency = "需開啟對應的全局光照。", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_IL_SL_IgnoreBehind", friendlyName = "Ignore Behind Fade (Shadow Light)", 
                    desc = "勾選後，當 Shadow Light 位於角色背後時將忽略漸隱衰減，強制在背面也產生內框光。", dependency = "Light Source 需包含 ShadowLight。", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_InlineColor", friendlyName = "Inline Color (HDR)", desc = "內邊光的基礎顏色。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_InlineWidth", friendlyName = "Inline Width", desc = "內邊光的厚度。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_InlineFadeSteps", friendlyName = "Fade Steps", desc = "控制邊緣光向內衰減的層次感（數值越高越柔和）。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_InlineZTest", friendlyName = "Inline ZTest", desc = "深度測試模式。Always 可讓內框穿透其他物件顯示。", dependency = "", bugFix = "" });
    
                modLine.props.Add(new HelpDocProp { propName = "_EnableDirectionalInline", friendlyName = "Mask By Light Direction (方向遮罩)", desc = "使內框光只出現在迎光面。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_IL_RT_Power", friendlyName = "RT / Probe / Shadow Focus Power", desc = "分別控制各類光源在內框邊緣的集中程度。", dependency = "需啟用 Mask By Light Direction。", bugFix = "" });
                
                modLine.props.Add(new HelpDocProp { propName = "_InlineTintByLight", friendlyName = "Tint By Light Color (光源染色)", desc = "讓內框顏色混入場景燈光的顏色。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_IL_RT_Blend", friendlyName = "RT / Probe / Shadow Color Blend & Intensity", desc = "分別控制各類光源顏色的染入程度與亮度倍率。", dependency = "需啟用 Tint By Light Color。", bugFix = "" });
                
                modLine.props.Add(new HelpDocProp { propName = "_IL_EnableNoise", friendlyName = "Enable Noise Mask", desc = "用噪點侵蝕內邊光。", dependency = "需先開啟 Global Noise Settings。", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_IL_NoiseIntensity", friendlyName = "Noise Intensity", desc = "噪點侵蝕的強度。", dependency = "需開啟 Enable Noise Mask。", bugFix = "" });
    
                modLine.props.Add(new HelpDocProp { propName = "_EnableOutline", friendlyName = "Enable Outline (外框線)", 
                    desc = "在角色邊緣向「外」長出硬邊描邊。", dependency = "", bugFix = "外框太粗/太細可開啟 Advanced Options 中的 Screen Space Width。" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineLightingMode", friendlyName = "Light Source (光照來源)", desc = "決定外框是否受光源影響。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OL_SL_IgnoreBehind", friendlyName = "Ignore Behind Fade (Shadow Light)", 
                    desc = "勾選後，當 Shadow Light 位於角色背後時將忽略漸隱衰減，強制在背面也產生外框光。", dependency = "Light Source 需包含 ShadowLight。", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineWidth", friendlyName = "Outline Width", desc = "外框的厚度。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineColor", friendlyName = "Outline Color (HDR)", desc = "外框的基礎顏色。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_MultiplyEdgeColor", friendlyName = "Multiply Edge Color", desc = "將外框顏色與角色原圖邊緣的顏色相乘。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_ThresholdEnd", friendlyName = "Outline Smoothness", desc = "外框邊緣的抗鋸齒平滑度。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineAlphaCutoff", friendlyName = "Alpha Cutoff", desc = "決定何種透明度的像素才要長出外框。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineZTest", friendlyName = "Outline ZTest", desc = "深度測試模式。Always 可讓外框穿透遮擋物顯示。", dependency = "", bugFix = "" });
    
                modLine.props.Add(new HelpDocProp { propName = "_EnableDirectionalOutline", friendlyName = "Mask By Light Direction (方向遮罩)", desc = "使外框只出現在迎光面。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OL_RT_Power", friendlyName = "RT / Probe / Shadow Focus Power", desc = "分別控制各類光源在外框邊緣的集中程度。", dependency = "需啟用 Mask By Light Direction。", bugFix = "" });
                
                modLine.props.Add(new HelpDocProp { propName = "_OutlineTintByLight", friendlyName = "Tint By Light Color (光源染色)", desc = "讓外框顏色混入場景燈光的顏色。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OL_RT_Blend", friendlyName = "RT / Probe / Shadow Color Blend & Intensity", desc = "分別控制各類光源顏色的染入程度與亮度倍率。", dependency = "需啟用 Tint By Light Color。", bugFix = "" });
                
                modLine.props.Add(new HelpDocProp { propName = "_OL_EnableNoise", friendlyName = "Enable Noise Mask", desc = "用噪點侵蝕外框。", dependency = "需先開啟 Global Noise Settings。", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OL_NoiseIntensity", friendlyName = "Noise Intensity", desc = "噪點侵蝕的強度。", dependency = "需開啟 Enable Noise Mask。", bugFix = "" });
    
                modLine.props.Add(new HelpDocProp { propName = "_UseScreenSpaceOutlineWidth", friendlyName = "Screen Space Width (螢幕空間寬度)", 
                    desc = "勾選後，外框寬度會依據「螢幕佔比」固定，不管攝影機拉多遠，外框看起來都一樣粗。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_Fill", friendlyName = "Fill Inside (外框填滿內部)", 
                    desc = "將外框顏色直接填滿整個角色內部，常用於「受擊時閃白/紅」的效果。", dependency = "需開啟 Enable Outline。", bugFix = "【程式依賴】若是供程式 Runtime 呼叫的特效，請在儲存 Preset 時將此選項「取消勾選」以防覆寫衝突。" });
                modLine.props.Add(new HelpDocProp { propName = "_Use8Neighbourhood", friendlyName = "Sample 8 Neighbours", desc = "使用高精度的 8 向採樣計算外框，能使外框更圓滑，但會增加一點效能開銷。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineReferenceTexWidth", friendlyName = "Reference Texture Width", desc = "外框計算的參考貼圖寬度，可調整邊緣膨脹的精準度。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineOpaqueAlpha", friendlyName = "Opaque Alpha Threshold", desc = "設定何種 Alpha 視為完全不透明的主體。", dependency = "", bugFix = "" });
                modLine.props.Add(new HelpDocProp { propName = "_OutlineMipLevel", friendlyName = "Mip Level", desc = "讀取貼圖的 Mipmap 層級，越高外框越柔焦。", dependency = "", bugFix = "" });
    
                docModules.Add(modLine);
            }
        }

    // ─────────────────────────────────────────────────────────────
    // 現代化命名規則視窗 (支援搜尋與複製)
    // ─────────────────────────────────────────────────────────────
    public class SpineNamingRuleWindow : EditorWindow
    {
        private Vector2 scrollPos;
        private string searchQuery = "";

        private class NamingDocModule {
            public string moduleName;
            public string formattedName;
            public Color accentColor;
            public bool isExpanded;
        }

        private List<NamingDocModule> docModules;

        public static void ShowWindow()
        {
            var window = GetWindow<SpineNamingRuleWindow>("預設檔命名規則");
            window.minSize = new Vector2(600, 750);
            window.Show();
        }

        private void OnEnable()
        {
            InitDocs();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            
            Rect searchRect = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            Color bg = EditorGUIUtility.isProSkin ? new Color(.20f, .20f, .20f) : new Color(.85f, .85f, .85f);
            Color searchAccent = new Color(0.2f, 0.8f, 0.6f);
            
            EditorGUI.DrawRect(searchRect, bg);
            EditorGUI.DrawRect(new Rect(searchRect.x, searchRect.y, searchRect.width, 3f), searchAccent);

            GUIContent searchIcon = EditorGUIUtility.IconContent("Search Icon");
            if (searchIcon != null && searchIcon.image != null) {
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.3f, 0.3f, 0.3f);
                GUI.DrawTexture(new Rect(searchRect.x + 12, searchRect.y + 8, 16, 16), searchIcon.image);
                GUI.color = Color.white;
            } else {
                GUI.Label(new Rect(searchRect.x + 12, searchRect.y + 6, 20, 20), "🔍");
            }

            GUIStyle tfStyle = new GUIStyle(GUIStyle.none) {
                margin = new RectOffset(0,0,0,0), padding = new RectOffset(0,0,0,0),
                fontSize = 13, alignment = TextAnchor.MiddleLeft
            };
            tfStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            tfStyle.focused.textColor = tfStyle.normal.textColor;
            tfStyle.active.textColor = tfStyle.normal.textColor;
            tfStyle.hover.textColor = tfStyle.normal.textColor;
            
            Rect textRect = new Rect(searchRect.x + 36, searchRect.y + 3, searchRect.width - 65, searchRect.height - 3);
            
            if (string.IsNullOrEmpty(searchQuery)) {
                GUIStyle placeholderStyle = new GUIStyle(tfStyle) { fontStyle = FontStyle.Italic };
                placeholderStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(textRect, "輸入模塊名稱搜尋...", placeholderStyle);
            }

            searchQuery = GUI.TextField(textRect, searchQuery, tfStyle);
            
            if (!string.IsNullOrEmpty(searchQuery)) {
                if (GUI.Button(new Rect(searchRect.xMax - 26, searchRect.y + 8, 16, 16), EditorGUIUtility.IconContent("Clear"), GUIStyle.none)) {
                    searchQuery = "";
                    GUI.FocusControl(null); 
                }
            }
            
            GUILayout.Space(15);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold, margin = new RectOffset(10, 10, 10, 15) };
            GUILayout.Label("Spine Preset 預設檔命名規則", titleStyle);

            EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
            GUIStyle ruleTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, richText = true, margin = new RectOffset(5, 5, 5, 5) };
            ruleTitle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.4f, 0.8f, 1f) : new Color(0.1f, 0.4f, 0.8f);
            
            GUILayout.Label("🔹 角色特化功能", ruleTitle);
            GUILayout.Label("角色英文代稱_模塊名稱_設定檔序號(由001開始，預設命名使用Index，請自行更改)", new GUIStyle(EditorStyles.label) { wordWrap = true, margin = new RectOffset(15, 5, 2, 5) });
            
            GUILayout.Space(5);
            GUILayout.Label("🔹 通用型功能", ruleTitle);
            GUILayout.Label("General_模塊名稱_該效果實際用途簡稱(例如Toxic,Forzen等等，預設命名為Default)_設定檔序號(由001開始，預設命名使用Index，請自行更改)", new GUIStyle(EditorStyles.label) { wordWrap = true, margin = new RectOffset(15, 5, 2, 5) });
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);

            bool hasSearch = !string.IsNullOrEmpty(searchQuery);
            string queryLower = hasSearch ? searchQuery.ToLower() : "";

            foreach (var mod in docModules)
            {
                bool match = !hasSearch || mod.moduleName.ToLower().Contains(queryLower) || mod.formattedName.ToLower().Contains(queryLower);
                if (!match) continue;

                if (hasSearch) mod.isExpanded = true;

                DrawModuleHeader(mod);

                if (mod.isExpanded)
                {
                    EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
                    GUILayout.Space(5);
                    
                    DrawNamingEntry(mod, "通用型預設", $"General_{mod.formattedName}_Default_Index");
                    GUILayout.Space(5);
                    DrawNamingEntry(mod, "角色特化預設 (範例)", $"CharacterName_{mod.formattedName}_Index");

                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                }
            }

            GUILayout.Space(30);
            GUILayout.EndScrollView();
        }

        private void DrawNamingEntry(NamingDocModule mod, string label, string expectedName)
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle lblStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, margin = new RectOffset(15, 0, 4, 0) };
            GUILayout.Label($"<b>{label}:</b>  {expectedName}", lblStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("複製名稱", EditorStyles.miniButton, GUILayout.Width(80))) {
                EditorGUIUtility.systemCopyBuffer = expectedName;
                Debug.Log($"[SpineShaderGUI] 已複製名稱：{expectedName}");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawModuleHeader(NamingDocModule mod)
        {
            Rect r = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            Event e = Event.current; bool hover = r.Contains(e.mousePosition);
            if (e.type == EventType.MouseDown && e.button == 0 && hover) { mod.isExpanded = !mod.isExpanded; e.Use(); }

            Color bg = EditorGUIUtility.isProSkin
                ? (hover ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f));
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), mod.accentColor);

            var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
            ts.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            EditorGUI.LabelField(new Rect(r.x+12f, r.y+3f, r.width-30f, r.height-3f), "❖ " + mod.moduleName, ts);
            
            var is2 = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            is2.normal.textColor = ts.normal.textColor;
            EditorGUI.LabelField(new Rect(r.xMax-24f, r.y+4f, 20f, r.height-3f), mod.isExpanded ? "−" : "＋", is2);
        }

        private void InitDocs()
        {
            docModules = new List<NamingDocModule>();
            
            void AddMod(string name, Color c) {
                docModules.Add(new NamingDocModule { 
                    moduleName = name, 
                    formattedName = SpineCustomShaderGUI.GetFormattedModuleName(name), 
                    accentColor = c, 
                    isExpanded = false 
                });
            }

            AddMod("Main & Shadow Settings", SpineCustomShaderGUI.ColorMain);
            AddMod("Post-Processing Control", SpineCustomShaderGUI.ColorMain);
            AddMod("Global Noise Settings", SpineCustomShaderGUI.ColorNoise);
            AddMod("Native Lighting Settings", SpineCustomShaderGUI.ColorLighting);
            AddMod("Shadow Light Base Settings", SpineCustomShaderGUI.ColorLighting);
            AddMod("Normal Map Settings", SpineCustomShaderGUI.ColorLighting);
            AddMod("Light Probe Settings", SpineCustomShaderGUI.ColorLighting);
            AddMod("Emission Settings", SpineCustomShaderGUI.ColorEmission);
            AddMod("Effect Overlay", SpineCustomShaderGUI.ColorEffect);
            AddMod("Hit Sweep Settings", SpineCustomShaderGUI.ColorEffect);
            AddMod("BackLight Settings", SpineCustomShaderGUI.ColorEffect);
            AddMod("Inline Settings", SpineCustomShaderGUI.ColorLine);
            AddMod("Outline Settings", SpineCustomShaderGUI.ColorLine);
        }
    }
}
