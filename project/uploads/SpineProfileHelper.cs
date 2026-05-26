using UnityEngine;
using System.Collections.Generic;

namespace VFXTool.SpineSkeletonShaderTool
{
    /// <summary>
    /// 定義所有的 Spine Profile 模塊，提供程式人員安全的調用方式，避免字串拼寫錯誤
    /// </summary>
    public enum SpineProfileModule
    {
        MainAndShadow,
        PostProcessing,
        GlobalNoise,
        NativeLighting,
        ShadowLightBase,
        NormalMap,
        LightProbe,
        Emission,
        EffectOverlay,
        HitSweep,
        BackLight,
        Inline,
        Outline
    }

    public static class SpineProfileModuleExtensions
    {
        public static string ToModuleName(this SpineProfileModule module)
        {
            switch (module)
            {
                case SpineProfileModule.MainAndShadow:   return "Main & Shadow Settings";
                case SpineProfileModule.PostProcessing:  return "Post-Processing Control";
                case SpineProfileModule.GlobalNoise:     return "Global Noise Settings";
                case SpineProfileModule.NativeLighting:  return "Native Lighting Settings";
                case SpineProfileModule.ShadowLightBase: return "Shadow Light Base Settings";
                case SpineProfileModule.NormalMap:       return "Normal Map Settings";
                case SpineProfileModule.LightProbe:      return "Light Probe Settings";
                case SpineProfileModule.Emission:        return "Emission Settings";
                case SpineProfileModule.EffectOverlay:   return "Effect Overlay";
                case SpineProfileModule.HitSweep:        return "Hit Sweep Settings";
                case SpineProfileModule.BackLight:       return "BackLight Settings";
                case SpineProfileModule.Inline:          return "Inline Settings";
                case SpineProfileModule.Outline:         return "Outline Settings";
                default: return string.Empty;
            }
        }
    }

    /// <summary>
    /// Spine Profile 系統的統一調用接口。
    /// 提供安全的動態加載、臨時副本(TempProfile)竄改、以及底層 MPB 賦值方法。
    /// </summary>
    public static class SpineProfileHelper
    {
        // 靜態共用的 MPB，避免每次修改參數都產生 GC Allocation
        private static MaterialPropertyBlock _sharedMpb = new MaterialPropertyBlock();

        // 新增會觸發 Bootstrapper 重新掛載/移除輔助腳本的關鍵參數名單
        private static readonly HashSet<string> BootstrapperTriggers = new HashSet<string> {
            "_EnableInline", "_EnableHitSweep", "_EnableBloomSuppression",
            "_EnableNormalMap", "_EnableBackLight", "_EnableOutline",
            "_EnableLightProbe", "_EnableShadowLight", "_EnableNativeLighting",
            "_InlineLightingMode", "_OutlineLightingMode", "_BackLightLightingMode"
        };

        // ─────────────────────────────────────────────────────────────────────
        #region Profile 動態加載與移除
        // ─────────────────────────────────────────────────────────────────────

        public static void ApplyProfileRuntime(GameObject targetObj, SpineModuleProfile newProfile)
        {
            if (targetObj == null) return;
            ApplyProfileRuntime(targetObj.GetComponent<Renderer>(), newProfile);
        }

        public static void ApplyProfileRuntime(Renderer targetRenderer, SpineModuleProfile newProfile)
        {
            if (targetRenderer == null || newProfile == null) return;

            SpineProfileAnimator animator = targetRenderer.GetComponent<SpineProfileAnimator>();
            if (animator == null)
                animator = targetRenderer.gameObject.AddComponent<SpineProfileAnimator>();

            if (animator.profiles == null)
                animator.profiles = new List<SpineModuleProfile>();

            animator.profiles.RemoveAll(p => p != null && p.moduleName == newProfile.moduleName);
            animator.profiles.Add(newProfile);
            animator.Restart();

            NotifyBootstrapper(targetRenderer);
        }

        public static void RemoveProfileRuntime(Renderer targetRenderer, SpineProfileModule module)
        {
            RemoveProfileRuntime(targetRenderer, module.ToModuleName());
        }

        public static void RemoveProfileRuntime(Renderer targetRenderer, string moduleName)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(moduleName)) return;

            SpineProfileAnimator animator = targetRenderer.GetComponent<SpineProfileAnimator>();
            if (animator == null || animator.profiles == null) return;

            int removedCount = animator.profiles.RemoveAll(p => p != null && p.moduleName == moduleName);
            if (removedCount > 0)
            {
                CheckAndCleanupAnimator(animator);
                NotifyBootstrapper(targetRenderer);
            }
        }

        public static void ClearAllProfiles(Renderer targetRenderer)
        {
            if (targetRenderer == null) return;

            SpineProfileAnimator animator = targetRenderer.GetComponent<SpineProfileAnimator>();
            if (animator != null)
            {
                animator.profiles?.Clear();
                animator.ClearAllMPB();
                Object.Destroy(animator);
            }
            else
            {
                targetRenderer.SetPropertyBlock(null);
            }

            // 清除程式造成的 Runtime Override 緩存
            var bootstrapper = targetRenderer.GetComponent<SpineEffectBootstrapper>();
            if (bootstrapper != null) {
                bootstrapper.ClearRuntimeOverrides();
            }

            NotifyBootstrapper(targetRenderer);
        }

        private static void CheckAndCleanupAnimator(SpineProfileAnimator animator)
        {
            if (animator != null && (animator.profiles == null || animator.profiles.Count == 0))
            {
                animator.ClearAllMPB();
                Object.Destroy(animator);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region 臨時副本竄改 (Temp Profile)
        // ─────────────────────────────────────────────────────────────────────

        public static SpineModuleProfile ApplyTempProfile(
            Renderer targetRenderer,
            SpineModuleProfile sourceProfile,
            Dictionary<string, object> overrides)
        {
            if (sourceProfile == null || targetRenderer == null) return null;

            SpineModuleProfile tempProfile = Object.Instantiate(sourceProfile);
            tempProfile.name = sourceProfile.name + "_RuntimeTemp";

            if (overrides != null && overrides.Count > 0)
            {
                foreach (var kvp in overrides)
                {
                    var prop = tempProfile.properties.Find(p => p.name == kvp.Key);
                    if (prop == null) continue;

                    prop.useCurve = false;

                    if      (kvp.Value is float   f) prop.floatValue   = f;
                    else if (kvp.Value is Color   c) prop.colorValue   = c;
                    else if (kvp.Value is Vector4 v) prop.vectorValue  = v;
                    else if (kvp.Value is Texture t) prop.textureValue = t;
                }
            }

            ApplyProfileRuntime(targetRenderer, tempProfile);
            return tempProfile;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region MPB 安全控制
        // ─────────────────────────────────────────────────────────────────────

        public static void SetFloat(Renderer targetRenderer, string propertyName, float value)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName)) return;
            targetRenderer.GetPropertyBlock(_sharedMpb);
            _sharedMpb.SetFloat(propertyName, value);
            targetRenderer.SetPropertyBlock(_sharedMpb);

            // 🌟 核心修正：動態通知 Bootstrapper 有關鍵變數被覆寫了
            NotifyBootstrapperOverride(targetRenderer, propertyName, value);
        }

        public static void SetColor(Renderer targetRenderer, string propertyName, Color value)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName)) return;
            targetRenderer.GetPropertyBlock(_sharedMpb);
            _sharedMpb.SetColor(propertyName, value);
            targetRenderer.SetPropertyBlock(_sharedMpb);
        }

        public static void SetColor(Renderer targetRenderer, string propertyName, Color value, int index)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName))
                return;
            targetRenderer.GetPropertyBlock(_sharedMpb);
            _sharedMpb.SetColor(propertyName, value);
            targetRenderer.SetPropertyBlock(_sharedMpb, index);
        }

        public static void SetVector(Renderer targetRenderer, string propertyName, Vector4 value)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName)) return;
            targetRenderer.GetPropertyBlock(_sharedMpb);
            _sharedMpb.SetVector(propertyName, value);
            targetRenderer.SetPropertyBlock(_sharedMpb);
        }

        public static void SetTexture(Renderer targetRenderer, string propertyName, Texture value)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName)) return;
            targetRenderer.GetPropertyBlock(_sharedMpb);
            if (value != null) _sharedMpb.SetTexture(propertyName, value);
            targetRenderer.SetPropertyBlock(_sharedMpb);
        }

        public static float GetFloat(Renderer targetRenderer, string propertyName)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName))
            {
#if !DONT_PRINT_REDUNDANT_MESSAGE
                Debug.Log("發生錯誤");
#endif
                return 0;
            }
            targetRenderer.GetPropertyBlock(_sharedMpb);
            float value = _sharedMpb.GetFloat(propertyName);
            return value;
        }

        public static Color GetColor(Renderer targetRenderer, string propertyName)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName))
            {
#if !DONT_PRINT_REDUNDANT_MESSAGE
                Debug.Log("發生錯誤");
#endif
                return Color.white;
            }
            targetRenderer.GetPropertyBlock(_sharedMpb);
            Color value = _sharedMpb.GetColor(propertyName);
            return value;
        }

        public static Vector4 GetVector(Renderer targetRenderer, string propertyName)
        {
            if (targetRenderer == null || string.IsNullOrEmpty(propertyName))
            {
#if !DONT_PRINT_REDUNDANT_MESSAGE
                Debug.Log("發生錯誤");
#endif
                return Vector4.zero;
            }
            targetRenderer.GetPropertyBlock(_sharedMpb);
            Vector4 value = _sharedMpb.GetVector(propertyName);
            return value;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Bootstrapper 通知
        // ─────────────────────────────────────────────────────────────────────

        private static void NotifyBootstrapper(Renderer targetRenderer)
        {
            if (targetRenderer == null) return;

            var bootstrapper = targetRenderer.GetComponent<SpineEffectBootstrapper>();
            if (bootstrapper == null)
                bootstrapper = targetRenderer.gameObject.AddComponent<SpineEffectBootstrapper>();

            bootstrapper.OnProfilesChanged();
        }

        // 新增針對特定屬性的動態覆寫通知
        private static void NotifyBootstrapperOverride(Renderer targetRenderer, string propertyName, float value)
        {
            if (BootstrapperTriggers.Contains(propertyName))
            {
                var bootstrapper = targetRenderer.GetComponent<SpineEffectBootstrapper>();
                if (bootstrapper == null)
                    bootstrapper = targetRenderer.gameObject.AddComponent<SpineEffectBootstrapper>();

                bootstrapper.SetRuntimeOverride(propertyName, value);
            }
        }

        #endregion
    }
}