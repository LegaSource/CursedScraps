using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
using GameNetcodeStuff;
using HarmonyLib;
using LegaFusionCore.Utilities;

namespace CursedScraps.Patches;

public class PlayerControllerBPatch
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.GrabObjectClientRpc))]
    [HarmonyPostfix]
    private static void PostGrabObject(ref PlayerControllerB __instance) => ObjectCSManager.PostGrabObject(__instance, __instance.currentlyHeldObjectServer);

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
    [HarmonyPrefix]
    private static bool PreDropObject(ref PlayerControllerB __instance) => ObjectCSManager.PreDropObject(__instance, __instance.currentlyHeldObjectServer);

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
    [HarmonyPostfix]
    private static void PostDropObject(ref PlayerControllerB __instance) => ObjectCSManager.PostDropObject(__instance);

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.PlayerHitGroundEffects))]
    [HarmonyPostfix]
    private static void PlayerFall(ref PlayerControllerB __instance) => Fragile.PlayerFall(__instance);

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.UpdatePlayerPositionClientRpc))]
    [HarmonyPostfix]
    private static void UpdatePlayerPositionClientRpc(ref PlayerControllerB __instance) => Diminutive.PlayerCollision(__instance);

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
    [HarmonyPrefix]
    private static bool PreDamagePlayer(ref PlayerControllerB __instance, ref int damageNumber)
        => !__instance.IsOwner
        || __instance.isPlayerDead
        || !__instance.AllowPlayerDeath()
        || !Sacrifice.DamagePlayer(__instance, damageNumber);

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
    [HarmonyPostfix]
    private static void PostDamagePlayer(ref PlayerControllerB __instance)
    {
        if (!LFCUtilities.IsServer || __instance.isPlayerDead || !__instance.AllowPlayerDeath()) return;
        Fragile.DestroyHeldObjects(__instance);
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    [HarmonyPrefix]
    private static bool PreKillPlayer(ref PlayerControllerB __instance)
    {
        OneForAll.KillPlayer(__instance);
        bool killPlayer = !Sacrifice.KillPlayer(__instance);
        CursedScrapsNetworkManager.Instance.ClearPlayerCursesEveryoneRpc((int)__instance.playerClientId);

        return killPlayer;
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SpectateNextPlayer))]
    [HarmonyPostfix]
    private static void SwitchSpectatedPlayer(ref PlayerControllerB __instance) => HUDManagerPatch.RefreshCursesText(__instance.spectatedPlayerScript);
}
