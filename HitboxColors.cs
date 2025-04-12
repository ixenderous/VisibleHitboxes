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
        public static readonly Color TrackArea = new(0.40f, 0.40f, 0.30f);
        public static readonly Color UnplacableArea = new(0.60f, 0f, 0f);
    }
}