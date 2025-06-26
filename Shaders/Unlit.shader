Shader "LuB RP/Unlit"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
    }
    SubShader
    {
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            
            HLSLPROGRAM

            #pragma vertex vertex
            #pragma fragment fragment

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "../ShaderLibrary/Input.hlsl"

            float4 vertex(float3 position : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(position);
            }

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END

            real4 fragment() : SV_TARGET
            {
                return _Color;
            }
            
            ENDHLSL
        }
    }
}