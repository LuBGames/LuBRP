using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    partial class CameraRenderer
    {
        #if UNITY_EDITOR
        public string SampleName { get; private set; }

        
        private static readonly ShaderTagId[] LegacyShaderTagsIds =
        {
            new ("Always"),
            new ("ForwardBase"),
            new ("PrepassBase"),
            new ("Vertex"),
            new ("VertexLMRGBM"),
            new ("VertexLM")
        };

        private static Material _errorMaterial;
        
        private partial void DrawUnsupportedShaders()
        {
            if (_errorMaterial == null)
                _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            
            var drawingSettings = new DrawingSettings(LegacyShaderTagsIds[0], new SortingSettings(_camera))
            {
                overrideMaterial = _errorMaterial
            };
            var filteringSettings = FilteringSettings.defaultValue;
            for (int i = 0; i < LegacyShaderTagsIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, LegacyShaderTagsIds[i]);
            }
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private partial void DrawGizmos()
        {
            if (!Handles.ShouldRenderGizmos())
                return;
            
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }

        private partial void PrepareForSceneWindow()
        {
            if (_camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
            }
        }

        private partial void PrepareBuffer()
        {
            _cmd.name = SampleName = _camera.name;
        }
        #else

        private const string SampleName = BufferName;

#endif

        private partial void DrawUnsupportedShaders();

        private partial void DrawGizmos();
        private partial void PrepareForSceneWindow();
        private partial void PrepareBuffer();
    }
}