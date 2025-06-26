using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    public class LuBRenderPipeline : RenderPipeline
    {
        private readonly CameraRenderer _renderer = new();
        
        public readonly ShadowSettings ShadowSettings;

        public LuBRenderPipeline(ShadowSettings shadowSettings)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            ShadowSettings = shadowSettings;
        }
        
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                _renderer.Render(context, cameras[i], this);
            }
        }
    }
}