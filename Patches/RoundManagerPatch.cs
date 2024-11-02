using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
using CursedScraps.Values;
using HarmonyLib;
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
            ObjectCSManager.AddNewItems(ref __instance);
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
                    if (CurseCSManager.IsCursed(planetName))
                    {
                        CurseEffect curseEffect = CurseCSManager.GetRandomCurseEffect(planetName);
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
        }
    }
}
