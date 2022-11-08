using BTD_Mod_Helper;
using HitboxMod;
using MelonLoader;
using Main = HitboxMod.Main;

[assembly: MelonInfo(typeof(Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HitboxMod;

public class Main : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<Main>("TemplateMod loaded!");
    }
}