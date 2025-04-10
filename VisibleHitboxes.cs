using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using MelonLoader;
using UnityEngine;
using VisibleHitboxes;
using Object = Il2CppSystem.Object;

[assembly: MelonInfo(typeof(global::VisibleHitboxes.VisibleHitboxes), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace VisibleHitboxes;

public class VisibleHitboxes : BloonsTD6Mod
{
    private bool _isInGame;
    
    private static MelonLogger.Instance _mllog = null!;

    private const float CircleSizeMultiplier = 2f;
    public const float DefaultTransparency = 0.5f;
    private const string HitboxObjectName = "Hitbox_";
    
    private static readonly Color TowerColor = new(1f, 1f, 0.85f);
    private static readonly Color ProjectileColor = new(1f, 0f, 0f);
    private static readonly Color InvalidPositionColor = new(1f, 0f, 0f);
    private static readonly Color InvisibleProjectileColor = new(1f, 0.5f, 0.60f);
    private static readonly Color ModifierProjectileColor = new(1f, 0.20f, 0.60f);
    private static readonly Color BloonColor = new(1f, 1f, 0f);
    private static readonly Color PathColor = new(0.9f, 0.95f, 0.85f);
    private static readonly Color TrackAreaColor = new(0.40f, 0.40f, 0.30f);
    private static readonly Color UnplacableAreaColor = new(0.60f, 0f, 0f);

    private const int HeldTowerHitboxId = -1;
    private const int LineRendererId = -2;
    private const int MapAreaId = -3;

    private static List<string> _prevIdentifiers = new();

    private static readonly Dictionary<string, GameObject> HitboxDictionary = new();

    public static readonly ModSettingCategory Hotkeys = new("Hotkeys")
    {
        collapsed = false
    };

    public static readonly ModSettingHotkey ToggleAll = new(KeyCode.A, HotkeyModifier.Shift)
    {
        category = Hotkeys,
        icon = VanillaSprites.EvenFasterProductionUpgradeIcon
    };
    
    public static readonly ModSettingHotkey ToggleTowers = new(KeyCode.T, HotkeyModifier.Shift)
    {
        category = Hotkeys,
        icon = VanillaSprites.SuperMonkeyFanClubUpgradeIcon
    };
    
    public static readonly ModSettingHotkey ToggleProjectiles = new(KeyCode.P, HotkeyModifier.Shift)
    {
        category = Hotkeys,
        icon = VanillaSprites.AirburstDartsUpgradeIcon
    };
    
    public static readonly ModSettingHotkey ToggleBloons = new(KeyCode.B, HotkeyModifier.Shift)
    {
        category = Hotkeys,
        icon = VanillaSprites.FullMetalJacketUpgradeIcon
    };
    
    public static readonly ModSettingHotkey ToggleMapCollision = new(KeyCode.M, HotkeyModifier.Shift)
    {
        category = Hotkeys,
        icon = VanillaSprites.MetropolisUpgradeIcon
    };
    
    public static readonly ModSettingHotkey ToggleTransparency = new(KeyCode.D, HotkeyModifier.Shift)
    {
        category = Hotkeys,
        icon = VanillaSprites.ShimmerUpgradeIcon
    };

    public override void OnMatchStart()
    {
        base.OnMatchStart();
        _isInGame = true;
        SaveSystem.LoadSaveFile();
    }

    public override void OnMatchEnd()
    {
        base.OnMatchEnd();
        _isInGame = false;
        var inactiveIdentifiers = _prevIdentifiers;
        RemoveUnusedHitboxes(inactiveIdentifiers); // Cleanup
    }

    public override void OnInitialize()
    {
        _mllog = LoggerInstance;
    }

    private static List<HandledProjectile> _handledProjectiles = new();

    public override void OnProjectileCreated(Projectile projectile, Entity entity, Model modelToUse)
    {
        var projectileModel = modelToUse.Cast<ProjectileModel>();
        var isInvisible = !(projectileModel.display.guidRef != "" || Enumerable.Any(projectileModel.behaviors, behavior => behavior.TypeName() == "SetSpriteFromPierceModel"));

        _handledProjectiles.Add(new HandledProjectile
        {
            IsInvisible = isInvisible,
            Projectile = projectile,
        });
    }
    
    public class HandledProjectile
    { 
        public bool IsInvisible { get; init; }
        public Projectile? Projectile { get; init; }
    }

    public enum MessageType
    {
        Msg,
        Warn,
        Error
    }
    public static void Log(object thingtolog,MessageType type= MessageType.Msg)
    {
        switch (type) {
            case MessageType.Msg:
                _mllog.Msg(thingtolog);
                break;
            case MessageType.Warn:
                _mllog.Warning(thingtolog);
                break;
            case MessageType.Error:
                _mllog.Error(thingtolog);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
    {
        base.OnTowerUpgraded(tower, upgradeName, newBaseTowerModel);
        HitboxDictionary.Remove(tower.Id.Id.ToString());
    }
    
    public override void OnUpdate()
    {
        if (!_isInGame) return;

        var activeIdentifiers = new List<string>();
        var displayRoot = Game.instance.GetDisplayFactory().DisplayRoot;
        
        if (ToggleAll.JustPressed())
        {
            if (SaveSystem.IsAllEnabled)
            {
                SaveSystem.AreBloonsEnabled = false;
                SaveSystem.AreProjectilesEnabled = false;
                SaveSystem.AreTowersEnabled = false;
                SaveSystem.IsMapEnabled = false;
            }
            else
            {
                SaveSystem.AreBloonsEnabled = true;
                SaveSystem.AreProjectilesEnabled = true;
                SaveSystem.AreTowersEnabled = true;
                SaveSystem.IsMapEnabled = true;
            }
            SaveSystem.IsAllEnabled = SaveSystem.IsEverythingEnabled();
            SaveSystem.UpdateSaveFile();
        }

        if (ToggleTowers.JustPressed())
        {
            SaveSystem.AreTowersEnabled = !SaveSystem.AreTowersEnabled;
            SaveSystem.IsAllEnabled = SaveSystem.IsEverythingEnabled();
            SaveSystem.UpdateSaveFile();
        }

        if (ToggleProjectiles.JustPressed())
        {
            SaveSystem.AreProjectilesEnabled = !SaveSystem.AreProjectilesEnabled;
            SaveSystem.IsAllEnabled = SaveSystem.IsEverythingEnabled();
            SaveSystem.UpdateSaveFile();
        }

        if (ToggleBloons.JustPressed())
        {
            SaveSystem.AreBloonsEnabled = !SaveSystem.AreBloonsEnabled;
            SaveSystem.IsAllEnabled = SaveSystem.IsEverythingEnabled();
            SaveSystem.UpdateSaveFile();
        }

        if (ToggleMapCollision.JustPressed())
        {
            SaveSystem.IsMapEnabled = !SaveSystem.IsMapEnabled;
            SaveSystem.IsAllEnabled = SaveSystem.IsEverythingEnabled();
            SaveSystem.UpdateSaveFile();
        }

        if (ToggleTransparency.JustPressed())
        {
            SaveSystem.AreHitboxesTransparent = !SaveSystem.AreHitboxesTransparent;
            SaveSystem.Transparency = SaveSystem.AreHitboxesTransparent ? DefaultTransparency : 1f;
            SaveSystem.IsAllEnabled = SaveSystem.IsEverythingEnabled();
            SaveSystem.UpdateSaveFile();
            UpdateAllHitboxes();
        }

        if (SaveSystem.AreTowersEnabled)
        {
            foreach (var tower in InGame.Bridge.GetAllTowers().ToList())
            {
                if (tower.GetSimTower().GetUnityDisplayNode() == null) continue;
                var simDisplay = tower.GetSimTower().GetUnityDisplayNode().gameObject.transform;
                if (!simDisplay.gameObject.active) continue;
                var footprint = tower.Def.footprint;
                var towerId = tower.Id.Id;
                var hitbox = CreateTowerHitbox(simDisplay, TowerColor, footprint, towerId.ToString());
                if (hitbox == null) continue;
                activeIdentifiers.Add(tower.Id.Id.ToString());
                HitboxDictionary.TryAdd(towerId.ToString(), hitbox);
            }

            // Held tower hitbox
            // TODO this doesn't detect map obstacles, only paths
            var inputManager = InGame.instance.InputManager;
            var placementDisplayList = inputManager.placementGraphics;
            var placementModel = inputManager.placementModel;
            var placementTowerId = inputManager.placementEntityId;
            var towerPos = inputManager.entityPositionWorld;
            if (placementModel != null && placementDisplayList.Count > 0)
            {
                var placementDisplay = placementDisplayList.First();
                var simDisplay = placementDisplay.gameObject.transform;
                var footprint = placementModel.footprint;
                var inputId = InGame.Bridge.GetInputId(); // Still have no idea what this is
                var canPlace = InGame.Bridge.CanPlaceTowerAt(towerPos, placementModel, inputId, placementTowerId);
                var color = canPlace ? TowerColor : InvalidPositionColor;
                var hitbox = CreateTowerHitbox(simDisplay, color, footprint, HeldTowerHitboxId.ToString());
                if (hitbox != null)
                {
                    activeIdentifiers.Add(HeldTowerHitboxId.ToString());
                    HitboxDictionary.TryAdd(HeldTowerHitboxId.ToString(), hitbox);
                    UpdateHitbox(hitbox, simDisplay.position, color);
                }
            }
        }

        if (SaveSystem.AreProjectilesEnabled)
        {
            foreach (var handledProjectile in _handledProjectiles)
            {
                var projectile = handledProjectile.Projectile;
                var projectileId = projectile!.Id.Id;
                var radius = projectile.Radius;

                if (projectile.isDestroyed) continue;

                if (handledProjectile.IsInvisible)
                {
                    var projectilePos = projectile.Display.node.position.data;
                    var displayPos = new Vector3(projectilePos.x, 0f, -projectilePos.y);
                    var invhitbox = CreateCircularHitbox(displayRoot, InvisibleProjectileColor, radius, displayPos, projectileId.ToString());
                    if (invhitbox != null)
                    {
                        activeIdentifiers.Add(projectileId.ToString());
                        HitboxDictionary.TryAdd(projectileId.ToString(), invhitbox);
                        UpdateHitbox(invhitbox, displayPos, InvisibleProjectileColor);
                    }
                    continue;
                }
                
                if (projectile.GetUnityDisplayNode() == null) continue;
                
                var simDisplay = projectile.GetUnityDisplayNode().gameObject.transform;
                if (!simDisplay.gameObject.active) continue;
                var hitbox = CreateCircularHitbox(simDisplay, ProjectileColor, radius, Vector3.zero, projectileId.ToString());
                if (hitbox != null)
                {
                    activeIdentifiers.Add(projectileId.ToString());
                    HitboxDictionary.TryAdd(projectileId.ToString(), hitbox);
                }
                
                _handledProjectiles = _handledProjectiles
                    .Where(hProjectile => !hProjectile.Projectile!.IsDestroyed).ToList();
            }
        }

        if (SaveSystem.AreBloonsEnabled)
        {
            foreach (var bloon in InGame.Bridge.GetAllBloons().ToList())
            {
                var bloonId = bloon.id.Id;
                if (bloon.GetUnityDisplayNode() == null) continue;
                var simDisplay = bloon.GetUnityDisplayNode().gameObject.transform;
                if (!simDisplay.gameObject.active) continue;
                var collisionData = bloon.GetSimBloon().AdditionalCollisions();
                if (collisionData != null) // Bloons with multiple collisions. Usually reserved for MOAB class
                {
                    var count = 0;
                    foreach (var collision in collisionData)
                    {
                        var offset = new Vector3(collision.offset.x, 0f, collision.offset.y);
                        var radius = collision.radius;
                        var hName = bloonId + "_" + count;
                        var hitbox = CreateCircularHitbox(simDisplay, BloonColor, radius, offset, hName);
                        if (hitbox != null)
                        {
                            activeIdentifiers.Add(hName);
                            HitboxDictionary.TryAdd(hName, hitbox);
                            UpdateHitbox(hitbox, hitbox.transform.position, BloonColor);
                        }
                        count++;
                    }
                }
                else // Single collision bloons
                {
                    var radius = bloon.GetSimBloon().Radius;
                    var hitbox = CreateCircularHitbox(simDisplay, BloonColor, radius, Vector3.zero, bloonId.ToString());
                    if (hitbox != null)
                    {
                        activeIdentifiers.Add(bloonId.ToString());
                        HitboxDictionary.TryAdd(bloonId.ToString(), hitbox);
                        hitbox.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
                    }
                }
            }
        }

        if (SaveSystem.IsMapEnabled)
        {
            var index = 0;
            foreach (var path in InGame.instance.GetMap().mapModel.paths)
            {
                var hName = LineRendererId + "_" + index;
                var gmLineRenderer = CreateLineRenderer(displayRoot.gameObject, hName, path.points, PathColor);
                if (gmLineRenderer != null)
                {
                    activeIdentifiers.Add(hName);
                    HitboxDictionary.TryAdd(hName, gmLineRenderer);
                }
                index++;
            }

            var areas = InGame.instance.GetMap().mapModel.areas;
            for (var i = 0; i < areas.Count; i++)
            {
                var areaModel = areas[i];
                if (areaModel.type is not (AreaType.track or AreaType.unplaceable)) continue;
                var color = areaModel.type == AreaType.track ? TrackAreaColor : UnplacableAreaColor;
                var pointArray = areaModel.polygon.points.ToList();
                var hName = MapAreaId + "_" + i;
                var points = pointArray.Select(point => new Vector2(point.x, point.y)).ToList();
                var hitbox = Create2DMesh(displayRoot.gameObject, hName, points, color);
                
                if (hitbox != null)
                {
                    activeIdentifiers.Add(hName);
                    HitboxDictionary.TryAdd(hName, hitbox);
                }
            }
        }
        
        // Get list difference
        var difList = _prevIdentifiers.Except(activeIdentifiers).ToList();
        RemoveUnusedHitboxes(difList);
        _prevIdentifiers = activeIdentifiers.Duplicate();
    }

    private static void UpdateAllHitboxes()
    {
        foreach (var hitbox in HitboxDictionary.Values.Where(hitbox => hitbox != null))
        {
            UpdateHitbox(hitbox, hitbox.transform.position);
        }
    }

    private static GameObject CreateLineRenderer(GameObject displayRoot, string name, Il2CppReferenceArray<PointInfo> path, Color color)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (HitboxDictionary.TryGetValue(name, out var gameObject))
        {
            if (gameObject != null) return gameObject;
        }
        
        name = HitboxObjectName + name;

        var renderer = new GameObject(name)
        {
            transform =
            {
                parent = displayRoot.transform
            }
        };
        
        renderer.AddComponent<LineRenderer>();
        var lineRenderer = renderer.GetComponent<LineRenderer>();
        lineRenderer.material = GetMaterial("ShaderTransparent");
        lineRenderer.SetColors(color, color);
        
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

    private static GameObject Create2DMesh(GameObject displayRoot, string name, List<Vector2> points, Color color)
    {
        if (HitboxDictionary.TryGetValue(name, out var gameObject))
        {
            if (gameObject != null) return gameObject;
        }
        
        name = HitboxObjectName + name;

        var meshObject = new GameObject(name)
        {
            transform =
            {
                parent = displayRoot.transform
            }
        };

        var triangulator = new Triangulator(points.ToArray());
        var indices = triangulator.Triangulate();
            
        var convertedPoints = new Il2CppSystem.Collections.Generic.List<Vector3>();
        foreach (var point in points)
        {
            convertedPoints.Add(new Vector3(point.x, 0f, -point.y));
        }
        
        var mesh = new Mesh();
        mesh.SetVertices(convertedPoints);
        mesh.triangles = indices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshObject.AddComponent<MeshRenderer>();
        meshObject.GetComponent<MeshRenderer>().material = GetMaterial("ShaderTransparent");
        meshObject.GetComponent<MeshRenderer>().material.color = new Color(color.r, color.g, color.b, SaveSystem.Transparency);
        meshObject.AddComponent<MeshFilter>();
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        return meshObject;
    }

    private static GameObject GetGameObject(string name)
    {
        var bundle = ModContent.GetBundle(ModHelper.GetMod("VisibleHitboxes"), "debugmat");
        return bundle.LoadAsset(name).Cast<GameObject>().Duplicate();
    }

    private static Material GetMaterial(string name)
    {
        var bundle = ModContent.GetBundle(ModHelper.GetMod("VisibleHitboxes"), "debugmat");
        return bundle.LoadAsset(name).Cast<Material>().Duplicate();
    }
    
    private static void UpdateHitbox(GameObject hitbox, Vector3 newPosition, Color color = default)
    {
        hitbox.transform.position = newPosition;
        if (hitbox.HasComponent<SpriteRenderer>())
        {
            var spriteRenderer = hitbox.GetComponent<SpriteRenderer>();
            if (color == default)
            {
                color = spriteRenderer.color;
            }
            spriteRenderer.color = new Color(color.r, color.g, color.b, SaveSystem.Transparency);
        }
        else if (hitbox.HasComponent<LineRenderer>())
        {
            var lineRenderer = hitbox.GetComponent<LineRenderer>();
            if (color == default)
            {
                color = lineRenderer.startColor;
            }
            color = new Color(color.r, color.g, color.b, SaveSystem.Transparency);
            lineRenderer.SetColors(color, color);
        }
        else if (hitbox.HasComponent<MeshRenderer>())
        {
            var meshRenderer = hitbox.GetComponent<MeshRenderer>();
            if (color == default)
            {
                color = meshRenderer.material.color;
            }
            meshRenderer.material.color = new Color(color.r, color.g, color.b, SaveSystem.Transparency);
        }
    }

    private static void RemoveUnusedHitboxes(List<string> inactiveIdentifiers)
    {
        foreach (var identifier in inactiveIdentifiers)
        {
            var valueExists = HitboxDictionary.TryGetValue(identifier, out var value);
            if (!valueExists) continue;
            try
            {
                HitboxDictionary.Remove(identifier);
            }
            catch (Exception e)
            {
                Log(e);
            }
            UnityEngine.Object.Destroy(value);
        }
    }

    private static GameObject? CreateCircularHitbox(Transform simDisplay, Color color, float radius, Vector3 offset, string name)
    {
        if (HitboxDictionary.TryGetValue(name, out var gameObject))
        {
            if (gameObject != null && gameObject.transform.parent.gameObject.active) return gameObject;
            return null;
        }
        name = HitboxObjectName + name;
        
        if (radius <= 0) {  // Some projectiles use pixel-perfect hitboxes. This makes them visible at the cost of accuracy.
            radius = 1f;
            color = ModifierProjectileColor;
        }

        radius *= CircleSizeMultiplier;
        
        var circle = GetGameObject("Circle");
        circle.name = name;
        circle.transform.parent = simDisplay;
        circle.transform.localPosition = offset;
        circle.transform.localScale = new Vector3(
            radius, 
            radius, 
            radius);
        var spriteRenderer = circle.GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(color.r, color.g, color.b, SaveSystem.Transparency);
        spriteRenderer.sortingLayerName = "Bloons";
        return circle;
    }
    private static GameObject? CreateTowerHitbox(Transform simDisplay, Color color, Object footprint, string name)
    {
        if (HitboxDictionary.TryGetValue(name, out var gameObject))
        {
            if (gameObject != null && gameObject.transform.parent.gameObject.active) return gameObject;
            return null;
        }
        name = HitboxObjectName + name;
        var scaleModifier = 1f / simDisplay.localScale.x;

        if (footprint.IsType<RectangleFootprintModel>())
        {
            var footprintModel = footprint.Cast<RectangleFootprintModel>();
            var square = GetGameObject("Square");
            square.name = name;
            square.transform.parent = simDisplay;
            square.transform.localPosition = Vector3.zero;
            square.transform.localScale = new Vector3(
                footprintModel.xWidth * scaleModifier,
                footprintModel.yWidth * scaleModifier, 
                footprintModel.yWidth * scaleModifier);
            var spriteRenderer = square.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(color.r, color.g, color.b, SaveSystem.Transparency);
            spriteRenderer.sortingLayerName = "Bloons";
            return square;
        }

        else
        {
            var footprintModel = footprint.Cast<CircleFootprintModel>();
            var circle = GetGameObject("Circle");
            var radius = footprintModel.radius * CircleSizeMultiplier;
            circle.name = name;
            circle.transform.parent = simDisplay;
            circle.transform.localPosition = Vector3.zero;
            circle.transform.localScale = new Vector3(
                radius * scaleModifier, 
                radius * scaleModifier, 
                radius * scaleModifier);
            var spriteRenderer = circle.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(color.r, color.g, color.b, SaveSystem.Transparency);
            spriteRenderer.sortingLayerName = "Bloons";
            return circle;
        }
    }
}