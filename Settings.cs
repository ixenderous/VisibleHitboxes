using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;

namespace VisibleHitboxes
{
    public class Settings : ModSettings
    {
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
            icon = VanillaSprites.SuperMonkeyFanClubUpgradeIcon
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
            icon = VanillaSprites.MetropolisUpgradeIcon
        };

        public static readonly ModSettingHotkey ToggleTransparency = new(KeyCode.D, HotkeyModifier.Shift)
        {
            category = Hotkeys,
            icon = VanillaSprites.ShimmerUpgradeIcon
        };
    }
}
