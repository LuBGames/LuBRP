using System;
using UnityEditor;
using UnityEditor.ShaderGraph;

namespace LubRP.Editor.ShaderGraph.Targets
{
    class LuBLitSubTarget : LuBSubTarget
    {
        private static readonly GUID KSourceCodeGuid = new GUID("a1de9321d4ea491889eeb85e31114a74");

        public LuBLitSubTarget()
        {
            displayName = "Lit";
        }
        
        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(KSourceCodeGuid, AssetCollection.Flags.SourceDependency);
            base.Setup(ref context);
            
            context.AddSubShader(PostProcessSubShader(SubShaders.Lit(target, target.renderType, target.renderQueue)));
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(BlockFields.SurfaceDescription.Alpha, (target.surfaceType == SurfaceType.Transparent || target.alphaClip) || target.allowMaterialOverride);
            context.AddBlock(BlockFields.SurfaceDescription.AlphaClipThreshold, target.alphaClip || target.allowMaterialOverride);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, 
            Action onChange, Action<string> registerUndo)
        {
            var universalTarget = target;
            universalTarget.AddDefaultMaterialOverrideGUI(ref context, onChange, registerUndo);
            universalTarget.AddDefaultSurfacePropertiesGUI(ref context, onChange, registerUndo, showReceiveShadows: true);
        }
        
        #region SubShader
        static class SubShaders
        {
            public static SubShaderDescriptor Lit(LuBTarget target, string renderType, string renderQueue)
            {
                var result = new SubShaderDescriptor()
                {
                    customTags = LuBTarget.kLitMaterialTypeTag,
                    renderType = renderType,
                    renderQueue = renderQueue,
                    generatesPreview = true,
                    passes = new PassCollection()
                };

                result.passes.Add(LitPasses.Forward(target, LitKeywords.Forward));

                if (target.castShadows || target.allowMaterialOverride)
                    result.passes.Add(SubShaderUtils.PassVariant(CorePasses.ShadowCaster(target), CorePragmas.Base));
                
                return result;
            }
        }
        #endregion
        
        #region Pass
        static class LitPasses
        {
            public static PassDescriptor Forward(LuBTarget target, KeywordCollection keywords)
            {
                var result = new PassDescriptor
                {
                    // Definition
                    displayName = "LuB Forward",
                    referenceName = "SHADERPASS_LIT",
                    useInPreview = true,

                    // Template
                    passTemplatePath = LuBTarget.kUberTemplatePath,
                    sharedTemplateDirectories = LuBTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CoreBlockMasks.FragmentSurface,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = LitRequiredFields.Lit,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.UberSwitchedRenderState(target),
                    pragmas = CorePragmas.Forward,
                    defines = new DefineCollection {  },
                    keywords = new KeywordCollection { keywords },
                    includes = new IncludeCollection { LitIncludes.Lit },
                };

                CorePasses.AddShadowCastControlToPass(ref result, target);
                CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
                CorePasses.AddAlphaToMaskControlToPass(ref result, target);

                return result;
            }
        }
        #endregion
        
        #region RequiredFields
        static class LitRequiredFields
        {
            public static readonly FieldCollection Lit = new FieldCollection()
            {
                LuBStructs.LuBVaryings.positionWS,
                LuBStructs.LuBVaryings.normalWS
            };
        }
        #endregion
        
        #region Keywords
        static class LitKeywords
        {
            public static readonly KeywordCollection Forward = new KeywordCollection()
            {
                // This contain lightmaps because without a proper custom lighting solution in Shadergraph,
                // people start with the unlit then add lightmapping nodes to it.
                // If we removed lightmaps from the unlit target this would ruin a lot of peoples days.
                // CoreKeywordDescriptors.StaticLightmap,
                // CoreKeywordDescriptors.DirectionalLightmapCombined,
                // CoreKeywordDescriptors.SampleGI,
                // CoreKeywordDescriptors.DBuffer,
                // CoreKeywordDescriptors.DebugDisplay,
                // CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion,
            };
        }
        #endregion
        
        #region Includes
        static class LitIncludes
        {
            private const string kLitPass = "Packages/com.lub.rp/Editor/ShaderGraph/Includes/LitPass.hlsl";

            public static IncludeCollection Lit = new IncludeCollection
            {
                // Pre-graph
                // { CoreIncludes.WriteRenderLayersPregraph },
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kLitPass, IncludeLocation.Postgraph },
            };
        }
        #endregion
    }
}