using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisibleHitboxes.HitboxManagers
{
    public class ProjectileHitboxManager(ModSettingBool setting) : HitboxManager(setting)
    {
        private List<HandledProjectile> handledProjectiles = [];

        internal class HandledProjectile
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

        public override void Update()
        {
            if (!IsEnabled())
            {
                ClearAllHitboxes();
                return;
            }

            var activeIdentifiers = new List<string>();
            var displayRoot = Game.instance.GetDisplayFactory().DisplayRoot;

            foreach (var handledProjectile in handledProjectiles)
            {
                var projectile = handledProjectile.projectile;
                if (projectile.isDestroyed) continue;

                var projectileId = projectile!.Id.Id;
                var radius = projectile.Radius;

                Color color = radius <= 0
                    ? HitboxColors.ModifierProjectile
                    : handledProjectile.isInvisible
                        ? HitboxColors.InvisibleProjectile
                        : HitboxColors.Projectile;

                if (radius <= 0) radius = 1f;

                if (handledProjectile.isInvisible)
                {
                    var projectilePos = projectile.Display.node.position.data;
                    var displayPos = new Vector3(projectilePos.x, 0f, -projectilePos.y);
                    var invhitbox = CreateCircularHitbox(displayRoot, color, radius, displayPos, projectileId.ToString());
                    if (invhitbox != null)
                    {
                        activeIdentifiers.Add(projectileId.ToString());
                        Hitboxes.TryAdd(projectileId.ToString(), invhitbox);
                        UpdateHitbox(invhitbox, displayPos, color);
                    }
                }
                else
                {
                    var simDisplay = projectile.GetUnityDisplayNode()?.gameObject.transform;
                    if (simDisplay == null || !simDisplay.gameObject.active) continue;
                    var hitbox = CreateCircularHitbox(simDisplay, color, radius, Vector3.zero, projectileId.ToString());
                    if (hitbox != null)
                    {
                        activeIdentifiers.Add(projectile.ToString());
                        Hitboxes.TryAdd(projectileId.ToString(), hitbox);
                    }
                    handledProjectiles = handledProjectiles
                        .Where(hProjectile => !hProjectile.projectile!.IsDestroyed).ToList();
                }
            }
            CleanUpHitboxes(activeIdentifiers);
        }

        public void OnProjectileCreated(Projectile projectile, Model modelToUse)
        {
            handledProjectiles.Add(new HandledProjectile(projectile, modelToUse.Cast<ProjectileModel>()));
        }

        public override void ClearAllHitboxes()
        {
            base.ClearAllHitboxes();
        }

        public override void OnMatchEnd()
        {
            base.OnMatchEnd();

            handledProjectiles.Clear();
        }
    }
}