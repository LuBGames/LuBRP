using UnityEditor.ShaderGraph;

namespace LubRP.Editor.ShaderGraph
{
    static class LuBStructs
    {
        public static StructDescriptor Attributes = new StructDescriptor()
        {
            name = "Attributes",
            packFields = false,
            fields = new FieldDescriptor[]
            {
                StructFields.Attributes.positionOS,
                StructFields.Attributes.normalOS,
                StructFields.Attributes.tangentOS,
                StructFields.Attributes.uv0,
                StructFields.Attributes.uv1,
                StructFields.Attributes.uv2,
                StructFields.Attributes.uv3,
                StructFields.Attributes.color,
                // StructFields.Attributes.instanceID,
                // StructFields.Attributes.weights,
                // StructFields.Attributes.indices,
                // StructFields.Attributes.vertexID,
            }
        };
        
        public static StructDescriptor Varyings = new StructDescriptor()
        {
            name = "Varyings",
            packFields = false,
            fields = new FieldDescriptor[]
            {
                StructFields.Varyings.positionCS,
                LuBVaryings.positionWS,
                LuBVaryings.normalWS,
                // StructFields.Varyings.tangentWS,
                LuBVaryings.texCoord0,
                LuBVaryings.texCoord1,
                LuBVaryings.texCoord2,
                LuBVaryings.texCoord3,
                LuBVaryings.color,
                // StructFields.Varyings.screenPosition,
                // UniversalStructFields.Varyings.staticLightmapUV,
                // UniversalStructFields.Varyings.dynamicLightmapUV,
                // UniversalStructFields.Varyings.sh,
                // UniversalStructFields.Varyings.fogFactorAndVertexLight,
                // UniversalStructFields.Varyings.shadowCoord,
                // StructFields.Varyings.instanceID,
                // UniversalStructFields.Varyings.stereoTargetEyeIndexAsBlendIdx0,
                // UniversalStructFields.Varyings.stereoTargetEyeIndexAsRTArrayIdx,
                // StructFields.Varyings.cullFace,
            }
        };
        
        internal static class LuBVaryings
        {
            private const string Name = "Varyings";
            public static FieldDescriptor positionWS = new FieldDescriptor(Name, "positionWS", "VARYINGS_NEED_POSITION_WS", ShaderValueType.Float3,
                "WP_POS", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor normalWS = new FieldDescriptor(Name, "normalWS", "VARYINGS_NEED_NORMAL_WS", ShaderValueType.Float3,
                "NORMAL", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor texCoord0 = new FieldDescriptor(Name, "texCoord0", "VARYINGS_NEED_TEXCOORD0", ShaderValueType.Float4,
                "TEXCOORD0", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor texCoord1 = new FieldDescriptor(Name, "texCoord1", "VARYINGS_NEED_TEXCOORD1", ShaderValueType.Float4,
                "TEXCOORD1", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor texCoord2 = new FieldDescriptor(Name, "texCoord2", "VARYINGS_NEED_TEXCOORD2", ShaderValueType.Float4,
                "TEXCOORD2", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor texCoord3 = new FieldDescriptor(Name, "texCoord3", "VARYINGS_NEED_TEXCOORD3", ShaderValueType.Float4,
                "TEXCOORD3", subscriptOptions: StructFieldOptions.Optional);
            public static FieldDescriptor color = new FieldDescriptor(Varyings.name, "color", "VARYINGS_NEED_COLOR", ShaderValueType.Float4,
                "COLOR", subscriptOptions: StructFieldOptions.Optional);
        }
        
        public static DependencyCollection DepVaryings = new DependencyCollection
        {
            new FieldDependency(LuBVaryings.positionWS,                                                   StructFields.Attributes.positionOS),
            // new FieldDependency(StructFields.Varyings.positionPredisplacementWS,                                    StructFields.Attributes.positionOS),
            new FieldDependency(LuBVaryings.normalWS,                                                         StructFields.Attributes.normalOS),
            // new FieldDependency(StructFields.Varyings.tangentWS,                                                    StructFields.Attributes.tangentOS),
            new FieldDependency(LuBVaryings.texCoord0,                                                        StructFields.Attributes.uv0),
            new FieldDependency(LuBVaryings.texCoord1,                                                    StructFields.Attributes.uv1),
            new FieldDependency(LuBVaryings.texCoord2,                                                    StructFields.Attributes.uv2),
            new FieldDependency(LuBVaryings.texCoord3,                                                    StructFields.Attributes.uv3),
            // new FieldDependency(LuBVaryings.color,                                                        StructFields.Attributes.color),
            // new FieldDependency(StructFields.Varyings.instanceID,                                                   StructFields.Attributes.instanceID),
            // new FieldDependency(StructFields.Varyings.vertexID,                                                     StructFields.Attributes.vertexID),*/
        };
        
        public static DependencyCollection DepVertexDescription = new DependencyCollection
        {
            new FieldDependency(StructFields.VertexDescriptionInputs.ObjectSpaceNormal,                             StructFields.Attributes.normalOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.WorldSpaceNormal,                              StructFields.Attributes.normalOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.ViewSpaceNormal,                               StructFields.VertexDescriptionInputs.WorldSpaceNormal),

            new FieldDependency(StructFields.VertexDescriptionInputs.ObjectSpaceTangent,                            StructFields.Attributes.tangentOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.WorldSpaceTangent,                             StructFields.Attributes.tangentOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.ViewSpaceTangent,                              StructFields.VertexDescriptionInputs.WorldSpaceTangent),

            new FieldDependency(StructFields.VertexDescriptionInputs.ObjectSpaceBiTangent,                          StructFields.Attributes.normalOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.ObjectSpaceBiTangent,                          StructFields.Attributes.tangentOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.WorldSpaceBiTangent,                           StructFields.VertexDescriptionInputs.ObjectSpaceBiTangent),
            new FieldDependency(StructFields.VertexDescriptionInputs.ViewSpaceBiTangent,                            StructFields.VertexDescriptionInputs.WorldSpaceBiTangent),

            new FieldDependency(StructFields.VertexDescriptionInputs.ObjectSpacePosition,                           StructFields.Attributes.positionOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.WorldSpacePosition,                            StructFields.Attributes.positionOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.AbsoluteWorldSpacePosition,                    StructFields.Attributes.positionOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.ViewSpacePosition,                             StructFields.VertexDescriptionInputs.WorldSpacePosition),

            new FieldDependency(StructFields.VertexDescriptionInputs.WorldSpacePositionPredisplacement,             StructFields.Attributes.positionOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.AbsoluteWorldSpacePositionPredisplacement,     StructFields.Attributes.positionOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.ObjectSpacePositionPredisplacement,            StructFields.Attributes.positionOS),
            new FieldDependency(StructFields.VertexDescriptionInputs.ViewSpacePositionPredisplacement,              StructFields.VertexDescriptionInputs.WorldSpacePosition),

            new FieldDependency(StructFields.VertexDescriptionInputs.WorldSpaceViewDirection,                       StructFields.VertexDescriptionInputs.WorldSpacePosition),
            new FieldDependency(StructFields.VertexDescriptionInputs.ObjectSpaceViewDirection,                      StructFields.VertexDescriptionInputs.WorldSpaceViewDirection),
            new FieldDependency(StructFields.VertexDescriptionInputs.ViewSpaceViewDirection,                        StructFields.VertexDescriptionInputs.WorldSpaceViewDirection),
            new FieldDependency(StructFields.VertexDescriptionInputs.TangentSpaceViewDirection,                     StructFields.VertexDescriptionInputs.WorldSpaceViewDirection),
            new FieldDependency(StructFields.VertexDescriptionInputs.TangentSpaceViewDirection,                     StructFields.VertexDescriptionInputs.WorldSpaceTangent),
            new FieldDependency(StructFields.VertexDescriptionInputs.TangentSpaceViewDirection,                     StructFields.VertexDescriptionInputs.WorldSpaceBiTangent),
            new FieldDependency(StructFields.VertexDescriptionInputs.TangentSpaceViewDirection,                     StructFields.VertexDescriptionInputs.WorldSpaceNormal),

            new FieldDependency(StructFields.VertexDescriptionInputs.ScreenPosition,                                StructFields.VertexDescriptionInputs.WorldSpacePosition),
            new FieldDependency(StructFields.VertexDescriptionInputs.NDCPosition,                                   StructFields.VertexDescriptionInputs.ScreenPosition),
            new FieldDependency(StructFields.VertexDescriptionInputs.PixelPosition,                                 StructFields.VertexDescriptionInputs.NDCPosition),

            new FieldDependency(StructFields.VertexDescriptionInputs.uv0,                                           StructFields.Attributes.uv0),
            new FieldDependency(StructFields.VertexDescriptionInputs.uv1,                                           StructFields.Attributes.uv1),
            new FieldDependency(StructFields.VertexDescriptionInputs.uv2,                                           StructFields.Attributes.uv2),
            new FieldDependency(StructFields.VertexDescriptionInputs.uv3,                                           StructFields.Attributes.uv3),
            new FieldDependency(StructFields.VertexDescriptionInputs.VertexColor,                                   StructFields.Attributes.color),

            new FieldDependency(StructFields.VertexDescriptionInputs.BoneWeights,                                   StructFields.Attributes.weights),
            new FieldDependency(StructFields.VertexDescriptionInputs.BoneIndices,                                   StructFields.Attributes.indices),
            new FieldDependency(StructFields.VertexDescriptionInputs.VertexID,                                      StructFields.Attributes.vertexID),
        };

        public static DependencyCollection SurfaceDescription = new DependencyCollection
        {
            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpaceNormal,                             LuBVaryings.normalWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ObjectSpaceNormal,                            StructFields.SurfaceDescriptionInputs.WorldSpaceNormal),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ViewSpaceNormal,                              StructFields.SurfaceDescriptionInputs.WorldSpaceNormal),

            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpaceTangent,                            StructFields.Varyings.tangentWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpaceTangent,                            StructFields.SurfaceDescriptionInputs.WorldSpaceNormal),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ObjectSpaceTangent,                           StructFields.SurfaceDescriptionInputs.WorldSpaceTangent),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ViewSpaceTangent,                             StructFields.SurfaceDescriptionInputs.WorldSpaceTangent),

            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpaceBiTangent,                          StructFields.SurfaceDescriptionInputs.WorldSpaceNormal),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpaceBiTangent,                          StructFields.SurfaceDescriptionInputs.WorldSpaceTangent),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ObjectSpaceBiTangent,                         StructFields.SurfaceDescriptionInputs.WorldSpaceBiTangent),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ViewSpaceBiTangent,                           StructFields.SurfaceDescriptionInputs.WorldSpaceBiTangent),

            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpacePosition,                           LuBVaryings.positionWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.AbsoluteWorldSpacePosition,                   LuBVaryings.positionWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ObjectSpacePosition,                          LuBVaryings.positionWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ViewSpacePosition,                            LuBVaryings.positionWS),

            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpacePositionPredisplacement,            StructFields.Varyings.positionPredisplacementWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.AbsoluteWorldSpacePositionPredisplacement,    StructFields.Varyings.positionPredisplacementWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ObjectSpacePositionPredisplacement,           StructFields.Varyings.positionPredisplacementWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ViewSpacePositionPredisplacement,             StructFields.Varyings.positionPredisplacementWS),

            new FieldDependency(StructFields.SurfaceDescriptionInputs.WorldSpaceViewDirection,                      LuBVaryings.positionWS),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ObjectSpaceViewDirection,                     StructFields.SurfaceDescriptionInputs.WorldSpaceViewDirection),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.ViewSpaceViewDirection,                       StructFields.SurfaceDescriptionInputs.WorldSpaceViewDirection),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.TangentSpaceViewDirection,                    StructFields.SurfaceDescriptionInputs.WorldSpaceViewDirection),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.TangentSpaceViewDirection,                    StructFields.SurfaceDescriptionInputs.WorldSpaceTangent),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.TangentSpaceViewDirection,                    StructFields.SurfaceDescriptionInputs.WorldSpaceBiTangent),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.TangentSpaceViewDirection,                    StructFields.SurfaceDescriptionInputs.WorldSpaceNormal),

            new FieldDependency(StructFields.SurfaceDescriptionInputs.ScreenPosition,                               StructFields.SurfaceDescriptionInputs.WorldSpacePosition),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.NDCPosition,                                  StructFields.SurfaceDescriptionInputs.PixelPosition),

            new FieldDependency(StructFields.SurfaceDescriptionInputs.uv0,                                          LuBVaryings.texCoord0),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.uv1,                                          LuBVaryings.texCoord1),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.uv2,                                          LuBVaryings.texCoord2),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.uv3,                                          LuBVaryings.texCoord3),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.VertexColor,                                  LuBVaryings.color),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.FaceSign,                                     StructFields.Varyings.cullFace),
            new FieldDependency(StructFields.SurfaceDescriptionInputs.VertexID,                                     StructFields.Varyings.vertexID),
        };
        
        public static DependencyCollection Default = new DependencyCollection
        {
            { DepVaryings },
            { DepVertexDescription },
            { SurfaceDescription },
        };
    }
}