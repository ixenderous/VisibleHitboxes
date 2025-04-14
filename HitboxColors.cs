using Il2CppAssets.Scripts.Models.Map;
using UnityEngine;

namespace VisibleHitboxes
{
    public static class HitboxColors
    {
        public static readonly Color Tower = new(1f, 1f, 0.85f);
        public static readonly Color Projectile = new(1f, 0f, 0f);
        public static readonly Color InvalidPosition = new(1f, 0f, 0f);
        public static readonly Color InvisibleProjectile = new(1f, 0.5f, 0.60f);
        public static readonly Color ModifierProjectile = new(1f, 0.20f, 0.60f);
        public static readonly Color Bloon = new(1f, 1f, 0f);
        public static readonly Color Path = new(0.9f, 0.95f, 0.85f);

        public static readonly Color AreaTrack = new(0.40f, 0.40f, 0.30f);
        public static readonly Color AreaWater = new(0f, 0f, 1f);
        public static readonly Color AreaLand = new(0f, 1f, 0f);
        public static readonly Color AreaUnplaceable = new(0.60f, 0f, 0f);
        public static readonly Color AreaIce = new(0.5f, 1f, 1f);
        public static readonly Color AreaRemovable = new(1f, 0.5f, 0f);
        public static readonly Color AreaWaterMonkey = new(0.2f, 0.4f, 1f);

        private static readonly Color[] AreaColors = {
            AreaTrack,
            AreaWater,
            AreaLand,
            AreaUnplaceable,
            AreaIce,
            AreaRemovable,
            AreaWaterMonkey
        };

        public static Color GetAreaColor(AreaType areaType)
        {
            return AreaColors[(int)areaType];
        }
    }
}
