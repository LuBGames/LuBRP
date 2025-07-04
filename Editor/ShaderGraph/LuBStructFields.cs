﻿using UnityEditor.ShaderGraph;

namespace LubRP.Editor.ShaderGraph
{
    static class LuBStructFields
    {
        public struct Varyings
        {
            public static string name = "Varyings";
            public static FieldDescriptor staticLightmapUV = new FieldDescriptor(Varyings.name, "staticLightmapUV", "", ShaderValueType.Float2,
                preprocessor: "defined(LIGHTMAP_ON)", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor dynamicLightmapUV = new FieldDescriptor(Varyings.name, "dynamicLightmapUV", "", ShaderValueType.Float2,
                preprocessor: "defined(DYNAMICLIGHTMAP_ON)", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor sh = new FieldDescriptor(Varyings.name, "sh", "", ShaderValueType.Float3,
                preprocessor: "!defined(LIGHTMAP_ON)", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor fogFactorAndVertexLight = new FieldDescriptor(Varyings.name, "fogFactorAndVertexLight", "VARYINGS_NEED_FOG_AND_VERTEX_LIGHT", ShaderValueType.Float4,
                subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor shadowCoord = new FieldDescriptor(Varyings.name, "shadowCoord", "VARYINGS_NEED_SHADOWCOORD", ShaderValueType.Float4,
                subscriptOptions: StructFieldOptions.Optional, preprocessor: "defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)");
            public static FieldDescriptor stereoTargetEyeIndexAsRTArrayIdx = new FieldDescriptor(Varyings.name, "stereoTargetEyeIndexAsRTArrayIdx", "", ShaderValueType.Uint,
                "SV_RenderTargetArrayIndex", "(defined(UNITY_STEREO_INSTANCING_ENABLED))", StructFieldOptions.Generated);
            public static FieldDescriptor stereoTargetEyeIndexAsBlendIdx0 = new FieldDescriptor(Varyings.name, "stereoTargetEyeIndexAsBlendIdx0", "", ShaderValueType.Uint,
                "BLENDINDICES0", "(defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))");
        }
    }
}