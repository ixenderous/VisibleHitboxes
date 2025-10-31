﻿using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using MelonLoader;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VisibleHitboxes;
using VisibleHitboxes.HitboxManagers;

[assembly:
    MelonInfo(typeof(VisibleHitboxes.VisibleHitboxes), ModHelperData.Name, ModHelperData.Version,
        ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace VisibleHitboxes;

public class VisibleHitboxes : BloonsTD6Mod
{
    private bool isInGame;
    private bool wasPlacing;
    private bool forceHitboxes = false;

    private readonly List<HitboxManager> managers;
    private readonly TowerHitboxManager towerManager;
    private readonly MapHitboxManager mapManager;

    public VisibleHitboxes()
    {
        towerManager = new();
        mapManager = new();
        managers = new List<HitboxManager>
        {
            towerManager,
            mapManager
        };
    }

    public override void OnMatchStart()
    {
        base.OnMatchStart();
        
        HitboxManager.Initialize();
        
        isInGame = true;
        wasPlacing = false;

        Camera cam = InGame.instance.sceneCamera;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        }

        foreach (var manager in managers)
            manager.OnMatchStart();
    }

    public override void OnMatchEnd()
    {
        base.OnMatchEnd();
        isInGame = false;
        wasPlacing = false;
        forceHitboxes = false;
        
        foreach (var manager in managers)
            manager.OnMatchEnd();
    }

    public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
    {
        base.OnTowerUpgraded(tower, upgradeName, newBaseTowerModel);
        towerManager.OnTowerUpgraded(tower);
    }

    public override void OnUpdate()
    {
        if (!isInGame || InGame.instance == null) return;
        
        if (Settings.ForceHitboxesToggle.JustPressed())
        {
            forceHitboxes = !forceHitboxes;
            
            ToggleMapRendering(!forceHitboxes); 
        }
        
        var inputManager = InGame.instance.InputManager;
        var isPlacing = inputManager.placementModel != null;
        
        bool shouldBeActive = isPlacing || forceHitboxes;
        
        if (!forceHitboxes && (isPlacing != wasPlacing))
        {
            ToggleMapRendering(!isPlacing);
        }
        
        foreach (var manager in managers)
            manager.Update(shouldBeActive);

        wasPlacing = isPlacing;
    }
    
    static void ToggleMapRendering(bool enabled)
    {
        var mapName = InGame.instance.GetMap().mapModel.mapName;
        Scene scene = SceneManager.GetSceneByName(mapName);
        GameObject mapObject = scene.GetRootGameObjects().First();
        
        Renderer[] renderers = mapObject.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = enabled;
        }
    }
}