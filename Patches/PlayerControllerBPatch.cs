using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.CustomInputs;
using CursedScraps.Managers;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
        [HarmonyPostfix]
        private static void ConnectPlayer(ref PlayerControllerB __instance)
        {
            if (__instance.isPlayerControlled && __instance.GetComponent<PlayerCSBehaviour>() == null)
            {
                PlayerCSBehaviour playerBehaviour = __instance.gameObject.AddComponent<PlayerCSBehaviour>();
                playerBehaviour.playerProperties = __instance;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
        [HarmonyPostfix]
        private static void UpdatePlayerControllerB(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null && playerBehaviour.targetDoor != null)
            {
                if (Vector3.Distance(playerBehaviour.targetDoor.transform.position, __instance.transform.position) > ConfigManager.explorationDistance.Value)
                {
                    if (!playerBehaviour.isRendered) CustomPassManager.SetupCustomPassForDoor(playerBehaviour.targetDoor);
                    playerBehaviour.isRendered = true;
                }
                else
                {
                    CustomPassManager.RemoveAuraFromDoor();
                    playerBehaviour.isRendered = false;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.GrabObjectClientRpc))]
        [HarmonyPostfix]
        private static void PostGrabObject(ref PlayerControllerB __instance)
            => ObjectCSManager.PostGrabObject(__instance, __instance.currentlyHeldObjectServer);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Jump_performed))]
        [HarmonyPrefix]
        private static bool PreventJump(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                if ((!string.IsNullOrEmpty(playerBehaviour.blockedAction) && playerBehaviour.blockedAction.Equals("Jump")) || playerBehaviour.actionsBlockedBy.Count > 0)
                    return false;
                if (Diminutive.PreventJump(playerBehaviour)) return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Crouch_performed))]
        [HarmonyPrefix]
        private static bool PreventCrouch(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                if ((!string.IsNullOrEmpty(playerBehaviour.blockedAction) && playerBehaviour.blockedAction.Equals("Crouch")) || playerBehaviour.actionsBlockedBy.Count > 0)
                    return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static bool PreDropObject(ref PlayerControllerB __instance)
            => ObjectCSManager.PreDropObject(__instance, __instance.currentlyHeldObjectServer);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPostfix]
        private static void PostDropObject(ref PlayerControllerB __instance)
            => ObjectCSManager.PostDropObject(__instance);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.PlayerHitGroundEffects))]
        [HarmonyPostfix]
        private static void PlayerFall(ref PlayerControllerB __instance)
            => Fragile.PlayerFall(__instance);

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.UpdatePlayerPositionClientRpc))]
        [HarmonyPostfix]
        private static void UpdatePlayerPositionClientRpc(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                if (__instance.isInHangarShipRoom)
                {
                    // Suppression des malédictions
                    foreach (CurseEffect curseEffect in playerBehaviour.activeCurses.ToList())
                        PlayerCSManager.SetPlayerCurseEffect(__instance, curseEffect, false);
                }
                Diminutive.PlayerCollision(playerBehaviour);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPrefix]
        private static bool PreDamagePlayer(ref PlayerControllerB __instance, ref int damageNumber)
        {
            if (!__instance.IsOwner || __instance.isPlayerDead || !__instance.AllowPlayerDeath()) return true;

            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (Sacrifice.DamagePlayer(playerBehaviour, damageNumber)) return false;

            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPostfix]
        private static void PostDamagePlayer(ref PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || __instance.isPlayerDead || !__instance.AllowPlayerDeath()) return;

            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            Fragile.DestroyHeldObjects(playerBehaviour);
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPrefix]
        private static bool PreKillPlayer(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            OneForAll.KillPlayer(playerBehaviour);
            bool killPlayer = !Sacrifice.KillPlayer(playerBehaviour);

            CursedScrapsNetworkManager.Instance.RemoveAllPlayerCurseEffectServerRpc((int)__instance.playerClientId);

            return killPlayer;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool PreventTeleportPlayer(ref PlayerControllerB __instance)
        {
            if (__instance == GameNetworkManager.Instance.localPlayerController)
            {
                PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
                if (!__instance.isInsideFactory
                    && (Exploration.IsExploration(playerBehaviour)
                        || !Communication.CanEscape(playerBehaviour, "A curse prevented you from being teleported.")))
                {
                    __instance.isInElevator = false;
                    __instance.isInHangarShipRoom = false;
                    __instance.isInsideFactory = true;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SpectateNextPlayer))]
        [HarmonyPostfix]
        private static void SwitchSpectatedPlayer(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.spectatedPlayerScript?.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                HUDCSManager.RefreshCursesText(playerBehaviour);
                Communication.ApplyCommunicationForDeadPlayer(playerBehaviour);
                CommunicationInputs.Instance.EnableInputs();
            }
        }
    }
}
