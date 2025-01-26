using BepInEx.Configuration;
using CursedScraps.Behaviours.Curses;
using System.Collections.Generic;

namespace CursedScraps.Managers
{
    public class ConfigManager
    {
        // GLOBAL
        public static ConfigEntry<string> globalChance;
        public static ConfigEntry<bool> globalPrevent;
        public static ConfigEntry<string> scrapExclusions;
        public static ConfigEntry<float> minParticleScale;
        public static ConfigEntry<float> maxParticleScale;
        public static ConfigEntry<string> rendererNames;
        // HUD
        public static ConfigEntry<bool> isCurseInfoOn;
        public static ConfigEntry<float> deadCursesPosX;
        public static ConfigEntry<float> deadCursesPosY;
        public static ConfigEntry<float> communicationDistancePosX;
        public static ConfigEntry<float> communicationDistancePosY;
        // HIDE MECHANIC
        public static ConfigEntry<bool> isRedScanOn;
        public static ConfigEntry<bool> isParticleOn;
        public static ConfigEntry<bool> isHideLine;
        public static ConfigEntry<bool> isHideName;
        public static ConfigEntry<bool> isHideValue;
        // PENALTY MECHANIC
        public static ConfigEntry<string> penaltyMode;
        public static ConfigEntry<int> penaltyCounter;
        // HOLY WATER
        public static ConfigEntry<bool> isHolyWater;
        public static ConfigEntry<int> holyWaterRarity;
        public static ConfigEntry<int> minHolyWater;
        public static ConfigEntry<int> maxHolyWater;
        // OLD SCROLL
        public static ConfigEntry<bool> isOldScrollSpawnable;
        public static ConfigEntry<int> oldScrollRarity;
        public static ConfigEntry<int> minOldScroll;
        public static ConfigEntry<int> maxOldScroll;
        public static ConfigEntry<float> oldScrollAura;
        // INHIBITION
        public static ConfigEntry<bool> isInhibition;
        public static ConfigEntry<float> inhibitionMultiplier;
        public static ConfigEntry<string> inhibitionWeight;
        public static ConfigEntry<bool> isInhibitionTip;
        public static ConfigEntry<float> inhibitionCooldown;
        public static ConfigEntry<string> inhibitionActions;
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
        public static ConfigEntry<string> errantExclusions;
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
        // COMMUNICATION
        public static ConfigEntry<bool> isCommunication;
        public static ConfigEntry<float> communicationMultiplier;
        public static ConfigEntry<string> communicationWeight;
        public static ConfigEntry<float> communicationCooldown;
        // FRAGILE
        public static ConfigEntry<bool> isFragile;
        public static ConfigEntry<float> fragileMultiplier;
        public static ConfigEntry<string> fragileWeight;
        public static ConfigEntry<string> fragileExclusions;
        // ONE FOR ALL
        public static ConfigEntry<bool> isOneForAll;
        public static ConfigEntry<float> oneForAllMultiplier;
        public static ConfigEntry<string> oneForAllWeight;
        public static ConfigEntry<bool> isOneForAllInfoOn;
        // SACRIFICE
        public static ConfigEntry<bool> isSacrifice;
        public static ConfigEntry<float> sacrificeMultiplier;
        public static ConfigEntry<string> sacrificeWeight;
        public static ConfigEntry<bool> isSacrificeInfoOn;

        public static void Load()
        {
            // GLOBAL
            globalChance = CursedScraps.configFile.Bind(Constants.GLOBAL, "Chance", "default:15", "Overall chance of scrap appearing.\nThis value does not replace the chance of appearance for each curse; the latter are considered after the overall chance to determine which curse is chosen.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            globalPrevent = CursedScraps.configFile.Bind(Constants.GLOBAL, "Preventing settings changes", true, "Set to false to allow players to change their settings when a curse modifying controls is active.\nThis configuration is mainly there in case of unforeseen bugs or potential incompatibility.");
            scrapExclusions = CursedScraps.configFile.Bind(Constants.GLOBAL, "Exclusion list", "Key", "List of scraps that will not be cursed.\nYou can add scraps by separating them with a comma.");
            minParticleScale = CursedScraps.configFile.Bind(Constants.GLOBAL, "Min particle scale", 0.1f, "Min cursed particle scale.");
            maxParticleScale = CursedScraps.configFile.Bind(Constants.GLOBAL, "Max particle scale", 1f, "Max cursed particle scale.");
            rendererNames = CursedScraps.configFile.Bind(Constants.GLOBAL, "Renderer names", "Cube.001,Cube.002,DoorMesh,Trigger,EntranceTeleport", "List of renderer names for displaying the door's aura, to use in case custom map doors have different names.");
            // HUD
            isCurseInfoOn = CursedScraps.configFile.Bind(Constants.HUD, "Curse info", true, "Does the information popup appear when a player is cursed?");
            deadCursesPosX = CursedScraps.configFile.Bind(Constants.HUD, "Dead curses pos X", -30f, "X position of player curses text on the interface as a spectator.");
            deadCursesPosY = CursedScraps.configFile.Bind(Constants.HUD, "Dead curses pos Y", 50f, "Y position of player curses text on the interface as a spectator.");
            communicationDistancePosX = CursedScraps.configFile.Bind(Constants.HUD, "Communication distance pos X", 30f, "X position of player's distance text, from the {Constants.COMMUNICATION} curse, on the interface as a spectator.");
            communicationDistancePosY = CursedScraps.configFile.Bind(Constants.HUD, "Communication distance pos Y", -50f, "Y position of player's distance text, from the {Constants.COMMUNICATION} curse, on the interface as a spectator.");
            // HIDE MECHANIC
            isRedScanOn = CursedScraps.configFile.Bind(Constants.HIDING_MECHANIC, "Enable red scan", true, "Is red scan on cursed scraps enabled?");
            isParticleOn = CursedScraps.configFile.Bind(Constants.HIDING_MECHANIC, "Enable particle", true, "Is cursed particle enabled?");
            isHideLine = CursedScraps.configFile.Bind(Constants.HIDING_MECHANIC, "Hide curse line", false, "Hide curse line in scan node");
            isHideName = CursedScraps.configFile.Bind(Constants.HIDING_MECHANIC, "Hide curse name", false, "Replace the curse name with '???'");
            isHideValue = CursedScraps.configFile.Bind(Constants.HIDING_MECHANIC, "Hide scrap value", false, "Replace the cursed scrap value with '???'");
            // PENALTY MECHANIC
            penaltyMode = CursedScraps.configFile.Bind(Constants.PENALTY_MECHANIC, "Mode", Constants.PENALTY_HARD, "Mode for the penalty mechanic.\n" +
                                                                                                                   $"{Constants.PENALTY_HARD} - When the counter is reached, all players receive the curse of the next scrap scanned.\n" +
                                                                                                                   $"{Constants.PENALTY_MEDIUM} - When the counter is reached, only the player who scans the next cursed scrap receives the curse.\n" +
                                                                                                                   $"{Constants.PENALTY_NONE} - Never apply the penalty.");
            penaltyCounter = CursedScraps.configFile.Bind(Constants.PENALTY_MECHANIC, "Counter", 10, "Number of cursed objects to be scanned before the next one affects the penalty.\nThe counter is reset after the penalty is applied.");
            // HOLY WATER
            isHolyWater = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Enable", true, $"Is {Constants.HOLY_WATER} item enabled?\nConsumable that removes all active curses on the player.");
            holyWaterRarity = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Rarity", 20, $"{Constants.HOLY_WATER} spawn rarity.");
            minHolyWater = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Min spawn", 1, $"Min {Constants.HOLY_WATER} to spawn"); 
            maxHolyWater = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Max spawn", 4, $"Max {Constants.HOLY_WATER} to spawn");
            // OLD SCROLL
            isOldScrollSpawnable = CursedScraps.configFile.Bind(Constants.OLD_SCROLL, "Spawnable", true, $"Is {Constants.OLD_SCROLL} item enabled?\nConsumable that shows the aura of the entrances.");
            oldScrollRarity = CursedScraps.configFile.Bind(Constants.OLD_SCROLL, "Rarity", 20, $"{Constants.OLD_SCROLL} spawn rarity.");
            minOldScroll = CursedScraps.configFile.Bind(Constants.OLD_SCROLL, "Min spawn", 1, $"Min {Constants.OLD_SCROLL} to spawn"); 
            maxOldScroll = CursedScraps.configFile.Bind(Constants.OLD_SCROLL, "Max spawn", 2, $"Max {Constants.OLD_SCROLL} to spawn");
            oldScrollAura = CursedScraps.configFile.Bind(Constants.OLD_SCROLL, "Aura duration", 10f, "Duration for which the door's aura is visible through walls");
            // INHIBITION
            isInhibition = CursedScraps.configFile.Bind(Constants.INHIBITION, "Enable", true, $"Is {Constants.INHIBITION} curse enabled?\nPrevents the player from jumping and crouching.");
            inhibitionMultiplier = CursedScraps.configFile.Bind(Constants.INHIBITION, "Multiplier", 1.75f, $"Value multiplier for scraps with the {Constants.INHIBITION} curse.");
            inhibitionWeight = CursedScraps.configFile.Bind(Constants.INHIBITION, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.INHIBITION} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            isInhibitionTip = CursedScraps.configFile.Bind(Constants.INHIBITION, "Message", false, "Display a message when the blocked action is modified.");
            inhibitionCooldown = CursedScraps.configFile.Bind(Constants.INHIBITION, "Cooldown", 20f, "Cooldown duration before a new action can be triggered.");
            inhibitionActions = CursedScraps.configFile.Bind(Constants.INHIBITION, "Actions", $"Jump,Crouch,Interact,Sprint,PingScan", "Actions that can be blocked by the curse {Constants.INHIBITION}.");
            // CONFUSION
            isConfusion = CursedScraps.configFile.Bind(Constants.CONFUSION, "Enable", true, $"Is {Constants.CONFUSION} curse enabled?\nReverses movement controls and jump/crouch keys for the player.");
            confusionMultiplier = CursedScraps.configFile.Bind(Constants.CONFUSION, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.CONFUSION} curse.");
            confusionWeight = CursedScraps.configFile.Bind(Constants.CONFUSION, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.CONFUSION} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // CAPTIVE
            isCaptive = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Enable", true, $"Is {Constants.CAPTIVE} curse enabled?\nPrevents the item from being thrown until it has been brought back to the ship.");
            captiveMultiplier = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Multiplier", 1.5f, $"Value multiplier for scraps with the {Constants.CAPTIVE} curse.");
            captiveWeight = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.CAPTIVE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // BLURRY
            isBlurry = CursedScraps.configFile.Bind(Constants.BLURRY, "Enable", true, $"Is {Constants.BLURRY} curse enabled?\nReduces visual clarity on the player's camera.");
            blurryMultiplier = CursedScraps.configFile.Bind(Constants.BLURRY, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.BLURRY} curse.");
            blurryWeight = CursedScraps.configFile.Bind(Constants.BLURRY, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.BLURRY} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            blurryIntensity = CursedScraps.configFile.Bind(Constants.BLURRY, "Intensity", 1f, "Intensity of the {Constants.BLURRY} curse.");
            // MUTE
            isMute = CursedScraps.configFile.Bind(Constants.MUTE, "Enable", true, $"Is {Constants.MUTE} curse enabled?\nMutes the player's microphone.");
            muteMultiplier = CursedScraps.configFile.Bind(Constants.MUTE, "Multiplier", 1.25f, $"Value multiplier for scraps with the {Constants.MUTE} curse.");
            muteWeight = CursedScraps.configFile.Bind(Constants.MUTE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.MUTE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // DEAFNESS
            isDeafness = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Enable", true, $"Is {Constants.DEAFNESS} curse enabled?\nRemoves the player's sound.");
            deafnessMultiplier = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.DEAFNESS} curse.");
            deafnessWeight = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.DEAFNESS} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            // ERRANT
            isErrant = CursedScraps.configFile.Bind(Constants.ERRANT, "Enable", true, $"Is {Constants.ERRANT} curse enabled?\nTeleports the player randomly when an item is picked up or placed down.");
            errantMultiplier = CursedScraps.configFile.Bind(Constants.ERRANT, "Multiplier", 2.5f, $"Value multiplier for scraps with the {Constants.ERRANT} curse.");
            errantWeight = CursedScraps.configFile.Bind(Constants.ERRANT, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.ERRANT} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            errantExclusions = CursedScraps.configFile.Bind(Constants.ERRANT, "Exclusion list", "Saw Tape,Pursuer Eye,Saw Key,Saw", "List of items not affected by the {Constants.ERRANT} curse.\nYou can add items by separating them with a comma.");
            // PARALYSIS
            isParalysis = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Enable", true, $"Is {Constants.PARALYSIS} curse enabled?\nParalyzes the player when scanning an enemy.");
            paralysisMultiplier = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Multiplier", 1.75f, $"Value multiplier for scraps with the {Constants.PARALYSIS} curse.");
            paralysisWeight = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.PARALYSIS} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            paralysisTime = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Time", 5f, "Player paralysis time in seconds.");
            // SHADOW
            isShadow = CursedScraps.configFile.Bind(Constants.SHADOW, "Enable", true, $"Is {Constants.SHADOW} curse enabled?\nAll enemies are invisible by default (their sound is still active), scanning reveals them.");
            shadowMultiplier = CursedScraps.configFile.Bind(Constants.SHADOW, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.SHADOW} curse.");
            shadowWeight = CursedScraps.configFile.Bind(Constants.SHADOW, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.SHADOW} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            shadowExclusions = CursedScraps.configFile.Bind(Constants.SHADOW, "Exclusion list", "Masked", "List of creatures that will not be affected by the {Constants.SHADOW} curse.\nYou can add enemies by separating them with a comma.");
            // DIMINUTIVE
            isDiminutive = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Enable", true, $"Is {Constants.DIMINUTIVE} curse enabled?\nReduces the player's size.");
            diminutiveMultiplier = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.DIMINUTIVE} curse.");
            diminutiveWeight = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.DIMINUTIVE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            diminutiveSpeed = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Speed", 2f, "Speed divider for player movement; the higher the value, the slower the player will move.");
            diminutiveGrab = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Grab", 4f, "Distance grab divider for the player; the higher the value, the less the player will be able to grab from a distance.");
            // EXPLORATION
            isExploration = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Enable", true, $"Is {Constants.EXPLORATION} curse enabled?\nPrevents the player from exiting or entering the factory through all doors except one.");
            explorationMultiplier = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Multiplier", 1.5f, $"Value multiplier for scraps with the {Constants.EXPLORATION} curse.");
            explorationWeight = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.EXPLORATION} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            explorationDistance = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Distance", 30f, "Distance between the player and the door at which the door's aura disappears.");
            // COMMUNICATION
            isCommunication = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Enable", true, $"Is {Constants.COMMUNICATION} curse enabled?.");
            communicationMultiplier = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.COMMUNICATION} curse.");
            communicationWeight = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.COMMUNICATION} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            communicationCooldown = CursedScraps.configFile.Bind(Constants.COMMUNICATION, "Cooldown duration", 10f, "Cooldown duration per player for sending indications to the cursed player (hot/cold particles).");
            // FRAGILE
            isFragile = CursedScraps.configFile.Bind(Constants.FRAGILE, "Enable", true, $"Is {Constants.FRAGILE} curse enabled?.");
            fragileMultiplier = CursedScraps.configFile.Bind(Constants.FRAGILE, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.FRAGILE} curse.");
            fragileWeight = CursedScraps.configFile.Bind(Constants.FRAGILE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.FRAGILE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            fragileExclusions = CursedScraps.configFile.Bind(Constants.FRAGILE, "Exclusion list", "Saw Tape,Pursuer Eye,Saw Key,Saw", $"List of items not affected by the {Constants.FRAGILE} curse.\nYou can add items by separating them with a comma.");
            // ONE FOR ALL
            isOneForAll = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, "Enable", true, $"Is {Constants.ONE_FOR_ALL} curse enabled?.");
            oneForAllMultiplier = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, "Multiplier", 2.5f, $"Value multiplier for scraps with the {Constants.ONE_FOR_ALL} curse.");
            oneForAllWeight = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.ONE_FOR_ALL} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            isOneForAllInfoOn = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, $"{Constants.ONE_FOR_ALL} info", true, $"Does the information popup appear to other players when someone is afflicted by the {Constants.ONE_FOR_ALL} curse?");
            // SACRIFICE
            isSacrifice = CursedScraps.configFile.Bind(Constants.SACRIFICE, "Enable", true, $"Is {Constants.SACRIFICE} curse enabled?.");
            sacrificeMultiplier = CursedScraps.configFile.Bind(Constants.SACRIFICE, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.SACRIFICE} curse.");
            sacrificeWeight = CursedScraps.configFile.Bind(Constants.SACRIFICE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.SACRIFICE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
            isSacrificeInfoOn = CursedScraps.configFile.Bind(Constants.SACRIFICE, $"{Constants.SACRIFICE} info", true, $"Does the information popup appear to other players when someone is afflicted by the {Constants.SACRIFICE} curse?");
            // MOVE OR DIE
            // HAUNTED
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
            if (isDiminutive.Value) curseEffects.Add(new CurseEffect(Constants.DIMINUTIVE, diminutiveMultiplier.Value, diminutiveWeight.Value));
            if (isExploration.Value) curseEffects.Add(new CurseEffect(Constants.EXPLORATION, explorationMultiplier.Value, explorationWeight.Value));
            if (isCommunication.Value) curseEffects.Add(new CurseEffect(Constants.COMMUNICATION, communicationMultiplier.Value, communicationWeight.Value));
            if (isFragile.Value) curseEffects.Add(new CurseEffect(Constants.FRAGILE, fragileMultiplier.Value, fragileWeight.Value));
            if (isOneForAll.Value) curseEffects.Add(new CurseEffect(Constants.ONE_FOR_ALL, oneForAllMultiplier.Value, oneForAllWeight.Value));
            if (isSacrifice.Value) curseEffects.Add(new CurseEffect(Constants.SACRIFICE, sacrificeMultiplier.Value, sacrificeWeight.Value));

            return curseEffects;
        }
    }
}
