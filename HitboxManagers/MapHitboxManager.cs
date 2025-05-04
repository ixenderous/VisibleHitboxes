using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace VisibleHitboxes.HitboxManagers
{
    public class MapHitboxManager(ModSettingBool setting) : HitboxManager(setting)
    {
        public override void Update()
        {
            if (!IsEnabled())
            {
                ClearAllHitboxes();
                return;
            }

            var activeIdentifiers = new List<string>();
            var displayRoot = Game.instance.GetDisplayFactory().DisplayRoot;

            var areas = InGame.instance.GetMap().mapModel.areas;
            for (var i = 0; i < areas.Count; i++)
            {
                var areaModel = areas[i];
                if ((int)areaModel.type < 0 || (int)areaModel.type >= Enum.GetValues(typeof(AreaType)).Length)
                    continue;

                var color = HitboxColors.GetAreaColor(areaModel.type);
                var areaHeight = areaModel.height;

                var pointArray = areaModel.polygon.points.ToList();
                var hName = ID_MAP_AREA + "_" + i;
                var points = pointArray.Select(point => new Vector2(point.x, point.y)).ToList();

                var hitbox = CreateAreaOutlineRenderer(displayRoot.gameObject, hName, points, color, areaHeight);

                if (hitbox != null)
                {
                    activeIdentifiers.Add(hName);
                    Hitboxes.TryAdd(hName, hitbox);
                }

                if (areaModel.holes != null)
                {
                    for (var j = 0; j < areaModel.holes.Length; j++)
                    {
                        var hole = areaModel.holes[j];
                        var holePointArray = hole.points.ToList();
                        var holeHName = ID_MAP_AREA + "_" + i + "_" + j;
                        var holePoints = holePointArray.Select(point => new Vector2(point.x, point.y)).ToList();

                        var holeHitbox = CreateAreaOutlineRenderer(displayRoot.gameObject, holeHName, holePoints, color, areaHeight);
                        if (holeHitbox != null)
                        {
                            activeIdentifiers.Add(holeHName);
                            Hitboxes.TryAdd(holeHName, holeHitbox);
                        }
                    }
                }
            }

            CleanUpHitboxes(activeIdentifiers);
        }

        private GameObject CreateAreaOutlineRenderer(GameObject displayRoot, string name, List<Vector2> points, Color color, float height)
        {
            if (Hitboxes.TryGetValue(name, out var gameObject))
            {
                if (gameObject != null) return gameObject;
            }

            name = HITBOX_OBJECT_NAME + name;

            var renderer = new GameObject(name)
            {
                transform =
                {
                    parent = displayRoot.transform
                }
            };

            renderer.AddComponent<LineRenderer>();
            var lineRenderer = renderer.GetComponent<LineRenderer>();
            lineRenderer.material = VisibleHitboxes.GetMaterial("ShaderTransparent");
            var color1 = new Color(color.r, color.g, color.b, Settings.GetTransparency());
            lineRenderer.startColor = new Color(color.r, color.g, color.b, Settings.GetTransparency());
            lineRenderer.endColor = new Color(color.r, color.g, color.b, Settings.GetTransparency());
            lineRenderer.SetColors(color1, color1);
            lineRenderer.SetWidth(0.5f, 0.5f);
            lineRenderer.loop = true;

            var convertedArray = new Vector3[points.Count];
            lineRenderer.positionCount = points.Count;

            for (var i = 0; i < points.Count; i++)
            {
                convertedArray[i] = new Vector3(points[i].x, 0f, -points[i].y);
            }
            lineRenderer.SetPositions(convertedArray);
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 0;
            lineRenderer.material.renderQueue = 1999;

            return renderer;
        }
    }
}

