using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;

namespace VisibleHitboxes
{
    public class Settings : ModSettings
    {
        public static readonly ModSettingBool AutoTogglePlacementMode = new(true)
        {
            description = "Automatically disable map rendering and enable tower and map hitbox overlays when placing towers"
        };

        public static readonly ModSettingCategory Hotkeys = new("Hotkeys") { collapsed = true };

        #region Hotkeys
        public static readonly ModSettingHotkey ForcePlacementMode = new(KeyCode.Backslash)
        {
            category = Hotkeys,
            description = "Toggle to disable map and enable map area and tower overlay rendering."
        };

        public static readonly ModSettingHotkey ToggleMapRendering = new(KeyCode.W, HotkeyModifier.Shift)
        {
            category = Hotkeys,
        };

        public static readonly ModSettingHotkey ToggleTowerOverlay = new(KeyCode.T, HotkeyModifier.Shift)
        {
            category = Hotkeys,
        };

        public static readonly ModSettingHotkey ToggleMapOverlay = new(KeyCode.M, HotkeyModifier.Shift)
        {
            category = Hotkeys,
        };

        public static readonly ModSettingHotkey ToggleBloonOverlay = new(KeyCode.B, HotkeyModifier.Shift)
        {
            category = Hotkeys,
        };

        public static readonly ModSettingHotkey ToggleProjectileOverlay = new(KeyCode.P, HotkeyModifier.Shift)
        {
            category = Hotkeys,
        };

        public static readonly ModSettingHotkey TogglePathOverlay = new(KeyCode.L, HotkeyModifier.Shift)
        {
            category = Hotkeys,
        };
        #endregion
    }
}
