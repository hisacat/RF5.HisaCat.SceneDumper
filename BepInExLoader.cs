using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnhollowerRuntimeLib;
using HarmonyLib;
using System.Linq;
using KeyCode = BepInEx.IL2CPP.UnityEngine.KeyCode;

namespace RF5.HisaCat.SceneDumper
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class BepInExLoader : BepInEx.IL2CPP.BasePlugin
    {
        public const string
            MODNAME = "SceneDumper",
            AUTHOR = "HisaCat",
            GUID = "RF5." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0.0";

        public static BepInEx.Logging.ManualLogSource log;
        public static ConfigEntry<string> shortCutConfig;
        public static List<KeyCode> shortCutKeys = null;
        public static ConfigEntry<bool> bIncludePath;
        public static ConfigEntry<bool> bDumpProperties;
        public static ConfigEntry<string> ignoreComponentTypesConfig;
        public static List<string> ignoreComponentTypesStr;
        public static ConfigEntry<string> componentWhitelistPropertiesConfig;
        public static Dictionary<string, List<string>> componentWhitelistPropertiesDic = null;
        public BepInExLoader()
        {
            log = Log;

            shortCutConfig = Config.Bind("Shortcuts",
                "Dump Scene",
                string.Join(" | ", new KeyCode[] { KeyCode.LeftControl, KeyCode.LeftShift, KeyCode.F1 }.Select(x => x.ToString())),
                new ConfigDescription("UnityEngine.KeyCode sets for dump scene (Combination with OR \'|\')\r\n" +
                "See KeyCodes at https://docs.bepinex.dev/master/api/BepInEx.IL2CPP.UnityEngine.KeyCode.html"));
            shortCutKeys = new List<KeyCode>();
            {
                var keysStr = shortCutConfig.Value.Split('|').Select(x => x.Replace(" ", ""));
                foreach (var keyStr in keysStr)
                {
                    KeyCode key;
                    if (System.Enum.TryParse(keyStr, out key))
                        shortCutKeys.Add(key);
                }
            }
            shortCutConfig.Value = string.Join(" | ", shortCutKeys.Select(x => x.ToString()));

            bIncludePath = Config.Bind("Options", "IncludePath", true, new ConfigDescription("Include gameobject's pathes in Hierarchy"));
            bDumpProperties = Config.Bind("Options", "DumpProperties", true, new ConfigDescription("dump component's properties"));

            ignoreComponentTypesConfig = Config.Bind("Options", "IgnoreComponentTypes", "", new ConfigDescription("ignore component types (FullName, Combination with OR \'|\')"));
            ignoreComponentTypesStr = new List<string>(ignoreComponentTypesConfig.Value.Split('|').Select(x => x.Replace(" ", "")));

            componentWhitelistPropertiesConfig = Config.Bind("Options", "ComponentWhitelistProperties", "", new ConfigDescription("whitelist for component property name (FullName:PropertyName, Combination with OR \'|\')"));
            {
                var values = new List<string>(componentWhitelistPropertiesConfig.Value.Split('|').Select(x => x.Replace(" ", "")));
                componentWhitelistPropertiesDic = new Dictionary<string, List<string>>();
                foreach (var value in values)
                {
                    var temp = value.Split(':');
                    if (temp.Length < 2) continue;

                    var componentType = temp[0];
                    var propertyName = temp[1];

                    if (componentWhitelistPropertiesDic.ContainsKey(componentType) == false)
                        componentWhitelistPropertiesDic.Add(componentType, new List<string>());
                    if(componentWhitelistPropertiesDic[componentType].Contains(propertyName) == false)
                        componentWhitelistPropertiesDic[componentType].Add(propertyName);
                }
            }


            BepInExLoader.log.LogMessage($"[SceneDumper] Shortcut: {shortCutConfig.Value}");
            BepInExLoader.log.LogMessage($"[SceneDumper] IncludePath: {bIncludePath.Value}");
            BepInExLoader.log.LogMessage($"[SceneDumper] DumpProperties: {bDumpProperties.Value}");
            BepInExLoader.log.LogMessage($"[SceneDumper] ignoreComponentTypes: {ignoreComponentTypesConfig.Value}");
        }

        public override void Load()
        {
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<Bootstrapper>();
                ClassInjector.RegisterTypeInIl2Cpp<SceneDumper>();
            }
            catch
            {
                log.LogError("[SceneDumper] FAILED to Register Il2Cpp Types!");
            }

            try
            {
                var harmony = new Harmony(GUID);

                var originalUpdate = AccessTools.Method(typeof(UnityEngine.UI.CanvasScaler), "Update");
                log.LogMessage("   Original Method: " + originalUpdate.DeclaringType.Name + "." + originalUpdate.Name);
                var postUpdate = AccessTools.Method(typeof(Bootstrapper), "Update");
                log.LogMessage("   Postfix Method: " + postUpdate.DeclaringType.Name + "." + postUpdate.Name);
                harmony.Patch(originalUpdate, postfix: new HarmonyMethod(postUpdate));
            }
            catch
            {
                log.LogError("[SceneDumper] Harmony - FAILED to Apply Patch's!");
            }
        }
    }
}
