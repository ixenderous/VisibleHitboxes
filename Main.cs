using System;
using System.Linq;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HitboxMod;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Main = HitboxMod.Main;
using Object = Il2CppSystem.Object;

// ReSharper disable MemberCanBePrivate.Global

[assembly: MelonInfo(typeof(Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HitboxMod;

public class Main : BloonsTD6Mod
{
    private bool _isInGame;
    
    private static MelonLogger.Instance _mllog = null!;

    private static bool _isAllEnabled = true;
    private static bool _areTowersEnabled = true;
    private static bool _areProjectilesEnabled = true;
    private static bool _areBloonsEnabled = true;
    private static bool _isMapEnabled = true;
    private static bool _areHitboxesTransparent = true;
    private static float _transparency = DefaultTransparency;

    private const float CircleSizeMultiplier = 2f;
    private const float DefaultTransparency = 0.5f;
    private const string HitboxObjectName = "Hitbox_";

    private static readonly Color TowerColor = new(1f, 1f, 0.85f);
    private static readonly Color ProjectileColor = new(1f, 0f, 0f);
    private static readonly Color InvalidPositionColor = new(1f, 0f, 0f);
    private static readonly Color InvisibleProjectileColor = new(1f, 0.5f, 0.60f);
    private static readonly Color ModifierProjectileColor = new(1f, 0.20f, 0.60f);
    private static readonly Color BloonColor = new(1f, 1f, 0f);
    private static readonly Color PathColor = new(0.9f, 0.95f, 0.85f);

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
        icon = VanillaSprites.PlasmaMonkeyFanClubUpgradeIcon
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
        icon = VanillaSprites.EzCollectUpgradeIcon
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
    }

    public override void OnMatchEnd()
    {
        base.OnMatchEnd();
        _isInGame = false;
        var activeIdentifiers = new List<int>();
        var displayRoot = Game.instance.GetDisplayFactory().DisplayRoot;
        RemoveUnusedHitboxes(displayRoot, activeIdentifiers); // Cleanup
    }
    
    public override void OnInitialize()
    {
        _mllog = LoggerInstance;
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

    private static List<GameObject> GetAllChilds(GameObject go)
    {
        var list = new List<GameObject>();
        for (var i = 0; i < go.transform.childCount; i++)
        {
            list.Add(go.transform.GetChild(i).gameObject);
        }
        return list;
    }

    private static bool IsAllEnabled()
    {
        return _areTowersEnabled && _areProjectilesEnabled && _areBloonsEnabled && _isMapEnabled;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        
        if (!_isInGame) return;

        var activeIdentifiers = new List<int>();
        var displayRoot = Game.instance.GetDisplayFactory().DisplayRoot;
        
        if (ToggleAll.JustPressed())
        {
            if (_isAllEnabled)
            {
                _areBloonsEnabled = false;
                _areProjectilesEnabled = false;
                _areTowersEnabled = false;
                _isMapEnabled = false;
                _isAllEnabled = false;
            }
            else
            {
                _areBloonsEnabled = true;
                _areProjectilesEnabled = true;
                _areTowersEnabled = true;
                _isMapEnabled = true;
                _isAllEnabled = true;
            }
        }

        if (ToggleTowers.JustPressed())
        {
            _areTowersEnabled = !_areTowersEnabled;
        }

        if (ToggleProjectiles.JustPressed())
        {
            _areProjectilesEnabled = !_areProjectilesEnabled;
        }

        if (ToggleBloons.JustPressed())
        {
            _areBloonsEnabled = !_areBloonsEnabled;
        }

        if (ToggleMapCollision.JustPressed())
        {
            _isMapEnabled = !_isMapEnabled;
        }

        if (ToggleTransparency.JustPressed())
        {
            _areHitboxesTransparent = !_areHitboxesTransparent;
            _transparency = _areHitboxesTransparent ? DefaultTransparency : 1f;
        }

        _isAllEnabled = IsAllEnabled();

        if (_areTowersEnabled)
        {
            foreach (var tower in InGame.Bridge.GetAllTowers())
            {
                if (tower.GetSimTower().GetUnityDisplayNode() == null) continue;
                var simDisplay = tower.GetSimTower().GetUnityDisplayNode().gameObject.transform;
                var footprint = tower.Def.footprint;
                var towerId = tower.Id.Id;
                activeIdentifiers.Add(tower.Id.Id);
                CreateTowerHitbox(simDisplay, TowerColor, footprint, towerId.ToString());
            }

            // Held tower hitbox
            var inputManager = InGame.instance.InputManager;
            var placementDisplayList = inputManager.placementGraphics;
            var placementModel = inputManager.placementModel;
            var placementTowerId = inputManager.placementTowerId;
            if (placementModel != null && placementDisplayList.Count > 0)
            {
                var placementDisplay = placementDisplayList.First();
                var simDisplay = placementDisplay.gameObject.transform;
                var footprint = placementModel.footprint;
                var mousePos = inputManager.cursorPositionWorld;
                var inputId = InGame.Bridge.GetInputId();
                var canPlace = InGame.Bridge.CanPlaceTowerAt(mousePos, placementModel, inputId, placementTowerId);
                var color = canPlace ? TowerColor : InvalidPositionColor;
                activeIdentifiers.Add(-1);
                CreateTowerHitbox(simDisplay, color, footprint, "-1");
                UpdateHitbox(simDisplay, simDisplay.transform.position, "-1", color);
            }
        }

        if (_areProjectilesEnabled)
        {
            foreach (var projectile in InGame.Bridge.GetAllProjectiles())
            {
                var projectileId = projectile.Id.Id;
                activeIdentifiers.Add(projectileId);
                var radius = projectile.radius;
                var color = ProjectileColor;
                if (projectile.GetUnityDisplayNode() == null)
                {
                    var projectileModel = projectile.projectileModel;
                    if (projectileModel.display.IsValid) continue;
                    var hasDisplay = Enumerable.Any(projectileModel.behaviors, behavior => behavior.TypeName() == "SetSpriteFromPierceModel");
                    if (hasDisplay) continue;

                    // Invisible model
                    color = InvisibleProjectileColor;
                    if (projectile.display.node != null)
                    {
                        var projectilePos = projectile.display.node.position.data;
                        var displayPos = new Vector3(projectilePos.x, 0f, -projectilePos.y);
                        CreateSphericalHitbox(displayRoot, color, radius, displayPos, projectileId.ToString());
                        UpdateHitbox(displayRoot, displayPos, projectileId.ToString(), color);
                    }
                    continue;
                }
                var simDisplay = projectile.GetUnityDisplayNode().gameObject.transform;
                CreateSphericalHitbox(simDisplay, color, radius, Vector3.zero, projectileId.ToString());
            }
        }

        if (_areBloonsEnabled)
        {
            foreach (var bloon in InGame.Bridge.GetAllBloons())
            {
                var bloonId = bloon.id.Id;
                activeIdentifiers.Add(bloonId);
                if (bloon.GetUnityDisplayNode() == null) continue;
                var simDisplay = bloon.GetUnityDisplayNode().gameObject.transform;
                var color = BloonColor;
                var collisionData = bloon.GetSimBloon().AdditionalCollisions();
                if (collisionData != null) // Bloons with multiple collisions. Usually reserved for MOAB class
                {
                    var count = 0;
                    foreach (var collision in collisionData)
                    {
                        var offset = new Vector3(collision.offset.x, 0f, collision.offset.y);
                        var radius = collision.radius;
                        CreateSphericalHitbox(simDisplay, color, radius, offset, bloonId + "_" + count);
                        count++;
                    }
                }
                else // Single collision bloons
                {
                    var radius = bloon.GetSimBloon().radius;
                    CreateSphericalHitbox(simDisplay, color, radius, Vector3.zero, bloonId.ToString());
                }
            }
        }

        if (_isMapEnabled)
        {
            activeIdentifiers.Add(-2);
            var index = 0;
            foreach (var path in InGame.instance.GetMap().mapModel.paths)
            {
                var color = PathColor;
                var gmLineRenderer = CreateLineRenderer(HitboxObjectName + "-2_" + index, color);
                if (gmLineRenderer != null)
                {
                    var lineRenderer = gmLineRenderer.GetComponent<LineRenderer>();
                    var pointArray = path.points;
                    var convertedArray = new Vector3[pointArray.Length];
                    lineRenderer.positionCount = pointArray.Length;
                    for (var i = 0; i < pointArray.Length; i++)
                    {
                        var curPoint = pointArray[i].point;
                        var convertedCur = new Vector3(curPoint.x, 0f, -curPoint.y);
                        convertedArray[i] = convertedCur;
                    }
                    lineRenderer.SetPositions(convertedArray);
                }
                index++;
            }

            // TODO create non-placeable area lines
            /*
            activeIdentifiers.Add(-3);
            index = 0;
            foreach (var blocker in InGame.instance.GetMap().mapModel.blockers)
            {
                var offset = new Vector3(blocker.circle.position.x, 0f, -blocker.circle.position.z);
                CreateSphericalHitbox(displayRoot, Color.black, blocker.circle.radius, offset, "-3_" + index);
                index++;
            }
            */
        }

        RemoveUnusedHitboxes(displayRoot, activeIdentifiers);
    }

    private static GameObject? CreateLineRenderer(string name, Color color)
    {
        var displayRoot = Game.instance.GetDisplayFactory().DisplayRoot.gameObject;
        foreach(var gameobject in GetAllChilds(displayRoot))
        {
            if (gameobject.name ==  name)
            {
                return null;
            }
        }

        var gameObject = new GameObject(name)
        {
            transform =
            {
                parent = displayRoot.transform
            }
        };
        gameObject.AddComponent<LineRenderer>();
        gameObject.GetComponent<LineRenderer>().material = GetMaterial("ShaderTransparent");
        gameObject.GetComponent<LineRenderer>().SetColors(color, color);
        return gameObject;
    }

    private static GameObject GetGameObject(string name)
    {
        var bundle = ModContent.GetBundle(ModHelper.GetMod("HitboxMod"), "debugmat");
        return bundle.LoadAsset(name).Cast<GameObject>().Duplicate();
    }

    private static Material GetMaterial(string name)
    {
        var bundle = ModContent.GetBundle(ModHelper.GetMod("HitboxMod"), "debugmat");
        return bundle.LoadAsset(name).Cast<Material>().Duplicate();
    }

    private static void UpdateHitbox(Transform parent, Vector3 newPosition, string name, Color color)
    {
        var hitbox = parent.FindChild(HitboxObjectName + name);
        hitbox.position = newPosition;
        hitbox.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
    }

    private static void RemoveUnusedHitboxes(Transform parent, List<int> activeIdentifiers)
    {
        foreach (var gameObject in GetAllChilds(parent.gameObject))
        {
            if (gameObject.name.StartsWith(HitboxObjectName))
            {
                if (RemoveIfNotInList(gameObject, activeIdentifiers)) continue;
                if (gameObject.HasComponent<SpriteRenderer>())
                {
                    var color = gameObject.GetComponent<SpriteRenderer>().color;
                    gameObject.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
                }
                else if (gameObject.HasComponent<LineRenderer>())
                {
                    var renderer = gameObject.GetComponent<LineRenderer>();
                    var color = renderer.startColor;
                    color = new Color(color.r, color.g, color.b, _transparency);
                    renderer.SetColors(color, color);
                }
            }
            else
            {
                foreach (var child in GetAllChilds(gameObject))
                {
                    if (child.name.StartsWith(HitboxObjectName))
                    {
                        if (RemoveIfNotInList(child, activeIdentifiers)) continue;
                        if (child.HasComponent<SpriteRenderer>())
                        {
                            var color = child.GetComponent<SpriteRenderer>().color;
                            child.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
                        }
                    }
                }
            }
        }
    }

    private static bool RemoveIfNotInList(GameObject gameObject, List<int> activeIdentifiers)
    {
        var tokens = gameObject.name.Split('_');
        var id = int.Parse(tokens[1]);
        if (activeIdentifiers.Contains(id)) return false;
        UnityEngine.Object.Destroy(gameObject);
        return true;
    }

    private static void CreateSphericalHitbox(Transform simDisplay, Color color, float radius, Vector3 offset, string name)
    {
        name = HitboxObjectName + name;
        if (radius <= 0) {  // Some towers use pixel-perfect hitboxes. This makes them visible, but not accurate
            radius = 1f;
            color = ModifierProjectileColor;
        }
        foreach(var gameobject in GetAllChilds(simDisplay.gameObject))
        {
            if (gameobject.name == name)
            {
                return;
            }
        }

        radius *= CircleSizeMultiplier;
        
        var circle = GetGameObject("Circle");
        circle.name = name;
        circle.transform.parent = simDisplay;
        circle.transform.localPosition = offset;
        circle.transform.localScale = new Vector3(radius, radius, radius);
        circle.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
    }
    private static void CreateTowerHitbox(Transform simDisplay, Color color, Object footprint, string name)
    {
        name = HitboxObjectName + name;
        foreach(var gameobject in GetAllChilds(simDisplay.gameObject))
        {
            if (gameobject.name == name)
            {
                return;
            }
        }
        
        if (footprint.IsType<RectangleFootprintModel>())
        {
            var footprintModel = footprint.Cast<RectangleFootprintModel>();
            var square = GetGameObject("Square");
            square.name = name;
            square.transform.parent = simDisplay;
            square.transform.localPosition = Vector3.zero;
            square.transform.localScale = new Vector3(footprintModel.xWidth, footprintModel.yWidth, footprintModel.yWidth);
            square.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
        }
        else if (footprint.IsType<CircleFootprintModel>())
        {
            var footprintModel = footprint.Cast<CircleFootprintModel>();
            var circle = GetGameObject("Circle");
            var radius = footprintModel.radius * CircleSizeMultiplier;
            circle.name = name;
            circle.transform.parent = simDisplay;
            circle.transform.localPosition = Vector3.zero;
            circle.transform.localScale = new Vector3(radius, radius, radius);
            circle.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
        }
    }
}