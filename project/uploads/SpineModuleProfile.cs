using UnityEngine;
using System.Collections.Generic;
using VFXTool.SpineSkeletonShaderTool;

namespace VFXTool.SpineSkeletonShaderTool
{
    [System.Serializable]
    public class SpineModuleProfile : ScriptableObject
    {
        public enum ProfilePropType { Float, Color, Vector, Texture }

        [System.Serializable]
        public class ProfileProperty
        {
            public ProfilePropType type;
            public string name;
            public string displayName;
            
            public float floatValue;
            public Color colorValue;
            public Vector4 vectorValue;
            public Texture textureValue;

            public bool useCurve;
            public AnimationCurve curveX = new AnimationCurve();
            public AnimationCurve curveY = new AnimationCurve();
            public AnimationCurve curveZ = new AnimationCurve();
            public AnimationCurve curveW = new AnimationCurve();
        }

        public string moduleName;
        [HideInInspector] public List<ProfileProperty> properties = new List<ProfileProperty>();

        // 防錯：當 Profile 被 Reset 時，確保 List 不為 null
        private void OnEnable() {
            if (properties == null) properties = new List<ProfileProperty>();
        }
    }
}
