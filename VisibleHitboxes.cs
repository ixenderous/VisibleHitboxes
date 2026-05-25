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
    private bool forcePlacementMode = false;
    private int scheduledToggle = -1;

    private bool enableTowerOverlay = false;
    private bool enableMapOverlay = false;
    private bool enableMapRendering = true;   // true = rendering on (the normal state)
    private bool lastMapRenderingOn = true;
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
        enableTowerOverlay = false;
        enableMapOverlay = false;
        enableMapRendering = true;
        lastMapRenderingOn = true;

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
        forcePlacementMode = false;

        enableBloons = false;
        enableProjectiles = false;
        enablePaths = false;
        enableTowerOverlay = false;
        enableMapOverlay = false;
        enableMapRendering = true;
        lastMapRenderingOn = true;

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

        // --- Force placement mode hotkey ---
        if (Settings.ForcePlacementMode.JustPressed())
        {
            forcePlacementMode = !forcePlacementMode;
            ToggleMapRendering(!forcePlacementMode);
        }

        var inputManager = InGame.instance.InputManagers.First();
        var isPlacing = inputManager.placementModel != null;

        // Tick the scheduled-toggle countdown down, clamped at -1 (idle).
        scheduledToggle = Mathf.Max(scheduledToggle - 1, -1);

        // --- Auto placement mode ---
        if (!forcePlacementMode)
        {
            if (!Settings.AutoTogglePlacementMode)
            {
                scheduledToggle = -1;
            }
            else
            {
                if (scheduledToggle == 0 && !isPlacing)
                {
                    ToggleMapRendering(true);
                    scheduledToggle = -1;
                }
                else if (!isPlacing && wasPlacing)
                {
                    scheduledToggle = TOGGLE_ON_DELAY;
                }
                else if (isPlacing && !wasPlacing)
                {
                    ToggleMapRendering(false);
                }
            }
        }

        bool autoPlacing = Settings.AutoTogglePlacementMode && (isPlacing || scheduledToggle != -1);
        bool shouldBeActive = autoPlacing || forcePlacementMode;

        // --- Manual overlay/rendering hotkeys ---
        // Each toggles independently of placement mode. These persist across
        // frames just like enableBloons etc., and OR with placement mode below.
        if (Settings.ToggleTowerOverlay.JustPressed())
            enableTowerOverlay = !enableTowerOverlay;

        if (Settings.ToggleMapOverlay.JustPressed())
            enableMapOverlay = !enableMapOverlay;

        if (Settings.ToggleMapRendering.JustPressed())
        {
            // Flip the manual toggle and immediately apply, since ToggleMapRendering
            // is only called reactively elsewhere (on placement state changes).
            enableMapRendering = !enableMapRendering;
            ToggleMapRendering(enableMapRendering);
        }

        // --- Resolve final per-component state ---
        // Each component is active if placement mode wants it OR the user manually enabled it.
        // Map rendering is the inverse: it's disabled when placement mode is active OR
        // when the user has manually toggled it off.
        bool towerOverlayActive = shouldBeActive || enableTowerOverlay;
        bool mapOverlayActive = shouldBeActive || enableMapOverlay;
        bool mapRenderingOn = !shouldBeActive && enableMapRendering;

        towerManager.Update(towerOverlayActive);
        mapManager.Update(mapOverlayActive);

        // Only call ToggleMapRendering when the desired state actually changes,
        // to avoid hammering the renderer every frame.
        if (mapRenderingOn != lastMapRenderingOn)
        {
            ToggleMapRendering(mapRenderingOn);
            lastMapRenderingOn = mapRenderingOn;
        }

        wasPlacing = isPlacing;

        // --- Other debug overlay hotkeys ---
        if (Settings.ToggleBloonOverlay.JustPressed())
            enableBloons = !enableBloons;

        if (Settings.ToggleProjectileOverlay.JustPressed())
            enableProjectiles = !enableProjectiles;

        if (Settings.TogglePathOverlay.JustPressed())
            enablePaths = !enablePaths;

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
