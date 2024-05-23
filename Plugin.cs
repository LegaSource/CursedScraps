using BepInEx;
using BepInEx.Configuration;
using CursedScraps.Patches;
using HarmonyLib;
using System.Collections.Generic;

namespace CursedScraps
{
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class CursedScraps : BaseUnityPlugin
    {
        private const string modGUID = "Lega.CursedScraps";
        private const string modName = "Cursed Scraps";
        private const string modVersion = "1.0.8";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static CursedScraps Instance;

        public static ConfigFile configFile;
        public static List<CurseEffect> curseEffects = new List<CurseEffect>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            configFile = Config;
            ConfigManager.Load();
            curseEffects = ConfigManager.GetCurseEffectsFromConfig();

            harmony.PatchAll(typeof(CursedScraps));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(ItemManagerPatch));
            harmony.PatchAll(typeof(PlayerManagerPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            harmony.PatchAll(typeof(MenuManagerPatch));
        }
    }
}
