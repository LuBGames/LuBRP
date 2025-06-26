using UnityEngine;

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
}