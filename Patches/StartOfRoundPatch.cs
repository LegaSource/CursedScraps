using CursedScraps.Behaviours;
using CursedScraps.Managers;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        [HarmonyBefore(["evaisa.lethallib"])]
        [HarmonyPostfix]
        private static void StartRound(ref StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                if (CursedScrapsNetworkManager.Instance == null)
                {
                    GameObject gameObject = Object.Instantiate(CursedScraps.managerPrefab, __instance.transform.parent);
                    gameObject.GetComponent<NetworkObject>().Spawn();
                    CursedScraps.mls.LogInfo("Spawning CursedScrapsNetworkManager");
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            CursedScrapsNetworkManager.Instance = null;
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
        [HarmonyPostfix]
        private static void PlayerConnection(ref StartOfRound __instance)
        {
            foreach (PlayerControllerB player in __instance.allPlayerScripts)
            {
                if (player.isPlayerControlled && player.GetComponent<PlayerCSBehaviour>() == null)
                {
                    PlayerCSBehaviour playerBehaviour = player.gameObject.AddComponent<PlayerCSBehaviour>();
                    playerBehaviour.playerProperties = player;
                }
            }
        }
    }
}
