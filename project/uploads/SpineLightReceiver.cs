using UnityEngine;
using System.Collections.Generic;
using VFXTool.SpineSkeletonShaderTool;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VFXTool.SpineSkeletonShaderTool
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public class SpineLightReceiver : MonoBehaviour
    {
        private Renderer targetRenderer;

        // ─────────────────────────────────────────────────────────────
        //  Debug：目前接收到的燈光（僅供 Inspector 顯示，不影響渲染）
        // ─────────────────────────────────────────────────────────────
        [Header("Debug — Active Lights (Read Only)")]
        [SerializeField] private List<Light>            _debugRealtimeLights = new List<Light>();
        [SerializeField] private List<SpineShadowLight> _debugShadowLights   = new List<SpineShadowLight>();

        void OnEnable()
        {
            targetRenderer = GetComponent<Renderer>();
            SpineLightManager mgr = SpineLightManager.EnsureInstance();
            if (mgr != null)
                mgr.RegisterCharacter(targetRenderer);

    #if UNITY_EDITOR
            // Edit Mode 下透過 EditorApplication.update 驅動每幀查詢
            if (!Application.isPlaying)
                EditorApplication.update += EditorRefreshDebug;
    #endif
        }

        void OnDisable()
        {
            if (SpineLightManager.Instance != null && targetRenderer != null)
                SpineLightManager.Instance.UnregisterCharacter(targetRenderer);

            // ✅ 清除殘留在 Renderer 上的 MaterialPropertyBlock
            // UnregisterCharacter 只是將此 Renderer 從 Manager 追蹤列表移除，
            // 但過去 SetPropertyBlock(mpb) 寫入的光照資料仍然存在 Renderer 上。
            // 若不主動清除，Shader 會繼續讀取這份殘留資料，造成光照效果無法立即消失。
            if (targetRenderer != null)
                targetRenderer.SetPropertyBlock(null);

            _debugRealtimeLights.Clear();
            _debugShadowLights.Clear();

    #if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorApplication.update -= EditorRefreshDebug;
    #endif
        }

        // Play Mode：Manager.LateUpdate 執行完後查詢
        void LateUpdate()
        {
            if (!Application.isPlaying) return; // Edit Mode 由 EditorRefreshDebug 處理
            RefreshDebugLists();
        }

    #if UNITY_EDITOR
        // Edit Mode：每個 Editor 幀查詢並強制刷新 Inspector
        private void EditorRefreshDebug()
        {
            if (Application.isPlaying) return; // 切換到 Play Mode 後交由 LateUpdate 接手
            if (this == null) { EditorApplication.update -= EditorRefreshDebug; return; }
            RefreshDebugLists();
        }
    #endif

        private void RefreshDebugLists()
        {
            SpineLightManager mgr = SpineLightManager.Instance;
            if (mgr == null || targetRenderer == null)
            {
                _debugRealtimeLights.Clear();
                _debugShadowLights.Clear();
                return;
            }
            mgr.GetActiveLightsForRenderer(targetRenderer, _debugRealtimeLights, _debugShadowLights);
        }
    }


    // ══════════════════════════════════════════════════════════════════
    //  SpineLightReceiverEditor：讓 Debug 列表在 Inspector 唯讀顯示
    // ══════════════════════════════════════════════════════════════════
    #if UNITY_EDITOR
    [CustomEditor(typeof(SpineLightReceiver))]
    public class SpineLightReceiverEditor : Editor
    {
        private static readonly Color ColorRealtime = new Color(1.00f, 0.80f, 0.20f); // 金黃
        private static readonly Color ColorShadow   = new Color(0.45f, 0.75f, 1.00f); // 天藍

        private SerializedProperty _debugRealtimeLights;
        private SerializedProperty _debugShadowLights;

        private bool _showRealtime = true;
        private bool _showShadow   = true;

        void OnEnable()
        {
            _debugRealtimeLights = serializedObject.FindProperty("_debugRealtimeLights");
            _debugShadowLights   = serializedObject.FindProperty("_debugShadowLights");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 正常欄位（排除 Debug lists，用自訂方式繪製）
            DrawPropertiesExcluding(serializedObject,
                "_debugRealtimeLights", "_debugShadowLights");

            EditorGUILayout.Space(8);

            // ── Debug 區塊標題列 ──────────────────────────────────────
            Rect titleRect = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
            Color titleBg = EditorGUIUtility.isProSkin
                ? new Color(.18f, .18f, .18f) : new Color(.80f, .80f, .80f);
            EditorGUI.DrawRect(titleRect, titleBg);
            EditorGUI.DrawRect(new Rect(titleRect.x, titleRect.y, titleRect.width, 2f),
                new Color(.5f, .5f, .5f, .5f));
            var ts = new GUIStyle(EditorStyles.boldLabel)
                { alignment = TextAnchor.MiddleLeft, fontSize = 11 };
            ts.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(.75f, .75f, .75f) : new Color(.20f, .20f, .20f);
            EditorGUI.LabelField(
                new Rect(titleRect.x + 8f, titleRect.y, titleRect.width, titleRect.height),
                "🔍  Active Lights  （Runtime Debug）", ts);

            EditorGUILayout.Space(4);

            bool isPlaying = Application.isPlaying;

            DrawLightList(ref _showRealtime, "Realtime Lights",
                _debugRealtimeLights, ColorRealtime);
            EditorGUILayout.Space(4);
            DrawLightList(ref _showShadow, "Shadow Lights",
                _debugShadowLights, ColorShadow);

            serializedObject.ApplyModifiedProperties();

            // Edit Mode 與 Play Mode 都每幀刷新 Inspector
            Repaint();
        }

        private void DrawLightList(ref bool foldout, string title,
            SerializedProperty listProp, Color accent)
        {
            int count = listProp.arraySize;

            // ── 子標題列（可折疊）────────────────────────────────────
            Rect r = GUILayoutUtility.GetRect(10f, 22f, GUILayout.ExpandWidth(true));
            Event e = Event.current;

            Color subBg = EditorGUIUtility.isProSkin
                ? new Color(.22f, .22f, .22f) : new Color(.85f, .85f, .85f);
            EditorGUI.DrawRect(r, subBg);
            EditorGUI.DrawRect(new Rect(r.x, r.y, 3f, r.height), accent);

            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition))
                { foldout = !foldout; e.Use(); }

            var st = new GUIStyle(EditorStyles.boldLabel)
                { alignment = TextAnchor.MiddleLeft, fontSize = 11 };
            st.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(.85f, .85f, .85f) : new Color(.10f, .10f, .10f);

            // 數量 badge
            string badge = count == 0 ? "  （無）" : $"  （{count} 個）";
            EditorGUI.LabelField(
                new Rect(r.x + 10f, r.y, r.width - 28f, r.height),
                title + badge, st);

            var ic = new GUIStyle(EditorStyles.label)
                { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
            ic.normal.textColor = st.normal.textColor;
            EditorGUI.LabelField(
                new Rect(r.xMax - 22f, r.y, 20f, r.height),
                foldout ? "−" : "＋", ic);

            if (!foldout) return;

            // ── 列表內容 ─────────────────────────────────────────────
            if (count == 0)
            {
                using (new EditorGUI.IndentLevelScope(1))
                    EditorGUILayout.LabelField("— 目前沒有接收到任何燈光 —",
                        EditorStyles.centeredGreyMiniLabel);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                Object obj = elem.objectReferenceValue;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // 左側色條
                Rect dot = GUILayoutUtility.GetRect(6f, EditorGUIUtility.singleLineHeight,
                    GUILayout.Width(6f));
                EditorGUI.DrawRect(new Rect(dot.x, dot.y + 1f, 4f, dot.height - 2f), accent);

                // 唯讀 ObjectField（可點選 highlight）
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(GUIContent.none, obj, typeof(Object), true);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
        }
    }
    #endif
}