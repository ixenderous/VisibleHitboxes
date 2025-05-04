using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisibleHitboxes.HitboxManagers
{
    public abstract class HitboxManager(ModSettingBool setting)
    {
        protected const string HITBOX_OBJECT_NAME = "Hitbox_";
        protected const float CIRCLE_SIZE_MULTIPLIER = 2f;
        protected const int ID_HELD_TOWER_HITBOX = -1;
        protected const int ID_LINE_RENDERER = -2;
        protected const int ID_MAP_AREA = -3;

        protected readonly Dictionary<string, GameObject> Hitboxes = [];
        protected List<string> previousIdentifiers = [];
        protected readonly ModSettingBool setting = setting;

        public abstract void Update();

        public bool IsEnabled()
        {
            return (bool)setting.GetValue();
        }

        public void OnMatchStart() { }

        public virtual void OnMatchEnd() {
            ClearAllHitboxes();
            previousIdentifiers.Clear();
        }

        public virtual void LogDebugInfo()
        {
            MelonLogger.Msg($"  {this.GetType().Name}:");
            MelonLogger.Msg($"\tManaging {Hitboxes.Count} hitboxes.");
            MelonLogger.Msg($"\tIsEnabled: {IsEnabled()}");
        }

        protected void CleanUpHitboxes(List<string> activeIdentifiers)
        {
            var inactiveIdentifiers = previousIdentifiers.Except(activeIdentifiers).ToList();
            RemoveUnusedHitboxes(inactiveIdentifiers);
            previousIdentifiers = activeIdentifiers.Duplicate();
        }

        public virtual void ClearAllHitboxes()
        {
            foreach (var hitbox in Hitboxes.Values)
            {
                if (hitbox != null)
                {
                    UnityEngine.Object.Destroy(hitbox);
                }
            }
            Hitboxes.Clear();
            previousIdentifiers.Clear();
        }

        private void RemoveUnusedHitboxes(List<string> inactiveIdentifiers)
        {
            foreach (var identifier in inactiveIdentifiers)
                DestroyHitbox(identifier);
        }

        protected void DestroyHitbox(string identifier)
        {
            if (Hitboxes.TryGetValue(identifier, out var hitbox))
            {
                Hitboxes.Remove(identifier);
                UnityEngine.Object.Destroy(hitbox);
            }
        }

        public GameObject? CreateCircularHitbox(Transform simDisplay, Color color, float radius, Vector3 offset, string name)
        {
            if (Hitboxes.TryGetValue(name, out var gameObject))
            {
                if (gameObject == null)
                    return null;

                if (!gameObject.transform.parent.gameObject.active)
                    return null;
                
                return gameObject;
            }

            if (radius <= 0) radius = 1f;

            radius *= CIRCLE_SIZE_MULTIPLIER;

            var circle = VisibleHitboxes.GetGameObject("Circle");

            circle.name = HITBOX_OBJECT_NAME + name;
            circle.transform.parent = simDisplay;
            circle.transform.localPosition = offset;
            circle.transform.localScale = new Vector3(radius, radius, radius);

            var spriteRenderer = circle.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(color.r, color.g, color.b, Settings.GetTransparency());
            spriteRenderer.sortingLayerName = "Bloons";

            return circle;
        }

        public void UpdateHitboxes()
        {
            foreach (var hitbox in Hitboxes.Values.Where(hitbox => hitbox != null))
            {
                UpdateHitbox(hitbox, hitbox.transform.position);
            }
        }

        public static void UpdateHitbox(GameObject hitbox, Vector3 newPosition, Color color = default)
        {
            hitbox.transform.position = newPosition;
            if (hitbox.HasComponent<SpriteRenderer>())
            {
                var spriteRenderer = hitbox.GetComponent<SpriteRenderer>();
                if (color == default)
                {
                    color = spriteRenderer.color;
                }
                spriteRenderer.color = new Color(color.r, color.g, color.b, Settings.GetTransparency());
            }
            else if (hitbox.HasComponent<LineRenderer>())
            {
                var lineRenderer = hitbox.GetComponent<LineRenderer>();
                if (color == default)
                {
                    color = lineRenderer.startColor;
                }
                color = new Color(color.r, color.g, color.b, Settings.GetTransparency());
                lineRenderer.SetColors(color, color);
            }
            else if (hitbox.HasComponent<MeshRenderer>())
            {
                var meshRenderer = hitbox.GetComponent<MeshRenderer>();
                if (color == default)
                {
                    color = meshRenderer.material.color;
                }
                meshRenderer.material.color = new Color(color.r, color.g, color.b, Settings.GetTransparency());
            }
        }
    }
}
