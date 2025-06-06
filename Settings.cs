using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;

namespace VisibleHitboxes
{
    public class Settings : ModSettings
    {
        public static readonly ModSettingCategory Hotkeys = new("Hotkeys") { collapsed = true };
        public static readonly ModSettingCategory Toggles = new("Toggles") { collapsed = true };

        #region Hotkeys
        public static readonly ModSettingHotkey ToggleAll = new(KeyCode.A, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle All Overlays"
        };

        public static readonly ModSettingHotkey ToggleTransparency = new(KeyCode.D, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle Transparency"
        };

        public static readonly ModSettingHotkey ToggleTowerHitboxes = new(KeyCode.T, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle Tower Hitboxes"
        };

        public static readonly ModSettingHotkey ToggleProjectileHitboxes = new(KeyCode.P, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle Projectile Hitboxes"
        };

        public static readonly ModSettingHotkey ToggleBloonHitboxes = new(KeyCode.B, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle Bloon Hitboxes"
        };

        public static readonly ModSettingHotkey ToggleMapOverlay = new(KeyCode.M, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle Map Overlay"
        };

        public static readonly ModSettingHotkey TogglePathsOverlay = new(KeyCode.L, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle path Overlay"
        };

        public static readonly ModSettingHotkey ToggleMapRendering = new(KeyCode.M, HotkeyModifier.Alt)
        {
            category = Hotkeys,
            displayName = "Toggle Map Rendering"
        };

        //public static readonly ModSettingHotkey DebugHotkey = new(KeyCode.None)
        //{
        //    category = Hotkeys
        //};
        #endregion

        #region Toggles
        public static readonly ModSettingBool UseTransparency = new(true)
        {
            category = Toggles,
            displayName = "Use Transparency"
        };

        public static readonly ModSettingBool ShowTowerHitboxes = new(true)
        {
            category = Toggles,
            displayName = "Show Tower Hitboxes"
        };

        public static readonly ModSettingBool ShowProjectileHitboxes = new(true)
        {
            category = Toggles,
            displayName = "Show Projectile Hitboxes"
        };

        public static readonly ModSettingBool ShowBloonHitboxes = new(true)
        {
            category = Toggles,
            displayName = "Show Bloon Hitboxes"
        };

        public static readonly ModSettingBool ShowMapOverlay = new(true)
        {
            category = Toggles,
            displayName = "Show Map Overlay"
        };

        public static readonly ModSettingBool ShowPathOverlay = new(true)
        {
            category = Toggles,
            displayName = "Show Path Overlay"
        };

        public static readonly ModSettingBool RenderMap = new(true)
        {
            category = Toggles,
            displayName = "Render Map"
        };
        #endregion

        public static readonly ModSettingFloat TransparencyLevel = new(0.5f)
        {
            displayName = "Transparency Level",
            min = 0f,
            max = 1f,
            slider = true,
        };

        public static bool IsEverythingEnabled()
        {
            return ShowTowerHitboxes && ShowProjectileHitboxes && ShowBloonHitboxes && ShowMapOverlay && ShowPathOverlay;
        }

        public static float GetTransparency()
        {
            return UseTransparency ? TransparencyLevel : 1.0f;
        }
    }
}
