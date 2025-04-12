using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using BTD_Mod_Helper.Extensions;

namespace VisibleHitboxes
{
    public class HandledProjectile
    {
        public Projectile projectile { get; }
        public bool isInvisible { get; }

        public HandledProjectile(Projectile projectile, ProjectileModel projectileModel)
        {
            this.projectile = projectile;
            this.isInvisible = IsProjectileInvisible(projectileModel);
        }

        private static bool IsProjectileInvisible(ProjectileModel model)
        {
            return string.IsNullOrEmpty(model.display.guidRef) ||
                   !model.HasBehavior<SetSpriteFromPierceModel>();
        }
    }
}
