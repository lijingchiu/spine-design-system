using UnityEngine;
using System.Collections.Generic;
using VFXTool.SpineSkeletonShaderTool;

namespace VFXTool.SpineSkeletonShaderTool
{
    
    public class SpineProfileTester : MonoBehaviour
    {
        [Header("【 目標設定 】")]
        public Renderer targetRenderer;
        
        [Header("【 左鍵：動態加載 Profile 】")]
        public SpineModuleProfile presetProfile;

        [Header("【 右鍵：MPB 參數控制 】")]
        public string propertyName = "_EnableHitSweep";
        public float targetValue = 1f;

        [Header("【 中鍵：Temp Profile 實體竄改 】")]
        [Tooltip("作為基底的設定檔 (例如外框設定檔)")]
        public SpineModuleProfile baseProfileForTemp;

        // 狀態追蹤
        private bool isProfileApplied = false;
        private bool isParamToggled = false;
        private bool isTempProfileApplied = false;

        // 宣告一個變數來抓住產生的 Temp Profile，以便後續銷毀！
        private SpineModuleProfile _activeTempProfile;

        void Update()
        {
            if (targetRenderer == null) return;

            // ─────────────────────────────────────────────────────────────
            // 🧪 [左鍵] 測試動態加載/移除 Profile
            // ─────────────────────────────────────────────────────────────
            if (Input.GetMouseButtonDown(0) && presetProfile != null)
            {
                isProfileApplied = !isProfileApplied;
                if (isProfileApplied)
                {
                    SpineProfileHelper.ApplyProfileRuntime(targetRenderer, presetProfile);
                    Debug.Log($"<b>[左鍵]</b> 已動態【加載】 Profile: {presetProfile.moduleName}");
                }
                else
                {
                    SpineProfileHelper.RemoveProfileRuntime(targetRenderer, presetProfile.moduleName);
                    Debug.Log($"<b>[左鍵]</b> 已動態【移除】 Profile: {presetProfile.moduleName}");
                }
            }

            // ─────────────────────────────────────────────────────────────
            // 🧪 [右鍵] 測試手動控制 MPB (前提是該參數沒有被 Profile 控制)
            // ─────────────────────────────────────────────────────────────
            if (Input.GetMouseButtonDown(1) && !string.IsNullOrEmpty(propertyName))
            {
                isParamToggled = !isParamToggled;
                float valToSet = isParamToggled ? targetValue : 0f;
                
                SpineProfileHelper.SetFloat(targetRenderer, propertyName, valToSet);
                Debug.Log($"<b>[右鍵]</b> 透過 MPB 將參數 {propertyName} 設為: {valToSet}");
            }

            // ─────────────────────────────────────────────────────────────
            // 🧪 [中鍵] 測試 Temp Profile 實體竄改 (狀態切換的最佳實踐)
            // ─────────────────────────────────────────────────────────────
            if (Input.GetMouseButtonDown(2) && baseProfileForTemp != null)
            {
                isTempProfileApplied = !isTempProfileApplied;

                if (isTempProfileApplied)
                {
                    // 1. 定義你想要「竄改」的參數字典
                    var overrides = new Dictionary<string, object>
                    {
                        { "_OutlineColor", Color.green },  // 例如把外框改成綠色 (中毒)
                        { "_OutlineWidth", 5.0f }          // 把外框加粗
                    };

                    // 2. 呼叫 Helper 產生並套用副本，並將回傳的實體存起來
                    _activeTempProfile = SpineProfileHelper.ApplyTempProfile(targetRenderer, baseProfileForTemp, overrides);
                    
                    Debug.Log($"<b>[中鍵]</b> 已套用 Temp Profile: 綠色加粗外框 (已強制關閉曲線)");
                }
                else
                {
                    // 1. 從畫面上移除該 Profile 的效果
                    if (_activeTempProfile != null)
                    {
                        SpineProfileHelper.RemoveProfileRuntime(targetRenderer, _activeTempProfile.moduleName);
                        
                        // 2.手動銷毀避免 Memory Leak！
                        Destroy(_activeTempProfile);
                        _activeTempProfile = null;
                    }
                    Debug.Log($"<b>[中鍵]</b> 已移除並【銷毀】 Temp Profile");
                }
            }
        }

        // 確保當腳本被刪除或場景切換時，TempProfile 也能被清乾淨
        void OnDestroy()
        {
            if (_activeTempProfile != null)
            {
                Destroy(_activeTempProfile);
            }
        }
    }
}
