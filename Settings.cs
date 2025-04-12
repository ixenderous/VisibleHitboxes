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

        public static readonly ModSettingCategory Toggles = new("Toggles")
        {
            collapsed = false
        };

        public static readonly ModSettingCategory Appearance = new("Appearance")
        {
            collapsed = false
        };

        #region Hotkeys
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
        #endregion

        #region Bools
        public static readonly ModSettingBool ShowTowers = new(true)
        {
            category = Toggles,
            icon = VanillaSprites.SuperMonkeyFanClubUpgradeIcon,
            displayName = "Show Tower Hitboxes"
        };

        public static readonly ModSettingBool ShowProjectiles = new(true)
        {
            category = Toggles,
            icon = VanillaSprites.AirburstDartsUpgradeIcon,
            displayName = "Show Projectile Hitboxes"
        };

        public static readonly ModSettingBool ShowBloons = new(true)
        {
            category = Toggles,
            icon = VanillaSprites.FullMetalJacketUpgradeIcon,
            displayName = "Show Bloon Hitboxes"
        };

        public static readonly ModSettingBool ShowMapCollision = new(true)
        {
            category = Toggles,
            icon = VanillaSprites.MetropolisUpgradeIcon,
            displayName = "Show Map Collision"
        };

        public static readonly ModSettingBool UseTransparency = new(true)
        {
            category = Appearance,
            icon = VanillaSprites.ShimmerUpgradeIcon,
            displayName = "Use Transparency"
        };
        #endregion

        // Transparency slider
        public static readonly ModSettingFloat TransparencyLevel = new(0.5f)
        {
            category = Appearance,
            displayName = "Transparency Level",
            min = 0.01f,
            max = 1.0f,
            slider = true,
            icon = VanillaSprites.ShimmerUpgradeIcon,
            description = "Adjust the transparency level of the hitboxes (0.1 = mostly transparent, 1.0 = fully opaque)"
        };

        // Helper method to check if everything is enabled
        public static bool IsEverythingEnabled()
        {
            return ShowTowers && ShowProjectiles && ShowBloons && ShowMapCollision;
        }

        // Helper method to get the current transparency value
        public static float GetTransparency()
        {
            return UseTransparency ? TransparencyLevel : 1.0f;
        }
    }
}