using UnityEngine;
using UnityEngine.Rendering;

namespace LubRP
{
    public static class ShaderKeywordStrings
    {
        public const string _SURFACE_TYPE_TRANSPARENT = "_SURFACE_TYPE_TRANSPARENT";
        public const string _ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        public const string _ALPHAMODULATE_ON = "_ALPHAMODULATE_ON";
        public const string _ALPHATEST_ON = "_ALPHATEST_ON";
        
        public static readonly int scaledScreenParams = Shader.PropertyToID("_ScaledScreenParams");
    }

    public static class LuBRPCore
    {
        internal static class ShaderPropertyId
        {
            public static readonly int time = Shader.PropertyToID("_Time");
            public static readonly int sinTime = Shader.PropertyToID("_SinTime");
            public static readonly int cosTime = Shader.PropertyToID("_CosTime");
            public static readonly int deltaTime = Shader.PropertyToID("unity_DeltaTime");
            public static readonly int timeParameters = Shader.PropertyToID("_TimeParameters");
        }

        /// <summary>
        /// Set shader time variables as described in https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
        /// </summary>
        /// <param name="cmd">CommandBuffer to submit data to GPU.</param>
        /// <param name="time">Time.</param>
        /// <param name="deltaTime">Delta time.</param>
        /// <param name="smoothDeltaTime">Smooth delta time.</param>
        public static void SetShaderTimeValues(CommandBuffer cmd, float time, float deltaTime, float smoothDeltaTime)
        {
            float timeEights = time / 8f;
            float timeFourth = time / 4f;
            float timeHalf = time / 2f;

            // Time values
            Vector4 timeVector = time * new Vector4(1f / 20f, 1f, 2f, 3f);
            Vector4 sinTimeVector = new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time));
            Vector4 cosTimeVector = new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time));
            Vector4 deltaTimeVector = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
            Vector4 timeParametersVector = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f);

            cmd.SetGlobalVector(ShaderPropertyId.time, timeVector);
            cmd.SetGlobalVector(ShaderPropertyId.sinTime, sinTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.cosTime, cosTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.deltaTime, deltaTimeVector);
            cmd.SetGlobalVector(ShaderPropertyId.timeParameters, timeParametersVector);
        }
    }
}