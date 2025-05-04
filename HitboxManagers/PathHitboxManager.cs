using System.Collections.Generic;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace VisibleHitboxes.HitboxManagers
{
    public class PathHitboxManager(ModSettingBool setting) : HitboxManager(setting)
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

            var index = 0;
            foreach (var path in InGame.instance.GetMap().mapModel.paths)
            {
                var hName = ID_LINE_RENDERER + "_" + index;
                var gmLineRenderer = CreateLineRenderer(displayRoot.gameObject, hName, path.points, HitboxColors.Path);
                if (gmLineRenderer != null)
                {
                    activeIdentifiers.Add(hName);
                    Hitboxes.TryAdd(hName, gmLineRenderer);
                }
                index++;
            }

            CleanUpHitboxes(activeIdentifiers);
        }

        private GameObject? CreateLineRenderer(GameObject displayRoot, string name, Il2CppReferenceArray<PointInfo> path, Color color)
        {
            if (path == null) return null;
            if (Hitboxes.TryGetValue(name, out var gameObject))
            {
                if (gameObject != null) return gameObject;
            }

            name = HITBOX_OBJECT_NAME + name;

            var renderer = new GameObject(name)
            {
                transform = { parent = displayRoot.transform }
            };

            renderer.AddComponent<LineRenderer>();
            var lineRenderer = renderer.GetComponent<LineRenderer>();
            lineRenderer.material = VisibleHitboxes.GetMaterial("ShaderTransparent");
            var color1 = new Color(color.r, color.g, color.b, Settings.GetTransparency());
            lineRenderer.SetColors(color1, color1);
            lineRenderer.SetWidth(0.5f, 0.5f);

            var convertedArray = new Vector3[path.Length];
            lineRenderer.positionCount = path.Length;
            for (var i = 0; i < path.Length; i++)
            {
                var curPoint = path[i].point;
                var convertedCur = new Vector3(curPoint.x, 0f, -curPoint.y);
                convertedArray[i] = convertedCur;
            }
            lineRenderer.SetPositions(convertedArray);
            return renderer;
        }
    }
}
