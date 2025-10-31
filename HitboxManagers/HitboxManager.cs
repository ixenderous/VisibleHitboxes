using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisibleHitboxes.HitboxManagers
{
    public abstract class HitboxManager
    {
        public static readonly float TRANSPARENCY = 0.5f;
        
        protected const string HITBOX_OBJECT_NAME = "Hitbox_";
        protected const float CIRCLE_SIZE_MULTIPLIER = 2f;
        protected const int ID_HELD_TOWER_HITBOX = -1;
        protected const int ID_LINE_RENDERER = -2;
        protected const int ID_MAP_AREA = -3;

        protected readonly Dictionary<string, GameObject> Hitboxes = new();
        protected List<string> previousIdentifiers = new();

        public abstract void Update(bool isEnabled);

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

            var circle = GetCircleObject();

            circle.name = HITBOX_OBJECT_NAME + name;
            circle.transform.parent = simDisplay;
            circle.transform.localPosition = offset;
            circle.transform.localScale = new Vector3(radius, radius, radius);

            var meshRenderer = circle.GetComponent<MeshRenderer>();
            meshRenderer.material.color = new Color(color.r, color.g, color.b, TRANSPARENCY);
            meshRenderer.sortingLayerName = "Bloons";
            meshRenderer.material.renderQueue = 4000;

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

        public static GameObject GetCircleObject()
        {
            var rendererGo = new GameObject();

            // Components for rendering a filled shape
            rendererGo.AddComponent<MeshFilter>();
            var meshRenderer = rendererGo.AddComponent<MeshRenderer>();
            meshRenderer.material = GetMaterial();

            // Generate the circle mesh data
            const int segments = 50;
            const float radius = 0.5f;

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            // Center vertex
            vertices.Add(new Vector3(0f, 0f, 0f));

            // Perimeter vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * (360f / segments) * Mathf.Deg2Rad;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                vertices.Add(new Vector3(x, 0f, z));
            }

            // Create triangles (Fan pattern)
            for (int i = 0; i < segments; i++)
            {
                // Triangle connects: Center (0), current point (i+1), next point (i+2)
                triangles.Add(0); // Center point
                triangles.Add(i + 1); // Current perimeter point
                triangles.Add(i + 2); // Next perimeter point (or wraps to the start)
            }

            // Apply mesh data
            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            rendererGo.GetComponent<MeshFilter>().mesh = mesh;

            return rendererGo;
        }

        public static GameObject GetSquareObject()
        {
            var rendererGo = new GameObject();

            // Components for rendering a filled shape
            rendererGo.AddComponent<MeshFilter>();
            var meshRenderer = rendererGo.AddComponent<MeshRenderer>();
            meshRenderer.material = GetMaterial();

            // Vertices (4 corners)
            var vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0f, -0.5f), // Bottom-Left (0)
                new Vector3(0.5f, 0f, -0.5f), // Bottom-Right (1)
                new Vector3(0.5f, 0f, 0.5f), // Top-Right (2)
                new Vector3(-0.5f, 0f, 0.5f) // Top-Left (3)
            };

            // Triangles (Two triangles to make a square/quad)
            var triangles = new int[]
            {
                0, 2, 1, // First triangle: 0 -> 2 -> 1
                0, 3, 2 // Second triangle: 0 -> 3 -> 2
            };

            // Apply mesh data
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            rendererGo.GetComponent<MeshFilter>().mesh = mesh;

            return rendererGo;
        }

        public static Material GetMaterial()
        {
            return new Material(Shader.Find("Hidden/Internal-Colored"));
        }
    }
}