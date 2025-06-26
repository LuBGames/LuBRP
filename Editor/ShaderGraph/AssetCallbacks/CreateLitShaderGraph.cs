using System;
using LubRP.Editor.ShaderGraph.Targets;
using UnityEditor;
using UnityEditor.ShaderGraph;

namespace LubRP.Editor.ShaderGraph.AssetCallbacks
{
    static class CreateLitShaderGraph
    {
        [MenuItem("Assets/Create/Shader Graph/Lub RP/Lit Shader Graph")]
        public static void CreateLitGraph()
        {
            var target = Activator.CreateInstance<LuBTarget>();
            target.TrySetActiveSubTarget(typeof(LuBLitSubTarget));

            var descriptors = new[]
            {
                BlockFields.VertexDescription.Position,
                BlockFields.VertexDescription.Normal,
                BlockFields.VertexDescription.Tangent,
                BlockFields.SurfaceDescription.BaseColor,
            };
            GraphUtil.CreateNewGraphWithOutputs(new Target[] { target }, descriptors);
        }
    }
}