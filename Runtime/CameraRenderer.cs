using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    public partial class CameraRenderer
    {
        private ScriptableRenderContext _context;
        
        private Camera _camera;

        private const string BufferName = "Render Camera";
        
        private CullingResults _cullingResults;
        
        private readonly Lighting _lighting = new ();
        
        private readonly CommandBuffer _cmd = new ()
        {
            name = BufferName,
        };
        
        private static readonly 
            ShaderTagId UnlitShaderTagId = new ("SRPDefaultUnlit");
            ShaderTagId LitShaderTagId = new ("LuBLit");

        public void Render(ScriptableRenderContext context, Camera camera, LuBRenderPipeline rp)
        {
            _context = context;
            _camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull(rp.ShadowSettings.maxDistance))
            {
                return;
            }
            
            SetupCameraProps(camera);
            _cmd.BeginSample(SampleName);
            ExecuteBuffer();
            _lighting.Setup(context, _cullingResults, rp);
            _cmd.EndSample(SampleName);
            Setup();
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            _lighting.Cleanup();
            Submit();
        }

        private void Setup()
        {
            LuBRPCore.SetShaderTimeValues(_cmd, Time.time, Time.deltaTime, Time.smoothDeltaTime);
            _context.SetupCameraProperties(_camera);
            CameraClearFlags clearFlags = _camera.clearFlags;
            _cmd.ClearRenderTarget(clearFlags <= CameraClearFlags.Depth, 
                clearFlags == CameraClearFlags.Color, clearFlags == CameraClearFlags.Color
                ? _camera.backgroundColor.linear : Color.clear);
            _cmd.BeginSample(SampleName);
            ExecuteBuffer();
        }
        
        private void SetupCameraProps(Camera camera)
        {
            float cameraWidth = (float)camera.pixelWidth;
            float cameraHeight = (float)camera.pixelHeight;
            _cmd.SetGlobalVector(ShaderKeywordStrings.scaledScreenParams, new Vector4(cameraWidth, cameraHeight, 1.0f + 1.0f / cameraWidth, 1.0f + 1.0f / cameraHeight));
        }

        private void DrawVisibleGeometry()
        {
            var sortingSettings = new SortingSettings(_camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings);
            drawingSettings.SetShaderPassName(1, LitShaderTagId);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
            
            _context.DrawSkybox(_camera);
            
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private bool Cull(float maxShadowDistance)
        {
            if (_camera.TryGetCullingParameters(out var parameters))
            {
                parameters.shadowDistance = Mathf.Min(maxShadowDistance, _camera.farClipPlane);
                _cullingResults = _context.Cull(ref parameters);
                return true;
            }
            return false;
        }

        private void Submit()
        {
            _cmd.EndSample(SampleName);
            ExecuteBuffer();
            _context.Submit();
        }

        private void ExecuteBuffer()
        {
            _context.ExecuteCommandBuffer(_cmd);
            _cmd.Clear();
        }
    }
}