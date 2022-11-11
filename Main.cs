using System;
using Assets.Scripts.Models.Towers.Behaviors;
using Assets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
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
    //private static Shader _outlineShader = null!;
    
    private static MelonLogger.Instance _mllog = null!;
    private static bool _flatHitbox = true;
    private const float HHeight = 0.1f;

    public override void OnMatchStart()
    {
        base.OnMatchStart();
        _isInGame = true;
    }

    public override void OnMatchEnd()
    {
        base.OnMatchEnd();
        _isInGame = false;
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

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!_isInGame) return;
        
        foreach (var tower in InGame.Bridge.GetAllTowers())
        {
            if (tower.GetSimTower().GetUnityDisplayNode() == null) continue;
            var simDisplay = tower.GetSimTower().GetUnityDisplayNode().gameObject.transform;
            var footprint = tower.Def.footprint;
            var color = new Color(1f,1f,1f);
            CreateTowerHitbox(simDisplay, color, footprint);
        }

        foreach (var projectile in InGame.Bridge.GetAllProjectiles())
        {
            if (projectile.GetUnityDisplayNode() == null) continue;
            var simDisplay = projectile.GetUnityDisplayNode().gameObject.transform;
            var radius = projectile.radius;
            var color = new Color(1f, 0f, 0f);
            CreateSphericalHitbox(simDisplay, color, radius);
        }
    }

    private static Material GetTransparentOutlineMaterial()
    {
        var bundle = ModContent.GetBundle(ModHelper.GetMod("HitboxMod"), "debugmat");
        return bundle.LoadAsset("DebugMat").Cast<Material>();
    }
    
    private static void CreateSphericalHitbox(Transform simDisplay, Color color, float radius)
    {
        foreach(var gameobject in GetAllChilds(simDisplay.gameObject))
        {
            if (gameobject.name is "Sphere")
            {
                return;
            }
        }

        var mat = GetTransparentOutlineMaterial().Duplicate();
        mat.color = color;
        var height = HHeight;
        
        const int sizeMult = 2; // The primitive circle has 0.5 units of radius for some reason
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        if (!_flatHitbox) height = radius;
        sphere.transform.parent = simDisplay;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = new Vector3(radius*sizeMult, height*sizeMult, radius*sizeMult);
        sphere.GetComponent<MeshRenderer>().material = mat;
    }
    private static void CreateTowerHitbox(Transform simDisplay, Color color, Object footprint)
    {
        foreach(var gameobject in GetAllChilds(simDisplay.gameObject))
        {
            if (gameobject.name is "Sphere" or "Cube")
            {
                return;
            }
        }

        var mat = GetTransparentOutlineMaterial().Duplicate();
        mat.color = color;
        var height = HHeight;
        var sizeMult = 1/simDisplay.localScale.x;

        if (footprint.IsType<RectangleFootprintModel>())
        {
            var footprintModel = footprint.Cast<RectangleFootprintModel>();
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!_flatHitbox)
            {
                height = (footprintModel.xWidth + footprintModel.yWidth) / 2;
            }
            cube.transform.parent = simDisplay;
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(footprintModel.xWidth*sizeMult, height*sizeMult, footprintModel.yWidth*sizeMult);
            cube.GetComponent<MeshRenderer>().material = mat;
        }
        else if (footprint.IsType<CircleFootprintModel>())
        {
            sizeMult *= 2; // The primitive circle has 0.5 units of radius for some reason
            var footprintModel = footprint.Cast<CircleFootprintModel>();
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (!_flatHitbox)
            {
                height = footprintModel.radius;
            }
            sphere.transform.parent = simDisplay;
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = new Vector3(footprintModel.radius*sizeMult, height*sizeMult, footprintModel.radius*sizeMult);
            sphere.GetComponent<MeshRenderer>().material = mat;
        }
    }
}