using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;
using UnityEngine;

namespace VisibleHitboxes
{
    public class Settings : ModSettings
    {
        public static readonly ModSettingHotkey ForceHitboxesToggle = new(KeyCode.Backslash)
        {
            description = "Toggle for force hitbox rendering even when not placing anything."
        };
    }
}
