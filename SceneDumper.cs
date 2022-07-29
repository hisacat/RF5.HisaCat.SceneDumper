using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppSystem.Reflection;
using Newtonsoft.Json;
using Input = BepInEx.IL2CPP.UnityEngine.Input;

namespace RF5.HisaCat.SceneDumper
{
    public class SceneDumper : MonoBehaviour
    {
        public SceneDumper(IntPtr ptr) : base(ptr) { }

        public void Awake() { }
        public void Start() { }

        public void Update()
        {
            if (BepInExLoader.shortCutKeys.Count <= 0)
                return;

            if (Event.current != null && Event.current.type == EventType.KeyDown)
            {
                if (BepInExLoader.shortCutKeys.All(x => Input.GetKeyInt(x)))
                {
                    DumpSceneData(
                        includePath: BepInExLoader.bIncludePath.Value,
                        dumpProperties: BepInExLoader.bDumpProperties.Value);
                    Event.current.Use();
                }
            }
        }

        [System.Serializable]
        public class SceneDumpData
        {
            public List<SceneData> Scenes;
        }
        [System.Serializable]
        public class SceneData
        {
            public string Name;
            public List<GameObjectData> RootGameObjects;
        }
        [System.Serializable]
        public class GameObjectData
        {
            public string Name;
            public string Path;
            public bool ActiveSelf;
            public bool ActiveInHierarchy;
            public DefinedComponentTypes.TransformData TransformData;
            public DefinedComponentTypes.RectTransformData RectTransformData;
            public List<ComponentData> ComponentDatas;

            public List<GameObjectData> Childs;
        }

        [System.Serializable]
        public class ComponentData
        {
            public string Type;
            public bool Enabled;
            public List<PropertyData> Properties;
        }
        [System.Serializable]
        public class PropertyData
        {
            public string Name;
            public string Type;
            public string Value;
        }
        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        private static void DumpSceneData(bool includePath, bool dumpProperties)
        {
            //Dump
            try
            {
                var sceneDump = new SceneDumpData();
                sceneDump.Scenes = new List<SceneData>();

                int sceneCount = SceneManager.sceneCount;
                for (int sceneIdx = 0; sceneIdx < sceneCount; sceneIdx++)
                {
                    var scene = SceneManager.GetSceneAt(sceneIdx);
                    var sceneData = new SceneData();
                    sceneData.Name = scene.name;
                    sceneData.RootGameObjects = new List<GameObjectData>();

                    var rootGameObjects = scene.GetRootGameObjects();
                    var rootGameObjectsCount = rootGameObjects.Length;
                    for (int rootObjIdx = 0; rootObjIdx < rootGameObjectsCount; rootObjIdx++)
                    {
                        var go = rootGameObjects[rootObjIdx];
                        var goData = new GameObjectData();
                        DumpGameObjectRecursive(go, goData, includePath, dumpProperties);
                        sceneData.RootGameObjects.Add(goData);
                    }

                    sceneDump.Scenes.Add(sceneData);
                }


                var json = JsonConvert.SerializeObject(sceneDump);
                string path = Path.GetTempFileName();
                File.WriteAllText(path, json);
                Process.Start("notepad.exe", path);

                BepInExLoader.log.LogMessage($"[SceneDumper] Scene dump succeed");
            }
            catch (Exception e)
            {
                BepInExLoader.log.LogError($"[SceneDumper] Dump failed: {e}");
            }
        }

        public static void DumpGameObjectRecursive(GameObject target, GameObjectData entry, bool includePath, bool dumpProperties)
        {
            entry.Name = target.name;
            var path = GetGameObjectPath(target);
            if (BepInExLoader.ignorePathsStr.Any(x => path.StartsWith(x)))
                return;

            if (includePath) entry.Path = path;
            entry.ActiveSelf = target.activeSelf;
            entry.ActiveInHierarchy = target.activeInHierarchy;
            entry.TransformData = null;
            entry.ComponentDatas = new List<ComponentData>();

            var components = target.GetComponents<Component>();
            int componentCount = components.Length;
            for (int i = 0; i < componentCount; i++)
            {
                var component = components[i];
                if (component == null) continue;

                var il2CppType = component.GetIl2CppType();

                //Dump definded component types
                if (DefinedComponentTypes.TransformData.IsTypeOf(il2CppType))
                {
                    entry.TransformData = new DefinedComponentTypes.TransformData();
                    entry.TransformData.Convert(component);
                    continue;
                }
                else if (DefinedComponentTypes.RectTransformData.IsTypeOf(il2CppType))
                {
                    entry.RectTransformData = new DefinedComponentTypes.RectTransformData();
                    entry.RectTransformData.Convert(component);
                    continue;
                }

                //Ignore specific component types
                if (BepInExLoader.ignoreComponentTypesConfig.Value.Contains(il2CppType.FullName))
                    continue;

                var whitelistPropertiesName = BepInExLoader.componentWhitelistPropertiesDic.ContainsKey(il2CppType.FullName) ? BepInExLoader.componentWhitelistPropertiesDic[il2CppType.FullName] : null;

                //Dump undefinded component type
                var componentData = new ComponentData();
                componentData.Type = il2CppType.FullName;
                if (il2CppType.IsAssignableFrom(UnhollowerRuntimeLib.Il2CppType.Of<Behaviour>()))
                    componentData.Enabled = (component as Behaviour).enabled;
                else
                    componentData.Enabled = true;

                if (dumpProperties)
                {
                    BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

                    componentData.Properties = new List<PropertyData>();
                    PropertyInfo[] properties = il2CppType.GetProperties(flags);
                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        if (whitelistPropertiesName != null)
                        {
                            if (whitelistPropertiesName.Contains(propertyInfo.Name) == false)
                                continue;
                        }

                        var propertyData = new PropertyData();
                        propertyData.Name = propertyInfo.Name;
                        propertyData.Type = propertyInfo.PropertyType.FullName;
                        try
                        {
                            var value = propertyInfo.GetValue(component);
                            propertyData.Value = value == null ? "null" : value.ToString();
                        }
                        catch
                        {
                            propertyData.Value = "#ERROR#";
                        }
                        componentData.Properties.Add(propertyData);
                    }
                }

                entry.ComponentDatas.Add(componentData);
            }

            entry.Childs = new List<GameObjectData>();
            int childCount = target.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = target.transform.GetChild(i);
                var childEntry = new GameObjectData();
                DumpGameObjectRecursive(child.gameObject, childEntry, includePath, dumpProperties);
                entry.Childs.Add(childEntry);
            }
        }
    }
}
