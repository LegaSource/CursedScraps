using CursedScraps.Behaviours;
using CursedScraps.CustomInputs;
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

            if (__instance.GetComponent<SORCSBehaviour>() != null) return;
            __instance.gameObject.AddComponent<SORCSBehaviour>();
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
        [HarmonyPostfix]
        public static void OnDisable()
            => CursedScrapsNetworkManager.Instance = null;

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
        [HarmonyPostfix]
        private static void PlayerConnection(ref StartOfRound __instance)
        {
            foreach (PlayerControllerB player in __instance.allPlayerScripts)
            {
                if (!player.isPlayerControlled) continue;
                if (player.GetComponent<PlayerCSBehaviour>() != null) continue;

                PlayerCSBehaviour playerBehaviour = player.gameObject.AddComponent<PlayerCSBehaviour>();
                playerBehaviour.playerProperties = player;
            }
        }

        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ReviveDeadPlayers))]
        [HarmonyPostfix]
        private static void ReviveDeadPlayers()
            => CommunicationInputs.Instance.DisableInputs();
    }
}
