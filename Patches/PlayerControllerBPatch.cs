﻿using CursedScraps.Behaviours;
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

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.BeginGrabObject))]
        [HarmonyPrefix]
        private static bool PreventGrabObject(ref PlayerControllerB __instance)
        {
            __instance.interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
            if (!Physics.Raycast(__instance.interactRay, out __instance.hit, __instance.grabDistance, __instance.interactableObjectsMask) || __instance.hit.collider.gameObject.layer == 8 || !(__instance.hit.collider.tag == "PhysicsProp") || __instance.twoHanded || __instance.sinkingValue > 0.73f || Physics.Linecast(__instance.gameplayCamera.transform.position, __instance.hit.collider.transform.position + __instance.transform.up * 0.16f, 1073741824, QueryTriggerInteraction.Ignore))
            {
                return true;
            }
            GrabbableObject grabbableObject = __instance.hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
            if (grabbableObject != null)
            {
                ObjectCSBehaviour objectCSBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
                if (objectCSBehaviour != null && objectCSBehaviour.playerOwner != null && objectCSBehaviour.playerOwner != GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "You are not the owner of this object.");
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.GrabObjectClientRpc))]
        [HarmonyPostfix]
        private static void PostGrabObject(ref PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer != null)
            {
                ObjectCSBehaviour objectBehaviour = __instance.currentlyHeldObjectServer.GetComponent<ObjectCSBehaviour>();
                // Affectation de la malédiction au joueur
                if (objectBehaviour != null)
                {
                    foreach (CurseEffect curseEffect in objectBehaviour.curseEffects)
                    {
                        CSPlayerManager.SetPlayerCurseEffect(__instance, curseEffect, true);
                    }
                }

                // Comportements spécifiques pour les malédictions au moment du grab
                PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
                if (playerBehaviour != null)
                {
                    if (playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.ERRANT)) != null)
                    {
                        CSPlayerManager.TeleportPlayer(ref __instance);
                    }
                    if (playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.DIMINUTIVE)) != null)
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
                if (playerBehaviour.activeCurses.Select(c => c.CurseName).Contains(Constants.INHIBITION) || playerBehaviour.actionsBlockedBy.Count > 0)
                {
                    return false;
                }
                else if (playerBehaviour.activeCurses.Select(c => c.CurseName).Contains(Constants.DIMINUTIVE)
                    && !__instance.isExhausted
                    && __instance.playerBodyAnimator.GetBool("Jumping")
                    && !playerBehaviour.doubleJump)
                {
                    __instance.StartCoroutine(CSPlayerManager.PlayerDoubleJump(playerBehaviour));
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
                if (playerBehaviour.activeCurses.Select(c => c.CurseName).Contains(Constants.INHIBITION) || playerBehaviour.actionsBlockedBy.Count > 0)
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
                if (playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.CAPTIVE)) != null
                    && !__instance.isInHangarShipRoom)
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you to drop this object.");
                    return false;
                }

                ObjectCSBehaviour objectBehaviour = __instance.currentlyHeldObjectServer.GetComponent<ObjectCSBehaviour>();
                if (objectBehaviour != null
                    && objectBehaviour.curseEffects.Count > 0)
                {
                    CurseEffect curseEffect = objectBehaviour.curseEffects.FirstOrDefault(c => c.IsCoop);
                    if (curseEffect != null)
                    {
                        // Si object coop en phase de recherche d'un joueur, on peut désactiver l'effet au drop
                        if (objectBehaviour.playerOwner == null)
                        {
                            CSPlayerManager.DesactiveCoopEffect(ref playerBehaviour, curseEffect, false);
                        }
                        // Sinon impossible de drop l'objet
                        else if (!__instance.isInHangarShipRoom)
                        {
                            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you to drop this object.");
                            return false;
                        }
                    }

                    // Suppression des malédictions
                    if (__instance.isInHangarShipRoom && CSPlayerManager.RemoveCoopEffects(ref objectBehaviour))
                    {
                        CursedScrapsNetworkManager.Instance.RemoveAllScrapCurseEffectServerRpc(__instance.currentlyHeldObjectServer.GetComponent<NetworkObject>());
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
                && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.ERRANT)) != null
                && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.CAPTIVE)) == null)
            {
                CSPlayerManager.TeleportPlayer(ref __instance);
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
                    // Suppression des malédictions qui ne sont pas en coop
                    foreach (CurseEffect curseEffect in playerBehaviour.activeCurses.Where(c => !c.IsCoop).ToList())
                    {
                        CSPlayerManager.SetPlayerCurseEffect(__instance, curseEffect, false);
                    }
                }

                if (__instance == GameNetworkManager.Instance.localPlayerController)
                {
                    foreach (Collider collider in Physics.OverlapSphere(__instance.transform.position, 0.65f, StartOfRound.Instance.playersMask))
                    {
                        PlayerCSBehaviour playerBehaviourPushed = collider.GetComponent<PlayerControllerB>()?.GetComponent<PlayerCSBehaviour>();
                        if (playerBehaviourPushed != null
                            && playerBehaviourPushed.playerProperties != __instance
                            && playerBehaviourPushed.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.DIMINUTIVE)) != null)
                        {
                            Vector3 direction = (playerBehaviourPushed.playerProperties.transform.position - __instance.thisController.transform.position).normalized;
                            CursedScrapsNetworkManager.Instance.PushPlayerServerRpc((int)playerBehaviourPushed.playerProperties.playerClientId, direction * __instance.thisController.velocity.magnitude);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPrefix]
        private static void PlayerDeath(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                foreach (CurseEffect curseEffect in playerBehaviour.activeCurses.ToList())
                {
                    CSPlayerManager.DesactiveCoopEffect(ref playerBehaviour, curseEffect, true);
                    if (!curseEffect.IsCoop)
                    {
                        CSPlayerManager.SetPlayerCurseEffect(__instance, curseEffect, false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ItemTertiaryUse_performed))]
        [HarmonyPostfix]
        private static void TertiaryPressed(ref PlayerControllerB __instance)
        {
            PlayerCSBehaviour playerBehaviour = __instance.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null
                && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.SYNCHRONIZATION)) != null
                && playerBehaviour.coopPlayer != null)
            {
                // Seulement si l'objet synchronisé est équipé
                ObjectCSBehaviour objectBehaviour = playerBehaviour.playerProperties.currentlyHeldObjectServer?.GetComponent<ObjectCSBehaviour>();
                if (objectBehaviour != null && objectBehaviour.curseEffects.FirstOrDefault(c => c.CurseName.Equals(Constants.SYNCHRONIZATION)) != null)
                {
                    Vector3 directionToPlayer = __instance.playerGlobalHead.transform.position - playerBehaviour.coopPlayer.transform.position;
                    CursedScrapsNetworkManager.Instance.ForcePlayerRotationServerRpc((int)playerBehaviour.coopPlayer.playerClientId, directionToPlayer);
                }
            }
        }
    }
}
