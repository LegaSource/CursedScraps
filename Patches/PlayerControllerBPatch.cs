using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.CustomInputs;
using CursedScraps.Managers;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using Unity.Netcode;
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
        {
            if (__instance.currentlyHeldObjectServer != null)
            {
                ObjectCSBehaviour objectBehaviour = __instance.currentlyHeldObjectServer.GetComponent<ObjectCSBehaviour>();
                if (objectBehaviour != null)
                {
                    CursedScrapsNetworkManager.Instance.EnableParticleServerRpc(__instance.currentlyHeldObjectServer.GetComponent<NetworkObject>(), false);
                    // Affectation des malédictions au joueur
                    foreach (CurseEffect curseEffect in objectBehaviour.curseEffects)
                    {
                        PlayerCSManager.SetPlayerCurseEffect(__instance, curseEffect, true);
                    }
                }

                // Comportements spécifiques pour les malédictions au moment du grab
                PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
                if (playerBehaviour != null)
                {
                    if (!__instance.currentlyHeldObjectServer.itemProperties.itemName.Equals("Saw Tape")
                        && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.ERRANT)))
                    {
                        PlayerCSManager.TeleportPlayer(ref __instance);
                    }
                    if (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.DIMINUTIVE)))
                    {
                        __instance.currentlyHeldObjectServer.transform.localScale = __instance.currentlyHeldObjectServer.originalScale / 5;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Jump_performed))]
        [HarmonyPrefix]
        private static bool PreventJump(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                if ((!string.IsNullOrEmpty(playerBehaviour.blockedAction) && playerBehaviour.blockedAction.Equals("Jump")) || playerBehaviour.actionsBlockedBy.Count > 0)
                {
                    return false;
                }
                else if (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.DIMINUTIVE))
                    && !__instance.isExhausted
                    && __instance.playerBodyAnimator.GetBool("Jumping")
                    && !playerBehaviour.doubleJump)
                {
                    __instance.StartCoroutine(Diminutive.PlayerDoubleJump(playerBehaviour));
                    return false;
                }
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
                {
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static bool PreDropObject(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                // Cas annulations du drop
                if (!__instance.isInHangarShipRoom)
                {
                    if (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.CAPTIVE)))
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you to drop this object.");
                        return false;
                    }

                    if (!__instance.isCrouching && !__instance.currentlyHeldObjectServer.deactivated && Fragile.DestroyHeldObject(ref playerBehaviour))
                    {
                        return false;
                    }
                }

                ObjectCSBehaviour objectBehaviour = __instance.currentlyHeldObjectServer.GetComponent<ObjectCSBehaviour>();
                if (objectBehaviour != null
                    && objectBehaviour.curseEffects.Count > 0)
                {
                    // Suppression des malédictions
                    if (__instance.isInHangarShipRoom)
                    {
                        CursedScrapsNetworkManager.Instance.RemoveAllScrapCurseEffectServerRpc(__instance.currentlyHeldObjectServer.GetComponent<NetworkObject>());
                    }
                    else
                    {
                        CursedScrapsNetworkManager.Instance.EnableParticleServerRpc(__instance.currentlyHeldObjectServer.GetComponent<NetworkObject>(), true);
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPostfix]
        private static void PostDropObject(ref PlayerControllerB __instance)
        {
            // Faire la tp après le drop pour que l'objet reste sur place
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null
                && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.ERRANT))
                && !playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.CAPTIVE)))
            {
                PlayerCSManager.TeleportPlayer(ref __instance);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.PlayerHitGroundEffects))]
        [HarmonyPostfix]
        private static void PlayerFall(ref PlayerControllerB __instance)
        {
            if (__instance.fallValueUncapped < -20f && !__instance.isSpeedCheating)
            {
                PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
                Fragile.DestroyHeldObjects(ref playerBehaviour);
            }
        }

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
                    {
                        PlayerCSManager.SetPlayerCurseEffect(__instance, curseEffect, false);
                    }
                }

                // Gestion de la collision pour DIMINUTIVE
                if (__instance == GameNetworkManager.Instance.localPlayerController
                    && !playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.DIMINUTIVE)))
                {
                    foreach (Collider collider in Physics.OverlapSphere(__instance.transform.position, 0.65f, StartOfRound.Instance.playersMask))
                    {
                        PlayerCSBehaviour playerBehaviourPushed = collider.GetComponent<PlayerControllerB>()?.GetComponent<PlayerCSBehaviour>();
                        if (playerBehaviourPushed != null
                            && playerBehaviourPushed.playerProperties != __instance
                            && playerBehaviourPushed.activeCurses.Any(c => c.CurseName.Equals(Constants.DIMINUTIVE)))
                        {
                            if (__instance.isFallingFromJump)
                            {
                                CursedScrapsNetworkManager.Instance.KillPlayerServerRpc((int)playerBehaviourPushed.playerProperties.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Crushing);
                            }
                            else
                            {
                                Vector3 direction = (playerBehaviourPushed.playerProperties.transform.position - __instance.thisController.transform.position).normalized;
                                CursedScrapsNetworkManager.Instance.PushPlayerServerRpc((int)playerBehaviourPushed.playerProperties.playerClientId, direction * __instance.thisController.velocity.magnitude * 0.2f);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPostfix]
        private static void DamagePlayer(ref PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || __instance.isPlayerDead || !__instance.AllowPlayerDeath())
            {
                return;
            }
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            Fragile.DestroyHeldObjects(ref playerBehaviour);
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPrefix]
        private static void PlayerDeath(ref PlayerControllerB __instance)
        {
            CursedScrapsNetworkManager.Instance.RemoveAllPlayerCurseEffectServerRpc((int)__instance.playerClientId);
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.TeleportPlayer))]
        [HarmonyPrefix]
        private static bool PreventTeleportPlayer(ref PlayerControllerB __instance)
        {
            if (__instance == GameNetworkManager.Instance.localPlayerController)
            {
                PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
                if (!__instance.isInsideFactory
                    && playerBehaviour != null
                    && (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.EXPLORATION))
                        || !Communication.CanEscape(ref playerBehaviour, "A curse prevented you from being teleported.")))
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
                HUDCSManager.RefreshCursesText(ref playerBehaviour);
                if (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.COMMUNICATION)))
                {
                    Communication.ApplyCommunicationForDeadPlayer(ref playerBehaviour);
                }
                CommunicationInputs.Instance.EnableInputs();
            }
        }
    }
}
