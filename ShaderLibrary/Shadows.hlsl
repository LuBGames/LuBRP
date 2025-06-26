#ifndef LUB_SHADOWS_INCLUDED
#define LUB_SHADOWS_INCLUDED

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
    float4x4 _DirectionalShadowMatrices;
    float _ShadowStrength;
    float _ShadowDistance;
    float2 _ShadowDistanceFade;
CBUFFER_END

float SampleDirectionalShadowAtlas (float3 positionSTS) {
    return SAMPLE_TEXTURE2D_SHADOW(
        _DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS
    );
}

float FadedShadowStrength (float distance, float scale, float fade) {
    return saturate((1.0 - distance * scale) * fade);
}

float GetDirectionalShadowAttenuation (float3 positionWS) {
    if (_ShadowStrength <= 0.0)
        return 1.0;

    float depth = -TransformWorldToView(positionWS).z;
    
    float3 positionSTS = mul(_DirectionalShadowMatrices, float4(positionWS, 1.0)).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    float fade = FadedShadowStrength(depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);
    return lerp(1.0, shadow, min(fade, _ShadowStrength));
}

#endif