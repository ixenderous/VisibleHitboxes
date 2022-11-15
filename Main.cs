using System;
using System.IO;
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
using System.Collections.Generic;
using Assets.Scripts.Models.Map;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    
    private static string _saveFileLocation = "";

    private static readonly Color TowerColor = new(1f, 1f, 0.85f);
    private static readonly Color ProjectileColor = new(1f, 0f, 0f);
    private static readonly Color InvalidPositionColor = new(1f, 0f, 0f);
    private static readonly Color InvisibleProjectileColor = new(1f, 0.5f, 0.60f);
    private static readonly Color ModifierProjectileColor = new(1f, 0.20f, 0.60f);
    private static readonly Color BloonColor = new(1f, 1f, 0f);
    private static readonly Color PathColor = new(0.9f, 0.95f, 0.85f);

    private const int HeldTowerHitboxId = -1;
    private const int LineRendererId = -2;

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
        LoadSaveFile();
    }

    public override void OnMatchEnd()
    {
        base.OnMatchEnd();
        _isInGame = false;
        var activeIdentifiers = _prevIdentifiers;
        RemoveUnusedHitboxes(activeIdentifiers); // Cleanup
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

    private static void LoadSaveFile()
    {
        try
        {
            if (_saveFileLocation == "")
            {
                const string folder = "\\BloonsTD6 Mod Helper\\Mod Saves";
                _saveFileLocation = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)?.FullName + folder;
                Directory.CreateDirectory(_saveFileLocation);
            }
            const string saveFile = "HitboxMod.json";
            
            var fileName = _saveFileLocation + "\\" + saveFile;
            
            if (!File.Exists(fileName)) return;
            var json = JObject.Parse(File.ReadAllText(fileName));
            foreach (var (name, token) in json)
            {
                if (token == null) continue;
                
                switch (name)
                {
                    case "IsAllEnabled":
                        _isAllEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreTowersEnabled":
                        _areTowersEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreProjectilesEnabled":
                        _areProjectilesEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreBloonsEnabled":
                        _areBloonsEnabled = bool.Parse(token.ToString());
                        break;
                    case "IsMapEnabled":
                        _isMapEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreHitboxesTransparent":
                        _areHitboxesTransparent = bool.Parse(token.ToString());
                        break;
                    case "Transparency":
                        _transparency = float.Parse(token.ToString());
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Log(e, MessageType.Error);
        }
    }

    private static void UpdateSaveFile()
    {
        if (_saveFileLocation == "")
        {
            const string folder = "\\BloonsTD6 Mod Helper\\Mod Saves";
            _saveFileLocation = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)?.FullName + folder;
            Directory.CreateDirectory(_saveFileLocation);
        }
        const string saveFile = "HitboxMod.json";
        
        var json = new JObject
        {
            ["IsAllEnabled"] = _isAllEnabled,
            ["AreTowersEnabled"] = _areTowersEnabled,
            ["AreProjectilesEnabled"] = _areProjectilesEnabled,
            ["AreBloonsEnabled"] = _areBloonsEnabled,
            ["IsMapEnabled"] = _isMapEnabled,
            ["AreHitboxesTransparent"] = _areHitboxesTransparent,
            ["Transparency"] = _transparency
        };
        
        File.WriteAllText(_saveFileLocation + "\\" + saveFile, json.ToString(Formatting.Indented));
    }

    private static void TryAddDictionary (Dictionary<string, GameObject> dictionary, string key, GameObject value)
    {
        try
        {
            dictionary.Add(key, value);
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public override void OnUpdate()
    {
        if (!_isInGame) return;

        var activeIdentifiers = new List<string>();
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
            _isAllEnabled = IsAllEnabled();
            UpdateSaveFile();
        }

        if (ToggleTowers.JustPressed())
        {
            _areTowersEnabled = !_areTowersEnabled;
            _isAllEnabled = IsAllEnabled();
            UpdateSaveFile();
        }

        if (ToggleProjectiles.JustPressed())
        {
            _areProjectilesEnabled = !_areProjectilesEnabled;
            _isAllEnabled = IsAllEnabled();
            UpdateSaveFile();
        }

        if (ToggleBloons.JustPressed())
        {
            _areBloonsEnabled = !_areBloonsEnabled;
            _isAllEnabled = IsAllEnabled();
            UpdateSaveFile();
        }

        if (ToggleMapCollision.JustPressed())
        {
            _isMapEnabled = !_isMapEnabled;
            _isAllEnabled = IsAllEnabled();
            UpdateSaveFile();
        }

        if (ToggleTransparency.JustPressed())
        {
            _areHitboxesTransparent = !_areHitboxesTransparent;
            _transparency = _areHitboxesTransparent ? DefaultTransparency : 1f;
            _isAllEnabled = IsAllEnabled();
            UpdateSaveFile();
        }
        
        if (_areTowersEnabled)
        {
            foreach (var tower in InGame.Bridge.GetAllTowers())
            {
                if (tower.GetSimTower().GetUnityDisplayNode() == null) continue;
                var simDisplay = tower.GetSimTower().GetUnityDisplayNode().gameObject.transform;
                var footprint = tower.Def.footprint;
                var towerId = tower.Id.Id;
                activeIdentifiers.Add(tower.Id.Id.ToString());
                var hitbox = CreateTowerHitbox(simDisplay, TowerColor, footprint, towerId.ToString());
                if (hitbox != null)
                {
                    TryAddDictionary(HitboxDictionary, towerId.ToString(), hitbox);
                    UpdateHitbox(hitbox, simDisplay.position, TowerColor);
                }
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
                var simDisplayPosition = simDisplay.position;
                var towerPos = new Vector2(simDisplayPosition.x, -simDisplayPosition.z);
                var footprint = placementModel.footprint;
                var inputId = InGame.Bridge.GetInputId();
                var canPlace = InGame.Bridge.CanPlaceTowerAt(towerPos, placementModel, inputId, placementTowerId);
                var color = canPlace ? TowerColor : InvalidPositionColor;
                activeIdentifiers.Add(HeldTowerHitboxId.ToString());
                var hitbox = CreateTowerHitbox(simDisplay, color, footprint, HeldTowerHitboxId.ToString());
                if (hitbox != null)
                {
                    TryAddDictionary(HitboxDictionary, HeldTowerHitboxId.ToString(), hitbox);
                    UpdateHitbox(hitbox, simDisplay.position, color);
                }
            }
        }

        if (_areProjectilesEnabled)
        {
            foreach (var projectile in InGame.Bridge.GetAllProjectiles())
            {
                var projectileId = projectile.Id.Id;
                activeIdentifiers.Add(projectileId.ToString());
                var radius = projectile.radius;
                if (projectile.GetUnityDisplayNode() == null)
                {
                    var projectileModel = projectile.projectileModel;
                    if (projectileModel.display.IsValid) continue;
                    var hasDisplay = Enumerable.Any(projectileModel.behaviors, behavior => behavior.TypeName() == "SetSpriteFromPierceModel");
                    if (hasDisplay) continue;

                    // Invisible model
                    if (projectile.display.node != null)
                    {
                        var projectilePos = projectile.display.node.position.data;
                        var displayPos = new Vector3(projectilePos.x, 0f, -projectilePos.y);
                        var invhitbox = CreateSphericalHitbox(displayRoot, InvisibleProjectileColor, radius, displayPos, projectileId.ToString());
                        TryAddDictionary(HitboxDictionary, projectileId.ToString(), invhitbox);
                        UpdateHitbox(invhitbox, displayPos, InvisibleProjectileColor);
                    }
                    continue;
                }
                var simDisplay = projectile.GetUnityDisplayNode().gameObject.transform;
                var hitbox = CreateSphericalHitbox(simDisplay, ProjectileColor, radius, Vector3.zero, projectileId.ToString());
                if (hitbox != null)
                {
                    TryAddDictionary(HitboxDictionary, projectileId.ToString(), hitbox);
                    UpdateHitbox(hitbox, hitbox.transform.position, ProjectileColor);
                }
            }
        }

        if (_areBloonsEnabled)
        {
            foreach (var bloon in InGame.Bridge.GetAllBloons())
            {
                var bloonId = bloon.id.Id;
                if (bloon.GetUnityDisplayNode() == null) continue;
                var simDisplay = bloon.GetUnityDisplayNode().gameObject.transform;
                var collisionData = bloon.GetSimBloon().AdditionalCollisions();
                if (collisionData != null) // Bloons with multiple collisions. Usually reserved for MOAB class
                {
                    var count = 0;
                    foreach (var collision in collisionData)
                    {
                        var offset = new Vector3(collision.offset.x, 0f, collision.offset.y);
                        var radius = collision.radius;
                        var hName = bloonId + "_" + count;
                        var hitbox = CreateSphericalHitbox(simDisplay, BloonColor, radius, offset, hName);
                        if (hitbox != null)
                        {
                            activeIdentifiers.Add(hName);
                            TryAddDictionary(HitboxDictionary, hName, hitbox);
                            UpdateHitbox(hitbox, hitbox.transform.position, BloonColor);
                        }
                        count++;
                    }
                }
                else // Single collision bloons
                {
                    var radius = bloon.GetSimBloon().radius;
                    var hitbox = CreateSphericalHitbox(simDisplay, BloonColor, radius, Vector3.zero, bloonId.ToString());
                    if (hitbox != null)
                    {
                        activeIdentifiers.Add(bloonId.ToString());
                        TryAddDictionary(HitboxDictionary, bloonId.ToString(), hitbox);
                        UpdateHitbox(hitbox, hitbox.transform.position, BloonColor);
                    }
                }
            }
        }

        if (_isMapEnabled)
        {
            var index = 0;
            foreach (var path in InGame.instance.GetMap().mapModel.paths)
            {
                var hName = LineRendererId + "_" + index;
                activeIdentifiers.Add(hName);
                var gmLineRenderer = CreateLineRenderer(displayRoot.gameObject, hName, path, PathColor);
                if (gmLineRenderer != null)
                {
                    TryAddDictionary(HitboxDictionary, hName, gmLineRenderer);
                    UpdateHitbox(gmLineRenderer, Vector3.zero, PathColor);
                }
                index++;
            }

            // TODO create non-placeable area lines
        }
        
        // Get list difference
        var difList = _prevIdentifiers.Except(activeIdentifiers).ToList();
        RemoveUnusedHitboxes(difList);
        _prevIdentifiers = activeIdentifiers.Duplicate();
    }

    private static GameObject? CreateLineRenderer(GameObject displayRoot, string name, PathModel path, Color color)
    {
        name = HitboxObjectName + name;
        foreach (var gameobject in GetAllChilds(displayRoot).Where(gameobject => gameobject.name ==  name))
        {
            return gameobject;
        }

        var gameObject = new GameObject(name)
        {
            transform =
            {
                parent = displayRoot.transform
            }
        };
        
        gameObject.AddComponent<LineRenderer>();
        var lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = GetMaterial("ShaderTransparent");
        lineRenderer.SetColors(color, color);
        
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

    private static void UpdateHitbox(GameObject hitbox, Vector3 newPosition, Color color)
    {
        hitbox.transform.position = newPosition;
        if (hitbox.HasComponent<SpriteRenderer>())
        {
            hitbox.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
        }
        else if (hitbox.HasComponent<LineRenderer>())
        {
            var renderer = hitbox.GetComponent<LineRenderer>();
            color = new Color(color.r, color.g, color.b, _transparency);
            renderer.SetColors(color, color);
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

    private static GameObject CreateSphericalHitbox(Transform simDisplay, Color color, float radius, Vector3 offset, string name)
    {
        name = HitboxObjectName + name;
        if (radius <= 0) {  // Some towers use pixel-perfect hitboxes. This makes them visible, but not accurate
            radius = 1f;
            color = ModifierProjectileColor;
        }
        foreach (var gameobject in GetAllChilds(simDisplay.gameObject).Where(gameobject => gameobject.name == name))
        {
            return gameobject;
        }

        radius *= CircleSizeMultiplier;
        
        var circle = GetGameObject("Circle");
        circle.name = name;
        circle.transform.parent = simDisplay;
        circle.transform.localPosition = offset;
        circle.transform.localScale = new Vector3(radius, radius, radius);
        circle.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
        return circle;
    }
    private static GameObject? CreateTowerHitbox(Transform simDisplay, Color color, Object footprint, string name)
    {
        name = HitboxObjectName + name;
        foreach (var gameobject in GetAllChilds(simDisplay.gameObject).Where(gameobject => gameobject.name == name))
        {
            return gameobject;
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
            return square;
        }

        if (footprint.IsType<CircleFootprintModel>())
        {
            var footprintModel = footprint.Cast<CircleFootprintModel>();
            var circle = GetGameObject("Circle");
            var radius = footprintModel.radius * CircleSizeMultiplier;
            circle.name = name;
            circle.transform.parent = simDisplay;
            circle.transform.localPosition = Vector3.zero;
            circle.transform.localScale = new Vector3(radius, radius, radius);
            circle.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, _transparency);
            return circle;
        }

        return null;
    }
}