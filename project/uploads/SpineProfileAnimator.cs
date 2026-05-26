using UnityEngine;
using System.Collections.Generic;
using VFXTool.SpineSkeletonShaderTool;

namespace VFXTool.SpineSkeletonShaderTool
{
    // ─────────────────────────────────────────────────────────────
    // 曲線動畫與 Preset 驅動器 (完整支援靜態數值與動態曲線)
    // ─────────────────────────────────────────────────────────────
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public class SpineProfileAnimator : MonoBehaviour
    {
        public List<SpineModuleProfile> profiles = new List<SpineModuleProfile>();
        
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private float _startTime;
        private List<string> _lastDrivenProps = new List<string>();

        private Dictionary<string, float> _lastFloats = new Dictionary<string, float>();
        private Dictionary<string, Vector4> _lastVectors = new Dictionary<string, Vector4>();
        private Dictionary<string, Color> _lastColors = new Dictionary<string, Color>(); 
        private Dictionary<string, Texture> _lastTextures = new Dictionary<string, Texture>(); // 🌟 新增：貼圖緩存追蹤
        
        private bool _isCleared = false; 
        private bool _isDestroying = false; 

        void OnEnable() {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            Restart();
            
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.update += EditorUpdate;
    #endif
        }

        void OnDisable() {
            ClearAllMPB();
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= EditorUpdate;
    #endif
        }

        public void Restart() {
    #if UNITY_EDITOR
            _startTime = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
    #else
            _startTime = Time.time;
    #endif
        }

        public void ClearAllMPB() {
            if (_isCleared) return; 
            
            if (_renderer != null) {
                _renderer.SetPropertyBlock(null);
                if (_mpb != null) _mpb.Clear();
            }
            _lastFloats.Clear();
            _lastVectors.Clear();
            _lastColors.Clear();
            _lastTextures.Clear();
            _lastDrivenProps.Clear();
            _isCleared = true;
        }

    #if UNITY_EDITOR
        void EditorUpdate() {
            if (!Application.isPlaying) DoUpdate();
        }
    #endif

        void Update() {
            if (Application.isPlaying) DoUpdate();
        }

        void DoUpdate() {
            if (_renderer == null) return;
            
            float time;
    #if UNITY_EDITOR
            time = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
    #else
            time = Time.time;
    #endif
            float t = time - _startTime;

            if (profiles != null) {
                profiles.RemoveAll(p => p == null);
            }

            List<string> currentDriven = new List<string>();
            bool hasValidProfiles = false;
            
            if (profiles != null && profiles.Count > 0) {
                hasValidProfiles = true;
                foreach (var profile in profiles) {
                    foreach (var prop in profile.properties) {
                        // 🌟 修正：不論是否有開啟曲線，所有在 Profile 裡的屬性都要被驅動
                        currentDriven.Add(prop.name);
                    }
                }
            }

            if (currentDriven.Count == 0) {
                ClearAllMPB();
                if (!hasValidProfiles) {
    #if UNITY_EDITOR
                    if (!Application.isPlaying) {
                        if (!_isDestroying) { 
                            _isDestroying = true;
                            UnityEditor.EditorApplication.delayCall += () => {
                                if (this != null) DestroyImmediate(this);
                            };
                        }
                    }
    #else
                    Destroy(this);
    #endif
                }
                return;
            }

            bool needsClear = false;
            foreach (var p in _lastDrivenProps) {
                if (!currentDriven.Contains(p)) { needsClear = true; break; }
            }

            if (needsClear) {
                ClearAllMPB();
            }
            
            _lastDrivenProps = currentDriven;

            bool actuallyChanged = false;
            if (needsClear) {
                actuallyChanged = true; 
            }

            _renderer.GetPropertyBlock(_mpb);

            foreach (var profile in profiles) {
                foreach (var prop in profile.properties) {
                    
                    // 🌟 修正：根據 useCurve 決定要讀取動畫曲線數值，還是靜態固定數值
                    
                    if (prop.type == SpineModuleProfile.ProfilePropType.Float) {
                        float val = prop.useCurve ? Evaluate(prop.curveX, t, prop.floatValue) : prop.floatValue;
                        if (!_lastFloats.TryGetValue(prop.name, out float lastVal) || Mathf.Abs(lastVal - val) > 0.0001f) {
                            _lastFloats[prop.name] = val;
                            _mpb.SetFloat(prop.name, val);
                            actuallyChanged = true;
                        }
                    } 
                    else if (prop.type == SpineModuleProfile.ProfilePropType.Vector) {
                        float x = prop.useCurve ? Evaluate(prop.curveX, t, prop.vectorValue.x) : prop.vectorValue.x;
                        float y = prop.useCurve ? Evaluate(prop.curveY, t, prop.vectorValue.y) : prop.vectorValue.y;
                        float z = prop.useCurve ? Evaluate(prop.curveZ, t, prop.vectorValue.z) : prop.vectorValue.z;
                        float w = prop.useCurve ? Evaluate(prop.curveW, t, prop.vectorValue.w) : prop.vectorValue.w;
                        Vector4 val = new Vector4(x, y, z, w);
                        
                        if (!_lastVectors.TryGetValue(prop.name, out Vector4 lastVal) || Vector4.SqrMagnitude(lastVal - val) > 0.0001f) {
                            _lastVectors[prop.name] = val;
                            _mpb.SetVector(prop.name, val);
                            actuallyChanged = true;
                        }
                    }
                    else if (prop.type == SpineModuleProfile.ProfilePropType.Color) {
                        float r = prop.useCurve ? Evaluate(prop.curveX, t, prop.colorValue.r) : prop.colorValue.r;
                        float g = prop.useCurve ? Evaluate(prop.curveY, t, prop.colorValue.g) : prop.colorValue.g;
                        float b = prop.useCurve ? Evaluate(prop.curveZ, t, prop.colorValue.b) : prop.colorValue.b;
                        float a = prop.useCurve ? Evaluate(prop.curveW, t, prop.colorValue.a) : prop.colorValue.a;
                        Color val = new Color(r, g, b, a);
                        
                        if (!_lastColors.TryGetValue(prop.name, out Color lastVal) || Vector4.SqrMagnitude((Vector4)lastVal - (Vector4)val) > 0.0001f) {
                            _lastColors[prop.name] = val;
                            _mpb.SetColor(prop.name, val);
                            actuallyChanged = true;
                        }
                    }
                    else if (prop.type == SpineModuleProfile.ProfilePropType.Texture) {
                        Texture val = prop.textureValue;
                        if (!_lastTextures.TryGetValue(prop.name, out Texture lastVal) || lastVal != val) {
                            _lastTextures[prop.name] = val;
                            if (val != null) _mpb.SetTexture(prop.name, val);
                            actuallyChanged = true;
                        }
                    }
                }
            }

            if (actuallyChanged) {
                _renderer.SetPropertyBlock(_mpb);
                _isCleared = false; 
    #if UNITY_EDITOR
                if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null) {
                    UnityEditor.SceneView.lastActiveSceneView.Repaint();
                }
    #endif
            }
        }

        private float Evaluate(AnimationCurve curve, float t, float fallback) {
            if (curve == null || curve.keys.Length == 0) return fallback;
            return curve.Evaluate(t);
        }
    }
}
