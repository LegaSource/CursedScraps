using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalLib.Modules;
using BepInEx.Logging;
using CursedScraps.Patches;
using System.Collections.Generic;
using CursedScraps.Managers;
using CursedScraps.Values;
using CursedScraps.Patches.ModsPatches;
using System;
using CursedScraps.Behaviours.Items;
using CursedScraps.Behaviours.Curses;
using CursedScraps.CustomInputs;

namespace CursedScraps
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CursedScraps : BaseUnityPlugin
    {
        private const string modGUID = "Lega.CursedScraps";
        private const string modName = "Cursed Scraps";
        private const string modVersion = "2.0.7";

        private readonly Harmony harmony = new Harmony(modGUID);
        private readonly static AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "cursedscraps"));
        internal static ManualLogSource mls;
        public static ConfigFile configFile;

        public static List<CustomItem> customItems = new List<CustomItem>();
        public static List<CurseEffect> curseEffects = new List<CurseEffect>();
        public static GameObject managerPrefab = NetworkPrefabs.CreateNetworkPrefab("CursedScrapsNetworkManager");
        public static GameObject curseParticle;
        public static GameObject hotParticle;
        public static GameObject coldParticle;
        public static Material wallhackShader;

        public void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource("CursedScraps");
            configFile = Config;
            ConfigManager.Load();
            curseEffects = ConfigManager.GetCurseEffectsFromConfig();
            _ = CommunicationInputs.Instance;

            LoadManager();
            NetcodePatcher();
            LoadItems();
            LoadParticles();
            LoadShaders();

            harmony.PatchAll(typeof(IngamePlayerSettingsPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            PatchOtherMods(harmony);
        }

        public static void LoadManager()
        {
            Utilities.FixMixerGroups(managerPrefab);
            managerPrefab.AddComponent<CursedScrapsNetworkManager>();
        }

        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        public static void LoadItems()
        {
            customItems = new List<CustomItem>
            {
                new CustomItem(ConfigManager.isHolyWater.Value, typeof(HolyWater), bundle.LoadAsset<Item>("Assets/HolyWater/HolyWaterItem.asset"), true, ConfigManager.minHolyWater.Value, ConfigManager.maxHolyWater.Value, ConfigManager.holyWaterRarity.Value),
                new CustomItem(true, typeof(OldScroll), bundle.LoadAsset<Item>("Assets/OldScroll/OldScrollItem.asset"), ConfigManager.isOldScrollSpawnable.Value, ConfigManager.minOldScroll.Value, ConfigManager.maxOldScroll.Value, ConfigManager.oldScrollRarity.Value)
            };

            foreach (CustomItem customItem in customItems)
            {
                if (customItem.IsEnabled)
                {
                    var script = customItem.Item.spawnPrefab.AddComponent(customItem.Type) as PhysicsProp;
                    script.grabbable = true;
                    script.grabbableToEnemies = true;
                    script.itemProperties = customItem.Item;

                    NetworkPrefabs.RegisterNetworkPrefab(customItem.Item.spawnPrefab);
                    Utilities.FixMixerGroups(customItem.Item.spawnPrefab);
                    Items.RegisterItem(customItem.Item);
                }
            }
        }

        public static void LoadParticles()
        {
            curseParticle = bundle.LoadAsset<GameObject>("Assets/Particles/CurseParticle.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(curseParticle);
            Utilities.FixMixerGroups(curseParticle);

            hotParticle = bundle.LoadAsset<GameObject>("Assets/Particles/HotParticle.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(hotParticle);
            Utilities.FixMixerGroups(hotParticle);

            coldParticle = bundle.LoadAsset<GameObject>("Assets/Particles/ColdParticle.prefab");
            NetworkPrefabs.RegisterNetworkPrefab(coldParticle);
            Utilities.FixMixerGroups(coldParticle);
        }

        public static void LoadShaders()
        {
            wallhackShader = bundle.LoadAsset<Material>("Assets/Shaders/WallhackMaterial.mat");
        }

        public static void PatchOtherMods(Harmony harmony)
        {
            Type capsuleHoiPoiClass = Type.GetType("AddonFusion.Behaviours.CapsuleHoiPoi, AddonFusion");
            if (capsuleHoiPoiClass != null)
            {
                harmony.Patch (
                    AccessTools.Method(capsuleHoiPoiClass, "SetComponentClientRpc"),
                    prefix: new HarmonyMethod(typeof(AddonFusionPatch).GetMethod("PreGrabObject"))
                );
            }
        }
    }
}
