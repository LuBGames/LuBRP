using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    public class Shadows
    {
        private const int MaxShadowedDirectionalLightCount = 1;
        
        private static readonly int DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        
        private const string BufferName = "Shadows";
        
        private readonly CommandBuffer _cmd = new ()
        {
            name = BufferName,
        };
        
        private struct ShadowedDirectionalLight
        {
            public int visibleLightIndex;
        }

        private ShadowedDirectionalLight[] _shadowedDirectionalLights = 
            new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];
        
        private ScriptableRenderContext _context;
        private CullingResults _cullingResults;
        private ShadowSettings _shadowSettings;
        
        private int _shadowedDirectionalLightCount;

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            _shadowedDirectionalLightCount = 0;
            
            _context = context;
            _cullingResults = cullingResults;
            _shadowSettings = shadowSettings;
        }

        public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
        {
            if (light.shadows == LightShadows.None || light.shadowStrength == 0f)
                return;
            if (_shadowedDirectionalLightCount >= MaxShadowedDirectionalLightCount)
                return;
            if (!_cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds))
                return;
            
            _shadowedDirectionalLights[_shadowedDirectionalLightCount++] = new ShadowedDirectionalLight
            {
                visibleLightIndex = visibleLightIndex,
            };
        }

        public void Render()
        {
            if (_shadowedDirectionalLightCount == 0)
            {
                _cmd.GetTemporaryRT(DirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, 
                    RenderTextureFormat.Shadowmap);
                return;
            }
            
            RenderDirectionalShadows();
        }

        public void Cleanup()
        {
            _cmd.ReleaseTemporaryRT(DirShadowAtlasId);
            ExecuteCommand();
        }

        private void RenderDirectionalShadows()
        {
            int atlasSize = (int)_shadowSettings.directional.atlasSize;
            _cmd.GetTemporaryRT(DirShadowAtlasId, atlasSize, atlasSize, 32, 
                FilterMode.Bilinear, RenderTextureFormat.Shadowmap); //TODO: Maybe change depth
            _cmd.SetRenderTarget(DirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            _cmd.ClearRenderTarget(true, false, Color.clear);
            _cmd.BeginSample(BufferName);
            ExecuteCommand();

            for (int i = 0; i < _shadowedDirectionalLightCount; i++)
            {
                RenderDirectionalShadows(i, atlasSize);
            }
            
            _cmd.EndSample(BufferName);
            ExecuteCommand();
        }

        private void RenderDirectionalShadows(int index, int tileSize)
        {
            ShadowedDirectionalLight light = _shadowedDirectionalLights[index];
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 
                0, 1, Vector3.zero, tileSize, 0f, out var viewMatrix,
                out var projectionMatrix, out var splitData);
            var settings = new ShadowDrawingSettings(_cullingResults, light.visibleLightIndex, 
                BatchCullingProjectionType.Orthographic)
            {
                splitData = splitData,
            };
            _cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteCommand();
            _context.DrawShadows(ref settings);
        }

        private void ExecuteCommand()
        {
            _context.ExecuteCommandBuffer(_cmd);
            _cmd.Clear();
        }
    }
}