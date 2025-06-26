Shader "LuB RP/SimpleLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "LuBLit"
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "../ShaderLibrary/Input.hlsl"
            #include "Light.hlsl"

            struct VertInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct FragInput
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            float4 _MainTex_ST;

            FragInput vert(VertInput v)
            {
                FragInput f;
                f.vertex = TransformObjectToHClip(v.vertex);
                f.normal = TransformObjectToWorldNormal(v.normal);
                f.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return f;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            real4 frag(FragInput i) : SV_Target
            {
                Light dirLight = GetDirectionalLight();
                real4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                baseMap.rgb *= saturate(dot(normalize(i.normal), dirLight.direction));
                return baseMap;
            }
            ENDHLSL
        }
    }
}