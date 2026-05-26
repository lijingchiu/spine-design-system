using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using VFXTool.SpineSkeletonShaderTool;

namespace VFXTool.SpineSkeletonShaderTool
{
    // 修改 1：將 MeshRenderer 更改為通用的 Renderer，使其兼容 SpriteRenderer
    [RequireComponent(typeof(Renderer))]
    [ExecuteInEditMode]
    public class SpineAlphaMaskRenderer : MonoBehaviour
    {
        [Header("Alpha Mask Settings")]
        [Range(0.25f, 1f)]
        public float resolutionScale = 0.75f;

        public Shader alphaMaskShader;
        public Camera targetCamera;

        private CommandBuffer _cmd;
        private RenderTexture _alphaMaskRT;
        
        // 修改 2：將 _meshRenderer 變數改為通用 _renderer
        private Renderer _renderer; 
        private MaterialPropertyBlock _mpb;
        private int _lastRTWidth;
        private int _lastRTHeight;

        private List<Material> _maskMaterials = new List<Material>();

        private static readonly int PropAlphaMask = Shader.PropertyToID("_SpineAlphaMask");
        private static readonly int PropAlphaMaskTexelSize = Shader.PropertyToID("_SpineAlphaMask_TexelSize");
        private static readonly int PropMainTex = Shader.PropertyToID("_MainTex");

        void OnEnable()
        {
            // 修改 3：獲取通用的 Renderer
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();

            if (alphaMaskShader == null)
                alphaMaskShader = Shader.Find("Hidden/SpineAlphaMask");

            if (alphaMaskShader == null)
            {
                Debug.LogError("[SpineAlphaMaskRenderer] 找不到 Hidden/SpineAlphaMask shader！", this);
                enabled = false;
                return;
            }

            if (targetCamera == null)
                targetCamera = Camera.main;

            _cmd = new CommandBuffer { name = "SpineAlphaMask_" + gameObject.name };
        }

        void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
            }

            EnsureRT();
            RebuildCommandBuffer();
            InjectToRenderer();
        }

        private void EnsureRT()
        {
            int w = Mathf.Max(1, (int)(targetCamera.pixelWidth * resolutionScale));
            int h = Mathf.Max(1, (int)(targetCamera.pixelHeight * resolutionScale));

            if (_alphaMaskRT != null && _lastRTWidth == w && _lastRTHeight == h)
                return;

            if (_alphaMaskRT != null)
            {
                _alphaMaskRT.Release();
                DestroyHelper(_alphaMaskRT);
            }

            var format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf)
                ? RenderTextureFormat.RHalf
                : RenderTextureFormat.R8;

            _alphaMaskRT = new RenderTexture(w, h, 0, format)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "SpineAlphaMaskRT_" + gameObject.name,
                useMipMap = false,
                autoGenerateMips = false
            };
            _alphaMaskRT.Create();

            _lastRTWidth = w;
            _lastRTHeight = h;
        }

        private void SyncMaskMaterials(Material[] srcMaterials)
        {
            int count = srcMaterials.Length;

            while (_maskMaterials.Count < count)
            {
                var mat = new Material(alphaMaskShader) { hideFlags = HideFlags.HideAndDontSave };
                _maskMaterials.Add(mat);
            }

            while (_maskMaterials.Count > count)
            {
                int last = _maskMaterials.Count - 1;
                DestroyHelper(_maskMaterials[last]);
                _maskMaterials.RemoveAt(last);
            }

            // 修改 4：SpriteRenderer 的貼圖通常是透過 MaterialPropertyBlock 傳遞，因此需先讀取當前狀態
            MaterialPropertyBlock tempBlock = new MaterialPropertyBlock();
            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(tempBlock);
            }

            for (int i = 0; i < count; i++)
            {
                Material src = srcMaterials[i];
                if (src != null)
                {
                    Texture tex = null;

                    // 1. 優先嘗試從 MaterialPropertyBlock 抓取 (支援 SpriteRenderer 動態圖)
                    tex = tempBlock.GetTexture(PropMainTex);

                    // 2. 如果沒有，則嘗試從 Material 本身抓取 (支援 Spine / MeshRenderer)
                    if (tex == null && src.HasProperty(PropMainTex))
                    {
                        tex = src.GetTexture(PropMainTex);
                    }

                    // 3. 最後備案：如果是 SpriteRenderer 但前面都抓不到，直接向 Sprite 要貼圖
                    if (tex == null && _renderer is SpriteRenderer spriteRenderer && spriteRenderer.sprite != null)
                    {
                        tex = spriteRenderer.sprite.texture;
                    }

                    if (tex != null)
                        _maskMaterials[i].SetTexture(PropMainTex, tex);
                }
            }
        }

        private void RebuildCommandBuffer()
        {
            // 修改 5：對應 _renderer
            if (_cmd == null || _alphaMaskRT == null || _renderer == null)
                return;

            targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, _cmd);
            _cmd.Clear();

            Material[] srcMats = _renderer.sharedMaterials;
            SyncMaskMaterials(srcMats);

            _cmd.SetRenderTarget(_alphaMaskRT);
            _cmd.ClearRenderTarget(false, true, Color.clear);

            for (int i = 0; i < srcMats.Length; i++)
            {
                if (srcMats[i] == null) continue;
                if (i >= _maskMaterials.Count) break;
                // 修改 6：使用通用的 _renderer 繪製
                _cmd.DrawRenderer(_renderer, _maskMaterials[i], i, 0);
            }

            targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, _cmd);
        }

        private void InjectToRenderer()
        {
            // 修改 7：對應 _renderer
            if (_renderer == null || _alphaMaskRT == null) return;

            _renderer.GetPropertyBlock(_mpb);

            _mpb.SetTexture(PropAlphaMask, _alphaMaskRT);
            _mpb.SetVector(PropAlphaMaskTexelSize,
                new Vector4(
                    1f / _alphaMaskRT.width,
                    1f / _alphaMaskRT.height,
                    _alphaMaskRT.width,
                    _alphaMaskRT.height));

            _renderer.SetPropertyBlock(_mpb);
        }

        void OnDisable()
        {
            if (_cmd != null && targetCamera != null)
                targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, _cmd);

            if (_cmd != null) { _cmd.Clear(); _cmd.Dispose(); _cmd = null; }

            if (_alphaMaskRT != null)
            {
                _alphaMaskRT.Release();
                DestroyHelper(_alphaMaskRT);
                _alphaMaskRT = null;
            }

            for (int i = 0; i < _maskMaterials.Count; i++)
                DestroyHelper(_maskMaterials[i]);
            _maskMaterials.Clear();

            _lastRTWidth = 0;
            _lastRTHeight = 0;
        }

        void OnDestroy() { OnDisable(); }

        private static void DestroyHelper(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
        }

    #if UNITY_EDITOR
        void OnValidate() { _lastRTWidth = 0; _lastRTHeight = 0; }
    #endif
    }
}