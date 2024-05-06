using GameNetcodeStuff;
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
        public static GrabbableObject scrapCoopToDestroy;
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
                    case Constants.EXPLORATION_VALUE:
                        curseEffect = new CurseEffect(Constants.EXPLORATION, ConfigManager.explorationMultiplier.Value, ConfigManager.explorationWeight.Value);
                        break;
                    case Constants.COMMUNICATION_VALUE:
                        curseEffect = new CurseEffect(Constants.COMMUNICATION, ConfigManager.communicationMultiplier.Value, ConfigManager.communicationWeight.Value);
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
            if (!ProcessCoopScrap(ref scrapObject, Constants.SYNC_CORE, Constants.SYNC_REFLECTION)) return false;
            if (!ProcessCoopScrap(ref scrapObject, Constants.COMM_CORE, Constants.COMM_REFLECTION)) return false;
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
                    string planetName = new string(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
                    GrabbableObject scrap = networkObject.GetComponent<GrabbableObject>();
                    if (scrap != null && scrap.scrapValue > 0 && IsCursed(planetName))
                    {
                        if (i >= scrapValues.Length)
                        {
                            Debug.LogError($"spawnedScrap amount exceeded allScrapValue!: {spawnedScrap.Length}");
                            break;
                        }

                        CurseEffect curseEffect = GetRandomCurseEffect(planetName);
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
                                case Constants.EXPLORATION:
                                    scrapValues[i] = Constants.EXPLORATION_VALUE;
                                    break;
                                case Constants.COMMUNICATION:
                                    scrapValues[i] = Constants.COMMUNICATION_VALUE;
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
                        randomScrapSpawn.spawnUsed = true;
                        listRandomScrapSpawn.RemoveAt(indexRandomScrapSpawn);
                    }
                    else
                    {
                        randomScrapSpawn.transform.position = __instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, __instance.navHit, __instance.AnomalyRandom) + Vector3.up * itemToSpawn.verticalOffset;
                    }

                    Vector3 position = randomScrapSpawn.transform.position + Vector3.up * 0.5f;
                    SpawnScrap(ref itemToSpawn.spawnPrefab, ref position);
                }
            }
            catch (Exception arg)
            {
                Debug.LogError($"Error in SpawnNewItems: {arg}");
            }
        }

        internal static void SpawnScrap(ref GameObject spawnPrefab, ref Vector3 position)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                try
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(spawnPrefab, position, Quaternion.identity, StartOfRound.Instance.propsContainer);
                    GrabbableObject scrap = gameObject.GetComponent<GrabbableObject>();
                    scrap.fallTime = 0f;
                    gameObject.GetComponent<NetworkObject>().Spawn();
                }
                catch (Exception arg)
                {
                    Debug.LogError($"Error in SpawnScrap: {arg}");
                }
            }
        }

        private static bool IsCursed(string planetName)
        {
            if (new System.Random().Next(1, 100) <= GetPairValue(ConfigManager.globalChance.Value, planetName))
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

        private static CurseEffect GetRandomCurseEffect(string planetName)
        {
            // Ajout des malédictions éligibles en fonction de leur valeur d'importance
            List<CurseEffect> eligibleEffects = new List<CurseEffect>();
            foreach (CurseEffect effect in CursedScraps.curseEffects)
            {
                for (int i = 0; i < GetPairValue(effect.Weight, planetName); i++)
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

        private static int GetPairValue(string valueByMoons, string planetName)
        {
            // Tableau contenant tous les ensembles clé/valeur pour les noms de planètes/valeurs d'importances
            string[] valuePairs = valueByMoons.Split(',');
            int defaultValue = 0;

            foreach (string valuePair in valuePairs)
            {
                // Ensemble clé/valeur pour nom de la planète/valeur d'importance
                string[] valueTab = valuePair.Split(':');
                if (valueTab.Length == 2 && valueTab[0] == planetName)
                {
                    // Retourner la valeur d'importance si la clé est trouvée
                    return int.Parse(valueTab[1]);
                }
                else if (valueTab.Length == 2 && valueTab[0] == "default")
                {
                    defaultValue = int.Parse(valueTab[1]);
                }
            }
            return defaultValue;
        }

        internal static Vector3 GetFurthestPositionScrapSpawn(Vector3 position, ref Item itemToSpawn)
        {
            RandomScrapSpawn randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                .Where(p => !p.spawnUsed)
                .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                .FirstOrDefault();

            if (randomScrapSpawn == null)
            {
                // Au cas où, mieux vaut prendre un spawn déjà utilisé que de le faire apparaître devant le joueur
                randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                    .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                    .FirstOrDefault();
            }

            if (randomScrapSpawn.spawnedItemsCopyPosition)
            {
                randomScrapSpawn.spawnUsed = true;
            }
            else
            {
                randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * itemToSpawn.verticalOffset;
            }
            return randomScrapSpawn.transform.position + Vector3.up * 0.5f;
        }

        internal static void CloneScrap(ref GrabbableObject scrap, string nameCore, string nameReflection, ref Vector3 position, ref PlayerControllerB player)
        {
            SpawnScrap(ref scrap.itemProperties.spawnPrefab, ref position);
            GameNetworkManager.Instance.localPlayerController.StartCoroutine(ChangeCloneScrapCoroutine(scrap, position, nameReflection, player));

            ScanNodeProperties componentInChildren = ((Component)(object)scrap).gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (componentInChildren != null)
            {
                componentInChildren.subText = $"Value: ${componentInChildren.scrapValue}" + " \nCurse: " + nameCore;
            }
        }

        private static IEnumerator ChangeCloneScrapCoroutine(GrabbableObject grabbedScrap, Vector3 position, string curseEffect, PlayerControllerB player)
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

                    if (scrapClone == null) HUDManager.Instance.DisplayTip(Constants.ERROR_OCCURRED, "The scrap couldn't be cloned.");
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
                    componentInChildren.subText = $"Value: ${componentInChildren.scrapValue}" + " \nCurse: " + curseEffect;
                    if (curseEffect.Equals(Constants.COMM_REFLECTION) && player == GameNetworkManager.Instance.localPlayerController)
                    {
                        PlayerManagerPatch.trackedScrap = scrapClone;
                    }
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

        internal static GrabbableObject GetCloneScrapFromCurse(GrabbableObject scrapObject, string curseEffect, bool isInShip = true)
        {
            string curseEffectClone;
            GrabbableObject coreScrap = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList()
                    .Where(o => ((isInShip && (o.isInShipRoom || o.isInElevator)) || (!isInShip && !o.isInShipRoom && !o.isInElevator))
                        && !string.IsNullOrEmpty(curseEffectClone = PlayerManagerPatch.GetCurseEffect(ref o))
                        && curseEffectClone.Equals(curseEffect)
                        && o.itemProperties.spawnPrefab == scrapObject.itemProperties.spawnPrefab
                        && o.scrapValue == scrapObject.scrapValue)
                    .FirstOrDefault();
            return coreScrap;
        }

        private static bool ProcessCoopScrap(ref GrabbableObject scrapObject, string coreEffect, string reflectionEffect)
        {
            string curseEffect = PlayerManagerPatch.GetCurseEffect(ref scrapObject);
            if (!string.IsNullOrEmpty(curseEffect) && curseEffect.Equals(coreEffect))
            {
                GrabbableObject reflectionScrap = GetCloneScrapFromCurse(scrapObject, reflectionEffect);
                if (reflectionScrap == null)
                {
                    return false;
                }
                else
                {
                    scrapCoopToDestroy = reflectionScrap;
                }
            }
            else if (!string.IsNullOrEmpty(curseEffect) && curseEffect.Equals(reflectionEffect))
            {
                GrabbableObject coreScrap = GetCloneScrapFromCurse(scrapObject, coreEffect);
                if (coreScrap == null)
                {
                    return false;
                }
                else
                {
                    scrapCoopToDestroy = scrapObject;
                    scrapObject = coreScrap;
                }
            }
            return true;
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
