using UnityEngine;
using System.Collections.Generic;

namespace VFXTool.SpineSkeletonShaderTool
{
    [RequireComponent(typeof(Renderer))]
    [DisallowMultipleComponent]
    public class SpineEffectBootstrapper : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────────────
        #region 靜態查詢表
        // ─────────────────────────────────────────────────────────────────────

        private const int LM_LIGHTPROBE = 1 << 1; 

        private static readonly HashSet<string> s_ProfileIgnored = new HashSet<string>
        {
            "_EnableInline", "_EnableHitSweep", "_EnableBloomSuppression",
            "_EnableNormalMap", "_EnableBackLight", "_EnableOutline",
            "_EnableLightProbe", "_EnableShadowLight", "_EnableGlobalNoise",
            "_BackLightLightingMode", "_InlineLightingMode", "_OutlineLightingMode"
        };

        private static readonly string[] s_InlineSig     = { "_InlineColor", "_InlineWidth", "_InlineFadeSteps" };
        private static readonly string[] s_SweepSig      = { "_SweepColor", "_SweepWidth", "_SweepSoftness" };
        private static readonly string[] s_BloomSig      = { "_BloomSuppressThreshold" };
        private static readonly string[] s_NormalMapSig  = { "_NormalMap", "_NormalIntensity", "_InvertNormalMap" };
        private static readonly string[] s_LightProbeSig = { "_LightProbeIntensity" };
        private static readonly string[] s_ShadowLightSig= { "_SL_AffectsNormalMap" };
        private static readonly string[] s_BackLightSig  = { "_SpecularColor", "_SpecularBlend", "_BackLightMode", "_SpecularMulMain" };
        private static readonly string[] s_OutlineSig    = { "_OutlineColor", "_OutlineWidth", "_ThresholdEnd", "_OutlineAlphaCutoff" };

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region 欄位
        // ─────────────────────────────────────────────────────────────────────

        private Renderer             _renderer;
        private SpineProfileAnimator _animator;

        private bool _pendingEvaluate;
        private bool _initialized;

        private bool _lastInline;
        private bool _lastSweep;
        private bool _lastLight;
        private bool _lastBloom;

        private Behaviour _compAlphaMask;
        private Behaviour _compHitSweep;
        private Behaviour _compLightReceiver;
        private Behaviour _compBloomSync;

        // 🌟 新增：用來儲存由 SpineProfileHelper 在 Runtime 期間強制寫入 MPB 的 Override 變數
        private Dictionary<string, float> _runtimeOverrides = new Dictionary<string, float>();

        private readonly HashSet<string> _autoFixedProps = new HashSet<string>();

        private System.Type _tAlphaMask;
        private System.Type _tHitSweep;
        private System.Type _tLightReceiver;
        private System.Type _tBloomSync;
        private System.Type _tLightManager;

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Unity 生命週期
        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _animator  = GetComponent<SpineProfileAnimator>();

            CacheTypes();
            CacheExistingComponents();

            _initialized = true;
        }

        private void Start()
        {
            Evaluate(force: true);
        }

        private void Update()
        {
            if (!_pendingEvaluate) return;
            _pendingEvaluate = false;
            Evaluate(force: false);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region 公開 API
        // ─────────────────────────────────────────────────────────────────────

        public void OnProfilesChanged()
        {
            if (!_initialized) return;
            _pendingEvaluate = true;
        }

        public void ForceEvaluate()
        {
            Evaluate(force: true);
        }

        // 🌟 新增：寫入與清除 Override，並標記需要更新腳本
        public void SetRuntimeOverride(string propName, float value)
        {
            _runtimeOverrides[propName] = value;
            _pendingEvaluate = true;
        }

        public void ClearRuntimeOverrides()
        {
            _runtimeOverrides.Clear();
            _pendingEvaluate = true;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region 核心評估流程
        // ─────────────────────────────────────────────────────────────────────

        private void Evaluate(bool force)
        {
            if (_renderer == null) return;

            if (_animator == null)
                _animator = GetComponent<SpineProfileAnimator>();

            List<SpineModuleProfile> profiles = _animator?.profiles;
            Material mat = _renderer.sharedMaterial;

            RunAutoFix(profiles, mat);

            bool wantInline = ReadBool(profiles, mat, "_EnableInline");
            bool wantSweep  = ReadBool(profiles, mat, "_EnableHitSweep");
            bool wantBloom  = ReadBool(profiles, mat, "_EnableBloomSuppression");
            bool wantLight  = CalcNeedsLightData(profiles, mat);

            if (!force
                && wantInline == _lastInline
                && wantSweep  == _lastSweep
                && wantLight  == _lastLight
                && wantBloom  == _lastBloom) return;

            SyncBehaviour(ref _compAlphaMask,    _tAlphaMask,     wantInline, "SpineAlphaMaskRenderer",  "_EnableInline");
            SyncBehaviour(ref _compHitSweep,     _tHitSweep,      wantSweep,  "SpineHitSweepEffect",     "_EnableHitSweep");
            SyncBehaviour(ref _compLightReceiver,_tLightReceiver, wantLight,  "SpineLightReceiver",      "光照相關功能");
            SyncBehaviour(ref _compBloomSync,    _tBloomSync,     wantBloom,  "SpineBloomThresholdSync", "_EnableBloomSuppression");

            if (wantLight) EnsureLightManager();

            _lastInline = wantInline;
            _lastSweep  = wantSweep;
            _lastLight  = wantLight;
            _lastBloom  = wantBloom;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region 腳本同步
        // ─────────────────────────────────────────────────────────────────────

        private void SyncBehaviour(
            ref Behaviour  cachedComp,
            System.Type    scriptType,
            bool           isEnabled,
            string         scriptName,
            string         relatedProp)
        {
            if (scriptType == null) return;

            if (cachedComp != null && (cachedComp as Object) == null)
                cachedComp = null;

            if (cachedComp == null)
                cachedComp = GetComponent(scriptType) as Behaviour;

            if (cachedComp != null)
            {
                if (cachedComp.enabled != isEnabled)
                    cachedComp.enabled = isEnabled;
            }
            else if (isEnabled)
            {
                cachedComp = gameObject.AddComponent(scriptType) as Behaviour;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region AutoFix 防呆修復
        // ─────────────────────────────────────────────────────────────────────

        private void RunAutoFix(List<SpineModuleProfile> profiles, Material mat)
        {
            if (profiles == null || profiles.Count == 0) return;
            if (mat == null) return;

            if (ProfileContainsAny(profiles, s_InlineSig)) {
                TryEnableMat(mat, "_EnableInline", "Profile 含有 Inline 參數，但未開啟");
                if ((ReadMatInt(mat, "_InlineLightingMode") & LM_LIGHTPROBE) != 0)
                    TryEnableMat(mat, "_EnableLightProbe", "LightingMode 含 LightProbe 但未開啟");
            }

            if (ProfileContainsAny(profiles, s_SweepSig))
                TryEnableMat(mat, "_EnableHitSweep", "Profile 含有 Hit Sweep 參數，但未開啟");

            if (ProfileContainsAny(profiles, s_BloomSig))
                TryEnableMat(mat, "_EnableBloomSuppression", "Profile 含有 Bloom 參數，但未開啟");

            if (ProfileContainsAny(profiles, s_NormalMapSig))
                TryEnableMat(mat, "_EnableNormalMap", "Profile 含有 Normal Map 參數，但未開啟");

            if (ProfileContainsAny(profiles, s_LightProbeSig))
                TryEnableMat(mat, "_EnableLightProbe", "Profile 含有 Light Probe 參數，但未開啟");

            if (ProfileContainsAny(profiles, s_ShadowLightSig))
                TryEnableMat(mat, "_EnableShadowLight", "Profile 含有 Shadow Light 參數，但未開啟");

            if (ProfileContainsAny(profiles, s_BackLightSig)) {
                TryEnableMat(mat, "_EnableBackLight", "Profile 含有 BackLight 參數，但未開啟");
                if ((ReadMatInt(mat, "_BackLightLightingMode") & LM_LIGHTPROBE) != 0)
                    TryEnableMat(mat, "_EnableLightProbe", "LightingMode 含 LightProbe 但未開啟");
            }

            if (ProfileContainsAny(profiles, s_OutlineSig)) {
                TryEnableMat(mat, "_EnableOutline", "Profile 含有 Outline 參數，但未開啟");
                if ((ReadMatInt(mat, "_OutlineLightingMode") & LM_LIGHTPROBE) != 0)
                    TryEnableMat(mat, "_EnableLightProbe", "LightingMode 含 LightProbe 但未開啟");
            }
        }

        private void TryEnableMat(Material mat, string propName, string reason)
        {
            if (!mat.HasProperty(propName)) return;
            if (mat.GetFloat(propName) > 0.5f) return;
            if (_autoFixedProps.Contains(propName)) return;

            mat.SetFloat(propName, 1f);
            _autoFixedProps.Add(propName);

            Debug.LogWarning(
                $"[SpineEffectBootstrapper] AutoFix on <b>{gameObject.name}</b>\n" +
                $"原因：{reason}\n 已自動將 <b>{propName}</b> 設為啟用。\n", this);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region 數值讀取
        // ─────────────────────────────────────────────────────────────────────

        private bool ReadBool(List<SpineModuleProfile> profiles, Material mat, string propName)
        {
            // 🌟 優先讀取由 SpineProfileHelper 在 Runtime 強制覆寫的值
            if (_runtimeOverrides.TryGetValue(propName, out float overrideVal))
                return overrideVal > 0.5f;

            if (s_ProfileIgnored.Contains(propName))
                return MatGetBool(mat, propName);

            if (profiles != null)
            {
                for (int i = profiles.Count - 1; i >= 0; i--)
                {
                    var p = profiles[i];
                    if (p?.properties == null) continue;
                    var prop = p.properties.Find(x => x.name == propName);
                    if (prop != null) return prop.floatValue > 0.5f;
                }
            }

            return MatGetBool(mat, propName);
        }

        private int ReadInt(List<SpineModuleProfile> profiles, Material mat, string propName)
        {
            // 🌟 優先讀取由 SpineProfileHelper 在 Runtime 強制覆寫的值
            if (_runtimeOverrides.TryGetValue(propName, out float overrideVal))
                return Mathf.RoundToInt(overrideVal);

            if (s_ProfileIgnored.Contains(propName))
                return ReadMatInt(mat, propName);

            if (profiles != null)
            {
                for (int i = profiles.Count - 1; i >= 0; i--)
                {
                    var p = profiles[i];
                    if (p?.properties == null) continue;
                    var prop = p.properties.Find(x => x.name == propName);
                    if (prop != null) return Mathf.RoundToInt(prop.floatValue);
                }
            }

            return ReadMatInt(mat, propName);
        }

        private static bool MatGetBool(Material mat, string propName) =>
            mat != null && mat.HasProperty(propName) && mat.GetFloat(propName) > 0.5f;

        private static int ReadMatInt(Material mat, string propName) =>
            mat != null && mat.HasProperty(propName)
                ? Mathf.RoundToInt(mat.GetFloat(propName))
                : 0;

        private static bool ProfileContainsAny(List<SpineModuleProfile> profiles, string[] propNames)
        {
            foreach (var profile in profiles)
            {
                if (profile?.properties == null) continue;
                foreach (var name in propNames)
                    if (profile.properties.Exists(p => p.name == name))
                        return true;
            }
            return false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region SpineLightReceiver 需求計算
        // ─────────────────────────────────────────────────────────────────────

        private bool CalcNeedsLightData(List<SpineModuleProfile> profiles, Material mat)
        {
            if (ReadBool(profiles, mat, "_EnableNormalMap"))  return true;
            if (ReadBool(profiles, mat, "_EnableLightProbe")) return true;
            if (ReadBool(profiles, mat, "_EnableShadowLight"))return true;

            // 🌟 將 Native Lighting 納入腳本掛載判定，確保外部程式獨立控制時腳本能正確綁定給 LightManager 註冊
            if (ReadBool(profiles, mat, "_EnableNativeLighting")) return true;

            if (ReadBool(profiles, mat, "_EnableBackLight")
                && ReadInt(profiles, mat, "_BackLightLightingMode") != 0) return true;

            if (ReadBool(profiles, mat, "_EnableInline")
                && ReadInt(profiles, mat, "_InlineLightingMode") != 0) return true;

            if (ReadBool(profiles, mat, "_EnableOutline")
                && ReadInt(profiles, mat, "_OutlineLightingMode") != 0) return true;

            return false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region 快取與初始化
        // ─────────────────────────────────────────────────────────────────────

        private void CacheExistingComponents()
        {
            if (_tAlphaMask     != null) _compAlphaMask     = GetComponent(_tAlphaMask)     as Behaviour;
            if (_tHitSweep      != null) _compHitSweep      = GetComponent(_tHitSweep)      as Behaviour;
            if (_tLightReceiver != null) _compLightReceiver = GetComponent(_tLightReceiver) as Behaviour;
            if (_tBloomSync     != null) _compBloomSync     = GetComponent(_tBloomSync)     as Behaviour;
        }

        private void CacheTypes()
        {
            const string asm = "Assembly-CSharp";
            const string ns  = "VFXTool.SpineSkeletonShaderTool";

            _tAlphaMask     = System.Type.GetType($"{ns}.SpineAlphaMaskRenderer, {asm}");
            _tHitSweep      = System.Type.GetType($"{ns}.SpineHitSweepEffect, {asm}");
            _tLightReceiver = System.Type.GetType($"{ns}.SpineLightReceiver, {asm}");
            _tBloomSync     = System.Type.GetType($"{ns}.SpineBloomThresholdSync, {asm}");
            _tLightManager  = System.Type.GetType($"{ns}.SpineLightManager, {asm}");
        }

        private void EnsureLightManager()
        {
            if (_tLightManager == null) return;
            try
            {
                _tLightManager
                    .GetMethod("EnsureInstance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    ?.Invoke(null, null);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SpineEffectBootstrapper] EnsureLightManager 失敗：{ex.Message}");
            }
        }
        #endregion
    }
}