using CursedScraps.Behaviours;
using CursedScraps.Managers;
using CursedScraps.Values;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class RoundManagerPatch
    {
        private static bool hasBeenExecutedOnHost = false;

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPostfix]
        private static void LoadNewGame()
        {
            hasBeenExecutedOnHost = false;

            SORCSBehaviour sorBehaviour = StartOfRound.Instance.GetComponent<SORCSBehaviour>();
            sorBehaviour.counter = 0;
            sorBehaviour.scannedObjects.Clear();
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
        [HarmonyPostfix]
        private static void SpawnScraps(ref RoundManager __instance)
        {
            AddNewItems(ref __instance);
        }

        private static void AddNewItems(ref RoundManager roundManager)
        {
            foreach (CustomItem customItem in CursedScraps.customItems.Where(i => i.IsSpawnable))
            {
                for (int i = 0; i < customItem.MaxSpawn; i++)
                {
                    if (new System.Random().Next(1, 100) <= customItem.Rarity)
                    {
                        CSObjectManager.SpawnNewItem(ref roundManager, customItem.Item);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SyncScrapValuesClientRpc))]
        [HarmonyPostfix]
        private static void SetCurseObject()
        {
            if (!hasBeenExecutedOnHost
                && GameNetworkManager.Instance?.localPlayerController != null
                && (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer))
            {
                hasBeenExecutedOnHost = true;
                foreach (GrabbableObject grabbableObject in Object.FindObjectsOfType<GrabbableObject>()
                    .Where(g => (string.IsNullOrEmpty(ConfigManager.scrapExclusions.Value) || !ConfigManager.scrapExclusions.Value.Contains(g.itemProperties.itemName))
                                && g.isInFactory
                                && !g.isInShipRoom
                                && g.scrapValue > 0)
                    .ToList())
                {
                    string planetName = new(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
                    if (IsCursed(planetName))
                    {
                        CurseEffect curseEffect = GetRandomCurseEffect(planetName);
                        if (curseEffect != null)
                        {
                            NetworkObject networkObject = grabbableObject.GetComponent<NetworkObject>();
                            if (networkObject != null && networkObject.IsSpawned)
                            {
                                CursedScrapsNetworkManager.Instance.SetScrapCurseEffectServerRpc(networkObject, curseEffect.CurseName);
                            }
                        }
                    }
                }
            }
        }

        private static bool IsCursed(string planetName)
        {
            if (new System.Random().Next(1, 100) <= GetValueFromPair(ConfigManager.globalChance.Value, planetName))
            {
                return true;
            }
            return false;
        }

        private static CurseEffect GetRandomCurseEffect(string planetName)
        {
            bool isMultiplayer = StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isTestingPlayer).Count() > 1;
            // Ajout des malédictions éligibles en fonction de leur valeur d'importance
            List<CurseEffect> eligibleEffects = new List<CurseEffect>();
            foreach (CurseEffect effect in CursedScraps.curseEffects.Where(c => isMultiplayer || !c.IsCoop))
            {
                for (int i = 0; i < GetValueFromPair(effect.Weight, planetName); i++)
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

        private static int GetValueFromPair(string valueByMoons, string planetName)
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

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.CollectNewScrapForThisRound))]
        [HarmonyPrefix]
        private static bool CollectScrap(ref GrabbableObject scrapObject)
        {
            ObjectCSBehaviour objectBehaviour = scrapObject.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.FirstOrDefault(c => c.IsCoop) != null)
            {
                return CSObjectManager.IsCloneOnShip(ref scrapObject);
            }
            return true;
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DetectElevatorIsRunning))]
        [HarmonyPrefix]
        private static void EndGame()
        {
            // Destruction des objets qui possèdent toujours une malédiction en étant dans le vaisseau
            ObjectCSBehaviour objectBehaviour;
            foreach (GrabbableObject grabbableObject in Object.FindObjectsOfType<GrabbableObject>().Where(g => g.isInElevator && (objectBehaviour = g.GetComponent<ObjectCSBehaviour>()) != null && objectBehaviour.curseEffects.Count > 0))
            {
                CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(grabbableObject.GetComponent<NetworkObject>());
            }
            // Tuer les joueurs qui possèdent une malédiction en coop
            PlayerCSBehaviour playerBehaviour;
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts.Where(p => (playerBehaviour = p.GetComponent<PlayerCSBehaviour>()) != null && playerBehaviour.activeCurses.FirstOrDefault(c => c.IsCoop) != null))
            {
                player.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
            }
        }
    }
}
