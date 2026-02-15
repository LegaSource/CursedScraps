using BepInEx.Configuration;
using CursedScraps.Registries;

namespace CursedScraps.Managers;

public class ConfigManager
{
    // GLOBAL
    public static ConfigEntry<string> globalChance;
    public static ConfigEntry<bool> globalPrevent;
    public static ConfigEntry<bool> isCurseShader;
    public static ConfigEntry<string> scrapExclusions;
    // HUD
    public static ConfigEntry<bool> isCurseInfoOn;
    public static ConfigEntry<float> deadCursesPosX;
    public static ConfigEntry<float> deadCursesPosY;
    // HOLY WATER
    public static ConfigEntry<bool> isHolyWater;
    public static ConfigEntry<int> holyWaterRarity;
    public static ConfigEntry<int> minHolyWater;
    public static ConfigEntry<int> maxHolyWater;
    // INHIBITION
    public static ConfigEntry<bool> isInhibition;
    public static ConfigEntry<float> inhibitionMultiplier;
    public static ConfigEntry<string> inhibitionWeight;
    public static ConfigEntry<int> inhibitionDuration;
    public static ConfigEntry<bool> isInhibitionTip;
    public static ConfigEntry<float> inhibitionCooldown;
    public static ConfigEntry<string> inhibitionActions;
    // CONFUSION
    public static ConfigEntry<bool> isConfusion;
    public static ConfigEntry<float> confusionMultiplier;
    public static ConfigEntry<string> confusionWeight;
    public static ConfigEntry<int> confusionDuration;
    // CAPTIVE
    public static ConfigEntry<bool> isCaptive;
    public static ConfigEntry<float> captiveMultiplier;
    public static ConfigEntry<string> captiveWeight;
    public static ConfigEntry<int> captiveDuration;
    // BLURRY
    public static ConfigEntry<bool> isBlurry;
    public static ConfigEntry<float> blurryMultiplier;
    public static ConfigEntry<string> blurryWeight;
    public static ConfigEntry<int> blurryDuration;
    public static ConfigEntry<float> blurryIntensity;
    // MUTE
    public static ConfigEntry<bool> isMute;
    public static ConfigEntry<float> muteMultiplier;
    public static ConfigEntry<string> muteWeight;
    public static ConfigEntry<int> muteDuration;
    // DEAFNESS
    public static ConfigEntry<bool> isDeafness;
    public static ConfigEntry<float> deafnessMultiplier;
    public static ConfigEntry<string> deafnessWeight;
    public static ConfigEntry<int> deafnessDuration;
    // ERRANT
    public static ConfigEntry<bool> isErrant;
    public static ConfigEntry<float> errantMultiplier;
    public static ConfigEntry<string> errantWeight;
    public static ConfigEntry<int> errantDuration;
    public static ConfigEntry<string> errantExclusions;
    // PARALYSIS
    public static ConfigEntry<bool> isParalysis;
    public static ConfigEntry<float> paralysisMultiplier;
    public static ConfigEntry<string> paralysisWeight;
    public static ConfigEntry<int> paralysisDuration;
    public static ConfigEntry<float> paralysisTime;
    // SHADOW
    public static ConfigEntry<bool> isShadow;
    public static ConfigEntry<float> shadowMultiplier;
    public static ConfigEntry<string> shadowWeight;
    public static ConfigEntry<int> shadowDuration;
    public static ConfigEntry<string> shadowExclusions;
    // DIMINUTIVE
    public static ConfigEntry<bool> isDiminutive;
    public static ConfigEntry<float> diminutiveMultiplier;
    public static ConfigEntry<string> diminutiveWeight;
    public static ConfigEntry<int> diminutiveDuration;
    public static ConfigEntry<float> diminutiveSpeed;
    // EXPLORATION
    public static ConfigEntry<bool> isExploration;
    public static ConfigEntry<float> explorationMultiplier;
    public static ConfigEntry<string> explorationWeight;
    public static ConfigEntry<int> explorationDuration;
    public static ConfigEntry<float> explorationDistance;
    // FRAGILE
    public static ConfigEntry<bool> isFragile;
    public static ConfigEntry<float> fragileMultiplier;
    public static ConfigEntry<string> fragileWeight;
    public static ConfigEntry<int> fragileDuration;
    public static ConfigEntry<string> fragileExclusions;
    // ONE FOR ALL
    public static ConfigEntry<bool> isOneForAll;
    public static ConfigEntry<float> oneForAllMultiplier;
    public static ConfigEntry<string> oneForAllWeight;
    public static ConfigEntry<int> oneForAllDuration;
    public static ConfigEntry<bool> isOneForAllInfoOn;
    // SACRIFICE
    public static ConfigEntry<bool> isSacrifice;
    public static ConfigEntry<float> sacrificeMultiplier;
    public static ConfigEntry<string> sacrificeWeight;
    public static ConfigEntry<int> sacrificeDuration;
    public static ConfigEntry<bool> isSacrificeInfoOn;

    public static void Load()
    {
        // GLOBAL
        globalChance = CursedScraps.configFile.Bind(Constants.GLOBAL, "Chance", "default:15", "Overall chance of scrap appearing.\nThis value does not replace the chance of appearance for each curse; the latter are considered after the overall chance to determine which curse is chosen.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        globalPrevent = CursedScraps.configFile.Bind(Constants.GLOBAL, "Preventing settings changes", true, "Set to false to allow players to change their settings when a curse modifying controls is active.\nThis configuration is mainly there in case of unforeseen bugs or potential incompatibility.");
        isCurseShader = CursedScraps.configFile.Bind(Constants.GLOBAL, "Enable curse shader", true, "Enable shader on the player when cursed.");
        scrapExclusions = CursedScraps.configFile.Bind(Constants.GLOBAL, "Exclusion list", "Key", "List of scraps that will not be cursed.\nYou can add scraps by separating them with a comma.");
        // HUD
        isCurseInfoOn = CursedScraps.configFile.Bind(Constants.HUD, "Curse info", true, "Does the information popup appear when a player is cursed?");
        deadCursesPosX = CursedScraps.configFile.Bind(Constants.HUD, "Dead curses pos X", -30f, "X position of player curses text on the interface as a spectator.");
        deadCursesPosY = CursedScraps.configFile.Bind(Constants.HUD, "Dead curses pos Y", 50f, "Y position of player curses text on the interface as a spectator.");
        // HOLY WATER
        isHolyWater = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Enable", true, $"Is {Constants.HOLY_WATER} item enabled?\nConsumable that removes all active curses on the player.");
        holyWaterRarity = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Rarity", 20, $"{Constants.HOLY_WATER} spawn rarity.");
        minHolyWater = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Min spawn", 1, $"Min {Constants.HOLY_WATER} to spawn");
        maxHolyWater = CursedScraps.configFile.Bind(Constants.HOLY_WATER, "Max spawn", 4, $"Max {Constants.HOLY_WATER} to spawn");
        // INHIBITION
        isInhibition = CursedScraps.configFile.Bind(Constants.INHIBITION, "Enable", true, $"Is {Constants.INHIBITION} curse enabled?\nPrevents the player from jumping and crouching.");
        inhibitionMultiplier = CursedScraps.configFile.Bind(Constants.INHIBITION, "Multiplier", 1.75f, $"Value multiplier for scraps with the {Constants.INHIBITION} curse.");
        inhibitionWeight = CursedScraps.configFile.Bind(Constants.INHIBITION, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.INHIBITION} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        inhibitionDuration = CursedScraps.configFile.Bind(Constants.INHIBITION, "Duration", 90, $"Duration of the {Constants.INHIBITION} curse in seconds.");
        isInhibitionTip = CursedScraps.configFile.Bind(Constants.INHIBITION, "Message", false, "Display a message when the blocked action is modified.");
        inhibitionCooldown = CursedScraps.configFile.Bind(Constants.INHIBITION, "Cooldown", 10f, "Cooldown duration before a new action can be triggered.");
        inhibitionActions = CursedScraps.configFile.Bind(Constants.INHIBITION, "Actions", $"Jump,Crouch,Interact,Sprint,PingScan", $"Actions that can be blocked by the curse {Constants.INHIBITION}.");
        // CONFUSION
        isConfusion = CursedScraps.configFile.Bind(Constants.CONFUSION, "Enable", true, $"Is {Constants.CONFUSION} curse enabled?\nReverses movement controls and jump/crouch keys for the player.");
        confusionMultiplier = CursedScraps.configFile.Bind(Constants.CONFUSION, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.CONFUSION} curse.");
        confusionWeight = CursedScraps.configFile.Bind(Constants.CONFUSION, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.CONFUSION} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        confusionDuration = CursedScraps.configFile.Bind(Constants.CONFUSION, "Duration", 90, $"Duration of the {Constants.CONFUSION} curse in seconds.");
        // CAPTIVE
        isCaptive = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Enable", true, $"Is {Constants.CAPTIVE} curse enabled?\nPrevents the item from being thrown until it has been brought back to the ship.");
        captiveMultiplier = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Multiplier", 1.5f, $"Value multiplier for scraps with the {Constants.CAPTIVE} curse.");
        captiveWeight = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.CAPTIVE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        captiveDuration = CursedScraps.configFile.Bind(Constants.CAPTIVE, "Duration", 90, $"Duration of the {Constants.CAPTIVE} curse in seconds.");
        // BLURRY
        isBlurry = CursedScraps.configFile.Bind(Constants.BLURRY, "Enable", true, $"Is {Constants.BLURRY} curse enabled?\nReduces visual clarity on the player's camera.");
        blurryMultiplier = CursedScraps.configFile.Bind(Constants.BLURRY, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.BLURRY} curse.");
        blurryWeight = CursedScraps.configFile.Bind(Constants.BLURRY, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.BLURRY} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        blurryDuration = CursedScraps.configFile.Bind(Constants.BLURRY, "Duration", 60, $"Duration of the {Constants.BLURRY} curse in seconds.");
        blurryIntensity = CursedScraps.configFile.Bind(Constants.BLURRY, "Intensity", 1f, $"Intensity of the {Constants.BLURRY} curse.");
        // MUTE
        isMute = CursedScraps.configFile.Bind(Constants.MUTE, "Enable", true, $"Is {Constants.MUTE} curse enabled?\nMutes the player's microphone.");
        muteMultiplier = CursedScraps.configFile.Bind(Constants.MUTE, "Multiplier", 1.25f, $"Value multiplier for scraps with the {Constants.MUTE} curse.");
        muteWeight = CursedScraps.configFile.Bind(Constants.MUTE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.MUTE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        muteDuration = CursedScraps.configFile.Bind(Constants.MUTE, "Duration", 90, $"Duration of the {Constants.MUTE} curse in seconds.");
        // DEAFNESS
        isDeafness = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Enable", true, $"Is {Constants.DEAFNESS} curse enabled?\nRemoves the player's sound.");
        deafnessMultiplier = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.DEAFNESS} curse.");
        deafnessWeight = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.DEAFNESS} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        deafnessDuration = CursedScraps.configFile.Bind(Constants.DEAFNESS, "Duration", 90, $"Duration of the {Constants.DEAFNESS} curse in seconds.");
        // ERRANT
        isErrant = CursedScraps.configFile.Bind(Constants.ERRANT, "Enable", true, $"Is {Constants.ERRANT} curse enabled?\nTeleports the player randomly when an item is picked up or placed down.");
        errantMultiplier = CursedScraps.configFile.Bind(Constants.ERRANT, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.ERRANT} curse.");
        errantWeight = CursedScraps.configFile.Bind(Constants.ERRANT, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.ERRANT} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        errantDuration = CursedScraps.configFile.Bind(Constants.ERRANT, "Duration", 60, $"Duration of the {Constants.ERRANT} curse in seconds.");
        errantExclusions = CursedScraps.configFile.Bind(Constants.ERRANT, "Exclusion list", "Saw Tape,Pursuer Eye,Saw Key,Saw", $"List of items not affected by the {Constants.ERRANT} curse.\nYou can add items by separating them with a comma.");
        // PARALYSIS
        isParalysis = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Enable", true, $"Is {Constants.PARALYSIS} curse enabled?\nParalyzes the player when scanning an enemy.");
        paralysisMultiplier = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Multiplier", 1.75f, $"Value multiplier for scraps with the {Constants.PARALYSIS} curse.");
        paralysisWeight = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.PARALYSIS} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        paralysisDuration = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Duration", 90, $"Duration of the {Constants.PARALYSIS} curse in seconds.");
        paralysisTime = CursedScraps.configFile.Bind(Constants.PARALYSIS, "Time", 5f, "Player paralysis time in seconds.");
        // SHADOW
        isShadow = CursedScraps.configFile.Bind(Constants.SHADOW, "Enable", true, $"Is {Constants.SHADOW} curse enabled?\nAll enemies are invisible by default (their sound is still active), scanning reveals them.");
        shadowMultiplier = CursedScraps.configFile.Bind(Constants.SHADOW, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.SHADOW} curse.");
        shadowWeight = CursedScraps.configFile.Bind(Constants.SHADOW, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.SHADOW} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        shadowDuration = CursedScraps.configFile.Bind(Constants.SHADOW, "Duration", 90, $"Duration of the {Constants.SHADOW} curse in seconds.");
        shadowExclusions = CursedScraps.configFile.Bind(Constants.SHADOW, "Exclusion list", "Masked", $"List of creatures that will not be affected by the {Constants.SHADOW} curse.\nYou can add enemies by separating them with a comma.");
        // DIMINUTIVE
        isDiminutive = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Enable", true, $"Is {Constants.DIMINUTIVE} curse enabled?\nReduces the player's size.");
        diminutiveMultiplier = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.DIMINUTIVE} curse.");
        diminutiveWeight = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.DIMINUTIVE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        diminutiveDuration = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Duration", 90, $"Duration of the {Constants.DIMINUTIVE} curse in seconds.");
        diminutiveSpeed = CursedScraps.configFile.Bind(Constants.DIMINUTIVE, "Speed factor", -0.5f, "Speed factor for player movement.");
        // EXPLORATION
        isExploration = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Enable", true, $"Is {Constants.EXPLORATION} curse enabled?\nPrevents the player from exiting or entering the factory through all doors except one.");
        explorationMultiplier = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Multiplier", 1.5f, $"Value multiplier for scraps with the {Constants.EXPLORATION} curse.");
        explorationWeight = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.EXPLORATION} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        explorationDuration = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Duration", 120, $"Duration of the {Constants.EXPLORATION} curse in seconds.");
        explorationDistance = CursedScraps.configFile.Bind(Constants.EXPLORATION, "Distance", 30f, "Distance between the player and the door at which the door's aura disappears.");
        // FRAGILE
        isFragile = CursedScraps.configFile.Bind(Constants.FRAGILE, "Enable", true, $"Is {Constants.FRAGILE} curse enabled?.");
        fragileMultiplier = CursedScraps.configFile.Bind(Constants.FRAGILE, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.FRAGILE} curse.");
        fragileWeight = CursedScraps.configFile.Bind(Constants.FRAGILE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.FRAGILE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        fragileDuration = CursedScraps.configFile.Bind(Constants.FRAGILE, "Duration", 90, $"Duration of the {Constants.FRAGILE} curse in seconds.");
        fragileExclusions = CursedScraps.configFile.Bind(Constants.FRAGILE, "Exclusion list", "Saw Tape,Pursuer Eye,Saw Key,Saw", $"List of items not affected by the {Constants.FRAGILE} curse.\nYou can add items by separating them with a comma.");
        // ONE FOR ALL
        isOneForAll = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, "Enable", true, $"Is {Constants.ONE_FOR_ALL} curse enabled?.");
        oneForAllMultiplier = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.ONE_FOR_ALL} curse.");
        oneForAllWeight = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.ONE_FOR_ALL} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        oneForAllDuration = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, "Duration", 30, $"Duration of the {Constants.ONE_FOR_ALL} curse in seconds.");
        isOneForAllInfoOn = CursedScraps.configFile.Bind(Constants.ONE_FOR_ALL, $"{Constants.ONE_FOR_ALL} info", true, $"Does the information popup appear to other players when someone is afflicted by the {Constants.ONE_FOR_ALL} curse?");
        // SACRIFICE
        isSacrifice = CursedScraps.configFile.Bind(Constants.SACRIFICE, "Enable", true, $"Is {Constants.SACRIFICE} curse enabled?.");
        sacrificeMultiplier = CursedScraps.configFile.Bind(Constants.SACRIFICE, "Multiplier", 2f, $"Value multiplier for scraps with the {Constants.SACRIFICE} curse.");
        sacrificeWeight = CursedScraps.configFile.Bind(Constants.SACRIFICE, "Weight", "default:1", $"Spawn weight of a scrap with the {Constants.SACRIFICE} curse.\nYou can adjust this value according to the moon by adding its name along with its value (moon:value). Each key/value pair should be separated by a comma.");
        sacrificeDuration = CursedScraps.configFile.Bind(Constants.SACRIFICE, "Duration", 60, $"Duration of the {Constants.SACRIFICE} curse in seconds.");
        isSacrificeInfoOn = CursedScraps.configFile.Bind(Constants.SACRIFICE, $"{Constants.SACRIFICE} info", true, $"Does the information popup appear to other players when someone is afflicted by the {Constants.SACRIFICE} curse?");
        // MOVE OR DIE
        // HAUNTED
        // VIRUS
    }

    public static void RegisterCursesFromConfig()
    {
        if (isInhibition.Value) CSCurseRegistry.RegisterCurse(Constants.INHIBITION, inhibitionMultiplier.Value, inhibitionWeight.Value, inhibitionDuration.Value);
        if (isConfusion.Value) CSCurseRegistry.RegisterCurse(Constants.CONFUSION, confusionMultiplier.Value, confusionWeight.Value, confusionDuration.Value);
        if (isCaptive.Value) CSCurseRegistry.RegisterCurse(Constants.CAPTIVE, captiveMultiplier.Value, captiveWeight.Value, captiveDuration.Value);
        if (isBlurry.Value) CSCurseRegistry.RegisterCurse(Constants.BLURRY, blurryMultiplier.Value, blurryWeight.Value, blurryDuration.Value);
        if (isMute.Value) CSCurseRegistry.RegisterCurse(Constants.MUTE, muteMultiplier.Value, muteWeight.Value, muteDuration.Value);
        if (isDeafness.Value) CSCurseRegistry.RegisterCurse(Constants.DEAFNESS, deafnessMultiplier.Value, deafnessWeight.Value, deafnessDuration.Value);
        if (isErrant.Value) CSCurseRegistry.RegisterCurse(Constants.ERRANT, errantMultiplier.Value, errantWeight.Value, errantDuration.Value);
        if (isParalysis.Value) CSCurseRegistry.RegisterCurse(Constants.PARALYSIS, paralysisMultiplier.Value, paralysisWeight.Value, paralysisDuration.Value);
        if (isShadow.Value) CSCurseRegistry.RegisterCurse(Constants.SHADOW, shadowMultiplier.Value, shadowWeight.Value, shadowDuration.Value);
        if (isDiminutive.Value) CSCurseRegistry.RegisterCurse(Constants.DIMINUTIVE, diminutiveMultiplier.Value, diminutiveWeight.Value, diminutiveDuration.Value);
        if (isExploration.Value) CSCurseRegistry.RegisterCurse(Constants.EXPLORATION, explorationMultiplier.Value, explorationWeight.Value, explorationDuration.Value);
        if (isFragile.Value) CSCurseRegistry.RegisterCurse(Constants.FRAGILE, fragileMultiplier.Value, fragileWeight.Value, fragileDuration.Value);
        if (isOneForAll.Value) CSCurseRegistry.RegisterCurse(Constants.ONE_FOR_ALL, oneForAllMultiplier.Value, oneForAllWeight.Value, oneForAllDuration.Value);
        if (isSacrifice.Value) CSCurseRegistry.RegisterCurse(Constants.SACRIFICE, sacrificeMultiplier.Value, sacrificeWeight.Value, sacrificeDuration.Value);
    }
}
