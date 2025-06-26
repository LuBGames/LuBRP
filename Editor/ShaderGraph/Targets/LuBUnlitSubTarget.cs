using System;
using LubRP.Editor.ShaderGUI;
using UnityEditor;
using UnityEditor.ShaderGraph;

namespace LubRP.Editor.ShaderGraph.Targets
{
    class LuBUnlitSubTarget : LuBSubTarget
    {
        static readonly GUID kSourceCodeGuid = new GUID("90800f4ffc104fc7a067b21b18c5d45a"); // UniversalUnlitSubTarget.cs
        
        public LuBUnlitSubTarget()
        {
            displayName = "Unlit";
        }

        public override bool IsActive() => true;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(kSourceCodeGuid, AssetCollection.Flags.SourceDependency);
            base.Setup(ref context);

            var rpType = typeof(LubRenderPipelineAsset);
            if (!context.HasCustomEditorForRenderPipeline(rpType))
            {
                var gui = typeof(ShaderGraphUnlitGUI);
#if HAS_VFX_GRAPH
                if (TargetsVFX())
                    gui = typeof(VFXShaderGraphUnlitGUI);
#endif
                context.AddCustomEditorForRenderPipeline(gui.FullName, rpType);
            }
            // Process SubShaders
            context.AddSubShader(PostProcessSubShader(SubShaders.Unlit(target, target.renderType, target.renderQueue)));
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(BlockFields.SurfaceDescription.Alpha, (target.surfaceType == SurfaceType.Transparent || target.alphaClip) || target.allowMaterialOverride);
            context.AddBlock(BlockFields.SurfaceDescription.AlphaClipThreshold, target.alphaClip || target.allowMaterialOverride);
        }

        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<string> registerUndo)
        {
            var universalTarget = target;
            universalTarget.AddDefaultMaterialOverrideGUI(ref context, onChange, registerUndo);
            universalTarget.AddDefaultSurfacePropertiesGUI(ref context, onChange, registerUndo, showReceiveShadows: false);
        }
    }
    
    #region Keywords
    static class UnlitKeywords
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
    
    #region SubShader
    static class SubShaders
    {
        public static SubShaderDescriptor Unlit(LuBTarget target, string renderType, string renderQueue)
        {
            var result = new SubShaderDescriptor()
            {
                customTags = LuBTarget.kUnlitMaterialTypeTag,
                renderType = renderType,
                renderQueue = renderQueue,
                generatesPreview = true,
                passes = new PassCollection()
            };

            result.passes.Add(UnlitPasses.Forward(target, UnlitKeywords.Forward));

            if (target.castShadows || target.allowMaterialOverride)
                result.passes.Add(SubShaderUtils.PassVariant(CorePasses.ShadowCaster(target), CorePragmas.Base));
            
            return result;
        }
    }
    #endregion
    
    #region Pass
    static class UnlitPasses
    {
        public static PassDescriptor Forward(LuBTarget target, KeywordCollection keywords)
        {
            var result = new PassDescriptor
            {
                // Definition
                displayName = "LuB Forward",
                referenceName = "SHADERPASS_UNLIT",
                useInPreview = true,

                // Template
                passTemplatePath = LuBTarget.kUberTemplatePath,
                sharedTemplateDirectories = LuBTarget.kSharedTemplateDirectories,

                // Port Mask
                validVertexBlocks = CoreBlockMasks.Vertex,
                validPixelBlocks = CoreBlockMasks.FragmentColorAlpha,

                // Fields
                structs = CoreStructCollections.Default,
                fieldDependencies = CoreFieldDependencies.Default,

                // Conditional State
                renderStates = CoreRenderStates.UberSwitchedRenderState(target),
                pragmas = CorePragmas.Forward,
                defines = new DefineCollection {  },
                keywords = new KeywordCollection { keywords },
                includes = new IncludeCollection { UnlitIncludes.Unlit },

                // Custom Interpolator Support
                //customInterpolators = CoreCustomInterpDescriptors.Common
            };

            CorePasses.AddTargetSurfaceControlsToPass(ref result, target);
            CorePasses.AddAlphaToMaskControlToPass(ref result, target);

            return result;
        }
    }
    #endregion
    
    #region Includes
    static class UnlitIncludes
    {
        private const string kUnlitPass = "Packages/com.lub.rp/Editor/ShaderGraph/Includes/UnlitPass.hlsl";

        public static IncludeCollection Unlit = new IncludeCollection
        {
            // Pre-graph
            // { CoreIncludes.WriteRenderLayersPregraph },
            { CoreIncludes.CorePregraph },
            { CoreIncludes.ShaderGraphPregraph },

            // Post-graph
            { CoreIncludes.CorePostgraph },
            { kUnlitPass, IncludeLocation.Postgraph },
        };
    }
    #endregion
}