#ifndef UNIVERSAL_INPUT_INCLUDED
#define UNIVERSAL_INPUT_INCLUDED

// #include "Packages/com.unity.render-pipelines.universal-config/Runtime/ShaderConfig.cs.hlsl"
//
// #define MAX_VISIBLE_LIGHTS_UBO  32
// #define MAX_VISIBLE_LIGHTS_SSBO 256
//
// // Keep in sync with RenderingUtils.useStructuredBuffer
// #define USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA 0

// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderTypes.cs.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Deprecated.hlsl"

// // Must match: UniversalRenderPipeline.maxVisibleAdditionalLights
// #if defined(SHADER_API_MOBILE) && (defined(SHADER_API_GLES) || defined(SHADER_API_GLES30))
//     #define MAX_VISIBLE_LIGHTS MAX_VISIBLE_LIGHT_COUNT_LOW_END_MOBILE
// #elif defined(SHADER_API_MOBILE) || (defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) // Workaround because SHADER_API_GLCORE is also defined when SHADER_API_SWITCH is
//     #define MAX_VISIBLE_LIGHTS MAX_VISIBLE_LIGHT_COUNT_MOBILE
// #else
//     #define MAX_VISIBLE_LIGHTS MAX_VISIBLE_LIGHT_COUNT_DESKTOP
// #endif

// // Match with values in UniversalRenderPipeline.cs
// #define MAX_ZBIN_VEC4S 1024
// #if MAX_VISIBLE_LIGHTS <= 16
//     #define MAX_LIGHTS_PER_TILE 32
//     #define MAX_TILE_VEC4S 1024
//     #define MAX_REFLECTION_PROBES 16
// #elif MAX_VISIBLE_LIGHTS <= 32
//     #define MAX_LIGHTS_PER_TILE 32
//     #define MAX_TILE_VEC4S 1024
//     #define MAX_REFLECTION_PROBES 32
// #else
//     #define MAX_LIGHTS_PER_TILE MAX_VISIBLE_LIGHTS
//     #define MAX_TILE_VEC4S 4096
//     #define MAX_REFLECTION_PROBES 64
// #endif

// struct InputData
// {
//     float3  positionWS;
//     float4  positionCS;
//     float3  normalWS;
//     half3   viewDirectionWS;
//     float4  shadowCoord;
//     half    fogCoord;
//     half3   vertexLighting;
//     half3   bakedGI;
//     float2  normalizedScreenSpaceUV;
//     half4   shadowMask;
//     half3x3 tangentToWorld;
//
//     #if defined(DEBUG_DISPLAY)
//     half2   dynamicLightmapUV;
//     half2   staticLightmapUV;
//     float3  vertexSH;
//
//     half3 brdfDiffuse;
//     half3 brdfSpecular;
//     float2 uv;
//     uint mipCount;
//
//     // texelSize :
//     // x = 1 / width
//     // y = 1 / height
//     // z = width
//     // w = height
//     float4 texelSize;
//
//     // mipInfo :
//     // x = quality settings minStreamingMipLevel
//     // y = original mip count for texture
//     // z = desired on screen mip level
//     // w = loaded mip level
//     float4 mipInfo;
//     #endif
// };

///////////////////////////////////////////////////////////////////////////////
//                      Constant Buffers                                     //
///////////////////////////////////////////////////////////////////////////////

half4 _GlossyEnvironmentColor;
half4 _SubtractiveShadowColor;

// half4 _GlossyEnvironmentCubeMap_HDR;
// TEXTURECUBE(_GlossyEnvironmentCubeMap);
// SAMPLER(sampler_GlossyEnvironmentCubeMap);

#define _InvCameraViewProj unity_MatrixInvVP
float4 _ScaledScreenParams;

// x = Mip Bias
// y = 2.0 ^ [Mip Bias]
float2 _GlobalMipBias;

// 1.0 if it's possible for AlphaToMask to be enabled for this draw and 0.0 otherwise
float _AlphaToMaskAvailable;

float4 _MainLightPosition;
// In Forward+, .a stores whether the main light is using subtractive mixed mode.
half4 _MainLightColor;
half4 _MainLightOcclusionProbes;
uint _MainLightLayerMask;

// x: SSAO Enabled/Disabled (Needed for situations when OFF keyword is stripped out but feature disabled in runtime)
// yz are currently unused
// w: directLightStrength
half4 _AmbientOcclusionParam;

half4 _AdditionalLightsCount;

uint _RenderingLayerMaxInt;
float _RenderingLayerRcpMaxInt;

// Screen coord override.
float4 _ScreenCoordScaleBias;
float4 _ScreenSizeOverride;

#define UNITY_MATRIX_M     unity_ObjectToWorld
#define UNITY_MATRIX_I_M   unity_WorldToObject
#define UNITY_MATRIX_V     unity_MatrixV
#define UNITY_MATRIX_I_V   unity_MatrixInvV
#define UNITY_MATRIX_P     OptimizeProjectionMatrix(glstate_matrix_projection)
#define UNITY_MATRIX_I_P   unity_MatrixInvP
#define UNITY_MATRIX_VP    unity_MatrixVP
#define UNITY_MATRIX_I_VP  unity_MatrixInvVP
#define UNITY_MATRIX_MV    mul(UNITY_MATRIX_V, UNITY_MATRIX_M)
#define UNITY_MATRIX_T_MV  transpose(UNITY_MATRIX_MV)
#define UNITY_MATRIX_IT_MV transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))
#define UNITY_MATRIX_MVP   mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)
#define UNITY_PREV_MATRIX_M   unity_MatrixPreviousM
#define UNITY_PREV_MATRIX_I_M unity_MatrixPreviousMI

// Note: #include order is important here.
// UnityInput.hlsl must be included before UnityInstancing.hlsl, so constant buffer
// declarations don't fail because of instancing macros.
// UniversalDOTSInstancing.hlsl must be included after UnityInstancing.hlsl
#include "Packages/com.lub.rp/ShaderLibrary/UnityInput.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#endif
