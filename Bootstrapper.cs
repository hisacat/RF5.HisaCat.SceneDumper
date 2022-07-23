using System;
using HarmonyLib;
using UnityEngine;

namespace RF5.HisaCat.SceneDumper
{
    public class Bootstrapper : MonoBehaviour
    {
        private static SceneDumper instance_SceneDumper = null;

        public Bootstrapper(IntPtr intPtr) : base(intPtr) { }

        [HarmonyPostfix]
        public static void Update()
        {
            if (instance_SceneDumper == null)
            {
                BepInExLoader.log.LogMessage("[SceneDumper] Initializing...");
                GameObject containerObj = null;
                try
                {
                    containerObj = new GameObject("#SceneDumper#");
                    DontDestroyOnLoad(containerObj);
                    //instance_SceneDumper = new SceneDumper(obj.AddComponent(UnhollowerRuntimeLib.Il2CppType.Of<SceneDumper>()).Pointer);
                    instance_SceneDumper = containerObj.AddComponent<SceneDumper>();

                    if (instance_SceneDumper != null)
                    {
                        BepInExLoader.log.LogMessage("[SceneDumper] SceneDumper created!");
                    }
                    else
                    {
                        if (containerObj != null)
                        {
                            Destroy(containerObj);
                            containerObj = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    BepInExLoader.log.LogMessage($"[SceneDumper] Initialized faled. {e}");

                    if (containerObj != null)
                    {
                        Destroy(containerObj);
                        containerObj = null;
                    }
                }
            }
        }
    }
}
