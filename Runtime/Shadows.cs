using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    public class Shadows
    {
        private const int MaxShadowedDirectionalLightCount = 1;

        private static readonly int DirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
            DirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
            DirShadowStrengthId = Shader.PropertyToID("_ShadowStrength"),
            DirShadowDistanceId = Shader.PropertyToID("_ShadowDistance"),
            DirShadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade"),
            DirShadowBiasId = Shader.PropertyToID("_ShadowBias");
        
        private const string BufferName = "Shadows";
        
        private readonly CommandBuffer _cmd = new ()
        {
            name = BufferName,
        };
        
        private struct ShadowedDirectionalLight
        {
            public int visibleLightIndex;
            public Light light;
        }

        private ShadowedDirectionalLight[] _shadowedDirectionalLights = 
            new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];
        
        private Matrix4x4 _dirShadowMatrices;
        
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
                light = light
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

            _cmd.SetGlobalMatrix(DirShadowMatricesId, ConvertToAtlasMatrix(_dirShadowMatrices));
            _cmd.SetGlobalFloat(DirShadowDistanceId, _shadowSettings.maxDistance);
            _cmd.SetGlobalVector(DirShadowDistanceFadeId, 
                new Vector4(1f / _shadowSettings.maxDistance, 1f / _shadowSettings.distanceFade));
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

            if (index == 0)
            {
                _dirShadowMatrices = projectionMatrix * viewMatrix;
                _cmd.SetGlobalFloat(DirShadowStrengthId, light.light.shadowStrength);
                _cmd.SetGlobalFloat(DirShadowBiasId, light.light.shadowNormalBias);
                _cmd.SetGlobalDepthBias(0f, light.light.shadowBias);
            }
            
            _cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteCommand();
            _context.DrawShadows(ref settings);
            _cmd.SetGlobalDepthBias(0f, 0f);
        }

        private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m)
        {
            if (SystemInfo.usesReversedZBuffer) {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }
            m.m00 = 0.5f * (m.m00 + m.m30);
            m.m01 = 0.5f * (m.m01 + m.m31);
            m.m02 = 0.5f * (m.m02 + m.m32);
            m.m03 = 0.5f * (m.m03 + m.m33);
            m.m10 = 0.5f * (m.m10 + m.m30);
            m.m11 = 0.5f * (m.m11 + m.m31);
            m.m12 = 0.5f * (m.m12 + m.m32);
            m.m13 = 0.5f * (m.m13 + m.m33);
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            return m;
        }

        private void ExecuteCommand()
        {
            _context.ExecuteCommandBuffer(_cmd);
            _cmd.Clear();
        }
    }
}