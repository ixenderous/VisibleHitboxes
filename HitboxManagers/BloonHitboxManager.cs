using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisibleHitboxes.HitboxManagers
{
    public class BloonHitboxManager(ModSettingBool setting) : HitboxManager(setting)
    {
        public override void Update()
        {
            if (!IsEnabled()) {
                ClearAllHitboxes();
                return;
            }
                
            var activeIdentifiers = new List<string>();
            var displayRoot = Game.instance.GetDisplayFactory().DisplayRoot;

            foreach (var bloon in InGame.Bridge.GetAllBloons().ToList())
            {
                var bloonId = bloon.id.Id;

                if (bloon.GetUnityDisplayNode() == null) continue;

                var simDisplay = bloon.GetUnityDisplayNode().gameObject.transform;
                if (!simDisplay.gameObject.active) continue;

                var collisionData = bloon.GetSimBloon().AdditionalCollisions();

                if (collisionData == null)
                {
                    var radius = bloon.GetSimBloon().Radius;
                    var hitbox = CreateCircularHitbox(simDisplay, HitboxColors.Bloon, radius, Vector3.zero, bloonId.ToString());
                    if (hitbox != null)
                    {
                        activeIdentifiers.Add(bloonId.ToString());
                        Hitboxes.TryAdd(bloonId.ToString(), hitbox);
                        hitbox.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
                    } 
                }
                else
                {
                    int count = 0;
                    foreach (var collision in collisionData)
                    {
                        var offset = new Vector3(collision.offset.x, 0f, collision.offset.y);
                        var radius = collision.radius;
                        var hName = bloonId + "_" + count;
                        var hitbox = CreateCircularHitbox(simDisplay, HitboxColors.Bloon, radius, offset, hName);
                        if (hitbox != null)
                        {
                            activeIdentifiers.Add(hName);
                            Hitboxes.TryAdd(hName, hitbox);
                        }
                        count++;
                    }
                }
            }

            CleanUpHitboxes(activeIdentifiers);
        }
    }
}