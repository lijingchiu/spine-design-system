using UnityEngine;
using System.Reflection;
using VFXTool.SpineSkeletonShaderTool;

namespace VFXTool.SpineSkeletonShaderTool
{
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public class SpineBloomThresholdSync : MonoBehaviour
    {
        [Header("自動同步 Beautify 數值")]
        [Tooltip("自動從 WinkingBeautify 抓取的 Bloom 強度")]
        public float currentBloomIntensity = 1.0f;

        [Header("遞減函數曲線")]
        [Tooltip("X軸 = Bloom強度(1~10)，Y軸 = Bloom Suppress Threshold")]
        // 預設為圖片中的曲線：起點(1, 0.6)，終點(10, 0.5209)，呈現內凹平滑遞減
        public AnimationCurve thresholdCurve = new AnimationCurve(
            new Keyframe(1f, 0.6f, -0.02f, -0.02f),         // 起點：指定微幅的初始下墜斜率
            new Keyframe(10f, 0.5209f, -0.002f, -0.002f)    // 終點：指定趨於平緩的尾端斜率
        );

        private Renderer targetRenderer;
        private MaterialPropertyBlock mpb;
        private float lastAppliedBloom = -1f; // 效能優化：髒標記

        private void OnEnable()
        {
            targetRenderer = GetComponent<Renderer>();
            if (mpb == null) mpb = new MaterialPropertyBlock();
            lastAppliedBloom = -1f; // 強制初次更新
        }

        private void Update()
        {
            if (targetRenderer == null) return;

            // 1. 抓取 WinkingBeautify 的最終 Bloom 數值
            FetchWinkingBeautifyIntensity();

            // 2. 效能優化：如果 Bloom 沒有變動，就不需要重新計算和寫入 GPU
            if (Mathf.Approximately(currentBloomIntensity, lastAppliedBloom))
                return;

            // 3. 根據自訂曲線計算 Threshold，並傳遞給 Shader
            float targetThreshold = thresholdCurve.Evaluate(currentBloomIntensity);
            
            targetRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_BloomSuppressThreshold", targetThreshold);
            targetRenderer.SetPropertyBlock(mpb);

            lastAppliedBloom = currentBloomIntensity;
        }

        private void FetchWinkingBeautifyIntensity()
        {
            if (Camera.main != null)
            {
                // 抓取你指定的 WinkingBeautify 元件
                Component beautifyComp = Camera.main.GetComponent("WinkingBeautify");
                
                if (beautifyComp != null)
                {
                    System.Type type = beautifyComp.GetType();
                    
                    // 確認 Bloom 是否有啟用
                    PropertyInfo bloomEnabledProp = type.GetProperty("bloom");
                    bool isBloomEnabled = true;
                    if (bloomEnabledProp != null) 
                        isBloomEnabled = (bool)bloomEnabledProp.GetValue(beautifyComp, null);

                    if (isBloomEnabled)
                    {
                        // 抓取 Bloom Intensity 終值
                        PropertyInfo intensityProp = type.GetProperty("bloomIntensity");
                        if (intensityProp != null)
                        {
                            currentBloomIntensity = (float)intensityProp.GetValue(beautifyComp, null);
                        }
                    }
                    else
                    {
                        currentBloomIntensity = 0f;
                    }
                }
            }
        }
    }
}
