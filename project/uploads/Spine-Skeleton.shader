Shader "Spine/Skeleton" {
	Properties {
	    [HideInInspector] _MaterialResetCheck("Reset Check", Float) = 0
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		_MainTex ("Main Texture", 2D) = "black" {}
		[HideInInspector] _MainTex_ST("MainTex ST", Vector) = (1, 1, 0, 0)
		_Color ("Main Color", Color) = (1,1,1,1)
		
		[HDR] _EffectColor ("Effect Color", Color) = (1,1,1,1)
		_EffectIntensity ("Effect Intensity", Range(0,1)) = 0

		[Toggle] _EnableBloomSuppression("Enable Bloom Suppression", Float) = 0
		_BloomSuppressThreshold("Bloom Suppress Threshold", Range(0.1, 5.0)) = 0.85

		[Toggle] _EnableGlobalNoise("Enable Global Noise", Float) = 0
		_GlobalNoiseScale("Global Noise Scale", Vector) = (5, 5, 0, 0)
		_GlobalNoiseSpeed("Global Noise Speed", Vector) = (0.5, 0.5, 0, 0)

		[Toggle] _EnableNativeLighting("Enable Native Lighting", Float) = 0
		_NativeLightIntensity("Master Light Intensity", Range(0.0, 5.0)) = 1.0
		_DirectionalLightIntensity("Directional Light Intensity", Range(0.0, 5.0)) = 1.0
		_AdditionalLightIntensity("Additional Light Intensity", Range(0.0, 5.0)) = 1.0
		
        [Toggle] _EnableShadowLight("Enable Shadow Light Base", Float) = 0
        _ShadowLightBaseIntensity("Shadow Light Base Intensity", Range(0.0, 5.0)) = 1.0
        [Toggle] _SL_AffectsNormalMap("Shadow Light Affects Normal Map", Float) = 1
        
        [Toggle] _StraightAlphaInput("Straight Alpha Texture", Float) = 0
		[HideInInspector] _DoubleSidedLighting("Double-Sided Lighting", Float) = 1
		[Toggle] _LightAffectsAdditive("Light Affects Additive", Float) = 0
		_LightProbeNativeBlend("Probe Blend -> Native", Range(0,1)) = 0.5

		[Toggle] _EnableLightProbe("Enable Light Probe", Float) = 0
		_LightProbeIntensity("Light Probe Intensity", Range(0.0, 3.0)) = 1.0
		[HideInInspector] _SpineSHAr("SH Ar", Vector) = (0,0,0,0)
		[HideInInspector] _SpineSHAg("SH Ag", Vector) = (0,0,0,0)
		[HideInInspector] _SpineSHAb("SH Ab", Vector) = (0,0,0,0)
		[HideInInspector] _SpineSHBr("SH Br", Vector) = (0,0,0,0)
		[HideInInspector] _SpineSHBg("SH Bg", Vector) = (0,0,0,0)
		[HideInInspector] _SpineSHBb("SH Bb", Vector) = (0,0,0,0)
		[HideInInspector] _SpineSHC ("SH C",  Vector) = (0,0,0,0)

		[Toggle] _EnableHitSweep("Enable Hit Sweep", Float) = 0
		[HideInInspector] _HitPosition("Hit Position (World)", Vector) = (0,0,0,0)
		_SweepCenterOffset("Character Center (Local)", Vector) = (0, 0.5, 0, 0)
		_SweepProgress("Sweep Progress", Float) = -10.0
		[HDR] _SweepColor("Sweep Color", Color) = (2.0, 2.0, 2.0, 1.0)
		_SweepWidth("Sweep Width (Ratio)", Range(0.01, 1.0)) = 0.15
		_SweepSoftness("Sweep Softness (Ratio)", Range(0.001, 1.0)) = 0.1

        // ==========================================
        // === Normal Map ===========================
        // ==========================================
        [Toggle] _EnableNormalMap("Enable Normal Map Module", Float) = 0
        [Toggle] _InvertNormalMap("Invert Normal Height", Float) = 0
        [NoScaleOffset][Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalIntensity ("Normal Intensity", Range(0, 5)) = 0.0

        // ==========================================
        // === Emission =============================
        // ==========================================
        [Toggle] _EnableEmission("Enable Emission", Float) = 0
        [NoScaleOffset] _EmissionTex ("Emission Texture", 2D) = "black" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionBlend ("Emission Blend", Range(0,1)) = 1.0
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 1.0
        [Toggle] _EmissionMulMain ("Emission Multiply MainTex", Float) = 0

        // ==========================================
		// === BackLight ============================
        // ==========================================
		[Toggle] _EnableBackLight("Enable BackLight", Float) = 0
		[HideInInspector] _BackLightLightingMode("BackLight Lighting Mode", Int) = 0
		[NoScaleOffset] _SpecularTex ("Specular Texture", 2D) = "white" {}
		[HDR] _SpecularColor ("Specular Color", Color) = (1,1,1,1)
		[Enum(Replace, 0, Additive, 1)] _BackLightMode("BackLight Mode", Float) = 0
		_SpecularBlend ("Specular Blend", Range(0,1)) = 0
		[Toggle] _SpecularMulMain ("Specular Multiply MainTex", Float) = 0
		[Toggle] _EnableDirectionalBackLight("Enable Directional BackLight", Float) = 0
		[Toggle] _BackLightTintByLight("BackLight Tint By Light Color", Float) = 0
		[Toggle] _BL_EnableNoise("Enable Noise", Float) = 0
		_BL_NoiseIntensity("Noise Intensity", Range(0,1)) = 0
        
        _BL_RT_Power("BL RT Power", Range(0.1, 5.0)) = 1.0
        _BL_RT_Blend("BL RT Blend", Range(0,1)) = 0.5
        _BL_RT_Intensity("BL RT Intensity", Range(0, 100)) = 1.0

        _BL_Probe_Power("BL Probe Power", Range(0.1, 5.0)) = 1.0
        _BL_Probe_Blend("BL Probe Blend", Range(0,1)) = 0.5
        _BL_Probe_Intensity("BL Probe Intensity", Range(0, 100)) = 1.0

        _BL_Shadow_Power("BL Shadow Power", Range(0.1, 5.0)) = 1.0
        _BL_Shadow_Blend("BL Shadow Blend", Range(0,1)) = 0.5
        _BL_Shadow_Intensity("BL Shadow Intensity", Range(0, 100)) = 1.0
		[Toggle] _BL_SL_IgnoreBehind("Ignore Behind Fade (Shadow Light)", Float) = 0

        // ==========================================
		// === Inline ===============================
        // ==========================================
		[Toggle] _EnableInline("Enable Inline (Rim Light)", Float) = 0
		[HideInInspector] _InlineLightingMode("Inline Lighting Mode", Int) = 0
		[HDR]_InlineColor("Inline Color", Color) = (1,1,1,1)
		_InlineWidth("Inline Thickness", Range(0.0, 10.0)) = 0.0
		_InlineFadeSteps("Inline Fade Steps", Range(2, 8)) = 2
		[Toggle] _EnableDirectionalInline("Enable Directional Inline", Float) = 0
		[Toggle] _InlineTintByLight("Inline Tint By Light Color", Float) = 0
		_InlineZTest("Inline ZTest", Float) = 4
		[Toggle] _IL_EnableNoise("Enable Noise", Float) = 0
		_IL_NoiseIntensity("Noise Intensity", Range(0,1)) = 0

        _IL_RT_Power("IL RT Power", Range(0.1, 5.0)) = 1.0
        _IL_RT_Blend("IL RT Blend", Range(0,1)) = 0.5
        _IL_RT_Intensity("IL RT Intensity", Range(0, 100)) = 1.0

        _IL_Probe_Power("IL Probe Power", Range(0.1, 5.0)) = 1.0
        _IL_Probe_Blend("IL Probe Blend", Range(0,1)) = 0.5
        _IL_Probe_Intensity("IL Probe Intensity", Range(0, 100)) = 1.0

        _IL_Shadow_Power("IL Shadow Power", Range(0.1, 5.0)) = 1.0
        _IL_Shadow_Blend("IL Shadow Blend", Range(0,1)) = 0.5
        _IL_Shadow_Intensity("IL Shadow Intensity", Range(0, 100)) = 1.0
		[Toggle] _IL_SL_IgnoreBehind("Ignore Behind Fade (Shadow Light)", Float) = 0

		[HideInInspector] _SpineAlphaMask ("Alpha Mask (Auto)", 2D) = "white" {}
		[HideInInspector] _SpineAlphaMask_TexelSize ("Alpha Mask TexelSize (Auto)", Vector) = (0.001, 0.001, 1024, 1024)

        // ==========================================
		// === Outline ==============================
        // ==========================================
		_OutlineAlphaCutoff ("Outline Alpha Cutoff", Range(0,1)) = 0.1
		_StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8
		[Toggle] _EnableOutline("Enable Outline", Float) = 0
		[HideInInspector] _OutlineLightingMode("Outline Lighting Mode", Int) = 0
		_OutlineWidth("Outline Width", Range(0,8)) = 0.0
		
        [MaterialToggle(_USE_SCREENSPACE_OUTLINE_WIDTH)] _UseScreenSpaceOutlineWidth("Width in Screen Space", Float) = 0
		[MaterialToggle(_OUTLINE_FILL_INSIDE)] _Fill("Fill", Float) = 0
		[MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1

		[HDR]_OutlineColor("Outline Color", Color) = (1,1,1,1)
		[Toggle] _MultiplyEdgeColor("Multiply Edge Color", Float) = 0
		[Toggle] _EnableDirectionalOutline("Enable Directional Outline", Float) = 0
		[Toggle] _OutlineTintByLight("Outline Tint By Light Color", Float) = 0
		_OutlineZTest("Outline ZTest", Float) = 4
		[Toggle] _OL_EnableNoise("Enable Noise", Float) = 0
		_OL_NoiseIntensity("Noise Intensity", Range(0,1)) = 0

        _OL_RT_Power("OL RT Power", Range(0.1, 5.0)) = 1.0
        _OL_RT_Blend("OL RT Blend", Range(0,1)) = 0.5
        _OL_RT_Intensity("OL RT Intensity", Range(0, 100)) = 1.0

        _OL_Probe_Power("OL Probe Power", Range(0.1, 5.0)) = 1.0
        _OL_Probe_Blend("OL Probe Blend", Range(0,1)) = 0.5
        _OL_Probe_Intensity("OL Probe Intensity", Range(0, 100)) = 1.0

        _OL_Shadow_Power("OL Shadow Power", Range(0.1, 5.0)) = 1.0
        _OL_Shadow_Blend("OL Shadow Blend", Range(0,1)) = 0.5
        _OL_Shadow_Intensity("OL Shadow Intensity", Range(0, 100)) = 1.0
		[Toggle] _OL_SL_IgnoreBehind("Ignore Behind Fade (Shadow Light)", Float) = 0

		_OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		_ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		_OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		_OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		_OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	CGINCLUDE

	#include "UnityCG.cginc"

    // ==========================================
    // === Shader Feature 模組宣告 ==============
    // ==========================================
    #pragma shader_feature_local _MODULE_NATIVE_LIGHTING
    #pragma shader_feature_local _MODULE_SHADOW_LIGHT
    #pragma shader_feature_local _MODULE_LIGHT_PROBE
    #pragma shader_feature_local _MODULE_NORMALMAP
    #pragma shader_feature_local _MODULE_GLOBAL_NOISE

    #pragma shader_feature_local _MODULE_BACKLIGHT
    #pragma shader_feature_local _MODULE_EMISSION
    #pragma shader_feature_local _MODULE_INLINE
    #pragma shader_feature_local _MODULE_OUTLINE
    #pragma shader_feature_local _MODULE_HITSWEEP
    #pragma shader_feature_local _MODULE_EFFECT_OVERLAY
    #pragma shader_feature_local _MODULE_BLOOM_SUPPRESSION

    // ==========================================
    // === 動態分支 (Dynamic Branching) 使用的 Uniform ===
    uniform float _EnableBloomSuppression;
    uniform float _BloomSuppressThreshold;
    uniform float _EnableGlobalNoise;
    uniform float _EnableNativeLighting;
    uniform float _EnableShadowLight;
    uniform float _ShadowLightBaseIntensity;
    uniform float _SL_AffectsNormalMap;
    uniform float _StraightAlphaInput;
    uniform float _LightAffectsAdditive;
    uniform float _EnableLightProbe;
    uniform float _EnableHitSweep;
    uniform float _EnableNormalMap;
    uniform float _InvertNormalMap;
    uniform float _EnableEmission;
    uniform float _EmissionMulMain;
    uniform float _EnableBackLight;
    uniform float _BackLightMode;
    uniform float _SpecularMulMain;
    uniform float _EnableDirectionalBackLight;
    uniform float _BackLightTintByLight;
    uniform float _BL_EnableNoise;
    uniform float _EnableInline;
    uniform float _EnableDirectionalInline;
    uniform float _InlineTintByLight;
    uniform float _IL_EnableNoise;
    uniform float _EnableOutline;
    uniform float _MultiplyEdgeColor;
    uniform float _EnableDirectionalOutline;
    uniform float _OutlineTintByLight;
    uniform float _OL_EnableNoise;

	uniform float _BL_SL_IgnoreBehind;
	uniform float _IL_SL_IgnoreBehind;
	uniform float _OL_SL_IgnoreBehind;
    uniform int    _SpineLightCount;
    uniform float4 _SpineLightPos[8];
	uniform float4 _SpineLightColor[8];
	uniform float4 _SpineLightSpotParams[8];
    uniform int    _SpineShadowLightCount;
    uniform float4 _SpineShadowLightPos[8];
    uniform float4 _SpineShadowLightColor[8]; 
	uniform float4 _SpineShadowLightDir[8];   
	uniform float4 _SpineShadowLightSpot[8];
    uniform float4 _SpineShadowLightRight[8];
	uniform float4 _SpineShadowLightUp[8];
    uniform float4 _SpineShadowCookieParams[8];
    uniform float4 _SpineShadowParams2[8]; 

	sampler2D _SpineShadowCookie0; sampler2D _SpineShadowCookie1;
	sampler2D _SpineShadowCookie2; sampler2D _SpineShadowCookie3;
    sampler2D _SpineShadowCookie4; sampler2D _SpineShadowCookie5;
	sampler2D _SpineShadowCookie6; sampler2D _SpineShadowCookie7;

    uniform float4 _SpineSHAr, _SpineSHAg, _SpineSHAb;
	uniform float4 _SpineSHBr, _SpineSHBg, _SpineSHBb;
    uniform float4 _SpineSHC;
	uniform float  _LightProbeIntensity;
    uniform float  _LightProbeNativeBlend;

	uniform int   _BackLightLightingMode;
    uniform int   _InlineLightingMode;
	uniform int   _OutlineLightingMode;
    uniform float4 _Color;
	// ==========================================
	// === Shadow Light 相關函式 ================
	// ==========================================
	float3 SampleCookieTexture(int index, float2 uv) {
		if (index == 0) return tex2D(_SpineShadowCookie0, uv).rgb;
        if (index == 1) return tex2D(_SpineShadowCookie1, uv).rgb;
		if (index == 2) return tex2D(_SpineShadowCookie2, uv).rgb;
        if (index == 3) return tex2D(_SpineShadowCookie3, uv).rgb;
		if (index == 4) return tex2D(_SpineShadowCookie4, uv).rgb;
        if (index == 5) return tex2D(_SpineShadowCookie5, uv).rgb;
		if (index == 6) return tex2D(_SpineShadowCookie6, uv).rgb;
        if (index == 7) return tex2D(_SpineShadowCookie7, uv).rgb;
		return float3(1,1,1);
	}

	void ApplyShadowLights(float3 worldPos, float3 worldNormal, inout float3 baseColor, float alpha, float ignoreBehind, float intensityMult) {
		if (_SpineShadowLightCount <= 0) return;
        [unroll(8)]
		for (int i = 0; i < 8; i++) {
			if (i >= _SpineShadowLightCount) break;
            float  type        = _SpineShadowLightDir[i].w;
			float3 original_lv = _SpineShadowLightPos[i].xyz - worldPos;
            float atten        = 0.0;
            if (type > 2.5) {
                float3 L_r = normalize(_SpineShadowLightRight[i].xyz);
                float3 L_u = normalize(_SpineShadowLightUp[i].xyz);
                float3 L_f = _SpineShadowLightDir[i].xyz;

                float dX = dot(-original_lv, L_r);
                float dY = dot(-original_lv, L_u);
                float dZ = dot(-original_lv, L_f);

                float halfW = _SpineShadowLightSpot[i].z * 0.5;
                float halfH = _SpineShadowLightSpot[i].w * 0.5;
                if (dZ > 0.0 && abs(dX) <= halfW && abs(dY) <= halfH) {
                    float sqrRangeInv = _SpineShadowLightPos[i].w;
                    float dR = (dZ * dZ) * sqrRangeInv;
                    float dA = saturate(1.0 - dR * dR);
                    atten = dA * dA;

                    float softX = saturate((halfW - abs(dX)) / max(halfW * 0.1, 0.01));
                    float softY = saturate((halfH - abs(dY)) / max(halfH * 0.1, 0.01));
                    atten *= softX * softY;

                    float3 L = -normalize(_SpineShadowLightDir[i].xyz);
                    float behindFade = lerp(max(0.0, dot(worldNormal, L)), 1.0, ignoreBehind);
                    atten *= behindFade;
                }
            } else {
                float sqrDist = dot(original_lv, original_lv);
                float sqrRangeInv = _SpineShadowLightPos[i].w;
                float dR = sqrDist * sqrRangeInv;
                float dA = saturate(1.0 - dR * dR);
                atten = dA * dA;
                
                float dist = sqrt(sqrDist);
                float3 L = (dist > 1e-4) ?
                (original_lv / dist) : float3(0, 1, 0);
                
                float behindFade = lerp(max(0.0, dot(worldNormal, L)), 1.0, ignoreBehind);
                atten *= behindFade;
                if (type > 1.5) { 
                    float sA = saturate(dot(-L, _SpineShadowLightDir[i].xyz) * _SpineShadowLightSpot[i].x + _SpineShadowLightSpot[i].y);
                    atten *= sA * sA;
                }
            }

			float3 cookieColor = float3(1,1,1);
            float hasCookie = _SpineShadowCookieParams[i].z;
            if (hasCookie > 0.5) {
				float2 projUV = float2(dot(-original_lv, _SpineShadowLightRight[i].xyz), dot(-original_lv, _SpineShadowLightUp[i].xyz));
                float2 cookieUV = projUV + _SpineShadowCookieParams[i].xy;
                float cookieMode = _SpineShadowParams2[i].y;
				float3 c1 = SampleCookieTexture(i, cookieUV);
                if (cookieMode > 0.5) {
					float2 uv2 = projUV * 0.85 + _SpineShadowCookieParams[i].xy * -0.5;
                    float3 c2 = SampleCookieTexture(i, uv2);
                    cookieColor = max(c1, c2) * 1.5; 
				} else {
					cookieColor = c1;
				}

				float ch = _SpineShadowParams2[i].z;
                if (ch > 0.5 && ch < 1.5) cookieColor = cookieColor.rrr;
                else if (ch > 1.5 && ch < 2.5) cookieColor = cookieColor.ggg;
                else if (ch > 2.5 && ch < 3.5) cookieColor = cookieColor.bbb;
                else if (ch > 3.5) cookieColor = float3(cookieColor.b, cookieColor.b, cookieColor.b);
			}

			float blendMode = _SpineShadowParams2[i].x;
            // 乘上 Spine 專屬倍率 (_SpineShadowParams2[i].w)
			float intensity = _SpineShadowLightColor[i].w * _SpineShadowParams2[i].w;
            float3 targetColor = _SpineShadowLightColor[i].rgb * intensity * cookieColor * intensityMult;
            if (blendMode < 0.5) {
				baseColor *= lerp(float3(1, 1, 1), targetColor, atten);
            } else if (blendMode < 1.5) {
				baseColor += targetColor * (atten * alpha);
            } else if (blendMode < 2.5) {
				float3 screenColor = baseColor + targetColor - baseColor * targetColor;
                baseColor = lerp(baseColor, screenColor, atten * alpha);
			} else {
				float3 overlay = (baseColor < 0.5) ?
                (2.0 * baseColor * targetColor) : (1.0 - 2.0 * (1.0 - baseColor) * (1.0 - targetColor));
                baseColor = lerp(baseColor, overlay, saturate(atten) * alpha);
			}
		}
	}

    float3 SampleShadowCookie(int i, float3 original_lv) {
        float hasCookie = _SpineShadowCookieParams[i].z;
        if (hasCookie < 0.5) return float3(1, 1, 1);

        float2 projUV = float2(dot(-original_lv, _SpineShadowLightRight[i].xyz), dot(-original_lv, _SpineShadowLightUp[i].xyz));
        float2 cookieUV = projUV + _SpineShadowCookieParams[i].xy;
        float cookieMode = _SpineShadowParams2[i].y;

        float3 cookieColor;
        float3 c1 = SampleCookieTexture(i, cookieUV);
        if (cookieMode > 0.5) {
            float2 uv2 = projUV * 0.85 + _SpineShadowCookieParams[i].xy * -0.5;
            float3 c2 = SampleCookieTexture(i, uv2);
            cookieColor = max(c1, c2) * 1.5;
        } else {
            cookieColor = c1;
        }

        float ch = _SpineShadowParams2[i].z;
        if      (ch > 0.5 && ch < 1.5) cookieColor = cookieColor.rrr;
        else if (ch > 1.5 && ch < 2.5) cookieColor = cookieColor.ggg;
        else if (ch > 2.5 && ch < 3.5) cookieColor = cookieColor.bbb;
        else if (ch > 3.5)             cookieColor = cookieColor.bbb;
        return cookieColor;
    }

    float CalculateShadowLightWithColor(float2 worldNormal2D, float3 worldPos, float3 worldNormal, float focusPower, float ignoreBehind, out float3 outColor) {
        outColor = float3(0, 0, 0);
        if (_SpineShadowLightCount <= 0) return 0.0;
        float total = 0; float3 totCol = float3(0, 0, 0);
        [unroll(8)]
        for (int i = 0; i < 8; i++) {
            if (i >= _SpineShadowLightCount) break;
            float type = _SpineShadowLightDir[i].w; 
            float3 original_lv = _SpineShadowLightPos[i].xyz - worldPos; 
            float atten = 0.0;
            float3 L = float3(0, 1, 0);
            if (type > 2.5) {
                float3 L_r = normalize(_SpineShadowLightRight[i].xyz);
                float3 L_u = normalize(_SpineShadowLightUp[i].xyz);
                float3 L_f = _SpineShadowLightDir[i].xyz;

                float dX = dot(-original_lv, L_r);
                float dY = dot(-original_lv, L_u);
                float dZ = dot(-original_lv, L_f);

                float halfW = _SpineShadowLightSpot[i].z * 0.5;
                float halfH = _SpineShadowLightSpot[i].w * 0.5;
                if (dZ > 0.0 && abs(dX) <= halfW && abs(dY) <= halfH) {
                    float sqrRangeInv = _SpineShadowLightPos[i].w;
                    float dR = (dZ * dZ) * sqrRangeInv;
                    float dA = saturate(1.0 - dR * dR);
                    atten = dA * dA;

                    float softX = saturate((halfW - abs(dX)) / max(halfW * 0.1, 0.01));
                    float softY = saturate((halfH - abs(dY)) / max(halfH * 0.1, 0.01));
                    atten *= softX * softY;
                }
                L = -normalize(L_f);
            } else {
                float sqrDist = dot(original_lv, original_lv);
                float sqrRangeInv = _SpineShadowLightPos[i].w;
                float dR = sqrDist * sqrRangeInv;
                float dA = saturate(1.0 - dR * dR);
                atten = dA * dA;
                
                float dist = sqrt(sqrDist);
                L = (dist > 1e-4) ?
                (original_lv / dist) : float3(0, 1, 0);

                if (type > 1.5) { 
                    float sA = saturate(dot(-L, _SpineShadowLightDir[i].xyz) * _SpineShadowLightSpot[i].x + _SpineShadowLightSpot[i].y);
                    atten *= sA * sA;
                }
            }

            float behindFade = lerp(max(0.0, dot(worldNormal, L)), 1.0, ignoreBehind);
            atten *= behindFade;

            float NdotL = dot(worldNormal2D, L.xy);
            float hL = saturate((NdotL + 1.0) * 0.5);
            float3 cookieColor = SampleShadowCookie(i, original_lv);
            // 乘上 Spine 專屬倍率 (_SpineShadowParams2[i].w)
            float intensity = _SpineShadowLightColor[i].w * _SpineShadowParams2[i].w;
            float3 rc = _SpineShadowLightColor[i].rgb * intensity * cookieColor;
            float ct = pow(hL, focusPower) * atten;
            totCol += rc * ct;
            total += ct * dot(rc, float3(0.299, 0.587, 0.114));
        }

        if (dot(totCol, float3(0.299, 0.587, 0.114)) > 1e-4) outColor = totCol;
        return total;
    }

    float CalculateShadowLightIntensity(float2 worldNormal2D, float3 worldPos, float3 worldNormal, float focusPower, float ignoreBehind) {
        float3 dummy;
        return CalculateShadowLightWithColor(worldNormal2D, worldPos, worldNormal, focusPower, ignoreBehind, dummy);
    }

    float3 CalculateAverageShadowLightColor(float3 worldPos, float3 worldNormal, float ignoreBehind) {
        if (_SpineShadowLightCount <= 0) return float3(0, 0, 0);
        float3 total = float3(0, 0, 0);
        [unroll(8)]
        for (int i = 0; i < 8; i++) {
            if (i >= _SpineShadowLightCount) break;
            float type = _SpineShadowLightDir[i].w;
            float3 original_lv = _SpineShadowLightPos[i].xyz - worldPos;
            float atten = 0.0;
            float3 L = float3(0, 1, 0);
            if (type > 2.5) {
                float3 L_r = normalize(_SpineShadowLightRight[i].xyz);
                float3 L_u = normalize(_SpineShadowLightUp[i].xyz);
                float3 L_f = _SpineShadowLightDir[i].xyz;

                float dX = dot(-original_lv, L_r);
                float dY = dot(-original_lv, L_u);
                float dZ = dot(-original_lv, L_f);

                float halfW = _SpineShadowLightSpot[i].z * 0.5;
                float halfH = _SpineShadowLightSpot[i].w * 0.5;
                if (dZ > 0.0 && abs(dX) <= halfW && abs(dY) <= halfH) {
                    float sqrRangeInv = _SpineShadowLightPos[i].w;
                    float dR = (dZ * dZ) * sqrRangeInv;
                    float dA = saturate(1.0 - dR * dR);
                    atten = dA * dA;

                    float softX = saturate((halfW - abs(dX)) / max(halfW * 0.1, 0.01));
                    float softY = saturate((halfH - abs(dY)) / max(halfH * 0.1, 0.01));
                    atten *= softX * softY;
                }
                L = -normalize(L_f);
            } else {
                float sqrDist = dot(original_lv, original_lv);
                float sqrRangeInv = _SpineShadowLightPos[i].w;
                float dR = sqrDist * sqrRangeInv;
                float dA = saturate(1.0 - dR * dR);
                atten = dA * dA;
                
                float dist = sqrt(sqrDist);
                L = (dist > 1e-4) ?
                (original_lv / dist) : float3(0, 1, 0);
                
                if (type > 1.5) {
                    float sA = saturate(dot(-L, _SpineShadowLightDir[i].xyz) * _SpineShadowLightSpot[i].x + _SpineShadowLightSpot[i].y);
                    atten *= sA * sA;
                }
            }

            float behindFade = lerp(max(0.0, dot(worldNormal, L)), 1.0, ignoreBehind);
            atten *= behindFade;

            float3 cookieColor = SampleShadowCookie(i, original_lv);
            // 乘上 Spine 專屬倍率 (_SpineShadowParams2[i].w)
            float intensity = _SpineShadowLightColor[i].w * _SpineShadowParams2[i].w;
            total += _SpineShadowLightColor[i].rgb * intensity * cookieColor * atten;
        }
        return total;
    }

	// ==========================================
	// === Global Noise 函式 ====================
	// ==========================================
	float simpleNoise(float2 p) {
		float2 i = floor(p);
        float2 f = frac(p);
        float2 u = f * f * (3.0 - 2.0 * f);
        float n00 = frac(sin(dot(i + float2(0, 0), float2(12.9898, 78.233))) * 43758.5453);
        float n10 = frac(sin(dot(i + float2(1, 0), float2(12.9898, 78.233))) * 43758.5453);
        float n01 = frac(sin(dot(i + float2(0, 1), float2(12.9898, 78.233))) * 43758.5453);
        float n11 = frac(sin(dot(i + float2(1, 1), float2(12.9898, 78.233))) * 43758.5453);
		return lerp(lerp(n00, n10, u.x), lerp(n01, n11, u.x), u.y);
    }

	// ==========================================
	// === Native Lighting 函式 =================
	// ==========================================
	float3 CustomShadeVertexLights(float3 viewPos, float3 viewNormal, float dirInt, float addInt) {
		float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
        for (int i = 0; i < 4; i++) {
			float3 toLight = unity_LightPosition[i].xyz - viewPos * unity_LightPosition[i].w;
            float lengthSq = max(dot(toLight, toLight), 0.000001);
			float3 dirToLight = toLight * rsqrt(lengthSq);
            float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z);
			float rho = max(0.0, dot(dirToLight, unity_SpotDirection[i].xyz));
            float spotAtt = (rho - unity_LightAtten[i].x) * unity_LightAtten[i].y;
			atten *= saturate(spotAtt);
			if (unity_LightPosition[i].w == 0) atten = 1.0;
            float diff = max(0.0, dot(viewNormal, dirToLight));
			float intensityMultiplier = (unity_LightPosition[i].w == 0) ? dirInt : addInt;
            lightColor += unity_LightColor[i].rgb * (diff * atten * intensityMultiplier);
		}
		return lightColor;
    }

	float3 CalculateNativeLightWithColor2D(float2 worldNormal2D, float3 worldPos, float focusPower, float dirInt, float addInt, float nativeInt) {
		float3 totCol = float3(0, 0, 0);
        for (int i = 0; i < 4; i++) {
			float3 lightPosWorld = mul(unity_CameraToWorld, float4(unity_LightPosition[i].xyz, 1.0)).xyz;
			float3 lightDirWorld = mul((float3x3)unity_CameraToWorld, unity_LightPosition[i].xyz);
            float3 lv = (unity_LightPosition[i].w == 0) ? lightDirWorld : (lightPosWorld - worldPos);
			float lengthSq = max(dot(lv, lv), 0.000001);
            float dist = sqrt(lengthSq);
			float3 L = (dist > 1e-4) ? (lv / dist) : float3(0, 1, 0);
            float NdotL = dot(worldNormal2D, L.xy);
			float hL = saturate((NdotL + 1.0) * 0.5);
			
			float3 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0)).xyz;
            float3 toLightView = unity_LightPosition[i].xyz - viewPos * unity_LightPosition[i].w;
			float viewLengthSq = max(dot(toLightView, toLightView), 0.000001);
            float atten = 1.0 / (1.0 + viewLengthSq * unity_LightAtten[i].z);
			
			float3 dirToLightView = toLightView * rsqrt(viewLengthSq);
            float rho = max(0.0, dot(dirToLightView, unity_SpotDirection[i].xyz));
			float spotAtt = (rho - unity_LightAtten[i].x) * unity_LightAtten[i].y;
			atten *= saturate(spotAtt);
            if (unity_LightPosition[i].w == 0) atten = 1.0;
			
			float ct = pow(hL, focusPower) * atten;
            float intensityMultiplier = (unity_LightPosition[i].w == 0) ? dirInt : addInt;
			
			totCol += unity_LightColor[i].rgb * (ct * intensityMultiplier * nativeInt);
        }
		return totCol;
	}

	// ==========================================
	// === Light Probe (SH) 函式 ================
	// ==========================================
	float3 GetSHDominantDir() {
		float3 dirX = float3(_SpineSHAr.x, _SpineSHAg.x, _SpineSHAb.x);
        float3 dirY = float3(_SpineSHAr.y, _SpineSHAg.y, _SpineSHAb.y);
        float3 dirZ = float3(_SpineSHAr.z, _SpineSHAg.z, _SpineSHAb.z);
        float3 domDir = float3(dot(dirX, float3(0.299, 0.587, 0.114)), dot(dirY, float3(0.299, 0.587, 0.114)), dot(dirZ, float3(0.299, 0.587, 0.114)));
        float len = length(domDir);
        return (len > 1e-4) ? (domDir / len) : float3(0, 1, 0);
    }

	float3 EvaluateSpineSH(float3 normal) {
        float4 vA = float4(normal, 1.0);
        float3 res;
        res.r = dot(_SpineSHAr, vA);
        res.g = dot(_SpineSHAg, vA);
        res.b = dot(_SpineSHAb, vA);
        float4 vB = float4(normal.x * normal.y, normal.y * normal.z, normal.z * normal.z, normal.z * normal.x);
        res.r += dot(_SpineSHBr, vB);
        res.g += dot(_SpineSHBg, vB);
        res.b += dot(_SpineSHBb, vB);
        float vC = normal.x * normal.x - normal.y * normal.y;
        res += _SpineSHC.rgb * vC;
        return max(res * _LightProbeIntensity, 0.0);
    }

	float3 GetProbeDominantColor() {
		return (EvaluateSpineSH(float3(0, 1, 0)) + EvaluateSpineSH(float3(0,-1, 0))) * 0.5;
    }

	float GetProbeDirectionalLuma(float2 worldNormal2D, float focusPower) {
		float3 domDir = GetSHDominantDir();
        float NdotL = dot(worldNormal2D, domDir.xy);
        float hL = saturate((NdotL + 1.0) * 0.5); float focus = pow(hL, focusPower);
        float3 col = EvaluateSpineSH(normalize(float3(worldNormal2D, 0.5)));
        return dot(col, float3(0.299, 0.587, 0.114)) * focus;
    }

	float3 GetProbeDirectionalColor(float2 worldNormal2D, float focusPower) {
		float3 domDir = GetSHDominantDir();
        float NdotL = dot(worldNormal2D, domDir.xy);
        float hL = saturate((NdotL + 1.0) * 0.5); float focus = pow(hL, focusPower);
        return EvaluateSpineSH(normalize(float3(worldNormal2D, 0.5))) * focus;
    }

	float2 TransformNormalToWorld(float2 texNorm, float2 col0, float2 col1) {
		float2 wn = float2(texNorm.x * col0.x + texNorm.y * col1.x, texNorm.x * col0.y + texNorm.y * col1.y);
        float  len = length(wn); return (len > 1e-4) ? (wn / len) : float2(0, 0);
    }

	float CalculateMultiLightIntensity(float2 worldNormal2D, float3 worldPos, float focusPower) {
		if (_SpineLightCount <= 0) return 0.0;
		float total = 0;
        [unroll(8)]
		for (int i = 0; i < 8; i++) {
			if (i >= _SpineLightCount) break;
			float  type = _SpineLightPos[i].w;
            float3 lv = _SpineLightPos[i].xyz - worldPos * step(0.5, type);
			float  sqrDist = dot(lv, lv); float  dist = sqrt(sqrDist);
            float3 L = (dist > 1e-4) ? (lv / dist) : float3(0, 1, 0);
			float  NdotL = dot(worldNormal2D, L.xy);
            float  hL = saturate((NdotL + 1.0) * 0.5);
			hL *= lerp(1.0, saturate((L.z + 1.0) * 0.5), step(0.5, type));
            float atten = 1.0;
			if (type > 0.5) {
				float sRI = _SpineLightColor[i].w; float dR = sqrDist * sRI;
                float dA = saturate(1.0 - dR * dR); atten = dA * dA;
                if (type > 1.5) { float sA = saturate(dot(-L, _SpineLightSpotParams[i].xyz) + _SpineLightSpotParams[i].w); atten *= sA * sA;
                }
			}
			total += pow(hL, focusPower) * atten * dot(_SpineLightColor[i].rgb, float3(0.299,0.587,0.114));
		}
		return total;
    }

	float3 CalculateAverageLightColor(float3 worldPos) {
		if (_SpineLightCount <= 0) return float3(0, 0, 0);
		float3 total = float3(0, 0, 0);
        [unroll(8)]
		for (int i = 0; i < 8; i++) {
			if (i >= _SpineLightCount) break;
			float  type = _SpineLightPos[i].w;
            float3 lv = _SpineLightPos[i].xyz - worldPos * step(0.5, type);
			float  sqrDist = dot(lv, lv); float  atten = 1.0;
            if (type > 0.5) {
				float sRI = _SpineLightColor[i].w; float dR = sqrDist * sRI;
                float dA = saturate(1.0 - dR * dR); atten = dA * dA;
                if (type > 1.5) { float dist = sqrt(sqrDist); float3 L = (dist > 1e-4) ?
                (lv / dist) : float3(0, 1, 0); float sA = saturate(dot(-L, _SpineLightSpotParams[i].xyz) + _SpineLightSpotParams[i].w); atten *= sA * sA;
                }
			}
			total += _SpineLightColor[i].rgb * atten;
		}
		return total;
	}

	float CalculateMultiLightWithColor(float2 worldNormal2D, float3 worldPos, float focusPower, out float3 outColor) {
		outColor = float3(0, 0, 0);
        if (_SpineLightCount <= 0) return 0.0;
		float  total = 0; float3 totCol = float3(0, 0, 0);
        [unroll(8)]
		for (int i = 0; i < 8; i++) {
			if (i >= _SpineLightCount) break;
			float  type = _SpineLightPos[i].w;
            float3 lv = _SpineLightPos[i].xyz - worldPos * step(0.5, type);
			float  sqrDist = dot(lv, lv); float  dist = sqrt(sqrDist);
            float3 L = (dist > 1e-4) ? (lv / dist) : float3(0, 1, 0);
			float  NdotL = dot(worldNormal2D, L.xy);
            float  hL = saturate((NdotL + 1.0) * 0.5);
			hL *= lerp(1.0, saturate((L.z + 1.0) * 0.5), step(0.5, type));
            float atten = 1.0;
			if (type > 0.5) {
				float sRI = _SpineLightColor[i].w; float dR = sqrDist * sRI;
                float dA = saturate(1.0 - dR * dR); atten = dA * dA;
                if (type > 1.5) { float sA = saturate(dot(-L, _SpineLightSpotParams[i].xyz) + _SpineLightSpotParams[i].w); atten *= sA * sA;
                }
			}
			float3 rc = _SpineLightColor[i].rgb; float  ct = pow(hL, focusPower) * atten;
			totCol += rc * ct;
            total += ct * dot(rc, float3(0.299,0.587,0.114));
		}
		if (dot(totCol, float3(0.299,0.587,0.114)) > 1e-4) outColor = totCol;
		return total;
    }

	float SampleEdgeAtRadius(sampler2D mask, float2 screenUV, float2 texelSize, float radius) {
		float dx = texelSize.x * radius, dy = texelSize.y * radius;
        float C = tex2D(mask, screenUV).r; float R = tex2D(mask, screenUV + float2( dx, 0)).r;
        float L = tex2D(mask, screenUV + float2(-dx, 0)).r;
		float U = tex2D(mask, screenUV + float2(0,  dy)).r;
        float D = tex2D(mask, screenUV + float2(0, -dy)).r;
		float UR = tex2D(mask, screenUV + float2( dx,  dy)).r;
        float UL = tex2D(mask, screenUV + float2(-dx,  dy)).r;
		float DR = tex2D(mask, screenUV + float2( dx, -dy)).r;
        float DL = tex2D(mask, screenUV + float2(-dx, -dy)).r;
		float minA = min(min(min(R, L), min(U, D)), min(min(UR, UL), min(DR, DL)));
        return saturate((C - minA) * 2.0);
	}

	void ApplyBloomSuppression(inout float3 c, float t) {
		float m = max(c.r, max(c.g, c.b));
        if (m > t) c *= (t / m);
	}

	ENDCG

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType" = "Sprite" "CanUseSpriteAtlas" = "True" "PreviewType"="Plane" "BloomSpine" = "Normal"}
		LOD 100
		Fog { Mode Off } 
		Cull Off 
		ZWrite Off 
		Blend One OneMinusSrcAlpha

		// ==========================================
		// === Outline Pass =========================
		// ==========================================
		Pass {
			Name "Outline"
			ZTest [_OutlineZTest]
			Stencil { Ref [_StencilRef] Comp NotEqual }

			CGPROGRAM
			#pragma shader_feature_local _USE8NEIGHBOURHOOD_ON
			#pragma shader_feature_local _USE_SCREENSPACE_OUTLINE_WIDTH
			#pragma shader_feature_local _OUTLINE_FILL_INSIDE
			#pragma shader_feature_local _MODULE_OUTLINE // 重新加入 Keyword 宣告

			#pragma vertex OutlineVertex
			#pragma fragment OutlineFragment

			#include "CGIncludes/Spine-Common.cginc"
			#include "CGIncludes/Spine-Outline-Common.cginc"

			sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
			float _OutlineWidth; float _OutlineReferenceTexWidth; float _OutlineMipLevel;
			float _OutlineSmoothness; float _ThresholdEnd; float _OutlineOpaqueAlpha;
			float4 _OutlineColor;
			
			float  _OL_NoiseIntensity;
            float4 _GlobalNoiseScale; float4 _GlobalNoiseSpeed;
            
            float _OL_RT_Power; float _OL_RT_Blend; float _OL_RT_Intensity;
            float _OL_Probe_Power; float _OL_Probe_Blend; float _OL_Probe_Intensity;
            float _OL_Shadow_Power; float _OL_Shadow_Blend;
            float _OL_Shadow_Intensity;
            float4 _EffectColor; float _EffectIntensity;

			struct VertexInputOutline {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
                float3 normal : NORMAL;
            };
			struct VertexOutputOutline {
				float4 pos : SV_POSITION; float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
                float3 worldPos : TEXCOORD1;
                float2 o2w_col0 : TEXCOORD2; float2 o2w_col1 : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
			};
            VertexOutputOutline OutlineVertex(VertexInputOutline v) {
				VertexOutputOutline o;
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertexColor = PMAGammaToTargetSpace(v.vertexColor);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.o2w_col0 = unity_ObjectToWorld._m00_m10; o.o2w_col1 = unity_ObjectToWorld._m01_m11;

				float3 wNorm = UnityObjectToWorldNormal(v.normal);
                if (dot(wNorm, wNorm) < 1e-4) wNorm = float3(0, 0, -1);
                o.worldNormal = wNorm;

                return o;
            }

			float4 OutlineFragment(VertexOutputOutline i) : SV_Target {
				// === 靜態剔除邏輯 ===
				#if !defined(_MODULE_OUTLINE)
				discard; // 變體完全剔除計算
				return float4(0,0,0,0);
				#endif

                if (_EnableOutline < 0.5) discard;
                float globalNoiseVal = 1.0;
				#if defined(_MODULE_GLOBAL_NOISE)
                if (_EnableGlobalNoise > 0.5) {
					float2 noiseUV = i.worldPos.xy * _GlobalNoiseScale.xy + _Time.y * _GlobalNoiseSpeed.xy;
                    globalNoiseVal = simpleNoise(noiseUV);
                }
				#endif

				float cw = _OutlineWidth;
				if (cw <= 0) discard;
                float useRT     = step(0.5, frac((float)_OutlineLightingMode * 0.5));
				float useProbe  = step(0.5, frac((float)_OutlineLightingMode * 0.25));
                float useShadow = step(0.5, frac((float)_OutlineLightingMode * 0.125));
				float dx = _MainTex_TexelSize.x, dy = _MainTex_TexelSize.y;
                float2 texNormal = float2(tex2D(_MainTex, i.uv + float2(-dx, 0)).a - tex2D(_MainTex, i.uv + float2(dx, 0)).a,
										  tex2D(_MainTex, i.uv + float2(0, -dy)).a - tex2D(_MainTex, i.uv + float2(0, dy)).a);
                float nLen = length(texNormal);
				float2 worldN2D = TransformNormalToWorld(nLen > 1e-4 ? texNormal / nLen : float2(0,0), i.o2w_col0, i.o2w_col1);
                float rtPresenceMask = 1.0;
                
                if (useRT > 0.5 && useProbe < 0.5 && useShadow < 0.5) {
                    float3 rtColAvg = CalculateAverageLightColor(i.worldPos) * _OL_RT_Intensity;
                    rtPresenceMask = saturate(dot(rtColAvg, float3(0.299, 0.587, 0.114)));
                }

				float ignoreBehind = _OL_SL_IgnoreBehind;
                if (_EnableDirectionalOutline > 0.5) {
				    if (nLen > 1e-4) {
					    float totalMul = 1.0 - max(max(useRT, useProbe), useShadow);
                        if (useRT > 0.5) totalMul += CalculateMultiLightIntensity(worldN2D, i.worldPos, _OL_RT_Power);
					    
					    #if defined(_MODULE_LIGHT_PROBE)
					    if (_EnableLightProbe > 0.5) {
					        if (useProbe > 0.5) totalMul += GetProbeDirectionalLuma(worldN2D, _OL_Probe_Power);
                        }
					    #endif

					    #if defined(_MODULE_SHADOW_LIGHT)
                        if (useShadow > 0.5) totalMul += CalculateShadowLightIntensity(worldN2D, i.worldPos, i.worldNormal, _OL_Shadow_Power, ignoreBehind);
                        #endif

                        cw *= saturate(totalMul);
                        } else { 
                        cw = 0;
                        }
                } else {
				    cw *= rtPresenceMask;
                }

				#if defined(_MODULE_GLOBAL_NOISE)
                if (_EnableGlobalNoise > 0.5 && _OL_EnableNoise > 0.5) {
					cw *= saturate(globalNoiseVal + 1.0 - _OL_NoiseIntensity * 2.0);
                }
				#endif

				float4 finalOutlineColor = _OutlineColor;

                if (_OutlineTintByLight > 0.5) {
                    float3 finalTint = _OutlineColor.rgb;
                    if (useRT > 0.5) {
						float3 rtCol = float3(0,0,0);
                        if (_EnableDirectionalOutline > 0.5) {
							CalculateMultiLightWithColor(worldN2D, i.worldPos, _OL_RT_Power, rtCol);
                        } else {
							rtCol = CalculateAverageLightColor(i.worldPos);
                        }
                        rtCol *= _OL_RT_Intensity;
                        float rtLuma = dot(rtCol, float3(0.299, 0.587, 0.114));
                        float rtBlendWeight = saturate(rtLuma) * _OL_RT_Blend;
                        finalTint = lerp(finalTint, rtCol, rtBlendWeight);
                    }
                    
                    #if defined(_MODULE_LIGHT_PROBE)
                    if (_EnableLightProbe > 0.5) {
					    if (useProbe > 0.5) {
						    float3 pc = float3(0,0,0);
                            if (_EnableDirectionalOutline > 0.5) {
							    pc = GetProbeDirectionalColor(worldN2D, _OL_Probe_Power);
                            } else {
							    pc = GetProbeDominantColor();
                            }
                            pc *= _OL_Probe_Intensity;
                            if (dot(pc, 1.0) > 1e-4) finalTint = lerp(finalTint, pc, _OL_Probe_Blend);
                        }
					}
                    #endif
                    
                    #if defined(_MODULE_SHADOW_LIGHT)
                    if (useShadow > 0.5) {
               
                         float3 sc = float3(0,0,0);
                        if (_EnableDirectionalOutline > 0.5) {
                            CalculateShadowLightWithColor(worldN2D, i.worldPos, i.worldNormal, _OL_Shadow_Power, ignoreBehind, sc);
                        } else {
                            sc = CalculateAverageShadowLightColor(i.worldPos, i.worldNormal, ignoreBehind);
                        }
                        sc *= _OL_Shadow_Intensity;
                        if (dot(sc, 1.0) > 1e-4) finalTint = lerp(finalTint, sc, _OL_Shadow_Blend);
                    }
                    #endif

                    finalOutlineColor.rgb = finalTint;
                } else {
                    #if defined(_MODULE_LIGHT_PROBE)
                    if (_EnableLightProbe > 0.5) {
				        if (useProbe > 0.5) {
					        float3 pa = GetProbeDominantColor();
                            finalOutlineColor.rgb = lerp(_OutlineColor.rgb, _OutlineColor.rgb * (1.0 + pa * 0.3 * _OL_Probe_Intensity), saturate(dot(pa, float3(0.299,0.587,0.114))) * _OL_Probe_Blend);
                        }
				    }
                    #endif
                }

				float4 result = computeOutlinePixel(
					_MainTex, _MainTex_TexelSize.xy, i.uv, i.vertexColor.a,
					cw, _OutlineReferenceTexWidth, _OutlineMipLevel, _OutlineSmoothness, _ThresholdEnd, _OutlineOpaqueAlpha, finalOutlineColor);
                result.a *= rtPresenceMask;
				result.a *= _Color.a;

                if (_MultiplyEdgeColor > 0.5) {
				    if (result.a > 0.001 && nLen > 1e-4) {
					    float4 ec = tex2D(_MainTex, i.uv + (-texNormal * (dx * cw * 0.5)));
                        if (ec.a > 0) ec.rgb /= ec.a; 
                        result.rgb *= ec.rgb;
                        }
                }

				#if defined(_MODULE_SHADOW_LIGHT)
                if (_EnableShadowLight > 0.5) {
				    if (useShadow > 0.5)
				        ApplyShadowLights(i.worldPos, i.worldNormal, result.rgb, result.a, ignoreBehind, 1.0);
                }
				#endif

                #if defined(_MODULE_EFFECT_OVERLAY)
				if (_EffectIntensity > 0.001 && result.a > 0.001) {
					float3 eRgb = _EffectColor.rgb * result.a;
                    result.rgb = lerp(result.rgb, eRgb, _EffectIntensity);
                }
                #endif

                #if defined(_MODULE_BLOOM_SUPPRESSION)
				if (_EnableBloomSuppression > 0.5) {
					ApplyBloomSuppression(result.rgb, _BloomSuppressThreshold);
                }
                #endif

				return result;
            }
			ENDCG
		}

		// ==========================================
		// === Normal Pass ==========================
		// ==========================================
		Pass {
			Name "Normal"
			Tags { "LightMode"="Vertex" }
			ZTest LEqual
			Stencil { Ref [_StencilRef] Comp [_StencilComp] }

			CGPROGRAM
			#pragma vertex CustomLitVertex
			#pragma fragment CustomLitFragment
			#pragma target 3.0
			#pragma multi_compile __ POINT SPOT

			#include "CGIncludes/Spine-Common.cginc"

			sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;

			#if defined(_MODULE_NORMALMAP)
			sampler2D _NormalMap; float _NormalIntensity;
			#endif

			float4    _EffectColor; float _EffectIntensity;
            #if defined(_MODULE_NATIVE_LIGHTING)
			float     _NativeLightIntensity;
            float _DirectionalLightIntensity; float _AdditionalLightIntensity;
            #endif
			
			#if defined(_MODULE_BACKLIGHT)
            sampler2D _SpecularTex; float4 _SpecularColor; float _SpecularBlend;
            float _BL_RT_Power; float _BL_RT_Blend; float _BL_RT_Intensity; 
            float _BL_Probe_Power; float _BL_Probe_Blend;
            float _BL_Probe_Intensity;
            float _BL_Shadow_Power; float _BL_Shadow_Blend; float _BL_Shadow_Intensity;
            #endif
			
			#if defined(_MODULE_INLINE)
            float4    _InlineColor; float _InlineWidth;
            float _InlineFadeSteps;
            float _IL_RT_Power;
            float _IL_RT_Blend; float _IL_RT_Intensity;
            float _IL_Probe_Power; float _IL_Probe_Blend; float _IL_Probe_Intensity;
            float _IL_Shadow_Power; float _IL_Shadow_Blend; float _IL_Shadow_Intensity;
            sampler2D _SpineAlphaMask; float4 _SpineAlphaMask_TexelSize;
			#endif
			
			#if defined(_MODULE_EMISSION)
            sampler2D _EmissionTex;
            float4 _EmissionColor;
            float _EmissionBlend;
			float _EmissionIntensity;
			#endif

			#if defined(_MODULE_GLOBAL_NOISE)
			float4 _GlobalNoiseScale;
			float4 _GlobalNoiseSpeed;
			#endif

			float4 _HitPosition;
            float4 _SweepCenterOffset;
			float _SweepProgress; float4 _SweepColor;
            float _SweepWidth; float _SweepSoftness;

			#if defined(_MODULE_BACKLIGHT) && defined(_MODULE_GLOBAL_NOISE)
			float  _BL_NoiseIntensity;
			#endif
			#if defined(_MODULE_INLINE) && defined(_MODULE_GLOBAL_NOISE)
			float  _IL_NoiseIntensity;
            #endif
			
            struct ExtendedVertexInput { 
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
				float4 vertexColor : COLOR; 
                float3 normal : NORMAL; 
            };
			
            struct ExtendedVertexOutput {
				float4 pos : SV_POSITION;
                float4 uvAndAlpha : TEXCOORD0;
				float3 color : COLOR; 
                float3 worldPos : TEXCOORD1;
				#if defined(_MODULE_INLINE)
				float4 screenPos : TEXCOORD2;
                #endif
                float2 o2w_col0 : TEXCOORD3;
                float2 o2w_col1 : TEXCOORD4;
				float3 objCenter : TEXCOORD5; 
                float3 viewNormal : TEXCOORD6;
                float3 worldNormal : TEXCOORD7;
			};
            ExtendedVertexOutput CustomLitVertex(ExtendedVertexInput v) {
				ExtendedVertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
                float4 vcG = PMAGammaToTargetSpace(v.vertexColor);
                o.objCenter = mul(unity_ObjectToWorld, float4(_SweepCenterOffset.xyz, 1.0)).xyz;
                float3 wNorm = UnityObjectToWorldNormal(v.normal);
                if (dot(wNorm, wNorm) < 1e-4) { wNorm = float3(0, 0, -1);
                }
                float3 viewDirWorld = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex).xyz));
                wNorm *= (dot(wNorm, viewDirWorld) < 0.0) ? -1.0 : 1.0;
                o.viewNormal = mul((float3x3)UNITY_MATRIX_V, wNorm);
                o.worldNormal = wNorm;

				o.color = vcG.rgb;
                o.uvAndAlpha = float4(TRANSFORM_TEX(v.texcoord.xy, _MainTex), 0, vcG.a); 
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#if defined(_MODULE_INLINE)
				o.screenPos = ComputeScreenPos(o.pos);
                #endif
                o.o2w_col0 = unity_ObjectToWorld._m00_m10;
                o.o2w_col1 = unity_ObjectToWorld._m01_m11;
				return o;
            }

			float4 CustomLitFragment(ExtendedVertexOutput i) : SV_Target {
				float4 texColor = tex2D(_MainTex, i.uvAndAlpha.xy);
                // === Global Noise =================================
				float globalNoiseVal = 1.0;
				#if defined(_MODULE_GLOBAL_NOISE)
                if (_EnableGlobalNoise > 0.5) {
					float2 noiseUV = i.worldPos.xy * _GlobalNoiseScale.xy + _Time.y * _GlobalNoiseSpeed.xy;
                    globalNoiseVal = simpleNoise(noiseUV);
                }
				#endif

				texColor.rgb *= _Color.rgb;
                texColor.a *= _Color.a;
                if (_StraightAlphaInput > 0.5) {
					texColor.rgb *= texColor.a;
                }
				float mainTexAlpha = texColor.a;
                texColor.rgb *= i.color.rgb; 
                texColor.a *= i.uvAndAlpha.w;

				// === Native Lighting + Light Probe ================
                float3 baseNativeLight = float3(1.0, 1.0, 1.0);
                #if defined(_MODULE_NATIVE_LIGHTING)
                if (_EnableNativeLighting > 0.5) {
                    float3 viewPos = mul(UNITY_MATRIX_V, float4(i.worldPos, 1.0)).xyz;
                    float3 vN = normalize(i.viewNormal);
                    baseNativeLight = CustomShadeVertexLights(viewPos, vN, _DirectionalLightIntensity, _AdditionalLightIntensity) * _NativeLightIntensity;
                }
				#endif

				#if defined(_MODULE_LIGHT_PROBE)
                if (_EnableLightProbe > 0.5) {
					#if defined(_MODULE_NATIVE_LIGHTING)
                    float3 probeNormal = normalize(i.worldNormal);
                    #else
					float3 probeNormal = float3(0, 0, -1);
                    #endif
                    float3 sh = EvaluateSpineSH(probeNormal);
                    #if defined(_MODULE_NATIVE_LIGHTING)
                    if (_EnableNativeLighting > 0.5) {
                        baseNativeLight = lerp(baseNativeLight, baseNativeLight + sh, _LightProbeNativeBlend);
                    } else {
                        baseNativeLight = lerp(float3(1.0, 1.0, 1.0), sh, _LightProbeNativeBlend);
                    }
					#else
					baseNativeLight = lerp(float3(1.0, 1.0, 1.0), sh, _LightProbeNativeBlend);
					#endif
                }
				#endif
                
                texColor.rgb *= baseNativeLight;
                // === 準備 wN2D（NormalMap / BackLight / Inline 共用）
                float2 uv = i.uvAndAlpha.xy;
                #if defined(_MODULE_NORMALMAP) || defined(_MODULE_BACKLIGHT) || defined(_MODULE_INLINE)
				float bDx = _MainTex_TexelSize.x * max(1.0, 
					#if defined(_MODULE_INLINE)
					_InlineWidth
					#else
					1.0
					#endif
				);
				float bDy = _MainTex_TexelSize.y * max(1.0,
					#if defined(_MODULE_INLINE)
					_InlineWidth
					#else
					1.0
					#endif
				);
                float2 texN = float2(tex2D(_MainTex, uv + float2(-bDx, 0)).a - tex2D(_MainTex, uv + float2(bDx, 0)).a,
									 tex2D(_MainTex, uv + float2(0, -bDy)).a - tex2D(_MainTex, uv + float2(0,  bDy)).a);
                float  nLen  = length(texN);
				float2 wN2D  = TransformNormalToWorld(nLen > 1e-4 ? texN / nLen : float2(0,0), i.o2w_col0, i.o2w_col1);
                #endif

				// === Normal Map Module ============================
                float3 nmDelta = float3(0,0,0);
                #if defined(_MODULE_NORMALMAP)
                if (_EnableNormalMap > 0.5) {
                    float4 packedNorm = tex2D(_NormalMap, uv);
                    float3 tNorm = UnpackNormal(packedNorm);
                    
                    if (_InvertNormalMap > 0.5) {
                        tNorm.xy = -tNorm.xy;
                    }

                    float2 bumpN2D = float2(
                        tNorm.x * i.o2w_col0.x + tNorm.y * i.o2w_col1.x,
                        tNorm.x * i.o2w_col0.y + tNorm.y * i.o2w_col1.y
                 
                   );
                    float bumpLen = length(bumpN2D);
					bumpN2D = (bumpLen > 1e-4) ? (bumpN2D / bumpLen) : float2(0, 0);
                    float2 flatN2D = float2(0, 0);
					float3 baseLight = float3(0,0,0);
					float3 bumpedLight = float3(0,0,0);
                    float3 rtBase = float3(0,0,0);
                    float3 rtBump = float3(0,0,0);
                    CalculateMultiLightWithColor(flatN2D, i.worldPos, 1.0, rtBase);
                    CalculateMultiLightWithColor(bumpN2D, i.worldPos, 1.0, rtBump);
                    baseLight += rtBase; bumpedLight += rtBump;
                    #if defined(_MODULE_NATIVE_LIGHTING)
                    if (_EnableNativeLighting > 0.5) {
                        float3 nativeBase = CalculateNativeLightWithColor2D(flatN2D, i.worldPos, 1.0, _DirectionalLightIntensity, _AdditionalLightIntensity, _NativeLightIntensity);
                        float3 nativeBump = CalculateNativeLightWithColor2D(bumpN2D, i.worldPos, 1.0, _DirectionalLightIntensity, _AdditionalLightIntensity, _NativeLightIntensity);
                        baseLight += nativeBase; bumpedLight += nativeBump;
                    }
					#endif

					#if defined(_MODULE_LIGHT_PROBE)
                    if (_EnableLightProbe > 0.5) {
                        baseLight += GetProbeDirectionalColor(flatN2D, 1.0);
                        bumpedLight += GetProbeDirectionalColor(bumpN2D, 1.0);
                    }
					#endif

					#if defined(_MODULE_SHADOW_LIGHT)
                    if (_EnableShadowLight > 0.5) {
                        float3 slBase = float3(0,0,0);
                        float3 slBump = float3(0,0,0);
                        // Normal Map Base 不套用背後忽略
                        CalculateShadowLightWithColor(flatN2D, i.worldPos, i.worldNormal, 1.0, 0.0, slBase);
                        if (_SL_AffectsNormalMap > 0.5) {
                            CalculateShadowLightWithColor(bumpN2D, i.worldPos, i.worldNormal, 1.0, 0.0, slBump);
                        } else {
                            slBump = slBase;
                        }
                        
                        baseLight += slBase;
                        bumpedLight += slBump;
                    }
					#endif

                    float3 rawDelta = (bumpedLight - baseLight) * texColor.rgb;
                    float3 scaledDelta = rawDelta * _NormalIntensity;
					float deltaStrength = saturate(dot(abs(rawDelta), float3(1,1,1)) * 10.0);
					nmDelta = scaledDelta * deltaStrength;
                }
				#endif 

				// === BackLight ====================================
				float3 blDelta = float3(0,0,0);
				#if defined(_MODULE_BACKLIGHT)
                if (_EnableBackLight > 0.5) {
					float blRT     = step(0.5, frac((float)_BackLightLightingMode * 0.5));
                    float blProbe  = step(0.5, frac((float)_BackLightLightingMode * 0.25));
					float blShadow = step(0.5, frac((float)_BackLightLightingMode * 0.125));

					float4 texSpec = tex2D(_SpecularTex, uv);
                    float specLum = dot(texSpec.rgb * _SpecularColor.rgb, float3(0.299,0.587,0.114));
					float3 specMask = texSpec.rgb * step(0.001, specLum);
                    if (_EnableDirectionalBackLight > 0.5) {
					    if (nLen > 1e-4) {
						    float totalMul = 1.0 - max(max(blRT, blProbe), blShadow);
                            if (blRT > 0.5) totalMul += CalculateMultiLightIntensity(wN2D, i.worldPos, _BL_RT_Power);

						    #if defined(_MODULE_LIGHT_PROBE)
                            if (_EnableLightProbe > 0.5) {
						        if (blProbe > 0.5) totalMul += GetProbeDirectionalLuma(wN2D, _BL_Probe_Power);
                            }
							#endif

							#if defined(_MODULE_SHADOW_LIGHT)
                            if (blShadow > 0.5) totalMul += CalculateShadowLightIntensity(wN2D, i.worldPos, i.worldNormal, _BL_Shadow_Power, _BL_SL_IgnoreBehind);
                            #endif

                            specMask *= saturate(totalMul);
                            } else { 
                            specMask = float3(0,0,0);
                            }
					}

                    if (_SpecularMulMain > 0.5) {
						specMask *= texColor.rgb;
                    }
					
                    specMask *= mainTexAlpha * _Color.a;
                    #if defined(_MODULE_GLOBAL_NOISE)
                    if (_EnableGlobalNoise > 0.5 && _BL_EnableNoise > 0.5) {
						specMask *= saturate(globalNoiseVal + 1.0 - _BL_NoiseIntensity * 2.0);
                    }
					#endif

					float3 finalBlColor = _SpecularColor.rgb;
                    
                    if (_BackLightTintByLight > 0.5) {
                        float3 finalTint = finalBlColor;
                        if (blRT > 0.5) {
                            float3 rtTint = float3(0,0,0);
                            if (_EnableDirectionalBackLight > 0.5) {
                                CalculateMultiLightWithColor(wN2D, i.worldPos, _BL_RT_Power, rtTint);
                            } else {
                                rtTint = CalculateAverageLightColor(i.worldPos);
                            }
                            rtTint *= _BL_RT_Intensity;
                            if (dot(rtTint, 1.0) > 1e-4) finalTint = lerp(finalTint, rtTint, _BL_RT_Blend);
                        }
                        
						#if defined(_MODULE_LIGHT_PROBE)
                        if (_EnableLightProbe > 0.5) {
                            if (blProbe > 0.5) {
               
                                  float3 pc = float3(0,0,0);
                                if (_EnableDirectionalBackLight > 0.5) {
                                    pc = GetProbeDirectionalColor(wN2D, _BL_Probe_Power);
                                } else {
                                    pc = GetProbeDominantColor();
                                }
                                pc *= _BL_Probe_Intensity;
                                if (dot(pc, 1.0) > 1e-4) finalTint = lerp(finalTint, pc, _BL_Probe_Blend);
                            }
                        }
						#endif

						#if defined(_MODULE_SHADOW_LIGHT)
                        if (blShadow > 0.5) {
                            float3 sc = float3(0,0,0);
                            if (_EnableDirectionalBackLight > 0.5) {
                                CalculateShadowLightWithColor(wN2D, i.worldPos, i.worldNormal, _BL_Shadow_Power, _BL_SL_IgnoreBehind, sc);
                            } else {
                                sc = CalculateAverageShadowLightColor(i.worldPos, i.worldNormal, _BL_SL_IgnoreBehind);
                            }
                            sc *= _BL_Shadow_Intensity;
                            if (dot(sc, 1.0) > 1e-4) finalTint = lerp(finalTint, sc, _BL_Shadow_Blend);
                        }
						#endif

                        finalBlColor = finalTint;
                    } else {
					    #if defined(_MODULE_LIGHT_PROBE)
					    if (_EnableLightProbe > 0.5) {
					        if (blProbe > 0.5) {
						        float3 pa = GetProbeDominantColor();
                                finalBlColor = lerp(finalBlColor, finalBlColor * (1.0 + pa * 0.5 * _BL_Probe_Intensity), saturate(dot(pa, float3(0.299,0.587,0.114))) * _BL_Probe_Blend);
                            }
                        }
					    #endif
					}

					float3 specResult = specMask * finalBlColor;
                    if (_BackLightMode > 0.5) {
						blDelta = specResult * _SpecularBlend;
					} else {
						blDelta = lerp(texColor.rgb, specResult, _SpecularBlend) - texColor.rgb;
                    }
				}
				#endif

				// === Emission =====================================
                float3 emissionDelta = float3(0,0,0);
                #if defined(_MODULE_EMISSION)
                if (_EnableEmission > 0.5) {
                    float4 texEmi = tex2D(_EmissionTex, uv);
                    float emiLum = dot(texEmi.rgb, float3(0.299, 0.587, 0.114));
                    float emiMask = step(0.001, emiLum);
					float3 emiResult = texEmi.rgb * _EmissionColor.rgb * _EmissionIntensity;
                    if (_EmissionMulMain > 0.5) {
                        emiResult *= texColor.rgb;
                    }
                    emiResult *= mainTexAlpha * _Color.a;
                    emissionDelta = lerp(texColor.rgb, emiResult, emiMask * _EmissionBlend) - texColor.rgb;
				}
				#endif

				// === Inline =======================================
				float3 inlineDelta = float3(0,0,0);
                #if defined(_MODULE_INLINE)
				if (_EnableInline > 0.5) {
					float ilRT     = step(0.5, frac((float)_InlineLightingMode * 0.5));
                    float ilProbe  = step(0.5, frac((float)_InlineLightingMode * 0.25));
                    float ilShadow = step(0.5, frac((float)_InlineLightingMode * 0.125));
                    float2 screenUV = i.screenPos.xy / i.screenPos.w;
					int steps = clamp((int)(_InlineFadeSteps + 0.5), 2, 8); float stepsF = (float)steps;
                    float wSum = stepsF * (stepsF + 1.0) * 0.5;
					float gMask = 0, outerE = 0;
                    [unroll(8)]
					for (int k = 0; k < 8; k++) {
						if (k >= steps) break;
                        float t = (steps > 1) ? ((float)k / (stepsF - 1.0)) : 0.0;
                        float r = _InlineWidth * lerp(1.0, 0.15, t); float wt = (stepsF - (float)k) / wSum;
                        float ev = SampleEdgeAtRadius(_SpineAlphaMask, screenUV, _SpineAlphaMask_TexelSize.xy, r); 
                        gMask += ev * wt; 
                        if (k == 0) outerE = ev;
                    }

					float inlineMask = gMask * step(0.001, outerE) * step(0.01, mainTexAlpha);

                    if (_EnableDirectionalInline > 0.5) {
						float mDx = _SpineAlphaMask_TexelSize.x * _InlineWidth;
                        float mDy = _SpineAlphaMask_TexelSize.y * _InlineWidth;
						float2 sGrad = float2(tex2D(_SpineAlphaMask, screenUV + float2(-mDx, 0)).r - tex2D(_SpineAlphaMask, screenUV + float2(mDx, 0)).r,
											  tex2D(_SpineAlphaMask, screenUV + float2(0, -mDy)).r - tex2D(_SpineAlphaMask, screenUV + float2(0,  mDy)).r);
                        if (length(sGrad) > 1e-4) {
							float2 wGrad = normalize(sGrad.x * unity_CameraToWorld._m00_m10_m20.xy + sGrad.y * unity_CameraToWorld._m01_m11_m21.xy);
                            float totalMul = 1.0 - max(max(ilRT, ilProbe), ilShadow);
							if (ilRT > 0.5) totalMul += CalculateMultiLightIntensity(wGrad, i.worldPos, _IL_RT_Power);
                            #if defined(_MODULE_LIGHT_PROBE)
                            if (_EnableLightProbe > 0.5) {
							    if (ilProbe > 0.5) totalMul += GetProbeDirectionalLuma(wGrad, _IL_Probe_Power);
                            }
							#endif

							#if defined(_MODULE_SHADOW_LIGHT)
                            if (ilShadow > 0.5) totalMul += CalculateShadowLightIntensity(wGrad, i.worldPos, i.worldNormal, _IL_Shadow_Power, _IL_SL_IgnoreBehind);
                            #endif

                            inlineMask *= saturate(totalMul);
                            } else { inlineMask = 0; }
					}

					#if defined(_MODULE_GLOBAL_NOISE)
                    if (_EnableGlobalNoise > 0.5 && _IL_EnableNoise > 0.5) {
						inlineMask *= saturate(globalNoiseVal + 1.0 - _IL_NoiseIntensity * 2.0);
                    }
					#endif

					float3 inlineRgb = _InlineColor.rgb;
                    
                    if (_InlineTintByLight > 0.5) {
                        float3 finalTint = inlineRgb;
                        if (ilRT > 0.5) {
                            float3 rtTint = CalculateAverageLightColor(i.worldPos);
                            rtTint *= _IL_RT_Intensity;
                            if (dot(rtTint, 1.0) > 1e-4) finalTint = lerp(finalTint, rtTint, _IL_RT_Blend);
                        }
						#if defined(_MODULE_LIGHT_PROBE)
                        if (_EnableLightProbe > 0.5) {
                            if (ilProbe > 0.5) {
                                float3 pc = GetProbeDominantColor();
                                pc *= _IL_Probe_Intensity;
                                if (dot(pc, 1.0) > 1e-4) finalTint = lerp(finalTint, pc, _IL_Probe_Blend);
                            }
                        }
						#endif
						#if defined(_MODULE_SHADOW_LIGHT)
                        if (ilShadow > 0.5) {
                            float3 sc = CalculateAverageShadowLightColor(i.worldPos, i.worldNormal, _IL_SL_IgnoreBehind);
                            sc *= _IL_Shadow_Intensity;
                            if (dot(sc, 1.0) > 1e-4) finalTint = lerp(finalTint, sc, _IL_Shadow_Blend);
                        }
						#endif
                        inlineRgb = finalTint;
                    } else {
						#if defined(_MODULE_LIGHT_PROBE)
                        if (_EnableLightProbe > 0.5) {
					        if (ilProbe > 0.5) {
						        float3 pa = GetProbeDominantColor();
                                inlineRgb = lerp(inlineRgb, inlineRgb * (1.0 + pa * 0.3 * _IL_Probe_Intensity), saturate(dot(pa, float3(0.299,0.587,0.114))) * _IL_Probe_Blend);
                            }
                        }
						#endif
					}

					float inlineAlpha = _InlineColor.a * _Color.a;
                    float3 inlineRgbFinal = inlineRgb;
                    
                    if (_StraightAlphaInput < 0.5) {
						inlineRgbFinal *= texColor.a;
					}
					inlineDelta = (lerp(texColor.rgb, inlineRgbFinal, inlineMask * inlineAlpha)) - texColor.rgb;
                }
				#endif 

				// === Shadow Light（主色）==========================
				#if defined(_MODULE_SHADOW_LIGHT)
                if (_EnableShadowLight > 0.5) {
				    ApplyShadowLights(i.worldPos, i.worldNormal, texColor.rgb, texColor.a, 0.0, _ShadowLightBaseIntensity);
                // 這裡使用了 _ShadowLightBaseIntensity
                }
				#endif

				texColor.rgb += nmDelta + blDelta + emissionDelta + inlineDelta;
                // === Hit Sweep (移至光照與陰影燈後方，確保不受光壓暗) ===
                #if defined(_MODULE_HITSWEEP)
                if (_EnableHitSweep > 0.5) {
					float3 sweepDirRaw = i.objCenter - _HitPosition.xyz;
                    float sweepTotalDist = length(sweepDirRaw);
					if (sweepTotalDist > 0.001) {
						float3 sweepDir = sweepDirRaw / sweepTotalDist; float3 pixelOffset = i.worldPos - _HitPosition.xyz;
                        float projDist = dot(pixelOffset, sweepDir); float distNorm = projDist / sweepTotalDist;
						float distToLine = abs(distNorm - _SweepProgress);
                        float sweepMask = smoothstep(_SweepWidth + _SweepSoftness, _SweepWidth, distToLine);
						float3 sweepFinalColor = _SweepColor.rgb * sweepMask * texColor.a;
						texColor.rgb += sweepFinalColor;
                    }
				}
                #endif

                #if defined(_MODULE_EFFECT_OVERLAY)
				if (_EffectIntensity > 0.001) {
					float3 eRgb = _EffectColor.rgb * texColor.a;
                    texColor.rgb = lerp(texColor.rgb, eRgb, _EffectIntensity);
				}
                #endif

                #if defined(_MODULE_BLOOM_SUPPRESSION)
                if (_EnableBloomSuppression > 0.5) {
					ApplyBloomSuppression(texColor.rgb, _BloomSuppressThreshold);
                }
                #endif

				return texColor;
            }
			ENDCG
		}

		// ==========================================
		// === InlineAlways Pass ====================
		// ==========================================
		Pass {
			Name "InlineAlways"
			Tags { "LightMode"="Vertex" }
			ZTest Always
			ZWrite On 
			Blend One OneMinusSrcAlpha
			Stencil { Ref [_StencilRef] Comp [_StencilComp] }

			CGPROGRAM
			#pragma vertex InlineAlwaysVertex
			#pragma fragment InlineAlwaysFragment
			#pragma target 3.0

			#include "CGIncludes/Spine-Common.cginc"

			sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            #if defined(_MODULE_INLINE)
            float4 _InlineColor; float _InlineWidth;
            float _InlineFadeSteps; float _InlineZTest;
            float _IL_RT_Power; float _IL_RT_Blend; float _IL_RT_Intensity;
            float _IL_Probe_Power; float _IL_Probe_Blend;
            float _IL_Probe_Intensity;
            float _IL_Shadow_Power; float _IL_Shadow_Blend;
            float _IL_Shadow_Intensity;
            float4 _EffectColor; float _EffectIntensity;
			sampler2D _SpineAlphaMask; float4 _SpineAlphaMask_TexelSize;
			#endif
			#if defined(_MODULE_INLINE) && defined(_MODULE_GLOBAL_NOISE)
			float  _IL_NoiseIntensity;
            float4 _GlobalNoiseScale; float4 _GlobalNoiseSpeed;
            #endif

			struct InlineAlwaysVertIn {
				float4 vertex : POSITION; float2 texcoord : TEXCOORD0; float4 vertexColor : COLOR;
                float3 normal : NORMAL;
			};
            struct InlineAlwaysVertOut {
				float4 pos : SV_POSITION; float4 uvAndAlpha : TEXCOORD0;
				#if defined(_MODULE_INLINE)
				float4 screenPos : TEXCOORD1;
                #endif
                float3 worldPos : TEXCOORD2;
                float  colorAlpha : TEXCOORD3;
                float3 worldNormal : TEXCOORD4;
			};

			InlineAlwaysVertOut InlineAlwaysVertex(InlineAlwaysVertIn v) {
				InlineAlwaysVertOut o;
				o.pos = UnityObjectToClipPos(v.vertex);
                #if defined(UNITY_REVERSED_Z)
					o.pos.z = o.pos.w * 0.9999;
                #else
					o.pos.z = o.pos.w * -0.9999; 
				#endif
				float4 vcG = PMAGammaToTargetSpace(v.vertexColor);
				o.uvAndAlpha = float4(TRANSFORM_TEX(v.texcoord.xy, _MainTex), 0, vcG.a);
                #if defined(_MODULE_INLINE)
                o.screenPos = ComputeScreenPos(o.pos);
                #endif
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.colorAlpha = vcG.a;

                float3 wNorm = UnityObjectToWorldNormal(v.normal);
                if (dot(wNorm, wNorm) < 1e-4) wNorm = float3(0, 0, -1);
                o.worldNormal = wNorm;

				return o;
            }

			float4 InlineAlwaysFragment(InlineAlwaysVertOut i) : SV_Target {
				#if !defined(_MODULE_INLINE)
				discard;
				return float4(0,0,0,0);
                #else

				if (_InlineZTest < 7.5) discard;
                if (_EnableInline < 0.5) discard;
                float globalNoiseVal = 1.0;
                #if defined(_MODULE_GLOBAL_NOISE)
                if (_EnableGlobalNoise > 0.5) {
					float2 noiseUV = i.worldPos.xy * _GlobalNoiseScale.xy + _Time.y * _GlobalNoiseSpeed.xy;
                    globalNoiseVal = simpleNoise(noiseUV);
                }
				#endif

				float2 uv = i.uvAndAlpha.xy;
				float4 texColor = tex2D(_MainTex, uv); texColor.a *= _Color.a;
                if (_StraightAlphaInput > 0.5) {
					texColor.a *= texColor.a;
                }
				float mainTexAlpha = texColor.a * i.colorAlpha;
				float2 screenUV = i.screenPos.xy / i.screenPos.w;
                int steps = clamp((int)(_InlineFadeSteps + 0.5), 2, 8);
                float stepsF = (float)steps;
                float wSum = stepsF * (stepsF + 1.0) * 0.5;
                float gMask = 0, outerE = 0;
                [unroll(8)]
				for (int k = 0; k < 8; k++) {
					if (k >= steps) break;
                    float t = (steps > 1) ? ((float)k / (stepsF - 1.0)) : 0.0;
                    float r = _InlineWidth * lerp(1.0, 0.15, t); float wt = (stepsF - (float)k) / wSum;
                    float ev = SampleEdgeAtRadius(_SpineAlphaMask, screenUV, _SpineAlphaMask_TexelSize.xy, r); gMask += ev * wt; if (k == 0) outerE = ev;
                }

				float inlineMask = gMask * step(0.001, outerE) * step(0.01, mainTexAlpha);
                float ilRT     = step(0.5, frac((float)_InlineLightingMode * 0.5)); 
                float ilProbe  = step(0.5, frac((float)_InlineLightingMode * 0.25));
                float ilShadow = step(0.5, frac((float)_InlineLightingMode * 0.125));
                float ignoreBehind = _IL_SL_IgnoreBehind;
                if (_EnableDirectionalInline > 0.5) {
					float mDx = _SpineAlphaMask_TexelSize.x * _InlineWidth;
                    float mDy = _SpineAlphaMask_TexelSize.y * _InlineWidth;
                    float2 sGrad = float2(tex2D(_SpineAlphaMask, screenUV + float2(-mDx, 0)).r - tex2D(_SpineAlphaMask, screenUV + float2(mDx, 0)).r,
										  tex2D(_SpineAlphaMask, screenUV + float2(0, -mDy)).r - tex2D(_SpineAlphaMask, screenUV + float2(0,  mDy)).r);
                    if (length(sGrad) > 1e-4) {
						float2 wGrad = normalize(sGrad.x * unity_CameraToWorld._m00_m10_m20.xy + sGrad.y * unity_CameraToWorld._m01_m11_m21.xy);
                        float totalMul = 1.0 - max(max(ilRT, ilProbe), ilShadow);
						if (ilRT > 0.5) totalMul += CalculateMultiLightIntensity(wGrad, i.worldPos, _IL_RT_Power);
                        #if defined(_MODULE_LIGHT_PROBE)
                        if (_EnableLightProbe > 0.5) {
						    if (ilProbe > 0.5) totalMul += GetProbeDirectionalLuma(wGrad, _IL_Probe_Power);
                        }
						#endif

						#if defined(_MODULE_SHADOW_LIGHT)
                        if (ilShadow > 0.5) totalMul += CalculateShadowLightIntensity(wGrad, i.worldPos, i.worldNormal, _IL_Shadow_Power, ignoreBehind);
                        #endif

                        inlineMask *= saturate(totalMul);
                        } else { inlineMask = 0; }
				}

				#if defined(_MODULE_GLOBAL_NOISE)
                if (_EnableGlobalNoise > 0.5 && _IL_EnableNoise > 0.5) {
					inlineMask *= saturate(globalNoiseVal + 1.0 - _IL_NoiseIntensity * 2.0);
                }
				#endif

				if (inlineMask < 0.001) discard;
				float3 inlineRgb = _InlineColor.rgb;

                if (_InlineTintByLight > 0.5) {
                    float3 finalTint = inlineRgb;
                    if (ilRT > 0.5) {
                        float3 rtTint = CalculateAverageLightColor(i.worldPos);
                        rtTint *= _IL_RT_Intensity;
                        if (dot(rtTint, 1.0) > 1e-4) finalTint = lerp(finalTint, rtTint, _IL_RT_Blend);
                    }
					#if defined(_MODULE_LIGHT_PROBE)
                    if (_EnableLightProbe > 0.5) {
                        if (ilProbe > 0.5) {
                            float3 pc = GetProbeDominantColor();
                            pc *= _IL_Probe_Intensity;
                            if (dot(pc, 1.0) > 1e-4) finalTint = lerp(finalTint, pc, _IL_Probe_Blend);
                        }
                    }
					#endif
					#if defined(_MODULE_SHADOW_LIGHT)
                    if (ilShadow > 0.5) {
                        float3 sc = CalculateAverageShadowLightColor(i.worldPos, i.worldNormal, ignoreBehind);
                        sc *= _IL_Shadow_Intensity;
                        if (dot(sc, 1.0) > 1e-4) finalTint = lerp(finalTint, sc, _IL_Shadow_Blend);
                    }
					#endif
                    inlineRgb = finalTint;
                } else {
					#if defined(_MODULE_LIGHT_PROBE)
                    if (_EnableLightProbe > 0.5) {
					    if (ilProbe > 0.5) {
						    float3 pa = GetProbeDominantColor();
                                inlineRgb = lerp(inlineRgb, inlineRgb * (1.0 + pa * 0.3 * _IL_Probe_Intensity), saturate(dot(pa, float3(0.299,0.587,0.114))) * _IL_Probe_Blend);
                            }
                    }
					#endif
				}

				float inlineAlpha = _InlineColor.a * _Color.a;
                float outA = inlineMask * inlineAlpha; float3 outRgb = inlineRgb * outA;
                
                if (_StraightAlphaInput < 0.5) {
					outRgb *= texColor.a;
                }
				
				#if defined(_MODULE_SHADOW_LIGHT)
                if (_EnableShadowLight > 0.5) {
				    if (ilShadow > 0.5)
				        ApplyShadowLights(i.worldPos, i.worldNormal, outRgb, outA, ignoreBehind, 1.0);
                }
				#endif

                #if defined(_MODULE_EFFECT_OVERLAY)
				if (_EffectIntensity > 0.001 && outA > 0.001) {
					float3 eRgb = _EffectColor.rgb * outA;
                    outRgb = lerp(outRgb, eRgb, _EffectIntensity);
                }
                #endif

				return float4(outRgb, outA);
                #endif // _MODULE_INLINE
			}
			ENDCG
		}

		// ==========================================
		// === Caster Pass ==========================
		// ==========================================
		Pass {
			Name "Caster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1 
			ZWrite On 
			ZTest LEqual 
			Fog { Mode Off } 
			Cull Off 
			Lighting Off

			CGPROGRAM
			#pragma vertex vertShadow
			#pragma fragment fragShadow
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#define SHADOW_CUTOFF _Cutoff
			#include "CGIncludes/Spine-Skeleton-Lit-Common-Shadow.cginc"
			ENDCG
		}
	}
	CustomEditor "VFXTool.SpineSkeletonShaderTool.SpineCustomShaderGUI"
}