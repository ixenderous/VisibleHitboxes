using BTD_Mod_Helper.Extensions;
using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using UnityEngine;

namespace VisibleHitboxes.HitboxManagers
{
    public abstract class HitboxManager
    {
        public static readonly float TRANSPARENCY = 0.5f;

        protected static AssetBundle bundle;
        protected static GameObject circle;
        protected static GameObject square;
        protected static Material material;
        
        protected const string HITBOX_OBJECT_NAME = "Hitbox_";
        protected const float CIRCLE_SIZE_MULTIPLIER = 2f;
        protected const int ID_HELD_TOWER_HITBOX = -1;
        protected const int ID_LINE_RENDERER = -2;
        protected const int ID_MAP_AREA = -3;

        protected readonly Dictionary<string, GameObject> Hitboxes = new();
        protected List<string> previousIdentifiers = new();

        public abstract void Update(bool isEnabled);

        public static void Initialize()
        {
            bundle = ModContent.GetBundle(ModHelper.GetMod("VisibleHitboxes"), "debugmat");
            circle = bundle.LoadAssetAsync("circle").GetResult().Cast<GameObject>();
            square = bundle.LoadAssetAsync("square").GetResult().Cast<GameObject>();
            material = bundle.LoadAssetAsync("ShaderTransparent").GetResult().Cast<Material>();
        }

        public virtual void OnMatchStart()
        {
        }

        public virtual void OnMatchEnd()
        {
            ClearAllHitboxes();
            previousIdentifiers.Clear();
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
                    Object.Destroy(hitbox);
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
                Object.Destroy(hitbox);
            }
        }

        public GameObject? CreateCircularHitbox(Transform simDisplay, Color color, float radius, Vector3 offset,
            string name)
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

            var circle = GetCircle();

            circle.name = HITBOX_OBJECT_NAME + name;
            circle.transform.parent = simDisplay;
            circle.transform.localPosition = offset;
            circle.transform.localScale = new Vector3(radius, radius, radius);

            var spriteRenderer = circle.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(color.r, color.g, color.b, TRANSPARENCY);
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

                spriteRenderer.color = new Color(color.r, color.g, color.b, TRANSPARENCY);
            }
            else if (hitbox.HasComponent<LineRenderer>())
            {
                var lineRenderer = hitbox.GetComponent<LineRenderer>();
                if (color == default)
                {
                    color = lineRenderer.startColor;
                }

                color = new Color(color.r, color.g, color.b, TRANSPARENCY);
                lineRenderer.SetColors(color, color);
            }
            else if (hitbox.HasComponent<MeshRenderer>())
            {
                var meshRenderer = hitbox.GetComponent<MeshRenderer>();
                if (color == default)
                {
                    color = meshRenderer.material.color;
                }

                meshRenderer.material.color = new Color(color.r, color.g, color.b, TRANSPARENCY);
            }
        }

        public static GameObject GetCircle()
        {
            return circle.Duplicate();
        }

        public static GameObject GetSquare()
        {
            return square.Duplicate();
        }

        public static Material GetMaterial()
        {
            return material.Duplicate();
        }
    }
}