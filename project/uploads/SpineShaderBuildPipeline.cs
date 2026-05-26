#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class SpineShaderBuildPipeline : IPreprocessBuildWithReport, IPreprocessShaders
{
    public int callbackOrder => 0;

    public const string ShaderName = "Spine/Skeleton";
    public const string SessionKeyEditorMode = "SpineShader_IsEditorDefault";
    private const string ConfigPathPrefix = "Assets/Scripts/VFXTool/BuildConfigs/SpineSkeletonShaderTool/ShaderConfig_";

    public static readonly string[] AllModuleKeywords =
    {
        "_MODULE_NATIVE_LIGHTING", "_MODULE_SHADOW_LIGHT", "_MODULE_LIGHT_PROBE", "_MODULE_NORMALMAP",
        "_MODULE_BACKLIGHT", "_MODULE_EMISSION", "_MODULE_INLINE", "_MODULE_OUTLINE",
        "_MODULE_HITSWEEP", "_MODULE_EFFECT_OVERLAY", "_MODULE_GLOBAL_NOISE", "_MODULE_BLOOM_SUPPRESSION",
        "_USE8NEIGHBOURHOOD_ON", "_USE_SCREENSPACE_OUTLINE_WIDTH", "_OUTLINE_FILL_INSIDE"
    };

    // 用於雙重剔除的記憶體快取 (打包時用)
    private static HashSet<string> _allowedByPlatform = new HashSet<string>();
    private static HashSet<string> _usedByMaterials = new HashSet<string>();

    // ==========================================
    // 階段一：Build 開始前 (打包掃描)
    // ==========================================
    public void OnPreprocessBuild(BuildReport report)
    {
        string platform = report.summary.platform.ToString();
        Debug.Log($"[SpineShaderBuildPipeline] === 開始建置前處理 ({platform}) ===");
        
        LoadPlatformConfig(platform);
        ScanAllMaterials();
    }

    private void LoadPlatformConfig(string platform)
    {
        _allowedByPlatform.Clear();
        var config = AssetDatabase.LoadAssetAtPath<SpineShaderBuildConfig>($"{ConfigPathPrefix}{platform}.asset");
        
        if (config != null) {
            foreach(var kw in config.GetEnabledKeywords()) _allowedByPlatform.Add(kw);
        } else {
            Debug.LogWarning($"[SpineShaderBuildPipeline] 找不到 {platform} 設定檔，預設開放所有 Keywords！");
            foreach(var kw in AllModuleKeywords) _allowedByPlatform.Add(kw);
        }
    }

    private void ScanAllMaterials()
    {
        _usedByMaterials.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids) {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat != null && mat.shader != null && mat.shader.name == ShaderName) {
                foreach (string kw in mat.shaderKeywords) _usedByMaterials.Add(kw);
            }
        }
    }

    // ==========================================
    // 階段二：Shader 變體編譯攔截 (雙重剔除)
    // ==========================================
    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (shader.name != ShaderName) return;

        bool outlineAllowedAndUsed = _allowedByPlatform.Contains("_MODULE_OUTLINE") && _usedByMaterials.Contains("_MODULE_OUTLINE");
        if (snippet.passName == "Outline" && !outlineAllowedAndUsed) { data.Clear(); return; }

        bool inlineAllowedAndUsed = _allowedByPlatform.Contains("_MODULE_INLINE") && _usedByMaterials.Contains("_MODULE_INLINE");
        if (snippet.passName == "InlineAlways" && !inlineAllowedAndUsed) { data.Clear(); return; }

        for (int i = data.Count - 1; i >= 0; i--)
        {
            var keywordSet = data[i].shaderKeywordSet;
            bool shouldStrip = false;

            foreach (string kw in AllModuleKeywords) {
                if (keywordSet.IsEnabled(new ShaderKeyword(shader, kw))) {
                    if (!_allowedByPlatform.Contains(kw) || !_usedByMaterials.Contains(kw)) {
                        shouldStrip = true; break;
                    }
                }
            }
            if (shouldStrip) data.RemoveAt(i);
        }
    }

    // ===============================================
    // 編輯器 UI 供需的 API (已加入智能刷新機制)
    // ===============================================

    public static void SyncKeywordsForTarget(string targetPlatformName)
    {
        var config = AssetDatabase.LoadAssetAtPath<SpineShaderBuildConfig>($"{ConfigPathPrefix}{targetPlatformName}.asset");
        if (config != null)
        {
            SyncForPlatformConfig(config);
            Debug.Log($"[Spine 變體系統] 成功切換並刷新編輯器至: {targetPlatformName}");
        }
    }

    public static bool IsEditorDefaultMode() => SessionState.GetBool(SessionKeyEditorMode, true);

    public static void SyncAllEnabled()
    {
        SessionState.SetBool(SessionKeyEditorMode, true);
        SessionState.SetString("SpineShader_ActiveConfigPath", ""); 
        
        ApplyAllKeywordsToAllMaterials();
        Debug.Log("[Spine 變體系統] 恢復 Editor Default / 全效預覽模式，已強制開啟全專案 Spine 材質球的所有 Shader Keywords。");
    }

    public static void SyncForPlatformConfig(SpineShaderBuildConfig config)
    {
        if (config == null) return;
        SessionState.SetBool(SessionKeyEditorMode, false);
        SessionState.SetString("SpineShader_ActiveConfigPath", AssetDatabase.GetAssetPath(config));
        
        ApplyKeywordsToAllMaterials(new HashSet<string>(config.GetEnabledKeywords()));
    }

    // --- 核心修復：安全、智能地刷新全專案材質球 ---
    private static void ApplyKeywordsToAllMaterials(HashSet<string> allowedByConfig)
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;
        
        foreach (string guid in guids)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat == null || mat.shader == null || mat.shader.name != ShaderName) continue;

            foreach (string kw in AllModuleKeywords)
            {
                bool isAllowedByConfig = allowedByConfig.Contains(kw);
                bool isToggledByUser = IsFeatureToggledOn(mat, kw);

                // 雙重鎖驗證：只有當「平台允許」且「美術有打勾」時，才真正開啟 Keyword
                if (isAllowedByConfig && isToggledByUser) 
                    mat.EnableKeyword(kw);
                else 
                    mat.DisableKeyword(kw);
            }
            
            EditorUtility.SetDirty(mat);
            count++;
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"[Spine 變體系統] 已成功刷新全專案共 {count} 顆 Spine 材質球！");
    }

    private static void ApplyAllKeywordsToAllMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;

        foreach (string guid in guids)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat == null || mat.shader == null || mat.shader.name != ShaderName) continue;

            foreach (string kw in AllModuleKeywords)
            {
                mat.EnableKeyword(kw);
            }

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Spine 變體系統] Editor Default 已套用全效預覽：共 {count} 顆 Spine 材質球已開啟所有 Shader Keywords。");
    }

    // 建立 Keyword 與 Float Toggle 的對應關係
    private static bool IsFeatureToggledOn(Material mat, string keyword)
    {
        switch (keyword)
        {
            case "_MODULE_NATIVE_LIGHTING": return mat.HasProperty("_EnableNativeLighting") && mat.GetFloat("_EnableNativeLighting") > 0.5f;
            case "_MODULE_SHADOW_LIGHT": return mat.HasProperty("_EnableShadowLight") && mat.GetFloat("_EnableShadowLight") > 0.5f;
            case "_MODULE_LIGHT_PROBE": return mat.HasProperty("_EnableLightProbe") && mat.GetFloat("_EnableLightProbe") > 0.5f;
            case "_MODULE_NORMALMAP": return mat.HasProperty("_EnableNormalMap") && mat.GetFloat("_EnableNormalMap") > 0.5f;
            case "_MODULE_BACKLIGHT": return mat.HasProperty("_EnableBackLight") && mat.GetFloat("_EnableBackLight") > 0.5f;
            case "_MODULE_EMISSION": return mat.HasProperty("_EnableEmission") && mat.GetFloat("_EnableEmission") > 0.5f;
            case "_MODULE_INLINE": return mat.HasProperty("_EnableInline") && mat.GetFloat("_EnableInline") > 0.5f;
            case "_MODULE_OUTLINE": return mat.HasProperty("_EnableOutline") && mat.GetFloat("_EnableOutline") > 0.5f;
            case "_MODULE_HITSWEEP": return mat.HasProperty("_EnableHitSweep") && mat.GetFloat("_EnableHitSweep") > 0.5f;
            case "_MODULE_EFFECT_OVERLAY": return mat.HasProperty("_EffectIntensity") && mat.GetFloat("_EffectIntensity") > 0.001f;
            case "_MODULE_GLOBAL_NOISE": return mat.HasProperty("_EnableGlobalNoise") && mat.GetFloat("_EnableGlobalNoise") > 0.5f;
            case "_MODULE_BLOOM_SUPPRESSION": return mat.HasProperty("_EnableBloomSuppression") && mat.GetFloat("_EnableBloomSuppression") > 0.5f;
            case "_USE8NEIGHBOURHOOD_ON": return mat.HasProperty("_Use8Neighbourhood") && mat.GetFloat("_Use8Neighbourhood") > 0.5f;
            case "_USE_SCREENSPACE_OUTLINE_WIDTH": return mat.HasProperty("_UseScreenSpaceOutlineWidth") && mat.GetFloat("_UseScreenSpaceOutlineWidth") > 0.5f;
            case "_OUTLINE_FILL_INSIDE": return mat.HasProperty("_Fill") && mat.GetFloat("_Fill") > 0.5f;
        }
        return false;
    }
}

// =================================================================================
// 編輯器視窗 (與您提供的原版相同)
// =================================================================================
public class SpinePlatformManagerWindow : EditorWindow
{
    private static List<SpineShaderBuildConfig> _configs = new List<SpineShaderBuildConfig>();
    private float _scrollX = 0f;
    private float _targetScrollX = 0f;
    private int _selectedIndex = 0;
    private bool _showGuide = false;
    private double _lastTime;
    private Vector2 _editScrollPos;
    private Vector2 _guideScrollPos;

    private bool _isDragging = false;
    private Vector2 _mouseDownPos;
    
    private readonly Color[] _randomColors = new Color[]
    {
        new Color(0.9f, 0.5f, 0.5f), new Color(0.5f, 0.8f, 0.5f), new Color(0.4f, 0.7f, 0.9f), new Color(0.9f, 0.8f, 0.4f),
        new Color(0.8f, 0.5f, 0.9f), new Color(0.4f, 0.9f, 0.8f), new Color(0.9f, 0.6f, 0.4f), new Color(0.9f, 0.5f, 0.7f)
    };

    private const float Spacing = 410f;

    public static void ShowWindow()
    {
        var w = GetWindow<SpinePlatformManagerWindow>("Spine 平台變體管理");
        w.minSize = new Vector2(1000, 800);
        w.RefreshConfigs();
        w.Show();
    }

    private void RefreshConfigs()
    {
        _configs.Clear();
        string[] guids = AssetDatabase.FindAssets("t:SpineShaderBuildConfig");
        foreach (var g in guids)
        {
            var c = AssetDatabase.LoadAssetAtPath<SpineShaderBuildConfig>(AssetDatabase.GUIDToAssetPath(g));
            if (c != null) _configs.Add(c);
        }
        
        if (_selectedIndex > _configs.Count) {
            _selectedIndex = _configs.Count;
            _targetScrollX = _selectedIndex * Spacing;
            _scrollX = _targetScrollX;
        }
    }

    private Color GetPlatformBaseColor(string name)
    {
        string n = name.ToLower();
        if (n.Contains("editor"))  return new Color(1.0f, 0.75f, 0.15f);
        if (n.Contains("xbox"))    return new Color(0.55f, 0.85f, 0.55f);
        if (n.Contains("ps5") || n.Contains("playstation")) return new Color(0.8f, 0.7f, 1.0f);
        if (n.Contains("switch"))  return new Color(1.0f, 0.6f, 0.6f);
        if (n.Contains("android")) return new Color(0.45f, 0.85f, 0.55f);
        if (n.Contains("ios"))     return new Color(0.35f, 0.75f, 1.0f);
        if (n.Contains("windows") || n.Contains("pc")) return new Color(0.25f, 0.55f, 0.95f);
        if (n.Contains("webgl"))   return new Color(1.0f, 0.45f, 0.4f);
        
        int hash = name.GetHashCode();
        int index = Mathf.Abs(hash) % _randomColors.Length;
        return _randomColors[index];
    }

    private Color GetColorForIndex(int index)
    {
        if (index == 0) return GetPlatformBaseColor("editor");
        int cfgIdx = index - 1;
        if (cfgIdx >= 0 && cfgIdx < _configs.Count) {
            if (_configs[cfgIdx] == null) return Color.gray; 
            return GetPlatformBaseColor(_configs[cfgIdx].name.Replace("ShaderConfig_", "").Replace("PC", "Windows"));
        }
        return GetPlatformBaseColor("default");
    }

    private Color GetDynamicInterpolatedColor()
    {
        float progress = _scrollX / Spacing;
        int i1 = Mathf.FloorToInt(progress);
        int i2 = Mathf.CeilToInt(progress);
        
        int maxIdx = _configs.Count;
        i1 = Mathf.Clamp(i1, 0, maxIdx);
        i2 = Mathf.Clamp(i2, 0, maxIdx);
        
        float t = progress - Mathf.Floor(progress);
        if (i1 == i2) return GetColorForIndex(i1);
        
        return Color.Lerp(GetColorForIndex(i1), GetColorForIndex(i2), t);
    }

    private void Update()
    {
        float dt = (float)(EditorApplication.timeSinceStartup - _lastTime);
        _lastTime = EditorApplication.timeSinceStartup;
        if (Mathf.Abs(_scrollX - _targetScrollX) > 0.1f)
        {
            _scrollX = Mathf.Lerp(_scrollX, _targetScrollX, 10f * dt);
            Repaint();
        }
    }

    private void OnGUI()
    {
        bool hasNullConfig = false;
        for (int i = 0; i < _configs.Count; i++) {
            if (_configs[i] == null) {
                hasNullConfig = true; break;
            }
        }

        Color dynamicAccent = hasNullConfig ? new Color(0.9f, 0.3f, 0.3f) : GetDynamicInterpolatedColor();
        Color baseBg = EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.85f, 0.85f, 0.85f);
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.Lerp(baseBg, dynamicAccent, 0.05f));

        DrawTopBar(dynamicAccent);

        if (hasNullConfig) { DrawMissingConfigWarning(dynamicAccent); return; }
        if (_showGuide) { DrawGuideOverlay(dynamicAccent); return; }

        float topBarHeight = 50f, cardWidth = 380f, cardHeight = 620f, gapToButton = 20f, buttonHeight = 45f;
        float availableHeight = position.height - topBarHeight;
        float totalLayoutHeight = cardHeight + gapToButton + buttonHeight;
        float centerY = topBarHeight + (availableHeight - totalLayoutHeight) / 2f + (cardHeight / 2f);
        centerY = Mathf.Max(centerY, topBarHeight + cardHeight / 2f + 10f);
        float centerX = position.width / 2f;

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0) { _mouseDownPos = e.mousePosition; _isDragging = false; }
        if (e.type == EventType.MouseDrag && e.button == 0) {
            if (Vector2.Distance(e.mousePosition, _mouseDownPos) > 5f) {
                _isDragging = true; _targetScrollX -= e.delta.x; _scrollX = _targetScrollX; e.Use();
            }
        }
        if (e.type == EventType.MouseUp && e.button == 0) {
            if (_isDragging) {
                int closest = Mathf.Clamp(Mathf.RoundToInt(_targetScrollX / Spacing), 0, _configs.Count);
                _targetScrollX = closest * Spacing; _selectedIndex = closest; e.Use();
            }
        }

        for (int i = 0; i <= _configs.Count; i++)
        {
            float xPos = centerX - (cardWidth / 2f) + (i * Spacing) - _scrollX;
            if (xPos + cardWidth < 0 || xPos > position.width) continue;

            float distToCenter = Mathf.Abs(xPos + cardWidth / 2f - centerX);
            float scale = Mathf.Clamp01(1f - (distToCenter / 2500f));
            float alpha = Mathf.Clamp01(1f - (distToCenter / 1000f));
            Rect cardRect = new Rect(xPos, centerY - (cardHeight / 2f) * scale, cardWidth * scale, cardHeight * scale);

            if (e.type == EventType.MouseUp && e.button == 0 && cardRect.Contains(e.mousePosition) && !_isDragging) {
                if (_selectedIndex != i) { _selectedIndex = i; _targetScrollX = i * Spacing; e.Use(); }
            }

            string rawName = (i == 0) ? "Editor Default" : _configs[i - 1].name.Replace("ShaderConfig_", "");
            string platformDisplay = rawName.Replace("PC", "Windows");
            Color cardAccent = GetPlatformBaseColor(platformDisplay);

            GUI.color = new Color(1, 1, 1, alpha);
            if (Event.current.type == EventType.Repaint) {
                Color panelBg = EditorGUIUtility.isProSkin ? new Color(0.13f, 0.13f, 0.13f) : new Color(0.96f, 0.96f, 0.96f);
                EditorGUI.DrawRect(cardRect, Color.Lerp(panelBg, cardAccent, 0.08f));
                float bw = 2.5f;
                Color borderCol = (i == _selectedIndex) ? cardAccent : (EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.7f, 0.7f, 0.7f));
                EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, cardRect.width, bw), borderCol);
                EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.yMax - bw, cardRect.width, bw), borderCol);
                EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, bw, cardRect.height), borderCol);
                EditorGUI.DrawRect(new Rect(cardRect.xMax - bw, cardRect.y, bw, cardRect.height), borderCol);
            }

            GUILayout.BeginArea(new Rect(cardRect.x + 20, cardRect.y + 25, cardRect.width - 40, cardRect.height - 50));
            GUILayout.Label($"❖ {platformDisplay}", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 21, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.95f, 0.95f, 0.95f) : Color.black } });
            GUILayout.Space(15);
            if (Event.current.type == EventType.Repaint) EditorGUI.DrawRect(new Rect(20, 35, cardRect.width - 80, 1), new Color(1, 1, 1, 0.1f));
            GUILayout.Space(15);

            if (i == 0) {
                GUILayout.Label("\n\n所有 Shader Feature 強制開啟\n提供編輯器下最完整的預覽環境\n\n(此模式由 ShaderGUI 接管變數設定)", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, wordWrap = true, normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } });
            } else {
                DrawVerticalConfigPanel(_configs[i - 1], i == _selectedIndex, cardAccent, cardWidth - 40f);
            }
            GUILayout.EndArea();
            GUI.color = Color.white;
        }

        string currentName = (_selectedIndex == 0) ? "Editor 預覽模式" : _configs[_selectedIndex - 1].name.Replace("ShaderConfig_", "").Replace("PC", "Windows");
        string btnLabel = $"套用 {currentName}";
        float applyWidth = (new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 }).CalcSize(new GUIContent(btnLabel)).x + 60f;
        float btnY = centerY + (cardHeight / 2f) + gapToButton;

        if (_selectedIndex == 0)
        {
            DrawDynamicButton(new Rect(centerX - applyWidth / 2f, btnY, applyWidth, buttonHeight), btnLabel, () => {
                SpineShaderBuildPipeline.SyncAllEnabled();
            }, dynamicAccent);
        }
        else
        {
            float deleteWidth = 100f, spacing = 15f;
            float totalWidth = applyWidth + spacing + deleteWidth;
            float startX = centerX - totalWidth / 2f;

            DrawDynamicButton(new Rect(startX, btnY, applyWidth, buttonHeight), btnLabel, () => {
                SpineShaderBuildPipeline.SyncForPlatformConfig(_configs[_selectedIndex - 1]);
            }, dynamicAccent);

            DrawDynamicButton(new Rect(startX + applyWidth + spacing, btnY, deleteWidth, buttonHeight), "🗑 移除", () => {
                if (EditorUtility.DisplayDialog("刪除設定檔", $"確定要刪除設定檔 {currentName} 嗎？\n\n注意：此操作會將 Asset 實體檔案直接刪除，且無法復原！", "確定刪除", "取消"))
                {
                    string assetPath = AssetDatabase.GetAssetPath(_configs[_selectedIndex - 1]);
                    AssetDatabase.DeleteAsset(assetPath);
                    RefreshConfigs();
                    GUIUtility.ExitGUI();
                }
            }, new Color(0.9f, 0.35f, 0.35f));
        }
    }

    private void DrawMissingConfigWarning(Color dynamicAccent)
    {
        _isDragging = false; 
        float panelWidth = 400f, panelHeight = 200f;
        Rect panelRect = new Rect(position.width / 2f - panelWidth / 2f, position.height / 2f - panelHeight / 2f, panelWidth, panelHeight);
        
        EditorGUI.DrawRect(panelRect, EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.9f, 0.9f, 0.9f));
        EditorGUI.DrawRect(new Rect(panelRect.x, panelRect.y, panelRect.width, 3f), dynamicAccent);
        
        GUILayout.BeginArea(new Rect(panelRect.x + 20, panelRect.y + 30, panelRect.width - 40, panelRect.height - 40));
        GUILayout.Label("⚠️ 發生異常 (MissingReferenceException)", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter, normal = { textColor = dynamicAccent } });
        GUILayout.Space(20);
        GUIStyle msgStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 13, alignment = TextAnchor.MiddleCenter, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.2f, 0.2f, 0.2f) } };
        GUILayout.Label("偵測到設定檔已在外部被刪除或遺失。\n請點擊下方按鈕刷新介面。", msgStyle);
        GUILayout.FlexibleSpace();
        
        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        float btnWidth = 200f;
        Rect btnRect = GUILayoutUtility.GetRect(btnWidth, 40f, GUILayout.Width(btnWidth), GUILayout.Height(40f));
        DrawDynamicButton(btnRect, "重新整理 (Refresh)", () => { RefreshConfigs(); GUIUtility.ExitGUI(); }, dynamicAccent);
        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
        GUILayout.Space(10); GUILayout.EndArea();
    }

    private string GetActivePlatformDisplayName()
    {
        if (SessionState.GetBool(SpineShaderBuildPipeline.SessionKeyEditorMode, true)) return "Editor Default (全開)";
        string activePath = SessionState.GetString("SpineShader_ActiveConfigPath", "");
        if (string.IsNullOrEmpty(activePath)) return $"{EditorUserBuildSettings.activeBuildTarget} (未同步)";
        return Path.GetFileNameWithoutExtension(activePath).Replace("ShaderConfig_", "").Replace("PC", "Windows");
    }

    private void DrawTopBar(Color dynamicAccent)
    {
        Rect bar = new Rect(0, 0, position.width, 50);
        if (Event.current.type == EventType.Repaint) {
            EditorGUI.DrawRect(bar, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.8f, 0.8f, 0.8f));
            EditorGUI.DrawRect(new Rect(0, 48, position.width, 2), dynamicAccent);
        }
        
        GUI.Label(new Rect(20, 15, 200, 30), "❖ 變體與平台切換中心", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.85f, 0.85f, 0.85f) : Color.black } });

        string activePlatformName = GetActivePlatformDisplayName();
        GUIStyle statusStyle = new GUIStyle(EditorStyles.helpBox) { 
            alignment = TextAnchor.MiddleCenter, fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = dynamicAccent }
        };
        
        Rect statusRect = new Rect(210, 13, 240, 26);
        if (Event.current.type == EventType.Repaint) EditorGUI.DrawRect(statusRect, new Color(0, 0, 0, 0.2f)); 
        GUI.Label(statusRect, $"當前套用：{activePlatformName}", statusStyle);

        DrawTopBarButton(new Rect(position.width - 120, 12, 100, 26), _showGuide ? "✕ 關閉" : "❓ 指南", () => _showGuide = !_showGuide, dynamicAccent);
        DrawTopBarButton(new Rect(position.width - 160, 12, 32, 26), "＋", () => {
            string defaultDir = Application.dataPath + "/Scripts/VFXTool/BuildConfigs/SpineSkeletonShaderTool";
            if (!Directory.Exists(defaultDir)) Directory.CreateDirectory(defaultDir);
            string absPath = EditorUtility.SaveFilePanel("創建平台設定檔", defaultDir, "ShaderConfig_Windows", "asset");
            if (!string.IsNullOrEmpty(absPath) && absPath.StartsWith(Application.dataPath)) {
                string relPath = "Assets" + absPath.Substring(Application.dataPath.Length);
                var newCfg = ScriptableObject.CreateInstance<SpineShaderBuildConfig>();
                AssetDatabase.CreateAsset(newCfg, relPath); AssetDatabase.SaveAssets(); RefreshConfigs();
            }
        }, dynamicAccent);
    }

    private void DrawTopBarButton(Rect rect, string label, Action onClick, Color accent)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && hover) { onClick?.Invoke(); Event.current.Use(); }
        if (Event.current.type == EventType.Repaint) {
            EditorGUI.DrawRect(rect, hover ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.18f, 0.18f, 0.18f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), accent);
            GUIStyle st = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 13, normal = { textColor = Color.white } };
            GUI.Label(rect, label, st);
        }
    }

    private void DrawGuideOverlay(Color dynamicAccent)
    {
        Rect overlay = new Rect(0, 50, position.width, position.height - 50);
        EditorGUI.DrawRect(overlay, new Color(0, 0, 0, 0.82f));
        Rect panel = new Rect(position.width/2 - 380, position.height/2 - 280, 760, 560);
        EditorGUI.DrawRect(panel, EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : Color.white);
        EditorGUI.DrawRect(new Rect(panel.x, panel.y, panel.width, 3), dynamicAccent);

        GUILayout.BeginArea(new Rect(panel.x + 35, panel.y + 35, panel.width - 70, panel.height - 70));
        _guideScrollPos = GUILayout.BeginScrollView(_guideScrollPos);
        GUILayout.Label("❖ 平台變體管理與程式調用指南", new GUIStyle(EditorStyles.boldLabel) { fontSize = 19, alignment = TextAnchor.MiddleCenter, normal = { textColor = dynamicAccent } });
        GUILayout.Space(25);
        GUIStyle s = new GUIStyle(EditorStyles.label) { fontSize = 14, wordWrap = true, richText = true };
        GUILayout.Label("<b>1. 操作機制</b>：左右滑動切換。點擊條目即時開關。滑動過程中設有防誤觸機制。\n", s);
        GUILayout.Label("<b>2. 效能優化</b>：關閉模組後，打包流程會暴力剔除 Shader Pass 以優化效能。\n", s);
        GUILayout.Label("<b>3. 程式調用 API</b>：您可以在打包自動化腳本中使用以下範例進行平台自動化切換：", s);
        GUILayout.Space(15);

        string code = "public class SpineBuildAutomation {\n    public void PrepareForTarget() {\n        // 傳入名稱刷新全專案材質球 (例如 \"Android\")\n        SpineShaderBuildPipeline.SyncKeywordsForTarget(\"Android\");\n    }\n}";
        Rect codeRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(120));
        EditorGUI.DrawRect(codeRect, new Color(0.1f, 0.1f, 0.11f));
        EditorGUI.DrawRect(new Rect(codeRect.x, codeRect.y, 4f, codeRect.height), dynamicAccent);
        GUI.Label(new Rect(codeRect.x + 15, codeRect.y + 10, codeRect.width - 20, codeRect.height - 20), code, new GUIStyle(EditorStyles.label) { fontSize = 13, normal = { textColor = new Color(0.6f, 0.85f, 0.6f) } });
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("複製範例代碼", GUILayout.Height(28))) { EditorGUIUtility.systemCopyBuffer = code; Debug.Log("[Spine] API 範例已複製。"); }
        if (GUILayout.Button("瀏覽設定檔目錄", GUILayout.Height(28))) {
            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Scripts/VFXTool/BuildConfigs/SpineSkeletonShaderTool");
            if (folder != null) { EditorUtility.FocusProjectWindow(); Selection.activeObject = folder; }
        }
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView(); GUILayout.EndArea();
    }

    private void DrawVerticalConfigPanel(SpineShaderBuildConfig cfg, bool isActiveCard, Color cardAccent, float contentWidth)
    {
        _editScrollPos = GUILayout.BeginScrollView(_editScrollPos, GUILayout.Width(contentWidth), GUILayout.Height(480));
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical();
        GUIStyle header = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, normal = { textColor = new Color(0.55f, 0.55f, 0.55f) } };
        GUILayout.Label("LIGHTING MODULES", header); GUILayout.Space(2);
        cfg.enableNativeLighting = DrawFeatureBar("Native Lighting", cfg.enableNativeLighting, isActiveCard, cfg, cardAccent);
        cfg.enableShadowLight    = DrawFeatureBar("Shadow Light Base", cfg.enableShadowLight, isActiveCard, cfg, cardAccent);
        cfg.enableLightProbe     = DrawFeatureBar("Light Probe", cfg.enableLightProbe, isActiveCard, cfg, cardAccent);
        cfg.enableNormalMap      = DrawFeatureBar("Normal Map", cfg.enableNormalMap, isActiveCard, cfg, cardAccent);
        GUILayout.Space(10); GUILayout.Label("VFX MODULES", header); GUILayout.Space(2);
        cfg.enableBackLight      = DrawFeatureBar("BackLight", cfg.enableBackLight, isActiveCard, cfg, cardAccent);
        cfg.enableEmission       = DrawFeatureBar("Emission", cfg.enableEmission, isActiveCard, cfg, cardAccent);
        cfg.enableInline         = DrawFeatureBar("Inline (Rim)", cfg.enableInline, isActiveCard, cfg, cardAccent);
        cfg.enableOutline        = DrawFeatureBar("Outline", cfg.enableOutline, isActiveCard, cfg, cardAccent);
        cfg.enableHitSweep       = DrawFeatureBar("Hit Sweep", cfg.enableHitSweep, isActiveCard, cfg, cardAccent);
        cfg.enableEffectOverlay  = DrawFeatureBar("Effect Overlay", cfg.enableEffectOverlay, isActiveCard, cfg, cardAccent);
        GUILayout.Space(10); GUILayout.Label("GLOBAL SETTINGS", header); GUILayout.Space(2);
        cfg.enableGlobalNoise    = DrawFeatureBar("Global Noise", cfg.enableGlobalNoise, isActiveCard, cfg, cardAccent);
        if (cfg.enableOutline) {
            cfg.use8Neighbourhood   = DrawFeatureBar("├ 8 Neighbourhood", cfg.use8Neighbourhood, isActiveCard, cfg, cardAccent);
            cfg.useScreenSpaceWidth = DrawFeatureBar("├ Screen Space Width", cfg.useScreenSpaceWidth, isActiveCard, cfg, cardAccent);
            cfg.outlineFillInside   = DrawFeatureBar("└ Fill Inside", cfg.outlineFillInside, isActiveCard, cfg, cardAccent);
        }
        GUILayout.EndVertical(); GUILayout.EndScrollView();
    }

    private bool DrawFeatureBar(string label, bool value, bool isActiveCard, SpineShaderBuildConfig cfg, Color accentColor)
    {
        Rect r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(26));
        bool hover = r.Contains(Event.current.mousePosition) && isActiveCard;
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && hover && !_isDragging) {
            value = !value; EditorUtility.SetDirty(cfg); Repaint();
            Debug.Log($"[Spine] {cfg.name} -> {label}: {(value ? "ON" : "OFF")}");
            GUI.changed = true; Event.current.Use();
        }
        if (Event.current.type == EventType.Repaint) {
            Color bg = value ? new Color(accentColor.r, accentColor.g, accentColor.b, 0.22f) : new Color(0, 0, 0, 0.2f);
            if (hover) bg.a += 0.12f; EditorGUI.DrawRect(r, bg);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 2f, r.width, 2f), value ? accentColor : new Color(0.2f, 0.2f, 0.2f));
            GUI.Label(new Rect(r.x + 12, r.y, r.width - 12, r.height - 2), (value ? "■ " : "□ ") + label, new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, normal = { textColor = value ? Color.white : new Color(0.55f, 0.55f, 0.55f) } });
        }
        return value;
    }

    private void DrawDynamicButton(Rect rect, string label, Action onClick, Color accent)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && hover && !_isDragging) { onClick?.Invoke(); Event.current.Use(); }
        if (Event.current.type == EventType.Repaint) {
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? (hover ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.18f, 0.18f, 0.18f)) : (hover ? new Color(0.92f, 0.92f, 0.92f) : new Color(0.85f, 0.85f, 0.85f)));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 3f, rect.width, 3f), accent);
            GUI.Label(rect, label, new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = { textColor = EditorGUIUtility.isProSkin ? accent : Color.black } });
        }
    }
}
#endif
