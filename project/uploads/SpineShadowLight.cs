using UnityEngine;
using VFXTool.SpineSkeletonShaderTool;

namespace VFXTool.SpineSkeletonShaderTool
{
#if UNITY_EDITOR
    using UnityEditor;
    using System.Collections.Generic;
    using System.Linq;
#endif

    [ExecuteAlways]
    [AddComponentMenu("Spine/Shadow & Area Light")]
    public class SpineShadowLight : MonoBehaviour
    {
        public enum LightType   { Point = 1, Spot = 2, Area = 3 }
        public enum BlendMode   { Multiply = 0, Add = 1, Screen = 2, Overlay = 3 }
        public enum CookieMode  { Normal = 0, Caustics = 1 }
        public enum ColorChannel { RGBA = 0, R = 1, G = 2, B = 3, Alpha = 4 }

        public LightType  type      = LightType.Point;
        public BlendMode  blendMode = BlendMode.Multiply;

        [Tooltip("陰影與濾鏡的顏色。大於1可產生 Bloom 過曝效果！")]
        [ColorUsage(true, true)] public Color color = Color.black;

        [Range(0f, 10f)] public float intensity  = 1f;
        [Range(0f, 10f)] public float spineIntensity = 1f;
        [Range(0f, 10f)] public float terrainIntensity = 1f;
        
        public float range    = 5f;
        [Range(1f, 179f)] public float spotAngle = 30f;
        public Vector2 areaSize = new Vector2(2f, 2f); // Area Light 的長寬尺寸
        public LayerMask cullingMask = (1 << 8) | (1 << 9) | (1 << 12) | (1 << 13) | (1 << 18) | (1 << 20);

        // ── Cookie ────────────────────────────────────────────────
        public Texture2D    cookieTexture;
        public CookieMode   cookieMode    = CookieMode.Normal;
        public ColorChannel colorChannel  = ColorChannel.RGBA;
        public Vector2 cookieScale  = new Vector2(1, 1);
        public Vector2 cookieOffset = Vector2.zero;
        public bool    autoScroll   = false;

        private Vector2 currentScrollOffset = Vector2.zero;

    // 嚴格隔離：編輯器內的計時器變數，包版時不參與編譯
    #if UNITY_EDITOR
        private double lastEditorTime = 0;
    #endif

        private void Reset()
        {
            // 預設 Layer：8, 9, 12, 13, 18, 20
            cullingMask = (1 << 8) | (1 << 9) | (1 << 12) | (1 << 13) | (1 << 18) | (1 << 20);
        }

        void OnEnable()
        {
    #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                lastEditorTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += EditorUpdate;
            }
    #endif
        }

        void OnDisable()
        {
    #if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorApplication.update -= EditorUpdate;
    #endif
        }

        void Update()
        {
            if (Application.isPlaying && autoScroll)
                currentScrollOffset += cookieOffset * Time.deltaTime;
        }

    // 嚴格隔離：Scene View 的即時預覽更新邏輯
    #if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (!Application.isPlaying && autoScroll)
            {
                double now = EditorApplication.timeSinceStartup;
                float dt = (float)(now - lastEditorTime);
                lastEditorTime = now;
                currentScrollOffset += cookieOffset * dt;
                SceneView.RepaintAll();
            }
            else lastEditorTime = EditorApplication.timeSinceStartup;
        }
    #endif

        public Vector2 GetCurrentCookieOffset() => autoScroll ? currentScrollOffset : cookieOffset;

        private void OnDrawGizmos()
        {
            Color baseColor = ResolveGizmoColor();
            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.4f);

            if (type == LightType.Point)
                Gizmos.DrawWireSphere(transform.position, range);
            else if (type == LightType.Spot)
                DrawSpotGizmo(baseColor, 0.4f);
            else if (type == LightType.Area)
                DrawAreaGizmo(baseColor, 0.4f);
        }

        private void OnDrawGizmosSelected()
        {
            Color baseColor = ResolveGizmoColor();
            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1.0f);

            if (type == LightType.Point)
                Gizmos.DrawWireSphere(transform.position, range);
            else if (type == LightType.Spot)
                DrawSpotGizmo(baseColor, 1.0f);
            else if (type == LightType.Area)
                DrawAreaGizmo(baseColor, 1.0f);
        }

        private Color ResolveGizmoColor()
        {
            Color c = new Color(color.r, color.g, color.b, 1f);
            return (c.r < 0.01f && c.g < 0.01f && c.b < 0.01f)
                ? new Color(0.9f, 0.8f, 0.1f, 1f) : c;
        }

        private void DrawSpotGizmo(Color col, float alpha)
        {
            Vector3 origin      = transform.position;
            Vector3 forward     = transform.forward;
            float   outerRadius = Mathf.Tan(spotAngle * 0.5f * Mathf.Deg2Rad) * range;
            Vector3 discCenter  = origin + forward * range;

            Gizmos.color = new Color(col.r, col.g, col.b, alpha);
            Gizmos.DrawLine(origin, discCenter + transform.up    * outerRadius);
            Gizmos.DrawLine(origin, discCenter - transform.up    * outerRadius);
            Gizmos.DrawLine(origin, discCenter + transform.right * outerRadius);
            Gizmos.DrawLine(origin, discCenter - transform.right * outerRadius);

    #if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(col.r, col.g, col.b, alpha);
            UnityEditor.Handles.DrawWireDisc(discCenter, forward, outerRadius);
    #endif
        }

        private void DrawAreaGizmo(Color col, float alpha)
        {
            Gizmos.color = new Color(col.r, col.g, col.b, alpha);
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.forward * (range * 0.5f), transform.rotation, new Vector3(areaSize.x, areaSize.y, range));
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = oldMatrix;
        }

    #if UNITY_EDITOR
        [MenuItem("GameObject/Light/Spine Shadow Light", false, 10)]
        static void CreateShadowLight(MenuCommand cmd)
        {
            GameObject go = new GameObject("Spine Shadow Light");
            go.AddComponent<SpineShadowLight>();
            GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Spine Shadow Light");
            Selection.activeObject = go;
        }
    #endif
    }

    // ══════════════════════════════════════════════════════════════════
    //  以下全數為編輯器介面與指南視窗，包版時將被完全剔除
    // ══════════════════════════════════════════════════════════════════
    #if UNITY_EDITOR
    [CustomEditor(typeof(SpineShadowLight))]
    [CanEditMultipleObjects]
    public class SpineShadowLightEditor : Editor
    {
        private static readonly Color AccentLight  = new Color(1.00f, 0.85f, 0.20f);
        private static readonly Color AccentCookie = new Color(0.40f, 0.75f, 1.00f);
        private static readonly Color AccentScroll = new Color(0.55f, 1.00f, 0.65f);

        private static bool showLight  = true;
        private static bool showCookie = true;

        SerializedProperty _type, _blendMode, _color, _intensity, _spineIntensity, _terrainIntensity, _range, _spotAngle, _areaSize, _cullingMask;
        SerializedProperty _cookieTexture, _cookieMode, _colorChannel, _cookieScale, _cookieOffset, _autoScroll;

        void OnEnable()
        {
            _type          = serializedObject.FindProperty("type");
            _blendMode     = serializedObject.FindProperty("blendMode");
            _color         = serializedObject.FindProperty("color");
            _intensity     = serializedObject.FindProperty("intensity");
            _spineIntensity = serializedObject.FindProperty("spineIntensity");
            _terrainIntensity = serializedObject.FindProperty("terrainIntensity");
            _range         = serializedObject.FindProperty("range");
            _spotAngle     = serializedObject.FindProperty("spotAngle");
            _areaSize      = serializedObject.FindProperty("areaSize");
            _cullingMask   = serializedObject.FindProperty("cullingMask");
            _cookieTexture = serializedObject.FindProperty("cookieTexture");
            _cookieMode    = serializedObject.FindProperty("cookieMode");
            _colorChannel  = serializedObject.FindProperty("colorChannel");
            _cookieScale   = serializedObject.FindProperty("cookieScale");
            _cookieOffset  = serializedObject.FindProperty("cookieOffset");
            _autoScroll    = serializedObject.FindProperty("autoScroll");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTitleBar();
            DrawTopHelpButton();

            DrawSectionHeader(ref showLight, "  💡  Light Settings", AccentLight);
            if (showLight)
            {
                BeginSection();
                DrawProp(_type, new GUIContent("Type", "Point：全向光　Spot：聚光燈　Area：體積區域光"));
                DrawProp(_blendMode, new GUIContent("Blend Mode", "光照與底色的混合方式"));
                GUILayout.Space(4);
                DrawProp(_color, new GUIContent("Color  (HDR)", "顏色，支援 HDR；大於 1 可觸發 Bloom。"));
                GUILayout.Space(4);
                DrawProp(_intensity, new GUIContent("Master Intensity", "燈光的總強度倍率"));
                
                EditorGUI.indentLevel++;
                DrawProp(_spineIntensity, new GUIContent("Spine / Sprite Intensity", "針對 Spine 與 Sprite 渲染的強度倍率"));
                DrawProp(_terrainIntensity, new GUIContent("Terrain Intensity", "針對地形 (Terrain) 渲染的強度倍率"));
                EditorGUI.indentLevel--;
                
                GUILayout.Space(4);
                DrawProp(_range, new GUIContent("Range"));
                
                if (_type.intValue == (int)SpineShadowLight.LightType.Spot)
                {
                    GUILayout.Space(4);
                    DrawProp(_spotAngle, new GUIContent("Spot Angle"));
                }
                else if (_type.intValue == (int)SpineShadowLight.LightType.Area)
                {
                    GUILayout.Space(4);
                    DrawProp(_areaSize, new GUIContent("Area Size", "控制長方體發光體積的寬度與高度"));
                }
                
                GUILayout.Space(4);
                DrawProp(_cullingMask, new GUIContent("Culling Mask", "必須包含角色所在的 Layer 才會生效"));
                EndSection();
            }

            GUILayout.Space(4);

            DrawSectionHeader(ref showCookie, "  🍪  Cookie Settings", AccentCookie);
            if (showCookie)
            {
                BeginSection();
                DrawProp(_cookieMode, new GUIContent("Cookie Mode"));
                bool isCaustics = _cookieMode.intValue == (int)SpineShadowLight.CookieMode.Caustics;

                if (isCaustics)
                    EditorGUILayout.HelpBox(
                        "Caustics 模式：建議使用 Voronoi（細胞雜訊）或水波黑白貼圖以獲得最佳焦散效果。",
                        MessageType.Info);

                GUILayout.Space(4);
                DrawProp(_cookieTexture, new GUIContent("Cookie Texture"));

                bool texEmpty = _cookieTexture.objectReferenceValue == null;
                if (isCaustics && texEmpty)
                {
                    Texture2D causticDefault = Resources.Load<Texture2D>("Effect/Effect_WBB/Textures/Caustics/T_Caustics_0100");
                    if (causticDefault != null)
                    {
                        _cookieTexture.objectReferenceValue = causticDefault;
                        serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.HelpBox("已自動套用預設 Caustics 貼圖：T_Caustics_0100", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            "Caustics 模式已啟用，但找不到預設貼圖。\n請手動指定一張 Voronoi / 水波貼圖至 Cookie Texture 欄位。",
                            MessageType.Warning);
                    }
                }

                if (_cookieTexture.objectReferenceValue != null)
                {
                    GUILayout.Space(6);
                    DrawProp(_colorChannel, new GUIContent("Color Channel", "萃取哪個通道作為 Cookie 遮罩"));
                    GUILayout.Space(4);
                    DrawProp(_cookieScale, new GUIContent("Scale  (X,Y)"));
                    GUILayout.Space(4);
                    DrawProp(_autoScroll, new GUIContent("Auto Scroll", "啟用後，Offset 轉為每秒流動速度（UV/秒）"));
                    
                    if (_autoScroll.boolValue)
                        DrawScrollSpeedField();
                    else
                        DrawProp(_cookieOffset, new GUIContent("Offset  (X,Y)", "靜態 UV 偏移"));
                }

                EndSection();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProp(SerializedProperty prop, GUIContent content)
        {
            EditorGUILayout.PropertyField(prop, content);
            Rect rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("📖 查看使用說明"), false, () => {
                    SpineShadowLightHelpWindow.ShowWindowAndScrollTo(prop.name);
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        private void DrawTopHelpButton()
        {
            GUILayout.Space(5);
            Rect r = GUILayoutUtility.GetRect(10f, 26f, GUILayout.ExpandWidth(true));
            Event e = Event.current; bool hover = r.Contains(e.mousePosition);
            Color accent = new Color(0.9f, 0.4f, 0.4f); 

            Color bg = EditorGUIUtility.isProSkin
                ? (hover ? new Color(.25f,.25f,.25f) : new Color(.20f,.20f,.20f))
                : (hover ? new Color(.90f,.90f,.90f) : new Color(.85f,.85f,.85f));
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax-1f, r.width, 1f),
                EditorGUIUtility.isProSkin ? new Color(.12f,.12f,.12f) : new Color(.6f,.6f,.6f));
            EditorGUI.DrawRect(new Rect(r.x, r.y, 4f, r.height), accent);

            if (e.type == EventType.MouseDown && e.button == 0 && hover) {
                SpineShadowLightHelpWindow.ShowWindow();
                e.Use();
            }

            var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            ts.normal.textColor = EditorGUIUtility.isProSkin ? accent : Color.Lerp(accent, Color.black, 0.4f);
            EditorGUI.LabelField(r, "▤ 場景燈光與濾鏡 使用指南", ts);
            GUILayout.Space(6);
        }

        private void OnSceneGUI()
        {
            SpineShadowLight sl = (SpineShadowLight)target;
            if (!sl.isActiveAndEnabled) return;

            Color hc = sl.color == Color.black ? new Color(.2f, .2f, .2f, 1f) : sl.color;
            hc.a = 1f;
            Handles.color = hc;

            if (sl.type == SpineShadowLight.LightType.Point)
            {
                EditorGUI.BeginChangeCheck();
                float nr = Handles.RadiusHandle(Quaternion.identity, sl.transform.position, sl.range);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(sl, "Change Shadow Light Range");
                    sl.range = Mathf.Max(0.1f, nr);
                }
            }
            else if (sl.type == SpineShadowLight.LightType.Spot)
            {
                Vector3 fwd    = sl.transform.forward;
                Vector3 up     = sl.transform.up;
                Vector3 right  = sl.transform.right;
                Vector3 origin = sl.transform.position;

                float   radius     = Mathf.Tan(sl.spotAngle * 0.5f * Mathf.Deg2Rad) * sl.range;
                Vector3 discCenter = origin + fwd * sl.range;
                float   handleSize = HandleUtility.GetHandleSize(discCenter) * 0.06f * 0.5f;

                EditorGUI.BeginChangeCheck();
                Vector3 newDiscCenter = Handles.FreeMoveHandle(
                    discCenter, Quaternion.identity, handleSize, Vector3.zero, Handles.RectangleHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(sl, "Change Shadow Light Range");
                    float newRange = Vector3.Dot(newDiscCenter - origin, fwd);
                    sl.range = Mathf.Max(0.1f, newRange);
                }

                Vector3[] edgeDirs = { up, -up, right, -right };
                for (int i = 0; i < 4; i++)
                {
                    Vector3 edgePt = discCenter + edgeDirs[i] * radius;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPt = Handles.FreeMoveHandle(
                        edgePt, Quaternion.identity, handleSize, Vector3.zero, Handles.RectangleHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        float newRadius = Mathf.Max(0.001f, Mathf.Abs(Vector3.Dot(newPt - discCenter, edgeDirs[i])));
                        Undo.RecordObject(sl, "Change Shadow Light Spot Angle");
                        sl.spotAngle = Mathf.Clamp(Mathf.Atan2(newRadius, sl.range) * Mathf.Rad2Deg * 2f, 1f, 179f);
                        break;
                    }
                }
            }
            else if (sl.type == SpineShadowLight.LightType.Area)
            {
                Vector3 fwd   = sl.transform.forward;
                Vector3 right = sl.transform.right;
                Vector3 up    = sl.transform.up;
                Vector3 center = sl.transform.position + fwd * (sl.range * 0.5f);
                float handleSize = HandleUtility.GetHandleSize(center) * 0.06f;

                EditorGUI.BeginChangeCheck();
                Vector3 rangePt = Handles.FreeMoveHandle(
                    sl.transform.position + fwd * sl.range, Quaternion.identity, handleSize, Vector3.zero, Handles.RectangleHandleCap);
                Vector3 rightPt = Handles.FreeMoveHandle(
                    sl.transform.position + right * (sl.areaSize.x * 0.5f), Quaternion.identity, handleSize, Vector3.zero, Handles.RectangleHandleCap);
                Vector3 upPt = Handles.FreeMoveHandle(
                    sl.transform.position + up * (sl.areaSize.y * 0.5f), Quaternion.identity, handleSize, Vector3.zero, Handles.RectangleHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(sl, "Change Area Light Bounds");
                    sl.range = Mathf.Max(0.1f, Vector3.Dot(rangePt - sl.transform.position, fwd));
                    sl.areaSize.x = Mathf.Max(0.1f, Vector3.Dot(rightPt - sl.transform.position, right) * 2f);
                    sl.areaSize.y = Mathf.Max(0.1f, Vector3.Dot(upPt - sl.transform.position, up) * 2f);
                }
            }
        }

        private void DrawTitleBar()
        {
            Rect r = GUILayoutUtility.GetRect(10f, 36f, GUILayout.ExpandWidth(true));
            Color bg = EditorGUIUtility.isProSkin ? new Color(.14f, .14f, .14f) : new Color(.78f, .78f, .78f);
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 3f), AccentLight);
            var s = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
            s.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(AccentLight, Color.white, .35f) : Color.Lerp(AccentLight, Color.black, .45f);
            EditorGUI.LabelField(new Rect(r.x, r.y + 3f, r.width, r.height - 3f), "✦  Spine Shadow Light  ✦", s);
        }

        private void DrawSectionHeader(ref bool foldout, string title, Color accent)
        {
            Rect  r = GUILayoutUtility.GetRect(10f, 28f, GUILayout.ExpandWidth(true));
            Event e = Event.current; bool  hover = r.Contains(e.mousePosition);

            Color bg = EditorGUIUtility.isProSkin
                ? (hover ? new Color(.25f, .25f, .25f) : new Color(.20f, .20f, .20f))
                : (hover ? new Color(.88f, .88f, .88f) : new Color(.82f, .82f, .82f));
            EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.y, 4f, r.height), accent);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1f, r.width, 1f),
                EditorGUIUtility.isProSkin ? new Color(.12f, .12f, .12f) : new Color(.60f, .60f, .60f));

            if (e.type == EventType.MouseDown && hover) { foldout = !foldout; e.Use(); }

            var ts = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, fontSize = 12 };
            ts.normal.textColor = foldout
                ? (EditorGUIUtility.isProSkin ? Color.Lerp(accent, Color.white, .25f) : Color.Lerp(accent, Color.black, .35f))
                : (EditorGUIUtility.isProSkin ? new Color(.55f, .55f, .55f) : new Color(.40f, .40f, .40f));
            EditorGUI.LabelField(new Rect(r.x + 14f, r.y, r.width - 34f, r.height), title, ts);

            var ic = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
            ic.normal.textColor = ts.normal.textColor;
            EditorGUI.LabelField(new Rect(r.xMax - 24f, r.y, 22f, r.height), foldout ? "−" : "＋", ic);
        }

        private static void BeginSection()
        {
            var style = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(10, 10, 8, 10), margin = new RectOffset(0, 0, 2, 0) };
            EditorGUILayout.BeginVertical(style);
            GUILayout.Space(2);
        }

        private static void EndSection()
        {
            GUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        private void DrawScrollSpeedField()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(2);
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.Lerp(AccentScroll, Color.white, .25f) : Color.Lerp(AccentScroll, Color.black, .35f);
            EditorGUILayout.LabelField(new GUIContent("⟳  Scroll Speed (X,Y)", "每秒流動的 UV 距離"), labelStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUI.BeginChangeCheck();
            float x = EditorGUILayout.FloatField(_cookieOffset.vector2Value.x);
            float y = EditorGUILayout.FloatField(_cookieOffset.vector2Value.y);
            if (EditorGUI.EndChangeCheck()) _cookieOffset.vector2Value = new Vector2(x, y);
            EditorGUILayout.EndHorizontal();

            Rect rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition)) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("📖 查看使用說明"), false, () => { SpineShadowLightHelpWindow.ShowWindowAndScrollTo("autoScroll"); });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }
    }

    public class SpineShadowLightHelpWindow : EditorWindow
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
            var window = GetWindow<SpineShadowLightHelpWindow>("Shadow Light 指南");
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
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Label("🔍", GUILayout.Width(20));
            
            Rect searchRect = GUILayoutUtility.GetRect(100, 24, GUILayout.ExpandWidth(true));
            GUI.Box(searchRect, "", new GUIStyle("HelpBox"));
            
            GUIStyle tfStyle = new GUIStyle(GUIStyle.none) {
                margin = new RectOffset(0,0,0,0), padding = new RectOffset(5,5,3,3),
                fontSize = 12, alignment = TextAnchor.MiddleLeft
            };
            tfStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            tfStyle.focused.textColor = tfStyle.normal.textColor;
            tfStyle.active.textColor = tfStyle.normal.textColor;
            tfStyle.hover.textColor = tfStyle.normal.textColor;
            
            Rect textRect = new Rect(searchRect.x + 5, searchRect.y + 2, searchRect.width - 30, searchRect.height - 4);
            
            if (string.IsNullOrEmpty(searchQuery)) {
                GUIStyle placeholderStyle = new GUIStyle(tfStyle) { fontStyle = FontStyle.Italic };
                placeholderStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(textRect, "輸入關鍵字搜尋效果或屬性...", placeholderStyle);
            }

            searchQuery = GUI.TextField(textRect, searchQuery, tfStyle);
            
            if (!string.IsNullOrEmpty(searchQuery)) {
                if (GUI.Button(new Rect(searchRect.xMax - 22, searchRect.y + 4, 16, 16), EditorGUIUtility.IconContent("Clear"), GUIStyle.none)) {
                    searchQuery = "";
                    GUI.FocusControl(null); 
                }
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.largeLabel) { fontSize = 16, fontStyle = FontStyle.Bold, margin = new RectOffset(10, 10, 10, 15) };
            GUILayout.Label("Spine Shadow Light 美術場景燈光指南", titleStyle);

            EditorGUILayout.HelpBox(
                "【系統運作限制與核心依賴】\n" +
                "1. 極限數量：場景中同一個角色最多只能同時受到 8 盞 Shadow Light 的影響。\n" +
                "2. 腳本依賴：角色材質球必須開啟 [Enable Shadow Light Base]，且場景必須具備 SpineLightManager 腳本方可運作。\n" +
                "3. 效能優勢：本燈光系統完全跳脫 Unity 原生燈光限制，不會打斷 Batching，適合大量發射體或場景濾鏡使用。", MessageType.Info);
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
                GUILayout.Label($"<b><color=#D45050>[除錯檢查]</color></b> {prop.bugFix}", contentStyle);
            }
            EditorGUI.indentLevel--;
            
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        private void InitDocs()
        {
            docModules = new List<HelpDocModule>();

            var modLight = new HelpDocModule { moduleName = "Light Settings (基礎光照設定)", accentColor = new Color(1.00f, 0.85f, 0.20f), isExpanded = false };
            modLight.props.Add(new HelpDocProp { propName = "type", friendlyName = "Type (光源類型)", 
                desc = "決定光照的形狀範圍：\n• <b>Point (點光):</b> 向四面八方發散，適合火把、子彈。\n• <b>Spot (聚光):</b> 錐形範圍，適合路燈、手電筒。\n• <b>Area (區域光):</b> 長方體體積光，適用於大範圍均勻的場景變色或濾鏡特效。", dependency = "", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "blendMode", friendlyName = "Blend Mode (混合模式)", 
                desc = "決定光照與角色底色的疊加公式：\n• <b>Multiply:</b> 將光色與底色相乘，常用於製作場景環境的「陰影層」。\n• <b>Add:</b> 純粹的發光提亮。\n• <b>Screen:</b> 較柔和的發光提亮，不會過曝死白。\n• <b>Overlay:</b> 保留對比度的高飽和度疊加。", dependency = "", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "color", friendlyName = "Color (光照顏色)", 
                desc = "光源的本體顏色。支援 HDR，如果色彩亮度大於 1，可使角色產生 Bloom 泛光效果。", dependency = "", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "intensity", friendlyName = "Master Intensity (亮度倍率)", 
                desc = "整體光照強度的放大倍率。", dependency = "", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "spineIntensity", friendlyName = "Spine/Sprite Intensity", 
                desc = "針對人物 Spine 或 Sprite 的受光強度倍率。", dependency = "", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "terrainIntensity", friendlyName = "Terrain Intensity", 
                desc = "針對地形 (Terrain) 渲染的受光強度倍率。", dependency = "", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "range", friendlyName = "Range (影響距離)", 
                desc = "光源能照亮的極限距離。超出此範圍的角色將完全不受此燈光影響。", dependency = "", bugFix = "燈光無效時，請優先檢查 Range 是否太小，沒有涵蓋到目標角色。" });
            modLight.props.Add(new HelpDocProp { propName = "spotAngle", friendlyName = "Spot Angle (聚光角度)", 
                desc = "設定聚光燈的圓錐張角 (1~179度)。", dependency = "只有 Type 選擇 Spot 時生效。", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "areaSize", friendlyName = "Area Size (區域長寬)", 
                desc = "設定體積區域光的 X(寬) 與 Y(高)，Z 軸深度則由 Range 參數決定。", dependency = "只有 Type 選擇 Area 時生效。", bugFix = "" });
            modLight.props.Add(new HelpDocProp { propName = "cullingMask", friendlyName = "Culling Mask (圖層過濾)", 
                desc = "【極度重要】決定這盞燈只照亮哪幾個 Layer 的物件。", dependency = "", bugFix = "如果燈光打在角色身上完全沒反應，請 100% 確認此 Mask 包含角色物件所在的 Layer (通常是 8, 9, 12, 13, 18, 20)。" });
            docModules.Add(modLight);

            var modCookie = new HelpDocModule { moduleName = "Cookie Settings (投影與焦散)", accentColor = new Color(0.40f, 0.75f, 1.00f), isExpanded = false };
            modCookie.props.Add(new HelpDocProp { propName = "cookieMode", friendlyName = "Cookie Mode (投影模式)", 
                desc = "• <b>Normal:</b> 靜態的單層貼圖投影，如同蝙蝠俠標誌的探照燈。\n• <b>Caustics:</b> 針對水波紋優化，會自動疊加兩層貼圖並產生錯位的動態折射焦散效果。", dependency = "", bugFix = "" });
            modCookie.props.Add(new HelpDocProp { propName = "cookieTexture", friendlyName = "Cookie Texture (投影貼圖)", 
                desc = "指定要投影在角色身上的黑白或彩色貼圖圖樣。", dependency = "若未指定貼圖，Cookie 系統不會啟動。", bugFix = "在 Caustics 模式下建議使用細胞雜訊 (Voronoi) 貼圖，系統會自動嘗試載入預設的水波紋路徑。" });
            modCookie.props.Add(new HelpDocProp { propName = "colorChannel", friendlyName = "Color Channel (遮罩通道)", 
                desc = "如果您的貼圖是灰階圖被包裝在特定通道裡，可用此選項單獨萃取 R、G、B 或 Alpha 作為強度遮罩。", dependency = "需指定 Cookie Texture。", bugFix = "" });
            modCookie.props.Add(new HelpDocProp { propName = "cookieScale", friendlyName = "Scale (投影縮放)", 
                desc = "投影圖樣在空間中的縮放比例，數值越小，投射在角色身上的圖案越大。", dependency = "需指定 Cookie Texture。", bugFix = "" });
            modCookie.props.Add(new HelpDocProp { propName = "autoScroll", friendlyName = "Auto Scroll (自動流動)", 
                desc = "啟用後，貼圖會自動隨著時間平移滑動，極適合用來製作樹蔭隨風飄動或水波流動。", dependency = "需指定 Cookie Texture。", bugFix = "" });
            modCookie.props.Add(new HelpDocProp { propName = "cookieOffset", friendlyName = "Offset / Scroll Speed", 
                desc = "• 未開啟 Auto Scroll 時：作為靜態的 UV 偏移座標。\n• 開啟 Auto Scroll 時：轉變為每秒在 X 與 Y 方向的流動速度。", dependency = "需指定 Cookie Texture。", bugFix = "" });
            docModules.Add(modCookie);
        }
    }
    #endif
}