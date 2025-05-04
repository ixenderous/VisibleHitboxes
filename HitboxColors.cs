using Il2CppAssets.Scripts.Models.Map;
using UnityEngine;

namespace VisibleHitboxes
{
    public static class HitboxColors
    {
        // Tower colors
        public static readonly Color Tower = new(0.25f, 1f, 0f);
        public static readonly Color InvalidPosition = new(1f, 0f, 0f);

        // Projectile colors
        public static readonly Color Projectile = new(0f, 1f, 1f);
        public static readonly Color InvisibleProjectile = new(0f, 1f, 1f);
        public static readonly Color ModifierProjectile = new(0f, 1f, 1f);

        // Bloon colors
        public static readonly Color Bloon = new(1f, 1f, 0f);

        // Path colors
        public static readonly Color Path = new(0.9f, 0.95f, 0.85f);

        // Area type colors
        public static readonly Color AreaTrack = new(1f, 0f, 0f);
        public static readonly Color AreaWater = new(0f, 0.8f, 1f);
        public static readonly Color AreaLand = new(0.25f, 1f, 0f);
        public static readonly Color AreaUnplaceable = new(1, 0f, 0f);
        public static readonly Color AreaIce = new(0, 1f, 0.75f);
        public static readonly Color AreaRemovable = new(1f, 0.75f, 0f);
        public static readonly Color AreaWaterMermonkey = new(0f, 0.2f, 1f);

        // Area colors lookup array
        private static readonly Color[] AreaColors = {
            AreaTrack,
            AreaWater,
            AreaLand,
            AreaUnplaceable,
            AreaIce,
            AreaRemovable,
            AreaWaterMermonkey
        };

        public static Color GetAreaColor(AreaType areaType)
        {
            return AreaColors[(int)areaType];
        }
    }
}