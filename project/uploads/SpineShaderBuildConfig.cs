using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Build/Spine Shader Feature Config")]
public class SpineShaderBuildConfig : ScriptableObject
{
    [Header("光照與陰影模組")]
    public bool enableNativeLighting = true;
    public bool enableShadowLight    = true;
    public bool enableLightProbe     = true;
    public bool enableNormalMap      = true;

    [Header("特效與色彩模組")]
    public bool enableBackLight      = true;
    public bool enableEmission       = true;
    public bool enableInline         = true;
    public bool enableOutline        = true;
    public bool enableHitSweep       = true;
    public bool enableEffectOverlay  = true;

    [Header("全域控制模組")]
    public bool enableGlobalNoise      = true;
    public bool enableBloomSuppression = true;

    [Header("Outline 變體 (若開啟 Outline)")]
    public bool use8Neighbourhood   = true;
    public bool useScreenSpaceWidth = false; 
    public bool outlineFillInside   = false;

    public List<string> GetEnabledKeywords()
    {
        var list = new List<string>();
        if (enableNativeLighting)   list.Add("_MODULE_NATIVE_LIGHTING");
        if (enableShadowLight)      list.Add("_MODULE_SHADOW_LIGHT");
        if (enableLightProbe)       list.Add("_MODULE_LIGHT_PROBE");
        if (enableNormalMap)        list.Add("_MODULE_NORMALMAP");
        if (enableBackLight)        list.Add("_MODULE_BACKLIGHT");
        if (enableEmission)         list.Add("_MODULE_EMISSION");
        if (enableInline)           list.Add("_MODULE_INLINE");
        if (enableOutline)          list.Add("_MODULE_OUTLINE");
        if (enableHitSweep)         list.Add("_MODULE_HITSWEEP");
        if (enableEffectOverlay)    list.Add("_MODULE_EFFECT_OVERLAY");
        if (enableGlobalNoise)      list.Add("_MODULE_GLOBAL_NOISE");
        if (enableBloomSuppression) list.Add("_MODULE_BLOOM_SUPPRESSION");

        if (enableOutline) {
            if (use8Neighbourhood)   list.Add("_USE8NEIGHBOURHOOD_ON");
            if (useScreenSpaceWidth) list.Add("_USE_SCREENSPACE_OUTLINE_WIDTH");
            if (outlineFillInside)   list.Add("_OUTLINE_FILL_INSIDE");
        }
        return list;
    }
}