using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
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
            => ObjectCSManager.AddNewItems(__instance);

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SyncScrapValuesClientRpc))]
        [HarmonyPostfix]
        private static void SetCurseObject()
        {
            if (hasBeenExecutedOnHost) return;
            if (GameNetworkManager.Instance?.localPlayerController == null) return;
            if (!(GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer)) return;

            hasBeenExecutedOnHost = true;
            foreach (GrabbableObject grabbableObject in Object.FindObjectsOfType<GrabbableObject>())
            {
                if (string.IsNullOrEmpty(grabbableObject.itemProperties?.itemName)) continue;
                if (!(string.IsNullOrEmpty(ConfigManager.scrapExclusions.Value) || !ConfigManager.scrapExclusions.Value.Contains(grabbableObject.itemProperties.itemName))) continue;
                if (!grabbableObject.isInFactory) continue;
                if (grabbableObject.isInShipRoom) continue;
                if (grabbableObject.scrapValue <= 0) continue;

                string planetName = new(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
                if (!CurseCSManager.IsCursed(planetName)) continue;

                CurseEffect curseEffect = CurseCSManager.GetRandomCurseEffect(planetName);
                if (curseEffect == null) continue;

                NetworkObject networkObject = grabbableObject.GetComponent<NetworkObject>();
                if (networkObject == null || !networkObject.IsSpawned) continue;
                    
                CursedScrapsNetworkManager.Instance.SetScrapCurseEffectServerRpc(networkObject, curseEffect.CurseName);
            }
        }
    }
}
