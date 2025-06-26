using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    [CreateAssetMenu(menuName = "Rendering/LuB Render Pipeline")]
    public class LubRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private ShadowSettings shadowSettings = default;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new LuBRenderPipeline(shadowSettings);
        }
    }
}