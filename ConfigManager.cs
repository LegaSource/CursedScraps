using BepInEx.Configuration;
using System.Collections.Generic;

namespace CursedScraps
{
    internal class ConfigManager
    {
        // GLOBAL
        public static ConfigEntry<int> globalChance;
        public static ConfigEntry<bool> globalPrevent;
        // ANTI-CURSE PILLS
        public static ConfigEntry<bool> isPills;
        public static ConfigEntry<float> pillsMultiplier;
        // INHIBITION
        public static ConfigEntry<bool> isInhibition;
        public static ConfigEntry<float> inhibitionMultiplier;
        public static ConfigEntry<string> inhibitionWeight;
        // CONFUSION
        public static ConfigEntry<bool> isConfusion;
        public static ConfigEntry<float> confusionMultiplier;
        public static ConfigEntry<string> confusionWeight;
        // CAPTIVE
        public static ConfigEntry<bool> isCaptive;
        public static ConfigEntry<float> captiveMultiplier;
        public static ConfigEntry<string> captiveWeight;
        // BLURRY
        public static ConfigEntry<bool> isBlurry;
        public static ConfigEntry<float> blurryMultiplier;
        public static ConfigEntry<string> blurryWeight;
        // MUTE
        public static ConfigEntry<bool> isMute;
        public static ConfigEntry<float> muteMultiplier;
        public static ConfigEntry<string> muteWeight;
        // DEAFNESS
        public static ConfigEntry<bool> isDeafness;
        public static ConfigEntry<float> deafnessMultiplier;
        public static ConfigEntry<string> deafnessWeight;
        // ERRANT
        public static ConfigEntry<bool> isErrant;
        public static ConfigEntry<float> errantMultiplier;
        public static ConfigEntry<string> errantWeight;
        // PARALYSIS
        public static ConfigEntry<bool> isParalysis;
        public static ConfigEntry<float> paralysisMultiplier;
        public static ConfigEntry<string> paralysisWeight;
        public static ConfigEntry<float> paralysisTime;
        // SHADOW
        public static ConfigEntry<bool> isShadow;
        public static ConfigEntry<float> shadowMultiplier;
        public static ConfigEntry<string> shadowWeight;

        internal static void Load()
        {
            // GLOBAL
            globalChance = CursedScraps.configFile.Bind<int>("_Global_", "Chance", 25, "Overall chance of scrap appearing.\nThis value does not replace the chance of appearance for each curse; the latter are considered after the overall chance to determine which curse is chosen.");
            globalPrevent = CursedScraps.configFile.Bind<bool>("_Global_", "Preventing Settings Changes", true, "Set to false to allow players to change their settings when a curse modifying controls is active.\nThis configuration is mainly there in case of unforeseen bugs or potential incompatibility.");
            // ANTI-CURSE PILLS
            isPills = CursedScraps.configFile.Bind<bool>(Constants.CURSE_PILLS, "Enable", true, "Is " + Constants.CURSE_PILLS + " item enabled?\nConsumable that removes all active curses on the player.");
            pillsMultiplier = CursedScraps.configFile.Bind<float>(Constants.CURSE_PILLS, "Multiplier", 2f, "Number of times the " + Constants.CURSE_PILLS + " item appears per cursed scrap.");
            // INHIBITION
            isInhibition = CursedScraps.configFile.Bind<bool>(Constants.INHIBITION, "Enable", true, "Is " + Constants.INHIBITION + " curse enabled?\nPrevents the player from jumping and crouching.");
            inhibitionMultiplier = CursedScraps.configFile.Bind<float>(Constants.INHIBITION, "Multiplier", 1.7f, "Value multiplier for scraps with the " + Constants.INHIBITION + " curse.");
            inhibitionWeight = CursedScraps.configFile.Bind<string>(Constants.INHIBITION, "Chance", "default:1,Experimentation:0", "Spawn chance of a scrap with the " + Constants.INHIBITION + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // CONFUSION
            isConfusion = CursedScraps.configFile.Bind<bool>(Constants.CONFUSION, "Enable", true, "Is " + Constants.CONFUSION + " curse enabled?\nReverses movement controls and jump/crouch keys for the player.");
            confusionMultiplier = CursedScraps.configFile.Bind<float>(Constants.CONFUSION, "Multiplier", 1.6f, "Value multiplier for scraps with the " + Constants.CONFUSION + " curse.");
            confusionWeight = CursedScraps.configFile.Bind<string>(Constants.CONFUSION, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.CONFUSION + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // CAPTIVE
            isCaptive = CursedScraps.configFile.Bind<bool>(Constants.CAPTIVE, "Enable", true, "Is " + Constants.CAPTIVE + " curse enabled?\nPrevents the item from being thrown until it has been brought back to the ship.");
            captiveMultiplier = CursedScraps.configFile.Bind<float>(Constants.CAPTIVE, "Multiplier", 1.3f, "Value multiplier for scraps with the " + Constants.CAPTIVE + " curse.");
            captiveWeight = CursedScraps.configFile.Bind<string>(Constants.CAPTIVE, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.CAPTIVE + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // BLURRY
            isBlurry = CursedScraps.configFile.Bind<bool>(Constants.BLURRY, "Enable", true, "Is " + Constants.BLURRY + " curse enabled?\nReduces visual clarity on the player's camera.");
            blurryMultiplier = CursedScraps.configFile.Bind<float>(Constants.BLURRY, "Multiplier", 1.4f, "Value multiplier for scraps with the " + Constants.BLURRY + " curse.");
            blurryWeight = CursedScraps.configFile.Bind<string>(Constants.BLURRY, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.BLURRY + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // MUTE
            isMute = CursedScraps.configFile.Bind<bool>(Constants.MUTE, "Enable", true, "Is " + Constants.MUTE + " curse enabled?\nMutes the player's microphone.");
            muteMultiplier = CursedScraps.configFile.Bind<float>(Constants.MUTE, "Multiplier", 1.2f, "Value multiplier for scraps with the " + Constants.MUTE + " curse.");
            muteWeight = CursedScraps.configFile.Bind<string>(Constants.MUTE, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.MUTE + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // DEAFNESS
            isDeafness = CursedScraps.configFile.Bind<bool>(Constants.DEAFNESS, "Enable", true, "Is " + Constants.DEAFNESS + " curse enabled?\nRemoves the player's sound.");
            deafnessMultiplier = CursedScraps.configFile.Bind<float>(Constants.DEAFNESS, "Multiplier", 1.7f, "Value multiplier for scraps with the " + Constants.DEAFNESS + " curse.");
            deafnessWeight = CursedScraps.configFile.Bind<string>(Constants.DEAFNESS, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.DEAFNESS + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // ERRANT
            isErrant = CursedScraps.configFile.Bind<bool>(Constants.ERRANT, "Enable", true, "Is " + Constants.ERRANT + " curse enabled?\nTeleports the player randomly when an item is picked up or placed down.");
            errantMultiplier = CursedScraps.configFile.Bind<float>(Constants.ERRANT, "Multiplier", 1.5f, "Value multiplier for scraps with the " + Constants.ERRANT + " curse.");
            errantWeight = CursedScraps.configFile.Bind<string>(Constants.ERRANT, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.ERRANT + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // PARALYSIS
            isParalysis = CursedScraps.configFile.Bind<bool>(Constants.PARALYSIS, "Enable", true, "Is " + Constants.PARALYSIS + " curse enabled?\nParalyzes the player when scanning an enemy.");
            paralysisMultiplier = CursedScraps.configFile.Bind<float>(Constants.PARALYSIS, "Multiplier", 1.2f, "Value multiplier for scraps with the " + Constants.PARALYSIS + " curse.");
            paralysisWeight = CursedScraps.configFile.Bind<string>(Constants.PARALYSIS, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.PARALYSIS + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            paralysisTime = CursedScraps.configFile.Bind<float>(Constants.PARALYSIS, "Time", 5f, "Player paralysis time in seconds.");
            // SHADOW
            isShadow = CursedScraps.configFile.Bind<bool>(Constants.SHADOW, "Enable", true, "Is " + Constants.SHADOW + " curse enabled?\nAll enemies are invisible by default (their sound is still active), scanning reveals them.");
            shadowMultiplier = CursedScraps.configFile.Bind<float>(Constants.SHADOW, "Multiplier", 1.3f, "Value multiplier for scraps with the " + Constants.SHADOW + " curse.");
            shadowWeight = CursedScraps.configFile.Bind<string>(Constants.SHADOW, "Chance", "default:1", "Spawn chance of a scrap with the " + Constants.SHADOW + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        }

        internal static List<CurseEffect> GetCurseEffectsFromConfig()
        {
            List<CurseEffect> curseEffects = new List<CurseEffect>();
            if (isInhibition.Value) curseEffects.Add(new CurseEffect(Constants.INHIBITION, inhibitionMultiplier.Value, inhibitionWeight.Value));
            if (isConfusion.Value) curseEffects.Add(new CurseEffect(Constants.CONFUSION, confusionMultiplier.Value, confusionWeight.Value));
            if (isCaptive.Value) curseEffects.Add(new CurseEffect(Constants.CAPTIVE, captiveMultiplier.Value, captiveWeight.Value));
            if (isBlurry.Value) curseEffects.Add(new CurseEffect(Constants.BLURRY, blurryMultiplier.Value, blurryWeight.Value));
            if (isMute.Value) curseEffects.Add(new CurseEffect(Constants.MUTE, muteMultiplier.Value, muteWeight.Value));
            if (isDeafness.Value) curseEffects.Add(new CurseEffect(Constants.DEAFNESS, deafnessMultiplier.Value, deafnessWeight.Value));
            if (isErrant.Value) curseEffects.Add(new CurseEffect(Constants.ERRANT, errantMultiplier.Value, errantWeight.Value));
            if (isParalysis.Value) curseEffects.Add(new CurseEffect(Constants.PARALYSIS, paralysisMultiplier.Value, paralysisWeight.Value));
            if (isShadow.Value) curseEffects.Add(new CurseEffect(Constants.SHADOW, shadowMultiplier.Value, shadowWeight.Value));
            return curseEffects;
        }
    }
}
