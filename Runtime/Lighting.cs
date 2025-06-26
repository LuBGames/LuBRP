using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    public class Lighting
    {
        private const string BufferName = "Lighting";
        
        private static readonly int
            DirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
            DirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
        
        private readonly CommandBuffer _cmd = new ()
        {
            name = BufferName,
        };
        
        private readonly Shadows _shadows = new();

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, LuBRenderPipeline rp)
        {
            _cmd.BeginSample(BufferName);
            _shadows.Setup(context, cullingResults, rp.ShadowSettings);
            foreach (var light in cullingResults.visibleLights)
            {
                SetupDirectionalLight(0, light);
                break;
            }
            _shadows.Render();
            _cmd.EndSample(BufferName);
            context.ExecuteCommandBuffer(_cmd);
            _cmd.Clear();
        }

        public void Cleanup()
        {
            _shadows.Cleanup();
        }

        private void SetupDirectionalLight(int index, VisibleLight visibleLight)
        {
            var light = visibleLight.light;
            _cmd.SetGlobalVector(DirLightColorId, light.color.linear * light.intensity);
            _cmd.SetGlobalVector(DirLightDirectionId, -light.transform.forward);
            _shadows.ReserveDirectionalShadows(light, index);
        }
    }
}