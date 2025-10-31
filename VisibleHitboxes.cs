using System;
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
using Object = UnityEngine.Object;

[assembly:
    MelonInfo(typeof(VisibleHitboxes.VisibleHitboxes), ModHelperData.Name, ModHelperData.Version,
        ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace VisibleHitboxes;

public class VisibleHitboxes : BloonsTD6Mod
{
    private bool isInGame;

    private readonly List<HitboxManager> managers;
    
    private readonly TowerHitboxManager towerManager;
    private readonly MapHitboxManager mapManager;

    public VisibleHitboxes()
    {
        towerManager = new(Settings.ShowTowerHitboxes);
        mapManager = new(Settings.ShowMapOverlay);
        managers = new List<HitboxManager>
        {
            towerManager,
            mapManager
        };
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
            Settings.ShowMapOverlay.SetValue(newState);
        }

        if (Settings.ToggleTowerHitboxes.JustPressed())
            Settings.ShowTowerHitboxes.SetValue(!Settings.ShowTowerHitboxes);

        if (Settings.ToggleMapOverlay.JustPressed())
            Settings.ShowMapOverlay.SetValue(!Settings.ShowMapOverlay);

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

    public static GameObject GetCircleObject()
    {
        var rendererGo = new GameObject();

        // Components for rendering a filled shape
        rendererGo.AddComponent<MeshFilter>();
        var meshRenderer = rendererGo.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));
        
        // Generate the circle mesh data
        const int segments = 50;
        const float radius = 0.5f;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        // Center vertex
        vertices.Add(new Vector3(0f, 0f, 0f));

        // Perimeter vertices
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments) * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            vertices.Add(new Vector3(x, 0f, z));
        }

        // Create triangles (Fan pattern)
        for (int i = 0; i < segments; i++)
        {
            // Triangle connects: Center (0), current point (i+1), next point (i+2)
            triangles.Add(0); // Center point
            triangles.Add(i + 1); // Current perimeter point
            triangles.Add(i + 2); // Next perimeter point (or wraps to the start)
        }

        // Apply mesh data
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        rendererGo.GetComponent<MeshFilter>().mesh = mesh;

        return rendererGo;
    }

    public static GameObject GetSquareObject()
    {
        var rendererGo = new GameObject();

        // Components for rendering a filled shape
        rendererGo.AddComponent<MeshFilter>();
        var meshRenderer = rendererGo.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

        // Vertices (4 corners)
        var vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0f, -0.5f), // Bottom-Left (0)
            new Vector3(0.5f, 0f, -0.5f), // Bottom-Right (1)
            new Vector3(0.5f, 0f, 0.5f), // Top-Right (2)
            new Vector3(-0.5f, 0f, 0.5f) // Top-Left (3)
        };

        // Triangles (Two triangles to make a square/quad)
        var triangles = new int[]
        {
            0, 2, 1, // First triangle: 0 -> 2 -> 1
            0, 3, 2 // Second triangle: 0 -> 3 -> 2
        };

        // Apply mesh data
        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        rendererGo.GetComponent<MeshFilter>().mesh = mesh;

        return rendererGo;
    }

    public static Material GetMaterial()
    {
        return new Material(Shader.Find("Hidden/Internal-Colored"));
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