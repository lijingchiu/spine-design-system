using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using VFXTool.SpineSkeletonShaderTool;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace VFXTool.SpineSkeletonShaderTool
{
    [ExecuteAlways]
    public class SpineLightManager : MonoBehaviour
    {
        public static SpineLightManager Instance { get; private set; }

        // ─────────────────────────────────────────────────────────────
        //  Shader Property IDs
        // ─────────────────────────────────────────────────────────────
        private static readonly int _SpineLightCount      = Shader.PropertyToID("_SpineLightCount");
        private static readonly int _SpineLightPos        = Shader.PropertyToID("_SpineLightPos");
        private static readonly int _SpineLightColor      = Shader.PropertyToID("_SpineLightColor");
        private static readonly int _SpineLightSpotParams = Shader.PropertyToID("_SpineLightSpotParams");

        private static readonly int _SpineShadowLightCount = Shader.PropertyToID("_SpineShadowLightCount");
        private static readonly int _SpineShadowLightPos   = Shader.PropertyToID("_SpineShadowLightPos");
        private static readonly int _SpineShadowLightColor = Shader.PropertyToID("_SpineShadowLightColor");
        private static readonly int _SpineShadowLightDir   = Shader.PropertyToID("_SpineShadowLightDir");
        private static readonly int _SpineShadowLightSpot  = Shader.PropertyToID("_SpineShadowLightSpot");

        private static readonly int _SpineShadowLightRight   = Shader.PropertyToID("_SpineShadowLightRight");
        private static readonly int _SpineShadowLightUp      = Shader.PropertyToID("_SpineShadowLightUp");
        private static readonly int _SpineShadowCookieParams = Shader.PropertyToID("_SpineShadowCookieParams");
        private static readonly int _SpineShadowParams2      = Shader.PropertyToID("_SpineShadowParams2");

        private static readonly int[] _SpineShadowCookies = new int[8] {
            Shader.PropertyToID("_SpineShadowCookie0"), Shader.PropertyToID("_SpineShadowCookie1"),
            Shader.PropertyToID("_SpineShadowCookie2"), Shader.PropertyToID("_SpineShadowCookie3"),
            Shader.PropertyToID("_SpineShadowCookie4"), Shader.PropertyToID("_SpineShadowCookie5"),
            Shader.PropertyToID("_SpineShadowCookie6"), Shader.PropertyToID("_SpineShadowCookie7")
        };

        private static readonly int _SpineSHAr = Shader.PropertyToID("_SpineSHAr");
        private static readonly int _SpineSHAg = Shader.PropertyToID("_SpineSHAg");
        private static readonly int _SpineSHAb = Shader.PropertyToID("_SpineSHAb");
        private static readonly int _SpineSHBr = Shader.PropertyToID("_SpineSHBr");
        private static readonly int _SpineSHBg = Shader.PropertyToID("_SpineSHBg");
        private static readonly int _SpineSHBb = Shader.PropertyToID("_SpineSHBb");
        private static readonly int _SpineSHC  = Shader.PropertyToID("_SpineSHC");

        [Header("光照搜集篩選")]
        public LayerMask collectLightMask = (1 << 8) | (1 << 9) | (1 << 12) | (1 << 13) | (1 << 18) | (1 << 20);

        [Header("Scene Lights")]
        public List<Light> targetLights = new List<Light>();
        public List<SpineShadowLight> targetShadowLights = new List<SpineShadowLight>();

        [Header("Spine Characters")]
        public List<Renderer> targetCharacters = new List<Renderer>();

        [Header("Light Probe Settings")]
        public bool enableLightProbe = true;
        public Vector3 probeSampleNormal = Vector3.up;

        private MaterialPropertyBlock mpb;
        private readonly HashSet<Light> _lightSet = new HashSet<Light>();
        private readonly HashSet<SpineShadowLight> _shadowSet = new HashSet<SpineShadowLight>();

        private LightDistance[] tempDistances = new LightDistance[64];
        private ShadowDistance[] tempShadowDistances = new ShadowDistance[64];

        private Vector4[] tempPos  = new Vector4[8];
        private Vector4[] tempCol  = new Vector4[8];
        private Vector4[] tempSpot = new Vector4[8];

        private Vector4[] tempShadowPos   = new Vector4[8];
        private Vector4[] tempShadowColor = new Vector4[8];
        private Vector4[] tempShadowDir   = new Vector4[8];
        private Vector4[] tempShadowSpot  = new Vector4[8];
        private Vector4[] tempShadowRight = new Vector4[8];
        private Vector4[] tempShadowUp    = new Vector4[8];
        private Vector4[] tempShadowCookieParams = new Vector4[8];
        private Vector4[] tempShadowParams2 = new Vector4[8];

        private SphericalHarmonicsL2 _sh;
        private float lastFetchTime = 0f;
        private const float FETCH_INTERVAL = 1.0f;

    #if UNITY_EDITOR
        private bool _fetchPending = false;
    #endif

        private struct LightDistance : System.IComparable<LightDistance> {
            public Light light; public float sqrDist;
            public int CompareTo(LightDistance other) => sqrDist.CompareTo(other.sqrDist);
        }

        private struct ShadowDistance : System.IComparable<ShadowDistance> {
            public SpineShadowLight light; public float sqrDist;
            public int CompareTo(ShadowDistance other) => sqrDist.CompareTo(other.sqrDist);
        }

        private struct LightStableComparer : IComparer<LightDistance> {
            public int Compare(LightDistance x, LightDistance y) {
                return x.light.GetInstanceID().CompareTo(y.light.GetInstanceID());
            }
        }

        private struct ShadowStableComparer : IComparer<ShadowDistance> {
            public int Compare(ShadowDistance x, ShadowDistance y) {
                int modeCmp = ((int)x.light.blendMode).CompareTo((int)y.light.blendMode);
                if (modeCmp != 0) return modeCmp;
                return x.light.GetInstanceID().CompareTo(y.light.GetInstanceID());
            }
        }

        private const float HYSTERESIS_ENTER = 2.5f;
        private const float HYSTERESIS_EXIT  = 6.0f;

        private readonly HashSet<long> _activeLightPairs  = new HashSet<long>();
        private readonly HashSet<long> _activeShadowPairs = new HashSet<long>();

        private static long MakePairKey(int rendererID, int lightID)
            => ((long)rendererID << 32) | (uint)lightID;

        private void Reset()
        {
            collectLightMask = (1 << 8) | (1 << 9) | (1 << 12) | (1 << 13) | (1 << 18) | (1 << 20);
            FetchAllLights();
            RegisterAllExistingReceivers();
        }

        public static SpineLightManager EnsureInstance()
        {
            if (Instance != null) return Instance;

    #if UNITY_2023_1_OR_NEWER
            SpineLightManager existing = FindFirstObjectByType<SpineLightManager>(FindObjectsInactive.Include);
    #else
            SpineLightManager existing = FindObjectOfType<SpineLightManager>(true);
    #endif
            if (existing != null)
            {
                if (Instance == null) Instance = existing;
                return existing;
            }

            GameObject go = new GameObject("[SpineLightManager]");
            SpineLightManager mgr = go.AddComponent<SpineLightManager>();

            mgr.collectLightMask = (1 << 8) | (1 << 9) | (1 << 12) | (1 << 13) | (1 << 18) | (1 << 20);

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(go);
            }
    #if UNITY_EDITOR
            else
            {
                Undo.RegisterCreatedObjectUndo(go, "Create SpineLightManager");
            }
    #endif
            mgr.RegisterAllExistingReceivers();

            Debug.Log("[SpineLightManager] 自動建立 SpineLightManager，並已掃描全場景 Receiver。", go);
            return mgr;
        }

        private bool EnforceSingleton()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
                return false;
            }
            Instance = this;
            return true;
        }

        void Awake()
        {
            if (!EnforceSingleton()) return;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            if (!EnforceSingleton()) return;
            mpb = new MaterialPropertyBlock();
            FetchAllLights();
            RegisterAllExistingReceivers();
            
            CheckAndApplyLightProbeSetting();

    #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                SceneManager.sceneLoaded           += OnSceneLoaded;
                EditorSceneManager.sceneOpened     += OnEditorSceneOpened; 
                Undo.postprocessModifications      += OnUndoModifications;
                
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged; 
            }
    #endif
            if (Application.isPlaying) SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            if (Instance == this) Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
    #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
                EditorSceneManager.sceneOpened     -= OnEditorSceneOpened;
                Undo.postprocessModifications      -= OnUndoModifications;
                
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; 
            }
    #endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
        { 
            FetchAllLights(); 
            RegisterAllExistingReceivers(); 
            CheckAndApplyLightProbeSetting();
        }

    #if UNITY_EDITOR
        private void OnEditorSceneOpened(Scene scene, OpenSceneMode mode)
        {
            CheckAndApplyLightProbeSetting();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += () => {
                    if (this != null) 
                    {
                        CheckAndApplyLightProbeSetting();
                    }
                };
            }
        }

        private void OnHierarchyChanged() => FetchAllLights();
        private UndoPropertyModification[] OnUndoModifications(UndoPropertyModification[] mods)
        {
            foreach (var mod in mods) {
                if (mod.currentValue.target is Light || mod.currentValue.target is SpineShadowLight) {
                    if (!_fetchPending) {
                        _fetchPending = true;
                        EditorApplication.delayCall += () => { _fetchPending = false; if (this != null) FetchAllLights(); };
                    }
                    break;
                }
            }
            return mods;
        }
    #endif

        void OnValidate()
        {
    #if UNITY_EDITOR
            if (!Application.isPlaying) {
                if (!_fetchPending) {
                    _fetchPending = true;
                    EditorApplication.delayCall += () => { 
                        _fetchPending = false; 
                        if (this != null) {
                            FetchAllLights(); 
                            RegisterAllExistingReceivers();
                        }
                    };
                }
                return;
            }
    #endif
            FetchAllLights();
            RegisterAllExistingReceivers();
        }

        public void CheckAndApplyLightProbeSetting()
        {
            bool hasLightMap    = LightmapSettings.lightmaps    != null && LightmapSettings.lightmaps.Length > 0;
            bool hasLightProbes = LightmapSettings.lightProbes  != null && LightmapSettings.lightProbes.count > 0;

            bool shouldEnable = hasLightProbes;
            this.enableLightProbe = shouldEnable;

    #if UNITY_2023_1_OR_NEWER
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    #else
            Renderer[] renderers = FindObjectsOfType<Renderer>(true);
    #endif

            foreach (Renderer r in renderers)
            {
                if (r == null) continue;

                Material[] mats = Application.isPlaying ? r.materials : r.sharedMaterials;
                
                foreach (Material mat in mats)
                {
                    if (mat != null && mat.shader != null && mat.shader.name == "Spine/Skeleton")
                    {
                        float currentVal = mat.GetFloat("_EnableLightProbe");
                        float targetVal = shouldEnable ? 1f : 0f;

                        if (!Mathf.Approximately(currentVal, targetVal))
                        {
                            mat.SetFloat("_EnableLightProbe", targetVal);

    #if UNITY_EDITOR
                            if (!Application.isPlaying)
                            {
                                EditorUtility.SetDirty(mat);
                            }
    #endif
                        }
                    }
                }
            }
        }

        public void FetchAllLights()
        {
            int maskInt = (int)collectLightMask;

            _lightSet.Clear();
            _shadowSet.Clear();
            targetLights.Clear();
            targetShadowLights.Clear();

    #if UNITY_2023_1_OR_NEWER
            Light[] allLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            SpineShadowLight[] allShadows = FindObjectsByType<SpineShadowLight>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    #else
            Light[] allLights = FindObjectsOfType<Light>();
            SpineShadowLight[] allShadows = FindObjectsOfType<SpineShadowLight>();
    #endif

            foreach (Light l in allLights) {
                if (l == null) continue;
                if ((l.cullingMask & maskInt) == 0 || l.lightmapBakeType == LightmapBakeType.Baked) continue;
                if (_lightSet.Add(l)) targetLights.Add(l);
            }

            foreach (SpineShadowLight sl in allShadows) {
                if (sl == null) continue;
                if ((sl.cullingMask & maskInt) == 0) continue;
                if (_shadowSet.Add(sl)) targetShadowLights.Add(sl);
            }
        }

        private void RegisterAllExistingReceivers()
        {
            // 優先踢除已經失效或是 Layer 被改掉而不再符合 CullingMask 條件的舊物件
            for (int i = targetCharacters.Count - 1; i >= 0; i--)
            {
                Renderer r = targetCharacters[i];
                if (r == null || ((1 << r.gameObject.layer) & collectLightMask.value) == 0)
                {
                    targetCharacters.RemoveAt(i);
                }
            }

    #if UNITY_2023_1_OR_NEWER
            // FindObjectsByType 預設會跨 Scene 搜尋，能夠抓到 DontDestroyOnLoad 中的活躍物件
            SpineLightReceiver[] receivers = FindObjectsByType<SpineLightReceiver>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    #else
            SpineLightReceiver[] receivers = FindObjectsOfType<SpineLightReceiver>();
    #endif
            foreach (var r in receivers)
            {
                if (r == null) continue;
                Renderer rend = r.GetComponent<Renderer>();
                RegisterCharacter(rend);
            }
        }

        public void RegisterCharacter(Renderer r) 
        { 
            if (r != null && !targetCharacters.Contains(r)) 
            {
                if (((1 << r.gameObject.layer) & collectLightMask.value) != 0) 
                {
                    targetCharacters.Add(r); 
                }
            } 
        }
        
        public void UnregisterCharacter(Renderer r) { if (r != null) targetCharacters.Remove(r); }

        public void GetActiveLightsForRenderer(Renderer r,
            System.Collections.Generic.List<Light> outLights,
            System.Collections.Generic.List<SpineShadowLight> outShadowLights)
        {
            outLights.Clear();
            outShadowLights.Clear();
            if (r == null) return;

            int rendID = r.GetInstanceID();
            foreach (Light l in targetLights)
            {
                if (l == null) continue;
                long key = MakePairKey(rendID, l.GetInstanceID());
                if (_activeLightPairs.Contains(key))
                    outLights.Add(l);
            }
            foreach (SpineShadowLight sl in targetShadowLights)
            {
                if (sl == null) continue;
                long key = MakePairKey(rendID, sl.GetInstanceID());
                if (_activeShadowPairs.Contains(key))
                    outShadowLights.Add(sl);
            }
        }

        private void InjectLightProbe(Vector3 worldPos)
        {
            if (LightmapSettings.lightProbes == null || LightmapSettings.lightProbes.count == 0) {
                mpb.SetVector(_SpineSHAr, Vector4.zero); mpb.SetVector(_SpineSHAg, Vector4.zero); mpb.SetVector(_SpineSHAb, Vector4.zero);
                mpb.SetVector(_SpineSHBr, Vector4.zero); mpb.SetVector(_SpineSHBg, Vector4.zero); mpb.SetVector(_SpineSHBb, Vector4.zero);
                mpb.SetVector(_SpineSHC,  Vector4.zero); return;
            }

            LightProbes.GetInterpolatedProbe(worldPos, null, out _sh);

            Vector4 shar = new Vector4(_sh[0, 3], _sh[0, 1], _sh[0, 2], _sh[0, 0]);
            Vector4 shag = new Vector4(_sh[1, 3], _sh[1, 1], _sh[1, 2], _sh[1, 0]);
            Vector4 shab = new Vector4(_sh[2, 3], _sh[2, 1], _sh[2, 2], _sh[2, 0]);

            Vector4 shbr = new Vector4(_sh[0, 4], _sh[0, 5], _sh[0, 6], _sh[0, 7]);
            Vector4 shbg = new Vector4(_sh[1, 4], _sh[1, 5], _sh[1, 6], _sh[1, 7]);
            Vector4 shbb = new Vector4(_sh[2, 4], _sh[2, 5], _sh[2, 6], _sh[2, 7]);

            Vector4 shc  = new Vector4(_sh[0, 8], _sh[1, 8], _sh[2, 8], 1.0f);

            mpb.SetVector(_SpineSHAr, shar); mpb.SetVector(_SpineSHAg, shag); mpb.SetVector(_SpineSHAb, shab);
            mpb.SetVector(_SpineSHBr, shbr); mpb.SetVector(_SpineSHBg, shbg); mpb.SetVector(_SpineSHBb, shbb);
            mpb.SetVector(_SpineSHC,  shc);
        }

        void LateUpdate()
        {
            if (Instance != this) return;

            if (Time.realtimeSinceStartup - lastFetchTime > FETCH_INTERVAL) { FetchAllLights(); lastFetchTime = Time.realtimeSinceStartup; }

            if (targetCharacters.Count == 0) return;

            for (int i = targetCharacters.Count - 1; i >= 0; i--)
                if (targetCharacters[i] == null) targetCharacters.RemoveAt(i);

            int totalLightCount = targetLights.Count;
            if (totalLightCount > 0 && tempDistances.Length < totalLightCount) tempDistances = new LightDistance[totalLightCount];

            int totalShadowCount = targetShadowLights.Count;
            if (totalShadowCount > 0 && tempShadowDistances.Length < totalShadowCount) tempShadowDistances = new ShadowDistance[totalShadowCount];

            foreach (Renderer charRenderer in targetCharacters)
            {
                if (!charRenderer.isVisible) continue;
                
                if (((1 << charRenderer.gameObject.layer) & collectLightMask.value) == 0) continue;

                Vector3 charPos  = charRenderer.bounds.center;
                int     charMask = 1 << charRenderer.gameObject.layer;
                charRenderer.GetPropertyBlock(mpb);

                // ── 1. 動態光照計算 ──
                int validCount = 0;
                for (int i = 0; i < totalLightCount; i++) {
                    Light l = targetLights[i];
                    if (l == null || !l.isActiveAndEnabled || l.intensity <= 0 || l.lightmapBakeType == LightmapBakeType.Baked) continue;
                    if ((l.cullingMask & charMask) == 0) continue;

                    float sortDist = 0f;
                    if (l.type != LightType.Directional) {
                        float sqrDistToBounds = charRenderer.bounds.SqrDistance(l.transform.position);
                        long pairKey = MakePairKey(charRenderer.GetInstanceID(), l.GetInstanceID());
                        bool wasActive = _activeLightPairs.Contains(pairKey);

                        float hyst = wasActive ? HYSTERESIS_EXIT : HYSTERESIS_ENTER;
                        float maxRange = l.range + hyst;
                        if (sqrDistToBounds > maxRange * maxRange) continue;

                        sortDist = sqrDistToBounds;
                    }
                    tempDistances[validCount++] = new LightDistance { light = l, sqrDist = sortDist };
                }

                int rendID = charRenderer.GetInstanceID();
                _activeLightPairs.RemoveWhere(k => (int)(k >> 32) == rendID);
                for (int i = 0; i < validCount; i++)
                {
                    if (tempDistances[i].light.type != LightType.Directional)
                        _activeLightPairs.Add(MakePairKey(rendID, tempDistances[i].light.GetInstanceID()));
                }

                if (validCount > 0) {
                    System.Array.Sort(tempDistances, 0, validCount);
                    int injectCount = Mathf.Min(8, validCount);
                    System.Array.Sort(tempDistances, 0, injectCount, new LightStableComparer());

                    for (int i = 0; i < injectCount; i++) {
                        Light l = tempDistances[i].light;
                        if (l.type == LightType.Directional) {
                            tempPos[i]  = new Vector4(-l.transform.forward.x, -l.transform.forward.y, -l.transform.forward.z, 0f);
                            tempSpot[i] = Vector4.zero;
                        } else if (l.type == LightType.Spot) {
                            tempPos[i] = new Vector4(l.transform.position.x, l.transform.position.y, l.transform.position.z, 2f);
                            float outerRad = Mathf.Deg2Rad * 0.5f * l.spotAngle; float cosOuter = Mathf.Cos(outerRad); float cosInner = Mathf.Cos(outerRad * 0.8f);
                            float invSmooth = 1f / Mathf.Max(cosInner - cosOuter, 0.001f);
                            tempSpot[i] = new Vector4(l.transform.forward.x * invSmooth, l.transform.forward.y * invSmooth, l.transform.forward.z * invSmooth, -cosOuter * invSmooth);
                        } else {
                            tempPos[i]  = new Vector4(l.transform.position.x, l.transform.position.y, l.transform.position.z, 1f);
                            tempSpot[i] = Vector4.zero;
                        }
                        Color col = l.color * l.intensity;
                        float sqrRangeInv = l.type == LightType.Directional ? 0f : 1f / Mathf.Max(l.range * l.range, 0.00001f);
                        tempCol[i] = new Vector4(col.r, col.g, col.b, sqrRangeInv);
                    }
                    mpb.SetInt(_SpineLightCount, injectCount); mpb.SetVectorArray(_SpineLightPos, tempPos);
                    mpb.SetVectorArray(_SpineLightColor, tempCol); mpb.SetVectorArray(_SpineLightSpotParams, tempSpot);
                } else { mpb.SetInt(_SpineLightCount, 0); }

                // ── 2. 陰影燈光與 Cookie 計算 ──
                int validShadowCount = 0;
                for (int i = 0; i < totalShadowCount; i++) {
                    SpineShadowLight sl = targetShadowLights[i];
                    if (sl == null || !sl.isActiveAndEnabled || sl.intensity <= 0) continue;
                    if ((sl.cullingMask & charMask) == 0) continue;

                    long pairKey = MakePairKey(charRenderer.GetInstanceID(), sl.GetInstanceID());
                    bool wasActive = _activeShadowPairs.Contains(pairKey);
                    float hyst = wasActive ? HYSTERESIS_EXIT : HYSTERESIS_ENTER;

                    float sqrDistToBounds;
                    
                    if (sl.type == SpineShadowLight.LightType.Area) {
                        Vector3 boxCenter = sl.transform.position + sl.transform.forward * (sl.range * 0.5f);
                        sqrDistToBounds = charRenderer.bounds.SqrDistance(boxCenter);
                        float halfX = sl.areaSize.x * 0.5f;
                        float halfY = sl.areaSize.y * 0.5f;
                        float halfZ = sl.range * 0.5f;
                        float boxBoundingRadius = Mathf.Sqrt(halfX * halfX + halfY * halfY + halfZ * halfZ);
                        float maxRange = boxBoundingRadius + hyst;
                        if (sqrDistToBounds > maxRange * maxRange) continue;
                    } else {
                        sqrDistToBounds = charRenderer.bounds.SqrDistance(sl.transform.position);
                        float maxRange = sl.range + hyst;
                        if (sqrDistToBounds > maxRange * maxRange) continue;
                    }

                    tempShadowDistances[validShadowCount++] = new ShadowDistance { light = sl, sqrDist = sqrDistToBounds };
                }

                _activeShadowPairs.RemoveWhere(k => (int)(k >> 32) == rendID);
                for (int i = 0; i < validShadowCount; i++)
                    _activeShadowPairs.Add(MakePairKey(rendID,
                        tempShadowDistances[i].light.GetInstanceID()));

                if (validShadowCount > 0) {
                    System.Array.Sort(tempShadowDistances, 0, validShadowCount);
                    int injectShadowCount = Mathf.Min(8, validShadowCount);
                    System.Array.Sort(tempShadowDistances, 0, injectShadowCount, new ShadowStableComparer());

                    for (int i = 0; i < injectShadowCount; i++) {
                        SpineShadowLight sl = tempShadowDistances[i].light;
                        tempShadowPos[i] = new Vector4(sl.transform.position.x, sl.transform.position.y, sl.transform.position.z, 1f / Mathf.Max(sl.range * sl.range, 0.00001f));
                        tempShadowColor[i] = new Vector4(sl.color.r, sl.color.g, sl.color.b, sl.intensity); // 總開關倍率
                        tempShadowDir[i] = new Vector4(sl.transform.forward.x, sl.transform.forward.y, sl.transform.forward.z, (float)sl.type);

                        if (sl.type == SpineShadowLight.LightType.Spot) {
                            float outerRad = Mathf.Deg2Rad * 0.5f * sl.spotAngle; float cosOuter = Mathf.Cos(outerRad); float cosInner = Mathf.Cos(outerRad * 0.8f);
                            float invSmooth = 1f / Mathf.Max(cosInner - cosOuter, 0.001f);
                            tempShadowSpot[i] = new Vector4(invSmooth, -cosOuter * invSmooth, sl.areaSize.x, sl.areaSize.y);
                        } else {
                            tempShadowSpot[i] = new Vector4(0, 0, sl.areaSize.x, sl.areaSize.y);
                        }

                        // 將 Spine 專屬的 Intensity 藏在 w 通道
                        tempShadowParams2[i] = new Vector4((int)sl.blendMode, (int)sl.cookieMode, (int)sl.colorChannel, sl.spineIntensity);

                        if (sl.cookieTexture != null) {
                            mpb.SetTexture(_SpineShadowCookies[i], sl.cookieTexture);
                            Vector2 scale = sl.cookieScale;
                            if (scale.x == 0) scale.x = 0.001f;
                            if (scale.y == 0) scale.y = 0.001f;

                            Vector3 right = sl.transform.right / scale.x;
                            Vector3 up = sl.transform.up / scale.y;
                            tempShadowRight[i] = new Vector4(right.x, right.y, right.z, 0);
                            tempShadowUp[i] = new Vector4(up.x, up.y, up.z, 0);

                            Vector2 offset = sl.GetCurrentCookieOffset();
                            // 將 Terrain 專屬的 Intensity 藏在 cookieParams 的 w 通道
                            tempShadowCookieParams[i] = new Vector4(offset.x, offset.y, 1f, sl.terrainIntensity);
                        } else {
                            mpb.SetTexture(_SpineShadowCookies[i], Texture2D.whiteTexture);
                            tempShadowRight[i] = new Vector4(sl.transform.right.x, sl.transform.right.y, sl.transform.right.z, 0);
                            tempShadowUp[i] = new Vector4(sl.transform.up.x, sl.transform.up.y, sl.transform.up.z, 0);
                            // 將 Terrain 專屬的 Intensity 藏在 cookieParams 的 w 通道
                            tempShadowCookieParams[i] = new Vector4(0, 0, 0, sl.terrainIntensity);
                        }
                    }
                    mpb.SetInt(_SpineShadowLightCount, injectShadowCount); mpb.SetVectorArray(_SpineShadowLightPos, tempShadowPos);
                    mpb.SetVectorArray(_SpineShadowLightColor, tempShadowColor); mpb.SetVectorArray(_SpineShadowLightDir, tempShadowDir);
                    mpb.SetVectorArray(_SpineShadowLightSpot, tempShadowSpot);

                    mpb.SetVectorArray(_SpineShadowLightRight, tempShadowRight);
                    mpb.SetVectorArray(_SpineShadowLightUp, tempShadowUp);
                    mpb.SetVectorArray(_SpineShadowCookieParams, tempShadowCookieParams);
                    mpb.SetVectorArray(_SpineShadowParams2, tempShadowParams2);
                } else { mpb.SetInt(_SpineShadowLightCount, 0); }

                if (enableLightProbe) InjectLightProbe(charPos);

                charRenderer.SetPropertyBlock(mpb);
            }
        }
    }
}