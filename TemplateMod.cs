using MelonLoader;
using BTD_Mod_Helper;
using TemplateMod;

[assembly: MelonInfo(typeof(TemplateMod.TemplateMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace TemplateMod;

public class TemplateMod : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<TemplateMod>("TemplateMod loaded!");
    }
}