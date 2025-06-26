using UnityEditor;
using UnityEditor.ShaderGraph;

namespace LubRP.Editor.ShaderGraph.Targets
{
    abstract class LuBSubTarget : SubTarget<LuBTarget>
    {
        //TODO: Change to file GUID
        static readonly GUID kSourceCodeGuid = new GUID("c10b4a9390694b62b2d70593e708bb90");  // UniversalSubTarget.cs
        
        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(kSourceCodeGuid, AssetCollection.Flags.SourceDependency);
        }
        
        protected SubShaderDescriptor PostProcessSubShader(SubShaderDescriptor subShaderDescriptor)
        {
#if HAS_VFX_GRAPH
            if (TargetsVFX())
                return VFXSubTarget.PostProcessSubShader(subShaderDescriptor, m_ContextVFX, m_ContextDataVFX);
#endif
            return subShaderDescriptor;
        }
        
        public override void GetFields(ref TargetFieldContext context)
        {
#if HAS_VFX_GRAPH
            if (TargetsVFX())
                VFXSubTarget.GetFields(ref context, m_ContextVFX);
#endif
        }
    }
    
    internal static class SubShaderUtils
    {
        // Overloads to do inline PassDescriptor modifications
        // NOTE: param order should match PassDescriptor field order for consistency
        #region PassVariant
        internal static PassDescriptor PassVariant(in PassDescriptor source, PragmaCollection pragmas)
        {
            var result = source;
            result.pragmas = pragmas;
            return result;
        }

        #endregion
    }
}