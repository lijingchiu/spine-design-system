using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
#endif

namespace VFXTool.SpineSkeletonShaderTool
{
    [RequireComponent(typeof(Renderer))]
    [ExecuteInEditMode]
    public class SpineHitSweepEffect : MonoBehaviour
    {
        [Tooltip("特效光帶掃過全身的總時間 (秒)")]
        public float duration = 0.5f;

        [Tooltip("設定刷光的起始值 (X) 與結束值 (Y)。預設為 -4 到 4")]
        public Vector2 sweepRange = new Vector2(-4f, 4f);

        [Tooltip("角色中心的 Local 座標偏移量 (與 Shader 同步)")]
        public Vector3 sweepCenterOffset = new Vector3(0, 0.5f, 0);

        [Tooltip("目前的打擊點座標 (世界空間)。供測試使用。")]
        public Vector3 debugHitPosition;

        private Renderer targetRenderer;
        
        private MaterialPropertyBlock mpb;
        private Coroutine sweepCoroutine;

        private AnimationCurve progressCurve;

#if UNITY_EDITOR
        private double editorStartTime;
        private bool isEditorPlaying = false;
#endif

        void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            UpdateCurve();
        }

        private void UpdateCurve()
        {
            progressCurve = AnimationCurve.Linear(0f, sweepRange.x, 1f, sweepRange.y);
        }

        public void PlaySweep(Vector3 hitWorldPosition)
        {
            debugHitPosition = hitWorldPosition;

            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            if (mpb == null) mpb = new MaterialPropertyBlock();

            UpdateCurve(); 

            if (Application.isPlaying)
            {
                if (sweepCoroutine != null) StopCoroutine(sweepCoroutine);
                sweepCoroutine = StartCoroutine(SweepRoutine(hitWorldPosition));
            }
#if UNITY_EDITOR
            else
            {
                editorStartTime = EditorApplication.timeSinceStartup;
                if (!isEditorPlaying)
                {
                    EditorApplication.update += EditorUpdate;
                    isEditorPlaying = true;
                }
            }
#endif
        }

        private IEnumerator SweepRoutine(Vector3 hitPos)
        {
            float timePassed = 0f;
            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                UpdateSweep(timePassed, hitPos);
                yield return null;
            }
            EndSweep();
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (targetRenderer == null)
            {
                StopEditorUpdate();
                return;
            }

            float timePassed = (float)(EditorApplication.timeSinceStartup - editorStartTime);
            if (timePassed < duration)
            {
                UpdateSweep(timePassed, debugHitPosition);
                SceneView.RepaintAll();
            }
            else
            {
                EndSweep();
                StopEditorUpdate();
            }
        }

        private void StopEditorUpdate()
        {
            EditorApplication.update -= EditorUpdate;
            isEditorPlaying = false;
            SceneView.RepaintAll();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying && isEditorPlaying) StopEditorUpdate();
            EndSweep();
        }
#endif

        // ─────────────────────────────────────────────────────────────
        // 透過 MaterialPropertyBlock 覆寫參數 (不影響材質球原檔)
        // ─────────────────────────────────────────────────────────────
        private void UpdateSweep(float timePassed, Vector3 hitPos)
        {
            float t = Mathf.Clamp01(timePassed / duration);
            float currentProgress = progressCurve.Evaluate(t);

            targetRenderer.GetPropertyBlock(mpb);
            
            mpb.SetVector("_HitPosition", hitPos);
            mpb.SetVector("_SweepCenterOffset", sweepCenterOffset); 
            mpb.SetFloat("_SweepProgress", currentProgress);
            
            targetRenderer.SetPropertyBlock(mpb);
        }

        private void EndSweep()
        {
            if (targetRenderer != null && mpb != null)
            {
                targetRenderer.GetPropertyBlock(mpb);
                // 結束時，把這隻角色的光帶推回 -10 (畫面外)
                mpb.SetFloat("_SweepProgress", -10f); 
                targetRenderer.SetPropertyBlock(mpb);
            }
        }
    }

// ══════════════════════════════════════════════════════════════════
// 編輯器介面與指南視窗，包版時剔除
// ══════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
    [CustomEditor(typeof(SpineHitSweepEffect))]
    public class SpineHitSweepEffectEditor : Editor
    {
        private static readonly Color AccentColor = new Color(0.20f, 0.70f, 1.00f); // 特效專屬水藍色
        private bool isEditingHitPos = false;

        SerializedProperty _duration, _sweepRange, _sweepCenterOffset, _debugHitPosition;

        void OnEnable()
        {
            _duration = serializedObject.FindProperty("duration");
            _sweepRange = serializedObject.FindProperty("sweepRange");
            _sweepCenterOffset = serializedObject.FindProperty("sweepCenterOffset");
            _debugHitPosition = serializedObject.FindProperty("debugHitPosition");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTitleBar();
            DrawTopHelpButton();

            // 1. 動畫與座標設定
            EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(10, 10, 8, 10) });
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            DrawPropWithContextMenu(_duration, new GUIContent("Duration", "掃光總時間(秒)"));
            DrawPropWithContextMenu(_sweepRange, new GUIContent("Sweep Range (Start, End)", "掃光的座標行進範圍"));
            DrawPropWithContextMenu(_sweepCenterOffset, new GUIContent("Center Offset", "角色受擊計算的本地基準中心點"));
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // 2. 測試與 Debug 工具
            EditorGUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(10, 10, 8, 10) });
            EditorGUILayout.LabelField("Debug & Test Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("實際遊戲中請由程式呼叫 PlaySweep(pos) 來觸發特效，以下僅供編輯器測試。", MessageType.Info);
            
            DrawPropWithContextMenu(_debugHitPosition, new GUIContent("Hit Position", "模擬打擊點(世界座標)"));

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            SpineHitSweepEffect effect = (SpineHitSweepEffect)target;

            if (GUILayout.Button("📍 取自身座標", GUILayout.Height(30)))
            {
                Undo.RecordObject(effect, "Set Hit Position to Self");
                effect.debugHitPosition = effect.transform.position;
                EditorUtility.SetDirty(effect);
            }

            GUIStyle editBtnStyle = new GUIStyle(GUI.skin.button);
            if (isEditingHitPos) editBtnStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);

            if (GUILayout.Button(isEditingHitPos ? "✅ 結束編輯點位" : " 在 Scene 編輯點位", editBtnStyle, GUILayout.Height(30)))
            {
                isEditingHitPos = !isEditingHitPos;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f);
            if (GUILayout.Button("▶ Test Sweep Effect (測試受擊光帶)", GUILayout.Height(35)))
            {
                effect.PlaySweep(effect.debugHitPosition);
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        // 繪製屬性並綁定右鍵選單
        private void DrawPropWithContextMenu(SerializedProperty prop, GUIContent content)
        {
            EditorGUILayout.PropertyField(prop, content, true);
            Rect rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("📖 查看使用說明"), false, () => {
                    SpineHitSweepHelpWindow.ShowWindowAndScrollTo(prop.name);
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        private void DrawTitleBar()
        {
            Rect r = GUILayoutUtility.GetRect(10f, 36f, GUILayout.ExpandWidth(true));
            Color bg = EditorGUIUtility.isProSkin ? new Color(.14f, .14f, .14f) : new Color(.78f, .78f, .78f);
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), AccentColor);
            var s = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
            s.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(AccentColor, Color.white, .35f) : Color.Lerp(AccentColor, Color.black, .45f);
            EditorGUI.LabelField(new Rect(r.x, r.y + 3f, r.width, r.height - 3f), "✦  Spine Hit Sweep (受擊掃光)  ✦", s);
            GUILayout.Space(5);
        }

        private void DrawTopHelpButton()
        {
            Rect r = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
            Event e = Event.current; bool hover = r.Contains(e.mousePosition);

            Color bg = EditorGUIUtility.isProSkin
                ? (hover ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f));
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax-1f, r.width, 1f), EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
            EditorGUI.DrawRect(new Rect(r.x, r.y, 4f, r.height), AccentColor);

            if (e.type == EventType.MouseDown && e.button == 0 && hover) {
                SpineHitSweepHelpWindow.ShowWindow();
                e.Use();
            }

            var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            ts.normal.textColor = EditorGUIUtility.isProSkin ? AccentColor : Color.Lerp(AccentColor, Color.black, 0.4f);
            EditorGUI.LabelField(r, "▤ 掃光系統 美術與程式使用指南", ts);
            GUILayout.Space(8);
        }

        private void OnSceneGUI()
        {
            SpineHitSweepEffect effect = (SpineHitSweepEffect)target;
            if (isEditingHitPos)
            {
                Tools.hidden = true;
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(effect.debugHitPosition, Quaternion.identity);

                Handles.color = Color.red;
                Handles.DrawWireDisc(newPos, Vector3.back, 0.15f);
                Handles.DrawWireDisc(newPos, Vector3.up, 0.15f);
                Handles.DrawWireDisc(newPos, Vector3.right, 0.15f);

                Vector3 actualCenterWorld = effect.transform.TransformPoint(effect.sweepCenterOffset);

                Handles.color = new Color(1, 0, 0, 0.5f);
                Handles.DrawDottedLine(newPos, actualCenterWorld, 2f);
                
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.red;
                labelStyle.fontStyle = FontStyle.Bold;
                Handles.Label(newPos + Vector3.up * 0.3f, "Hit Position", labelStyle);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(effect, "Move Hit Position");
                    effect.debugHitPosition = newPos;
                    EditorUtility.SetDirty(effect);
                }
            }
            else
            {
                Tools.hidden = false;
            }
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }
    }

// ══════════════════════════════════════════════════════════════════
// 使用指南視窗
// ══════════════════════════════════════════════════════════════════
    public class SpineHitSweepHelpWindow : EditorWindow
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
            var window = GetWindow<SpineHitSweepHelpWindow>("Hit Sweep 指南");
            window.minSize = new Vector2(600, 650);
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
            // === 現代化頂部搜尋列 ===
            GUILayout.Space(10);
            Rect searchRect = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
            
            Color bg = EditorGUIUtility.isProSkin ? new Color(.20f, .20f, .20f) : new Color(.85f, .85f, .85f);
            Color searchAccent = new Color(0.20f, 0.70f, 1.00f); 
            
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
            GUILayout.Label("Spine Hit Sweep 掃光系統指南", titleStyle);

            EditorGUILayout.HelpBox(
                "【系統運作原理】\n" +
                "此腳本透過程式將打擊點 (Hit Position) 傳遞給 Shader，並利用 MaterialPropertyBlock (MPB) 安全地驅動時間參數，產生一條穿過角色的動態光帶，完全不打斷合批 (Batching)。", MessageType.Info);
            
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

            // --- 程式 API 指南區塊 ---
            if (!hasSearch || "api play 呼叫".Contains(queryLower))
            {
                Rect rApi = GUILayoutUtility.GetRect(10f, 32f, GUILayout.ExpandWidth(true));
                Color apiAccent = new Color(0.9f, 0.4f, 0.4f);
                EditorGUI.DrawRect(rApi, EditorGUIUtility.isProSkin ? new Color(.20f,.20f,.20f) : new Color(.85f,.85f,.85f));
                EditorGUI.DrawRect(new Rect(rApi.x, rApi.y, rApi.width, 3f), apiAccent);

                var tsApi = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 14 };
                tsApi.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                EditorGUI.LabelField(new Rect(rApi.x+12f, rApi.y+3f, rApi.width-30f, rApi.height-3f), "❖ 程式 API 呼叫指南", tsApi);

                EditorGUILayout.BeginVertical(new GUIStyle("HelpBox"));
                GUILayout.Space(10);
                GUILayout.Label("在您的戰鬥腳本 / 受擊判斷邏輯中，請使用以下程式碼來觸發刷光特效：", new GUIStyle(EditorStyles.label) { richText = true });
                
                string apiCode = 
@"// 1. 取得角色身上的特效腳本
SpineHitSweepEffect hitEffect = GetComponent<SpineHitSweepEffect>();

if (hitEffect != null)
{
    // 2. 傳入受擊點的世界座標 (World Position) 即可自動播放光帶
    hitEffect.PlaySweep(hitWorldPosition);
}";
                GUIStyle codeBox = new GUIStyle(EditorStyles.textArea) { wordWrap = true, margin = new RectOffset(10, 10, 5, 10), padding = new RectOffset(10, 10, 10, 10) };
                GUILayout.TextArea(apiCode, codeBox);
                
                GUILayout.Space(5);
                if (GUILayout.Button("📋 複製代碼到剪貼簿", GUILayout.Height(30)))
                {
                    EditorGUIUtility.systemCopyBuffer = apiCode;
                    ShowNotification(new GUIContent("已成功複製到剪貼簿！"));
                }
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
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

            GUIStyle propTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, richText = true };
            propTitle.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(accentColor, Color.white, 0.4f) : Color.Lerp(accentColor, Color.black, 0.4f);
            
            GUI.Label(titleRect, $"🔹 {prop.friendlyName} <color=#888888>({prop.propName})</color>", propTitle);
            
            GUIStyle contentStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, margin = new RectOffset(5,0,2,2) };
            
            EditorGUI.indentLevel++;
            GUILayout.Label(prop.desc, contentStyle);
            
            if (!string.IsNullOrEmpty(prop.dependency)) {
                GUILayout.Label($"<b><color=#D4A017>[依賴條件]</color></b> {prop.dependency}", contentStyle);
            }
            
            if (!string.IsNullOrEmpty(prop.bugFix)) {
                GUILayout.Label($"<b><color=#D45050>[排錯檢查]</color></b> {prop.bugFix}", contentStyle);
            }
            EditorGUI.indentLevel--;
            
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        private void InitDocs()
        {
            docModules = new List<HelpDocModule>();

            var modAnim = new HelpDocModule { moduleName = "Animation & Offset (動畫與基準偏移)", accentColor = new Color(0.20f, 0.70f, 1.00f), isExpanded = true };
            modAnim.props.Add(new HelpDocProp { propName = "duration", friendlyName = "Duration (掃光總時間)", 
                desc = "呼叫一次 PlaySweep 後，光帶完整掃過全身所需的時間 (秒)。", dependency = "", bugFix = "" });
            modAnim.props.Add(new HelpDocProp { propName = "sweepRange", friendlyName = "Sweep Range (掃描範圍)", 
                desc = "定義 Shader 中光帶從起點(X) 走到終點(Y) 的座標數值。預設為 -4 到 4。", dependency = "", bugFix = "如果角色特別巨大，掃光還沒掃完就消失，請把此範圍加大 (例如改為 -8 到 8)。" });
            modAnim.props.Add(new HelpDocProp { propName = "sweepCenterOffset", friendlyName = "Center Offset (基準中心點)", 
                desc = "用來抵銷 Spine 原點通常在腳底的問題。設定這個偏移量，讓系統知道角色的「胸口(中心)」在哪裡。", dependency = "", bugFix = "若開啟 Scene 編輯模式時發現紅色的虛線終點不在角色中心，請調整此數值。" });
            docModules.Add(modAnim);

            var modDebug = new HelpDocModule { moduleName = "Debug Tools (測試工具)", accentColor = new Color(0.40f, 0.80f, 0.40f), isExpanded = false };
            modDebug.props.Add(new HelpDocProp { propName = "debugHitPosition", friendlyName = "Hit Position (模擬打擊點)", 
                desc = "在編輯器中模擬的攻擊命中座標。光帶會以此點為圓心向外擴散掃過全身。", dependency = "", bugFix = "請點擊 [在 Scene 編輯點位] 按鈕，直接在場景中拖曳紅圈來測試不同方位的受擊效果。" });
            docModules.Add(modDebug);
        }
    }
#endif
}