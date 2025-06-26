using UnityEngine;

namespace LubRP
{
    [System.Serializable]
    public class ShadowSettings
    {
        [Min(0f)] public float maxDistance = 100f;
        
        public Directional directional = new Directional()
        {
            atlasSize = MapSize._1024
        };
        
        [System.Serializable]
        public struct Directional
        {
            public MapSize atlasSize;
        }
        
        public enum MapSize
        {
            _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048
        }
    }
}