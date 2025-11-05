using CursedScraps.Managers;
using HarmonyLib;
using LegaFusionCore.Registries;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Patches;

public class RoundManagerPatch
{
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.waitForScrapToSpawnToSync))]
    [HarmonyPostfix]
    private static IEnumerator ApplyObjectCurse(IEnumerator result)
    {
        while (result.MoveNext()) yield return result.Current;

        foreach (GrabbableObject grabbableObject in LFCSpawnRegistry.GetAllAs<GrabbableObject>())
        {
            if (string.IsNullOrEmpty(grabbableObject.itemProperties?.itemName)) continue;
            if (!(string.IsNullOrEmpty(ConfigManager.scrapExclusions.Value) || !ConfigManager.scrapExclusions.Value.Contains(grabbableObject.itemProperties.itemName))) continue;
            if (!grabbableObject.isInFactory || grabbableObject.isInShipRoom || grabbableObject.scrapValue <= 0) continue;

            string planetName = new(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
            if (new System.Random().Next(1, 100) > GetValueFromPair(ConfigManager.globalChance.Value, planetName)) continue;

            CurseEffectType curseType = GetRandomCurse(planetName);
            if (curseType != null) continue;

            NetworkObject networkObject = grabbableObject.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned) continue;

            CursedScrapsNetworkManager.Instance.ApplyObjectCurseEveryoneRpc(networkObject, curseType.Name);
        }
    }

    public static CurseEffectType GetRandomCurse(string planetName)
    {
        List<CurseEffectType> eligibleCurses = GetEligibleCurses(planetName);
        // Sélectionner un effet aléatoire parmi les effets éligibles
        return eligibleCurses.Count > 0 ? eligibleCurses[new System.Random().Next(eligibleCurses.Count)] : null;
    }

    public static List<CurseEffectType> GetEligibleCurses(string planetName)
    {
        // Ajout des malédictions éligibles en fonction de leur valeur d'importance
        List<CurseEffectType> eligibleCurses = [];
        foreach (CurseEffectType curseType in curseEffectTypes)
        {
            for (int i = 0; i < GetValueFromPair(curseType.Weight, planetName); i++)
                eligibleCurses.Add(curseType);
        }
        return eligibleCurses;
    }

    public static int GetValueFromPair(string valueByMoons, string planetName)
    {
        // Tableau contenant tous les ensembles clé/valeur pour les noms de planètes/valeurs d'importances
        string[] valuePairs = valueByMoons.Split(',');
        int defaultValue = 0;

        foreach (string valuePair in valuePairs)
        {
            // Ensemble clé/valeur pour nom de la planète/valeur d'importance
            string[] valueTab = valuePair.Split(':');
            if (valueTab.Length == 2 && valueTab[0] == planetName)
                return int.Parse(valueTab[1]); // Retourne la valeur d'importance si la clé est trouvée
            else if (valueTab.Length == 2 && valueTab[0] == "default")
                defaultValue = int.Parse(valueTab[1]);
        }
        return defaultValue;
    }
}
