using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;

namespace VisibleHitboxes
{
    public class Settings : ModSettings
    {
        public static readonly ModSettingCategory Hotkeys = new("Hotkeys") { collapsed = true };
        // public static readonly ModSettingCategory Toggles = new("Toggles") { collapsed = true};

        #region Hotkeys
        public static readonly ModSettingHotkey ForceHitboxesToggle = new(KeyCode.Backslash)
        {
            category = Hotkeys,
            description = "Toggle for force hitbox rendering even when not placing anything."
        };

        public static readonly ModSettingHotkey ToggleBloonHitboxes = new(KeyCode.B, Il2CppAssets.Scripts.Unity.UI_New.InGame.HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle Bloon Hitboxes"
        };

        public static readonly ModSettingHotkey ToggleProjectileHitboxes = new(KeyCode.P, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle Projectile Hitboxes"
        };

        public static readonly ModSettingHotkey TogglePathsOverlay = new(KeyCode.L, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            displayName = "Toggle path Overlay"
        };
        #endregion
    }
}
