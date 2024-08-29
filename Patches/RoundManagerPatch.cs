using CursedScraps.Behaviours;
using CursedScraps.Managers;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class RoundManagerPatch
    {
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
