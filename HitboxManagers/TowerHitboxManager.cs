using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System.Collections.Generic;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace VisibleHitboxes.HitboxManagers
{
    public class TowerHitboxManager(ModSettingBool setting) : HitboxManager(setting)
    {
        private float scaleModifier = 1;

        public override void OnMatchStart()
        {
            base.OnMatchStart();

            scaleModifier = 1f / InGame.instance.GetGameModel().globalTowerScale;
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

            foreach (var tower in InGame.Bridge.GetAllTowers().ToList())
            { 
                var simDisplay = tower.GetSimTower().GetUnityDisplayNode()?.gameObject.transform;
                if (simDisplay == null || !simDisplay.gameObject.active) continue;

                var towerId = tower.Id.Id;
                var hitbox = CreateTowerHitbox(simDisplay, HitboxColors.Tower, tower.Def.footprint, towerId.ToString());

                if (hitbox == null) continue;

                activeIdentifiers.Add(towerId.ToString());
                Hitboxes.TryAdd(towerId.ToString(), hitbox);
            }

            var inputManager = InGame.instance.InputManager;
            var placementDisplayList = inputManager.placementGraphics;
            var placementModel = inputManager.placementModel;
            var placementTowerId = inputManager.placementEntityId;
            var towerPos = inputManager.entityPositionWorld;

            if(placementModel != null && placementDisplayList.Count > 0)
            {
                var placementDisplay= placementDisplayList.First();
                var simDisplay = placementDisplay.gameObject.transform;
                var footprint = placementModel.footprint;
                var inputId = InGame.Bridge.GetInputId();
                var canPlace = InGame.Bridge.CanPlaceTowerAt(towerPos, placementModel, inputId, placementTowerId);
                var color = canPlace ? HitboxColors.Tower : HitboxColors.InvalidPosition;

                var hitbox = CreateTowerHitbox(simDisplay, color, footprint, ID_HELD_TOWER_HITBOX.ToString());
                if (hitbox != null)
                {
                    activeIdentifiers.Add(ID_HELD_TOWER_HITBOX.ToString());
                    Hitboxes.TryAdd(ID_HELD_TOWER_HITBOX.ToString(), hitbox);
                    UpdateHitbox(hitbox, simDisplay.position, color);
                }
            }

            CleanUpHitboxes(activeIdentifiers);
        }

        private GameObject? CreateTowerHitbox(Transform simDisplay, Color color, Object footprint, string name)
        {
            if (Hitboxes.TryGetValue(name, out var gameObject))
            {
                if (gameObject == null)
                    return null;

                if (!gameObject.transform.parent.gameObject.active)
                    return null;
                
                return gameObject;
            }

            name = HITBOX_OBJECT_NAME + name;
            // var scaleModifier = 1f / simDisplay.localScale.x;

            if (footprint.IsType<RectangleFootprintModel>())
            {
                var footprintModel = footprint.Cast<RectangleFootprintModel>();
                var square = VisibleHitboxes.GetGameObject("Square");

                square.name = name;
                square.transform.parent = simDisplay;
                square.transform.localPosition = Vector3.zero;
                square.transform.localScale = new Vector3(footprintModel.xWidth, footprintModel.yWidth, footprintModel.yWidth) * scaleModifier;

                var spriteRenderer = square.GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(color.r, color.g, color.b, Settings.GetTransparency());
                spriteRenderer.sortingLayerName = "Bloons";
                return square;
            }
            else
            {
                var footprintModel = footprint.Cast<CircleFootprintModel>();
                var radius = footprintModel.radius * scaleModifier;

                var circle = CreateCircularHitbox(simDisplay, color, radius, Vector3.zero, name);
                
                if (circle == null) return null;

                var spriteRenderer = circle.GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(color.r, color.g, color.b, Settings.GetTransparency());
                spriteRenderer.sortingLayerName = "Bloons";
                return circle;
            }
        }

        public void OnTowerUpgraded(Tower tower)
        {
            DestroyHitbox(tower.Id.Id.ToString());
        }
    }
}
