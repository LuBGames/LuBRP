using System;
using LubRP.Editor.ShaderGraph.Targets;
using UnityEditor;
using UnityEditor.ShaderGraph;

namespace LubRP.Editor.ShaderGraph.AssetCallbacks
{
    static class CreateUnlitShaderGraph
    {
        [MenuItem("Assets/Create/Shader Graph/Lub RP/Unlit Shader Graph")]
        public static void CreateUnlitGraph()
        {
            var target = (LuBTarget)Activator.CreateInstance(typeof(LuBTarget));
            target.TrySetActiveSubTarget(typeof(LuBUnlitSubTarget));

            var blockDescriptors = new[]
            {
                BlockFields.VertexDescription.Position,
                BlockFields.VertexDescription.Normal,
                BlockFields.VertexDescription.Tangent,
                BlockFields.SurfaceDescription.BaseColor,
            };
            GraphUtil.CreateNewGraphWithOutputs(new [] { target }, blockDescriptors);
        }
    }
}