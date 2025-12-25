using BTD_Mod_Helper;
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
using Il2CppAssets.Scripts.Simulation.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Models;

[assembly:
    MelonInfo(typeof(VisibleHitboxes.VisibleHitboxes), ModHelperData.Name, ModHelperData.Version,
        ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace VisibleHitboxes;

public class VisibleHitboxes : BloonsTD6Mod
{
    private const int TOGGLE_ON_DELAY = 2;

    private bool isInGame;
    private bool wasPlacing;
    private bool forceHitboxes = false;
    private int scheduledToggle = -1;

    private bool enableBloons = false;
    private bool enableProjectiles = false;
    private bool enablePaths = false;
    
    private readonly List<HitboxManager> managers;
    private readonly TowerHitboxManager towerManager;
    private readonly MapHitboxManager mapManager;
    private readonly BloonHitboxManager bloonManager;
    private readonly ProjectileHitboxManager projectileManager;
    private readonly PathHitboxManager pathManager;

    public VisibleHitboxes()
    {
        towerManager = new();
        mapManager = new();
        bloonManager = new();
        projectileManager = new();
        pathManager = new();

        managers = new List<HitboxManager>
        {
            towerManager,
            mapManager,
            bloonManager,
            projectileManager,
            pathManager
        };
    }

    public override void OnMatchStart()
    {
        base.OnMatchStart();
        
        HitboxManager.Initialize();
        
        isInGame = true;
        wasPlacing = false;
        scheduledToggle = -1;

        enableBloons = false;
        enableProjectiles = false;
        enablePaths = false;

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

        enableBloons = false;
        enableProjectiles = false;
        enablePaths = false;
        
        foreach (var manager in managers)
            manager.OnMatchEnd();
    }

    public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
    {
        base.OnTowerUpgraded(tower, upgradeName, newBaseTowerModel);
        towerManager.OnTowerUpgraded(tower);
    }

    public override void OnProjectileCreated(Projectile projectile, Entity entity, Model modelToUse)
    {
        projectileManager?.OnProjectileCreated(projectile, modelToUse);
    }

    public override void OnUpdate()
    {
        if (!isInGame || InGame.instance == null) return;
        
        if (Settings.ForceHitboxesToggle.JustPressed())
        {
            forceHitboxes = !forceHitboxes;
            ToggleMapRendering(!forceHitboxes); 
        }
        
        var inputManager = InGame.instance.InputManagers.First();
        var isPlacing = inputManager.placementModel != null;
        
        scheduledToggle = Mathf.Max(scheduledToggle - 1, -1);
        
        if (!forceHitboxes)
        {
            if (scheduledToggle == 0 && !isPlacing)
            {
                ToggleMapRendering(true);
                scheduledToggle = -1;
            }
            else if (!isPlacing && wasPlacing)
            {
                // Schedule a toggle to not flicker the map
                scheduledToggle = TOGGLE_ON_DELAY;
            }
            else if (isPlacing && !wasPlacing) {
                ToggleMapRendering(false);
            }
        }
        
        bool shouldBeActive = isPlacing || forceHitboxes || scheduledToggle != -1;

        //foreach (var manager in managers)
        //    manager.Update(shouldBeActive);

        towerManager.Update(shouldBeActive);
        mapManager.Update(shouldBeActive);

        wasPlacing = isPlacing;

        // handle toggle hotkeys
        if (Settings.ToggleBloonHitboxes.JustPressed())
            enableBloons = !enableBloons;

        if (Settings.ToggleProjectileHitboxes.JustPressed())
            enableProjectiles = !enableProjectiles;

        if (Settings.TogglePathsOverlay.JustPressed())
            enablePaths = !enablePaths;

        // update managers
        bloonManager.Update(enableBloons);
        projectileManager.Update(enableProjectiles);
        pathManager.Update(enablePaths);
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
