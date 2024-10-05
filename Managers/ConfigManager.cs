﻿using BepInEx.Configuration;
using CursedScraps.Behaviours;
using System.Collections.Generic;

namespace CursedScraps.Managers
{
    internal class ConfigManager
    {
        // GLOBAL
        public static ConfigEntry<string> globalChance;
        public static ConfigEntry<bool> globalPrevent;
        public static ConfigEntry<string> scrapExclusions;
        public static ConfigEntry<bool> isCurseInfoOn;
        // HIDE MECHANIC
        public static ConfigEntry<bool> isRedScanOn;
        public static ConfigEntry<bool> isParticleOn;
        public static ConfigEntry<bool> isHideName;
        public static ConfigEntry<bool> isHideValue;
        // PENALTY MECHANIC
        public static ConfigEntry<string> penaltyMode;
        public static ConfigEntry<int> penaltyCounter;
        // ANTI-CURSE PILLS
        public static ConfigEntry<bool> isHolyWater;
        public static ConfigEntry<int> holyWaterRarity;
        public static ConfigEntry<int> maxHolyWater;
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
        public static ConfigEntry<float> blurryIntensity;
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
        public static ConfigEntry<string> shadowExclusions;
        // SYNCHRONIZATION
        public static ConfigEntry<bool> isSynchronization;
        public static ConfigEntry<float> synchronizationMultiplier;
        public static ConfigEntry<string> synchronizationWeight;
        // DIMINUTIVE
        public static ConfigEntry<bool> isDiminutive;
        public static ConfigEntry<float> diminutiveMultiplier;
        public static ConfigEntry<string> diminutiveWeight;
        public static ConfigEntry<float> diminutiveSpeed;
        public static ConfigEntry<float> diminutiveGrab;
        // EXPLORATION
        public static ConfigEntry<bool> isExploration;
        public static ConfigEntry<float> explorationMultiplier;
        public static ConfigEntry<string> explorationWeight;
        public static ConfigEntry<float> explorationDistance;
        public static ConfigEntry<string> explorationRendererNames;
        // COMMUNICATION
        public static ConfigEntry<bool> isCommunication;
        public static ConfigEntry<float> communicationMultiplier;
        public static ConfigEntry<string> communicationWeight;
        public static ConfigEntry<int> communicationChrono;

        internal static void Load()
        {
            // GLOBAL
            globalChance = CursedScraps.configFile.Bind("_Global_", "Chance", "default:30", "Overall chance of scrap appearing.\nThis value does not replace the chance of appearance for each curse; the latter are considered after the overall chance to determine which curse is chosen.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            globalPrevent = CursedScraps.configFile.Bind("_Global_", "Preventing settings changes", true, "Set to false to allow players to change their settings when a curse modifying controls is active.\nThis configuration is mainly there in case of unforeseen bugs or potential incompatibility.");
            scrapExclusions = CursedScraps.configFile.Bind("_Global_", "Exclusion list", "", "List of scraps that will not be cursed.\nYou can add scraps by separating them with a comma.");
            isCurseInfoOn = CursedScraps.configFile.Bind("_Global_", "Curse info", true, "Does the information popup appear when a player is cursed?");
            // HIDE MECHANIC
            isRedScanOn = CursedScraps.configFile.Bind("_Hiding mechanic_", "Enable red scan", true, "Is red scan on cursed scraps enabled?");
            isParticleOn = CursedScraps.configFile.Bind("_Hiding mechanic_", "Enable particle", true, "Is cursed particle enabled?");
            isHideName = CursedScraps.configFile.Bind("_Hiding mechanic_", "Hide curse name", false, "Replace the curse name with '???'");
            isHideValue = CursedScraps.configFile.Bind("_Hiding mechanic_", "Hide scrap value", false, "Replace the cursed scrap value with '???'");
            // PENALTY MECHANIC
            penaltyMode = CursedScraps.configFile.Bind("_Penalty mechanic_", "Mode", Constants.PENALTY_HARD, "Mode for the penalty mechanic.\n" +
                                                                                                             Constants.PENALTY_HARD + " - When the counter is reached, all players receive the curse of the next scrap scanned.\n" +
                                                                                                             Constants.PENALTY_MEDIUM + " - When the counter is reached, only the player who scans the next cursed scrap receives the curse.\n" +
                                                                                                             Constants.PENALTY_NONE + " - Never apply the penalty.");
            penaltyCounter = CursedScraps.configFile.Bind("_Penalty mechanic_", "Counter", 5, "Number of cursed objects to be scanned before the next one affects the penalty.\nThe counter is reset after the penalty is applied.");
            // HOLY WATER
            isHolyWater = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Enable", true, "Is " + Constants.HOLY_WATER + " item enabled?\nConsumable that removes all active curses on the player.");
            holyWaterRarity = CursedScraps.configFile.Bind<int>(Constants.HOLY_WATER, "Rarity", 20, Constants.HOLY_WATER + " spawn rarity.");
            maxHolyWater = CursedScraps.configFile.Bind<int>(Constants.HOLY_WATER, "Max spawn", 4, "Max " + Constants.HOLY_WATER + " to spawn");
            // INHIBITION
            isInhibition = CursedScraps.configFile.Bind(Constants.INHIBITION, "Enable", true, "Is " + Constants.INHIBITION + " curse enabled?\nPrevents the player from jumping and crouching.");
            inhibitionMultiplier = CursedScraps.configFile.Bind(Constants.INHIBITION, "Multiplier", 2.7f, "Value multiplier for scraps with the " + Constants.INHIBITION + " curse.");
            inhibitionWeight = CursedScraps.configFile.Bind(Constants.INHIBITION, "Weight", "default:1,Experimentation:0", "Spawn weight of a scrap with the " + Constants.INHIBITION + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // CONFUSION
            isConfusion = CursedScraps.configFile.Bind(Constants.CONFUSION, "Enable", true, "Is " + Constants.CONFUSION + " curse enabled?\nReverses movement controls and jump/crouch keys for the player.");
            confusionMultiplier = CursedScraps.configFile.Bind(Constants.CONFUSION, "Multiplier", 2.6f, "Value multiplier for scraps with the " + Constants.CONFUSION + " curse.");
            confusionWeight = CursedScraps.configFile.Bind(Constants.CONFUSION, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.CONFUSION + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // CAPTIVE
            isCaptive = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Enable", true, "Is " + Constants.CAPTIVE + " curse enabled?\nPrevents the item from being thrown until it has been brought back to the ship.");
            captiveMultiplier = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Multiplier", 1.8f, "Value multiplier for scraps with the " + Constants.CAPTIVE + " curse.");
            captiveWeight = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.CAPTIVE + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // BLURRY
            isBlurry = CursedScraps.configFile.Bind(Constants.BLURRY, "Enable", true, "Is " + Constants.BLURRY + " curse enabled?\nReduces visual clarity on the player's camera.");
            blurryMultiplier = CursedScraps.configFile.Bind(Constants.BLURRY, "Multiplier", 2.4f, "Value multiplier for scraps with the " + Constants.BLURRY + " curse.");
            blurryWeight = CursedScraps.configFile.Bind(Constants.BLURRY, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.BLURRY + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            blurryIntensity = CursedScraps.configFile.Bind(Constants.BLURRY, "Intensity", 1f, "Intensity of the " + Constants.BLURRY + " curse.");
            // MUTE
            isMute = CursedScraps.configFile.Bind(Constants.MUTE, "Enable", true, "Is " + Constants.MUTE + " curse enabled?\nMutes the player's microphone.");
            muteMultiplier = CursedScraps.configFile.Bind(Constants.MUTE, "Multiplier", 1.5f, "Value multiplier for scraps with the " + Constants.MUTE + " curse.");
            muteWeight = CursedScraps.configFile.Bind(Constants.MUTE, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.MUTE + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // DEAFNESS
            isDeafness = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Enable", true, "Is " + Constants.DEAFNESS + " curse enabled?\nRemoves the player's sound.");
            deafnessMultiplier = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Multiplier", 2.7f, "Value multiplier for scraps with the " + Constants.DEAFNESS + " curse.");
            deafnessWeight = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.DEAFNESS + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // ERRANT
            isErrant = CursedScraps.configFile.Bind(Constants.ERRANT, "Enable", true, "Is " + Constants.ERRANT + " curse enabled?\nTeleports the player randomly when an item is picked up or placed down.");
            errantMultiplier = CursedScraps.configFile.Bind(Constants.ERRANT, "Multiplier", 2.5f, "Value multiplier for scraps with the " + Constants.ERRANT + " curse.");
            errantWeight = CursedScraps.configFile.Bind(Constants.ERRANT, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.ERRANT + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // PARALYSIS
            isParalysis = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Enable", true, "Is " + Constants.PARALYSIS + " curse enabled?\nParalyzes the player when scanning an enemy.");
            paralysisMultiplier = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Multiplier", 1.8f, "Value multiplier for scraps with the " + Constants.PARALYSIS + " curse.");
            paralysisWeight = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.PARALYSIS + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            paralysisTime = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Time", 5f, "Player paralysis time in seconds.");
            // SHADOW
            isShadow = CursedScraps.configFile.Bind(Constants.SHADOW, "Enable", true, "Is " + Constants.SHADOW + " curse enabled?\nAll enemies are invisible by default (their sound is still active), scanning reveals them.");
            shadowMultiplier = CursedScraps.configFile.Bind(Constants.SHADOW, "Multiplier", 2.4f, "Value multiplier for scraps with the " + Constants.SHADOW + " curse.");
            shadowWeight = CursedScraps.configFile.Bind(Constants.SHADOW, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.SHADOW + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            shadowExclusions = CursedScraps.configFile.Bind(Constants.SHADOW, "Exclusion list", "Masked", "List of creatures that will not be affected by the " + Constants.SHADOW + " curse.\nYou can add enemies by separating them with a comma.");
            // SYNCHRONIZATION
            isSynchronization = CursedScraps.configFile.Bind(Constants.SYNCHRONIZATION, "Enable", true, "Is " + Constants.SYNCHRONIZATION + " curse enabled?\nThe scrap is split into two parts, when both parts are picked up by two different players, their cameras invert.");
            synchronizationMultiplier = CursedScraps.configFile.Bind(Constants.SYNCHRONIZATION, "Multiplier", 7f, "Value multiplier for scraps with the " + Constants.SYNCHRONIZATION + " curse.");
            synchronizationWeight = CursedScraps.configFile.Bind(Constants.SYNCHRONIZATION, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.SYNCHRONIZATION + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // DIMINUTIVE
            isDiminutive = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Enable", true, "Is " + Constants.DIMINUTIVE + " curse enabled?\nReduces the player's size.");
            diminutiveMultiplier = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Multiplier", 3f, "Value multiplier for scraps with the " + Constants.DIMINUTIVE + " curse.");
            diminutiveWeight = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.DIMINUTIVE + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            diminutiveSpeed = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Speed", 2f, "Speed divider for player movement; the higher the value, the slower the player will move.");
            diminutiveGrab = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Grab", 4f, "Distance grab divider for the player; the higher the value, the less the player will be able to grab from a distance.");
            // EXPLORATION
            isExploration = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Enable", true, "Is " + Constants.EXPLORATION + " curse enabled?\nPrevents the player from exiting or entering the factory through all doors except one.");
            explorationMultiplier = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Multiplier", 2.5f, "Value multiplier for scraps with the " + Constants.EXPLORATION + " curse.");
            explorationWeight = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.EXPLORATION + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            explorationDistance = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Distance", 30f, "Distance between the player and the door at which the door's aura disappears.");
            explorationRendererNames = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Renderer names", "Cube.001,Cube.002,DoorMesh,DoorMesh (1)", "List of renderer names for displaying the door's aura, used in case custom map doors have different names.");
            // COMMUNICATION
            isCommunication = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Enable", true, "Is " + Constants.COMMUNICATION + " curse enabled?\nThis curse affects two players in two stages. See README for more details.");
            communicationMultiplier = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Multiplier", 4f, "Value multiplier for scraps with the " + Constants.COMMUNICATION + " curse.");
            communicationWeight = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Weight", "default:1", "Spawn weight of a scrap with the " + Constants.COMMUNICATION + " curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            communicationChrono = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Chrono", 120, "Time limit for both players to return to the ship.");
        }

        internal static List<CurseEffect> GetCurseEffectsFromConfig()
        {
            List<CurseEffect> curseEffects = new List<CurseEffect>();
            if (isInhibition.Value) curseEffects.Add(new CurseEffect(Constants.INHIBITION, inhibitionMultiplier.Value, inhibitionWeight.Value, false));
            if (isConfusion.Value) curseEffects.Add(new CurseEffect(Constants.CONFUSION, confusionMultiplier.Value, confusionWeight.Value, false));
            if (isCaptive.Value) curseEffects.Add(new CurseEffect(Constants.CAPTIVE, captiveMultiplier.Value, captiveWeight.Value, false));
            if (isBlurry.Value) curseEffects.Add(new CurseEffect(Constants.BLURRY, blurryMultiplier.Value, blurryWeight.Value, false));
            if (isMute.Value) curseEffects.Add(new CurseEffect(Constants.MUTE, muteMultiplier.Value, muteWeight.Value, false));
            if (isDeafness.Value) curseEffects.Add(new CurseEffect(Constants.DEAFNESS, deafnessMultiplier.Value, deafnessWeight.Value, false));
            if (isErrant.Value) curseEffects.Add(new CurseEffect(Constants.ERRANT, errantMultiplier.Value, errantWeight.Value, false));
            if (isParalysis.Value) curseEffects.Add(new CurseEffect(Constants.PARALYSIS, paralysisMultiplier.Value, paralysisWeight.Value, false));
            if (isShadow.Value) curseEffects.Add(new CurseEffect(Constants.SHADOW, shadowMultiplier.Value, shadowWeight.Value, false));
            if (isSynchronization.Value) curseEffects.Add(new CurseEffect(Constants.SYNCHRONIZATION, synchronizationMultiplier.Value, synchronizationWeight.Value, true));
            if (isDiminutive.Value) curseEffects.Add(new CurseEffect(Constants.DIMINUTIVE, diminutiveMultiplier.Value, diminutiveWeight.Value, false));
            if (isExploration.Value) curseEffects.Add(new CurseEffect(Constants.EXPLORATION, explorationMultiplier.Value, explorationWeight.Value, false));
            if (isCommunication.Value) curseEffects.Add(new CurseEffect(Constants.COMMUNICATION, communicationMultiplier.Value, communicationWeight.Value, true));

            return curseEffects;
        }
    }
}