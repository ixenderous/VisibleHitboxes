using System;
using System.IO;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VisibleHitboxes;

public static class SaveSystem
{
    private static string _saveFileLocation = "";
    public static bool IsAllEnabled = true;
    public static bool AreTowersEnabled = true;
    public static bool AreProjectilesEnabled = true;
    public static bool AreBloonsEnabled = true;
    public static bool IsMapEnabled = true;
    public static bool AreHitboxesTransparent = true;
    public static float Transparency = VisibleHitboxes.DefaultTransparency;
    
    public static bool IsEverythingEnabled()
    {
        return AreTowersEnabled && AreProjectilesEnabled && AreBloonsEnabled && IsMapEnabled;
    }
    public static void LoadSaveFile()
    {
        try
        {
            if (_saveFileLocation == "")
            {
                const string folder = "\\BloonsTD6 Mod Helper\\Mod Saves";
                _saveFileLocation = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)?.FullName + folder;
                Directory.CreateDirectory(_saveFileLocation);
            }
            const string saveFile = "VisibleHitboxes.json";
            
            var fileName = _saveFileLocation + "\\" + saveFile;
            
            if (!File.Exists(fileName)) return;
            var json = JObject.Parse(File.ReadAllText(fileName));
            foreach (var (name, token) in json)
            {
                if (token == null) continue;
                
                switch (name)
                {
                    case "IsAllEnabled":
                        IsAllEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreTowersEnabled":
                        AreTowersEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreProjectilesEnabled":
                        AreProjectilesEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreBloonsEnabled":
                        AreBloonsEnabled = bool.Parse(token.ToString());
                        break;
                    case "IsMapEnabled":
                        IsMapEnabled = bool.Parse(token.ToString());
                        break;
                    case "AreHitboxesTransparent":
                        AreHitboxesTransparent = bool.Parse(token.ToString());
                        break;
                    case "Transparency":
                        Transparency = float.Parse(token.ToString());
                        break;
                }
            }
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
        }
    }

    public static void UpdateSaveFile()
    {
        if (_saveFileLocation == "")
        {
            const string folder = "\\BloonsTD6 Mod Helper\\Mod Saves";
            _saveFileLocation = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)?.FullName + folder;
            Directory.CreateDirectory(_saveFileLocation);
        }
        const string saveFile = "VisibleHitboxes.json";
        
        var json = new JObject
        {
            ["IsAllEnabled"] = IsAllEnabled,
            ["AreTowersEnabled"] = AreTowersEnabled,
            ["AreProjectilesEnabled"] = AreProjectilesEnabled,
            ["AreBloonsEnabled"] = AreBloonsEnabled,
            ["IsMapEnabled"] = IsMapEnabled,
            ["AreHitboxesTransparent"] = AreHitboxesTransparent,
            ["Transparency"] = Transparency
        };
        
        File.WriteAllText(_saveFileLocation + "\\" + saveFile, json.ToString(Formatting.Indented));
    }
}