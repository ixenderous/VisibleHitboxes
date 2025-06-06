using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using MelonLoader;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VisibleHitboxes;
using VisibleHitboxes.HitboxManagers;

[assembly: MelonInfo(typeof(VisibleHitboxes.VisibleHitboxes), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace VisibleHitboxes;

public class VisibleHitboxes : BloonsTD6Mod
{
    private bool isInGame;

    private readonly List<HitboxManager> managers;

    private readonly ProjectileHitboxManager projectileManager;
    private readonly TowerHitboxManager towerManager;
    private readonly BloonHitboxManager bloonManager;
    private readonly PathHitboxManager pathManager;
    private readonly MapHitboxManager mapManager;
    
    public VisibleHitboxes()
    {
        projectileManager = new(Settings.ShowProjectileHitboxes);
        towerManager = new(Settings.ShowTowerHitboxes);
        bloonManager = new(Settings.ShowBloonHitboxes);
        pathManager = new(Settings.ShowPathOverlay);
        mapManager = new(Settings.ShowMapOverlay);
        managers = [
            projectileManager,
            towerManager,
            bloonManager,
            pathManager,
            mapManager
        ];
    }

    public override void OnMatchStart()
    {
        base.OnMatchStart();
        isInGame = true;

        Camera cam = InGame.instance.sceneCamera;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        }

        ToggleMapRendering(Settings.RenderMap);

        foreach (var manager in managers)
            manager.OnMatchStart();
    }

    public override void OnMatchEnd()
    {
        base.OnMatchEnd();
        isInGame = false;

        foreach (var manager in managers)
            manager.OnMatchEnd();
    }

    public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
    {
        base.OnTowerUpgraded(tower, upgradeName, newBaseTowerModel);
        towerManager?.OnTowerUpgraded(tower);
    }

    public override void OnProjectileCreated(Projectile projectile, Entity entity, Model modelToUse)
    {
        projectileManager?.OnProjectileCreated(projectile, modelToUse);
    }

    public override void OnUpdate()
    {
        if (!isInGame || InGame.instance == null) return;

        HandleHotkeys();

        foreach (var manager in managers)
            manager.Update();
    }

    private void HandleHotkeys()
    {
        //if (Settings.DebugHotkey.JustPressed())
        //    DebugLog();

        if (Settings.ToggleMapRendering.JustPressed())
        {
            Settings.RenderMap.SetValue(!Settings.RenderMap);
            ToggleMapRendering(Settings.RenderMap);
        }

        if (Settings.ToggleAll.JustPressed())
        {
            bool newState = !Settings.IsEverythingEnabled();
            Settings.ShowTowerHitboxes.SetValue(newState);
            Settings.ShowProjectileHitboxes.SetValue(newState);
            Settings.ShowBloonHitboxes.SetValue(newState);
            Settings.ShowMapOverlay.SetValue(newState);
            Settings.ShowPathOverlay.SetValue(newState);
        }

        if (Settings.ToggleTowerHitboxes.JustPressed())
            Settings.ShowTowerHitboxes.SetValue(!Settings.ShowTowerHitboxes);

        if (Settings.ToggleProjectileHitboxes.JustPressed())
            Settings.ShowProjectileHitboxes.SetValue(!Settings.ShowProjectileHitboxes);

        if (Settings.ToggleBloonHitboxes.JustPressed())
            Settings.ShowBloonHitboxes.SetValue(!Settings.ShowBloonHitboxes);

        if (Settings.ToggleMapOverlay.JustPressed())
            Settings.ShowMapOverlay.SetValue(!Settings.ShowMapOverlay);

        if (Settings.TogglePathsOverlay.JustPressed())
            Settings.ShowPathOverlay.SetValue(!Settings.ShowPathOverlay);

        if (Settings.ToggleTransparency.JustPressed())
        {
            Settings.UseTransparency.SetValue(!Settings.UseTransparency);
            UpdateAllHitboxes();
        }
    }

    private void UpdateAllHitboxes()
    {
        foreach (var manager in managers)
            manager.UpdateHitboxes();
    }

    public static GameObject GetGameObject(string name)
    {
        var bundle = ModContent.GetBundle(ModHelper.GetMod("VisibleHitboxes"), "debugmat");
        return bundle.LoadAsset(name).Cast<GameObject>().Duplicate();
    }

    public static Material GetMaterial(string name)
    {
        var bundle = ModContent.GetBundle(ModHelper.GetMod("VisibleHitboxes"), "debugmat");
        return bundle.LoadAsset(name).Cast<Material>().Duplicate();
    }

    private void DebugLog()
    {
        MelonLogger.Msg("\n");
        MelonLogger.Msg("DebugLog");
        foreach (var manager in managers)
        {
            manager.LogDebugInfo();
            MelonLogger.Msg("");
        }
    }

    static void ToggleMapRendering(bool enabled)
    {
        var mapName = InGame.instance.GetMap().mapModel.mapName;
        Scene scene = SceneManager.GetSceneByName(mapName);
        GameObject mapObject = scene.GetRootGameObjects().First();

        ToggleRendering(mapObject, enabled);
    }

    static void ToggleRendering(GameObject root, bool enabled)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = enabled;
        }
    }
}