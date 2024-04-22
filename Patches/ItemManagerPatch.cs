using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class ItemManagerPatch
    {
        private static List<Item> consumableItems = new List<Item>();
        public static GrabbableObject scrapSyncToDestroy;
        public static int timeOut = 5;

        /**
            * Modification des montants en négatif pour indiquer l'effet à affecter à l'objet chez le client par la suite
            * NetworkObject n'étant pas modifiable c'est le moyen le moins coûteux que j'ai trouvé pour faire ce que je veux
        **/
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
        [HarmonyPrefix]
        private static void PreSyncValues(ref NetworkObjectReference[] spawnedScrap, ref int[] scrapValues, ref RoundManager __instance)
        {
            CurseScraps(ref spawnedScrap, ref scrapValues);
            if (ConfigManager.isPills.Value)
            {
                AddNewItems(scrapValues.Where(p => p < 0).Count());
                SpawnNewItems(ref __instance);
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
        [HarmonyPrefix]
        private static bool SetScrapValueOverride(int setValueTo, ref GrabbableObject __instance)
        {
            if (setValueTo < 0)
            {
                CurseEffect curseEffect = null;
                switch (setValueTo)
                {
                    case Constants.INHIBITION_VALUE:
                        curseEffect = new CurseEffect(Constants.INHIBITION, ConfigManager.inhibitionMultiplier.Value, ConfigManager.inhibitionWeight.Value);
                        break;
                    case Constants.CONFUSION_VALUE:
                        curseEffect = new CurseEffect(Constants.CONFUSION, ConfigManager.confusionMultiplier.Value, ConfigManager.confusionWeight.Value);
                        break;
                    case Constants.CAPTIVE_VALUE:
                        curseEffect = new CurseEffect(Constants.CAPTIVE, ConfigManager.captiveMultiplier.Value, ConfigManager.captiveWeight.Value);
                        break;
                    case Constants.BLURRY_VALUE:
                        curseEffect = new CurseEffect(Constants.BLURRY, ConfigManager.blurryMultiplier.Value, ConfigManager.blurryWeight.Value);
                        break;
                    case Constants.MUTE_VALUE:
                        curseEffect = new CurseEffect(Constants.MUTE, ConfigManager.muteMultiplier.Value, ConfigManager.muteWeight.Value);
                        break;
                    case Constants.DEAFNESS_VALUE:
                        curseEffect = new CurseEffect(Constants.DEAFNESS, ConfigManager.deafnessMultiplier.Value, ConfigManager.deafnessWeight.Value);
                        break;
                    case Constants.ERRANT_VALUE:
                        curseEffect = new CurseEffect(Constants.ERRANT, ConfigManager.errantMultiplier.Value, ConfigManager.errantWeight.Value);
                        break;
                    case Constants.PARALYSIS_VALUE:
                        curseEffect = new CurseEffect(Constants.PARALYSIS, ConfigManager.paralysisMultiplier.Value, ConfigManager.paralysisWeight.Value);
                        break;
                    case Constants.SHADOW_VALUE:
                        curseEffect = new CurseEffect(Constants.SHADOW, ConfigManager.shadowMultiplier.Value, ConfigManager.shadowWeight.Value);
                        break;
                    case Constants.SYNCHRONIZATION_VALUE:
                        curseEffect = new CurseEffect(Constants.SYNCHRONIZATION, ConfigManager.synchronizationMultiplier.Value, ConfigManager.synchronizationWeight.Value);
                        break;
                    case Constants.DIMINUTIVE_VALUE:
                        curseEffect = new CurseEffect(Constants.DIMINUTIVE, ConfigManager.diminutiveMultiplier.Value, ConfigManager.diminutiveWeight.Value);
                        break;
                    default:
                        Debug.LogError($"Effect not managed in SetScrapValueOverride: {curseEffect.Name}");
                        break;
                }

                ScanNodeProperties componentInChildren = ((Component)(object)__instance).gameObject.GetComponentInChildren<ScanNodeProperties>();
                if (componentInChildren != null)
                {
                    __instance.scrapValue = (int)Math.Round((__instance.itemProperties.minValue + __instance.itemProperties.maxValue) / 2 * RoundManager.Instance.scrapValueMultiplier * curseEffect.Multiplier);
                    componentInChildren.scrapValue = __instance.scrapValue;
                    componentInChildren.subText = $"Value: ${componentInChildren.scrapValue}" + " \nCurse: " + curseEffect.Name;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.CollectNewScrapForThisRound))]
        [HarmonyPrefix]
        private static bool CollectScrap(ref GrabbableObject scrapObject)
        {
            string curseEffect = PlayerManagerPatch.GetCurseEffect(ref scrapObject);
            if (!string.IsNullOrEmpty(curseEffect) && curseEffect.Equals(Constants.SYNC_CORE))
            {
                GrabbableObject reflectionScrap = GetCloneScrapFromCurse(scrapObject, Constants.SYNC_REFLECTION);
                if (reflectionScrap == null)
                {
                    return false;
                }
                else
                {
                    scrapSyncToDestroy = reflectionScrap;
                }
            }
            else if (!string.IsNullOrEmpty(curseEffect) && curseEffect.Equals(Constants.SYNC_REFLECTION))
            {
                GrabbableObject coreScrap = GetCloneScrapFromCurse(scrapObject, Constants.SYNC_CORE);
                if (coreScrap == null)
                {
                    return false;
                }
                else
                {
                    scrapSyncToDestroy = scrapObject;
                    scrapObject = coreScrap;
                }
            }
            else if (!string.IsNullOrEmpty(curseEffect) && curseEffect.Equals(Constants.DIMINUTIVE))
            {
                PlayerManagerPatch.ApplyDiminutive(false, ref GameNetworkManager.Instance.localPlayerController);
            }
            RemoveObjectEffect(ref scrapObject);
            return true;
        }

        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
        [HarmonyPostfix]
        private static void PostSyncValues(ref RoundManager __instance)
        {
            if (__instance != null && UnityEngine.Object.FindObjectsOfType<GrabbableObject>() != null)
            {
                if (ConfigManager.isPills.Value)
                {
                    foreach (GrabbableObject scrap in UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList()
                        .Where(o => !o.isInShipRoom && !o.isInElevator && o.itemProperties.name.Equals("PillBottle") && o.scrapValue == 0))
                    {
                        ScanNodeProperties componentInChildren = ((Component)(object)scrap).gameObject.GetComponentInChildren<ScanNodeProperties>();
                        if (componentInChildren != null)
                        {
                            scrap.itemProperties.itemName = Constants.CURSE_PILLS;
                            componentInChildren.headerText = Constants.CURSE_PILLS;
                            componentInChildren.subText = "Pills to cure all active curses on the player.";
                        }
                    }
                }

                __instance.totalScrapValueInLevel = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList()
                    .Where(o => !o.isInShipRoom && !o.isInElevator && o.itemProperties.minValue > 0)
                    .Select(o => o.scrapValue)
                    .Sum();
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.EquipItem))]
        [HarmonyPostfix]
        private static void SetControlTipForItem(ref GrabbableObject __instance)
        {
            if (__instance.itemProperties.name.Equals(Constants.CURSE_PILLS))
            {
                HUDManager.Instance.ChangeControlTip(1, "Consume pills: [LMB]");
            }
        }

        private static void CurseScraps(ref NetworkObjectReference[] spawnedScrap, ref int[] scrapValues)
        {
            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (spawnedScrap[i].TryGet(out var networkObject))
                {
                    GrabbableObject scrap = networkObject.GetComponent<GrabbableObject>();
                    if (scrap != null && scrap.scrapValue > 0 && IsCursed())
                    {
                        if (i >= scrapValues.Length)
                        {
                            Debug.LogError($"spawnedScrap amount exceeded allScrapValue!: {spawnedScrap.Length}");
                            break;
                        }

                        CurseEffect curseEffect = GetRandomCurseEffect();
                        if (curseEffect != null)
                        {
                            switch (curseEffect.Name)
                            {
                                case Constants.INHIBITION:
                                    scrapValues[i] = Constants.INHIBITION_VALUE;
                                    break;
                                case Constants.CONFUSION:
                                    scrapValues[i] = Constants.CONFUSION_VALUE;
                                    break;
                                case Constants.CAPTIVE:
                                    scrapValues[i] = Constants.CAPTIVE_VALUE;
                                    break;
                                case Constants.BLURRY:
                                    scrapValues[i] = Constants.BLURRY_VALUE;
                                    break;
                                case Constants.MUTE:
                                    scrapValues[i] = Constants.MUTE_VALUE;
                                    break;
                                case Constants.DEAFNESS:
                                    scrapValues[i] = Constants.DEAFNESS_VALUE;
                                    break;
                                case Constants.ERRANT:
                                    scrapValues[i] = Constants.ERRANT_VALUE;
                                    break;
                                case Constants.PARALYSIS:
                                    scrapValues[i] = Constants.PARALYSIS_VALUE;
                                    break;
                                case Constants.SHADOW:
                                    scrapValues[i] = Constants.SHADOW_VALUE;
                                    break;
                                case Constants.SYNCHRONIZATION:
                                    scrapValues[i] = Constants.SYNCHRONIZATION_VALUE;
                                    break;
                                case Constants.DIMINUTIVE:
                                    scrapValues[i] = Constants.DIMINUTIVE_VALUE;
                                    break;
                                default:
                                    Debug.LogError($"Effect not managed in CurseScrapValue: {curseEffect.Name}");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Scrap networkobject object did not contain grabbable object!: " + networkObject.gameObject.name);
                    }
                }
                else
                {
                    Debug.LogError($"Failed to get networkobject reference for scrap. id: {spawnedScrap[i].NetworkObjectId}");
                }
            }
        }

        private static void AddNewItems(int nbCursedScraps)
        {
            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.name == "PillBottle")
                {
                    for (int i = 0; i < nbCursedScraps; i++)
                    {
                        if (IsPillSpawned()) consumableItems.Add(item);
                    }
                    break;
                }
            }
        }

        private static void SpawnNewItems(ref RoundManager __instance)
        {
            try
            {
                System.Random random = new System.Random();
                List<RandomScrapSpawn> listRandomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList();

                foreach (Item itemToSpawn in consumableItems)
                {
                    if (listRandomScrapSpawn.Count <= 0)
                    {
                        break;
                    }

                    int indexRandomScrapSpawn = random.Next(0, listRandomScrapSpawn.Count);
                    RandomScrapSpawn randomScrapSpawn = listRandomScrapSpawn[indexRandomScrapSpawn];
                    if (randomScrapSpawn.spawnedItemsCopyPosition)
                    {
                        listRandomScrapSpawn.RemoveAt(indexRandomScrapSpawn);
                    }
                    else
                    {
                        randomScrapSpawn.transform.position = __instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, __instance.navHit, __instance.AnomalyRandom) + Vector3.up * itemToSpawn.verticalOffset;
                    }

                    GameObject gameObject = UnityEngine.Object.Instantiate(itemToSpawn.spawnPrefab, randomScrapSpawn.transform.position + Vector3.up * 0.5f, Quaternion.identity, StartOfRound.Instance.propsContainer);
                    GrabbableObject scrap = gameObject.GetComponent<GrabbableObject>();
                    scrap.fallTime = 0f;
                    gameObject.GetComponent<NetworkObject>().Spawn();
                }
            }
            catch (Exception arg)
            {
                Debug.LogError($"Error in SpawnNewItems: {arg}");
            }
        }

        private static bool IsCursed()
        {
            if (new System.Random().Next(1, 100) <= ConfigManager.globalChance.Value)
            {
                return true;
            }
            return false;
        }

        private static bool IsPillSpawned()
        {
            if (new System.Random().Next(1, 100) <= ConfigManager.pillsChance.Value)
            {
                return true;
            }
            return false;
        }

        private static CurseEffect GetRandomCurseEffect()
        {
            List<CurseEffect> eligibleEffects = new List<CurseEffect>();
            string planetName = new string(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());

            // Ajout des malédictions éligibles en fonction de leur valeur d'importance
            foreach (CurseEffect effect in CursedScraps.curseEffects)
            {
                for (int i = 0; i < GetWeight(effect.Weight, planetName); i++)
                {
                    eligibleEffects.Add(effect);
                }
            }
            // Sélectionner un effet aléatoire parmi les effets éligibles
            if (eligibleEffects.Count > 0)
            {
                return eligibleEffects[new System.Random().Next(eligibleEffects.Count)];
            }
            else
            {
                return null;
            }
        }

        private static int GetWeight(string weightByMoons, string planetName)
        {
            // Tableau contenant tous les ensembles clé/valeur pour les noms de planètes/valeurs d'importances
            string[] weightPairs = weightByMoons.Split(',');
            int defaultValue = 0;

            foreach (string weightPair in weightPairs)
            {
                // Ensemble clé/valeur pour nom de la planète/valeur d'importance
                string[] weightTab = weightPair.Split(':');
                if (weightTab.Length == 2 && weightTab[0] == planetName)
                {
                    // Retourner la valeur d'importance si la clé est trouvée
                    return int.Parse(weightTab[1]);
                }
                else if (weightTab.Length == 2 && weightTab[0] == "default")
                {
                    defaultValue = int.Parse(weightTab[1]);
                }
            }
            return defaultValue;
        }

        internal static IEnumerator ChangeReflectionScrapCoroutine(GrabbableObject grabbedScrap, Vector3 position)
        {
            GrabbableObject scrapClone;
            int timePassed = 0;
            while ((scrapClone = GetCloneScrapFromPosition(grabbedScrap, position)) == null)
            {
                yield return new WaitForSeconds(1f);
                timePassed++;

                if (timePassed >= timeOut)
                {
                    scrapClone = GetCloneScrapFromValue(grabbedScrap);
                    break;
                }
            }

            if (scrapClone != null)
            {
                ScanNodeProperties componentInChildren = ((Component)(object)scrapClone).gameObject.GetComponentInChildren<ScanNodeProperties>();
                if (componentInChildren != null)
                {
                    scrapClone.scrapValue = grabbedScrap.scrapValue;
                    componentInChildren.scrapValue = grabbedScrap.scrapValue;
                    componentInChildren.subText = $"Value: ${componentInChildren.scrapValue}" + " \nCurse: " + Constants.SYNC_REFLECTION;
                }
            }
        }

        private static GrabbableObject GetCloneScrapFromPosition(GrabbableObject grabbedScrap, Vector3 position)
        {
            GrabbableObject scrapClone = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList()
                .Where(o => !o.isInShipRoom && !o.isInElevator && o.itemProperties.spawnPrefab == grabbedScrap.itemProperties.spawnPrefab && o.transform.position == position && o != grabbedScrap)
                .FirstOrDefault();
            return scrapClone;
        }

        private static GrabbableObject GetCloneScrapFromValue(GrabbableObject grabbedScrap)
        {
            GrabbableObject scrapClone = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList()
                    .Where(o => !o.isInShipRoom && !o.isInElevator && o.itemProperties.spawnPrefab == grabbedScrap.itemProperties.spawnPrefab && o.scrapValue == 0 && o != grabbedScrap)
                    .FirstOrDefault();
            return scrapClone;
        }

        private static GrabbableObject GetCloneScrapFromCurse(GrabbableObject scrapObject, string curseEffect)
        {
            string curseEffectClone;
            GrabbableObject coreScrap = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList()
                    .Where(o => (o.isInShipRoom || o.isInElevator)
                        && !string.IsNullOrEmpty(curseEffectClone = PlayerManagerPatch.GetCurseEffect(ref o))
                        && curseEffectClone.Equals(curseEffect)
                        && o.itemProperties.spawnPrefab == scrapObject.itemProperties.spawnPrefab
                        && o.scrapValue == scrapObject.scrapValue)
                    .FirstOrDefault();
            return coreScrap;
        }

        private static void RemoveObjectEffect(ref GrabbableObject scrapObject)
        {
            string curseEffect = PlayerManagerPatch.GetCurseEffect(ref scrapObject);
            if (!string.IsNullOrEmpty(curseEffect))
            {
                ScanNodeProperties componentInChildren = ((Component)(object)scrapObject).gameObject.GetComponentInChildren<ScanNodeProperties>();
                if (componentInChildren != null)
                {
                    componentInChildren.subText = $"Value: ${componentInChildren.scrapValue}";
                }
            }
        }
    }
}
