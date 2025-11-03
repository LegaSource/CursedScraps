using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CursedScraps.Behaviours.Items;
using CursedScraps.Managers;
using CursedScraps.Patches;
using CursedScraps.Patches.ModsPatches;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using static LegaFusionCore.Registries.LFCSpawnableItemRegistry;

namespace CursedScraps;

[BepInPlugin(modGUID, modName, modVersion)]
public class CursedScraps : BaseUnityPlugin
{
    internal const string modGUID = "Lega.CursedScraps";
    internal const string modName = "Cursed Scraps";
    internal const string modVersion = "3.0.0";

    private readonly Harmony harmony = new Harmony(modGUID);
    private static readonly AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "cursedscraps"));
    internal static ManualLogSource mls;
    public static ConfigFile configFile;

    public static GameObject managerPrefab = NetworkPrefabs.CreateNetworkPrefab("CursedScrapsNetworkManager");

    // Shaders
    public static Material cursedShader;

    public void Awake()
    {
        mls = BepInEx.Logging.Logger.CreateLogSource("CursedScraps");
        configFile = Config;
        ConfigManager.Load();
        ConfigManager.RegisterCursesFromConfig();

        LoadManager();
        NetcodePatcher();
        LoadItems();
        LoadShaders();

        harmony.PatchAll(typeof(IngamePlayerSettingsPatch));
        harmony.PatchAll(typeof(StartOfRoundPatch));
        harmony.PatchAll(typeof(RoundManagerPatch));
        harmony.PatchAll(typeof(BeltBagInventoryUIPatch));
        harmony.PatchAll(typeof(BeltBagItemPatch));
        harmony.PatchAll(typeof(HUDManagerPatch));
        harmony.PatchAll(typeof(PlayerControllerBPatch));
        harmony.PatchAll(typeof(EnemyAIPatch));
        PatchOtherMods(harmony);
    }

    public static void LoadManager()
    {
        Utilities.FixMixerGroups(managerPrefab);
        _ = managerPrefab.AddComponent<CursedScrapsNetworkManager>();
    }

    private static void NetcodePatcher()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (Type type in types)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (MethodInfo method in methods)
            {
                object[] attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length == 0) continue;
                _ = method.Invoke(null, null);
            }
        }
    }

    public static void LoadItems()
    {
        if (ConfigManager.isHolyWater.Value)
            Add(typeof(HolyWater), bundle.LoadAsset<Item>("Assets/HolyWater/HolyWaterItem.asset"), ConfigManager.minHolyWater.Value, ConfigManager.maxHolyWater.Value, ConfigManager.holyWaterRarity.Value);
    }

    public static void LoadShaders() => cursedShader = bundle.LoadAsset<Material>("Assets/Shaders/CursedMaterial.mat");

    public static void PatchOtherMods(Harmony harmony)
    {
        BagConfigPatch(harmony);
        ToggleMutePatch(harmony);
    }

    public static void BagConfigPatch(Harmony harmony)
    {
        Type beltBagPatchClass = Type.GetType("BagConfig.Patches.BeltBagPatch, BagConfig");
        if (beltBagPatchClass == null) return;

        _ = harmony.Patch(
            AccessTools.Method(beltBagPatchClass, "EmptyBagCoroutine"),
            prefix: new HarmonyMethod(typeof(BagConfigPatch).GetMethod("EmptyBag"))
        );
    }

    public static void ToggleMutePatch(Harmony harmony)
    {
        Type toggleMutePatchClass = Type.GetType("ToggleMute.ToggleMuteManager, ToggleMute");
        if (toggleMutePatchClass == null) return;

        _ = harmony.Patch(
            AccessTools.Method(toggleMutePatchClass, "OnToggleMuteKeyPressed"),
            prefix: new HarmonyMethod(typeof(ToggleMutePatch).GetMethod("ToggleMute"))
        );
    }
}
