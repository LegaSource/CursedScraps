using CursedScraps.Managers;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Patches;

public class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    [HarmonyBefore(["evaisa.lethallib"])]
    [HarmonyPostfix]
    private static void StartRound(ref StartOfRound __instance)
    {
        if (NetworkManager.Singleton.IsHost && CursedScrapsNetworkManager.Instance == null)
        {
            GameObject gameObject = Object.Instantiate(CursedScraps.managerPrefab, __instance.transform.parent);
            gameObject.GetComponent<NetworkObject>().Spawn();
            CursedScraps.mls.LogInfo("Spawning CursedScrapsNetworkManager");
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable() => CursedScrapsNetworkManager.Instance = null;
}
