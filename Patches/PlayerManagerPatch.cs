using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine.Rendering;
using System;
using Unity.Netcode;
using System.Collections.Generic;
using DunGen;

namespace CursedScraps.Patches
{
    internal class PlayerManagerPatch : NetworkBehaviour
    {
        internal static int curseCounter = 0;
        internal static List<string> activeCurses = new List<string>();
        internal static List<string> actionsBlockedBy = new List<string>();
        private static GrabbableObject lastGrabbedObject;
        private static List<PlayerControllerB> immunedPlayers = new List<PlayerControllerB>();
        private static bool hasCoopCurseActive = false;
        // DEAFNESS
        private static float savedMasterVolume = 0f;
        // SYNCHRONIZATION
        private static PlayerControllerB switchedPlayer;
        // DIMINUTIVE
        private static Vector3 originalScale;
        private static readonly Vector3 DIMINUTIVE_SCALE = new Vector3(0.2f, 0.2f, 0.2f);
        private static bool doubleJump = false;
        // COMMUNICATION
        internal static PlayerControllerB communicationPlayer;
        internal static GrabbableObject trackedScrap;

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(ref PlayerControllerB __instance)
        {
            if (__instance.transform.localScale == DIMINUTIVE_SCALE)
            {
                SoundManager.Instance.playerVoicePitchTargets[__instance.playerClientId] = 5f;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "ActivateItem_performed")]
        [HarmonyPostfix]
        private static void UseItem(ref PlayerControllerB __instance)
        {
            if ((bool)AccessTools.Method(typeof(PlayerControllerB), "CanUseItem").Invoke(__instance, null)
                && __instance.currentlyHeldObjectServer.itemProperties.itemName.Equals(Constants.CURSE_PILLS))
            {
                if (immunedPlayers.Contains(__instance))
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "An immunity for the next picked up cursed scrap is already active.");
                }
                else
                {
                    for (int i = 0; i < __instance.ItemSlots.Length; i++)
                    {
                        if (__instance.ItemSlots[i] == __instance.currentlyHeldObjectServer)
                        {
                            __instance.DestroyItemInSlotAndSync(i);
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DestroyItemInSlot))]
        [HarmonyPrefix]
        private static void AddImmunedPlayer(ref int itemSlot, ref PlayerControllerB __instance)
        {
            if (__instance.ItemSlots[itemSlot] != null && __instance.ItemSlots[itemSlot].itemProperties.itemName.Equals(Constants.CURSE_PILLS))
            {
                if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost)
                {
                    immunedPlayers.Add(__instance);
                }
                immunedPlayers.Add(__instance);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Jump_performed")]
        [HarmonyPrefix]
        private static bool PreventJump(ref PlayerControllerB __instance)
        {
            if (activeCurses.Count > 0)
            {
                if (activeCurses.Contains(Constants.INHIBITION) || actionsBlockedBy.Count > 0)
                {
                    return false;
                }
                else if (activeCurses.Contains(Constants.DIMINUTIVE) && !__instance.isExhausted && __instance.playerBodyAnimator.GetBool("Jumping") && !doubleJump)
                {
                    __instance.StartCoroutine(PlayerDoubleJump(__instance));
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Crouch_performed")]
        [HarmonyPrefix]
        private static bool PreventCrouch()
        {
            if (activeCurses.Contains(Constants.INHIBITION) || actionsBlockedBy.Count > 0)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
        [HarmonyPrefix]
        private static bool PreventGrabObject(ref RaycastHit ___hit)
        {
            GrabbableObject grabbedScrap = ___hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
            string curseEffect = GetCurseEffect(ref grabbedScrap);
            if (!string.IsNullOrEmpty(curseEffect))
            {
                if (IsCoopCursed(curseEffect) || (communicationPlayer != null && !curseEffect.Equals(Constants.COMM_REFLECTION)))
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "You already have an active coop curse.");
                    return false;
                }

                if (curseEffect.Equals(Constants.COMM_REFLECTION))
                {
                    GrabbableObject scrap;
                    if (communicationPlayer == null)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "You are not the selected player.");
                        return false;
                    }
                    else if ((scrap = ItemManagerPatch.GetCloneScrapFromCurse(grabbedScrap, Constants.COMM_CORE, false)) != null && !scrap.isHeld)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "The core scrap has to be held for that.");
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectClientRpc")]
        [HarmonyPrefix]
        private static void PreGrabObjectClient(ref bool grabValidated, ref NetworkObjectReference grabbedObject, ref PlayerControllerB __instance)
        {
            if (grabValidated)
            {
                grabbedObject.TryGet(out var networkObject);
                GrabbableObject grabbedScrap = networkObject.GetComponent<GrabbableObject>();
                string curseEffect = GetCurseEffect(ref grabbedScrap);

                if (__instance == GameNetworkManager.Instance.localPlayerController)
                {
                    if (grabbedScrap != null && (lastGrabbedObject == null || grabbedScrap != lastGrabbedObject))
                    {
                        lastGrabbedObject = grabbedScrap;
                        if (activeCurses.Contains(Constants.ERRANT))
                        {
                            TeleportPlayer(ref __instance);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(curseEffect))
                {
                    if (grabbedScrap.transform.localScale == grabbedScrap.originalScale
                        && (__instance.transform.localScale == DIMINUTIVE_SCALE
                        || (curseEffect.Equals(Constants.DIMINUTIVE) && !immunedPlayers.Contains(__instance))))
                    {
                        grabbedScrap.transform.localScale /= 5;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectClientRpc")]
        [HarmonyPostfix]
        private static void PostGrabObjectClient(ref bool grabValidated, ref NetworkObjectReference grabbedObject, ref PlayerControllerB __instance)
        {
            if (grabValidated)
            {
                grabbedObject.TryGet(out var networkObject);
                GrabbableObject grabbedScrap = networkObject.GetComponent<GrabbableObject>();
                string curseEffect = GetCurseEffect(ref grabbedScrap);
                if (!string.IsNullOrEmpty(curseEffect))
                {
                    if (!immunedPlayers.Contains(__instance))
                    {
                        curseCounter = 0;
                        if (curseEffect.Equals(Constants.SYNCHRONIZATION))
                        {
                            Vector3 position = grabbedScrap.transform.position;
                            ItemManagerPatch.CloneScrap(ref grabbedScrap, Constants.SYNC_CORE, Constants.SYNC_REFLECTION, ref position, ref __instance);
                            // Immobiliser le joueur
                            if (GameNetworkManager.Instance.localPlayerController == __instance)
                            {
                                EnablePlayerActions(Constants.SYNCHRONIZATION, false);
                            }
                        }
                        // Si le joueur qui prend l'objet est soi-même, on applique la malédiction si on trouve un joueur possédant l'autre partie. Sinon on bloque ses mouvements
                        else if (!activeCurses.Contains(Constants.SYNCHRONIZATION) && (curseEffect.Equals(Constants.SYNC_REFLECTION) || curseEffect.Equals(Constants.SYNC_CORE)) && GameNetworkManager.Instance.localPlayerController == __instance)
                        {
                            ApplySynchronization(ref GameNetworkManager.Instance.localPlayerController, ref grabbedScrap);
                        }
                        /* Si le joueur qui prend l'objet est quelqu'un d'autre et que l'on possède l'une des parties, on applique la même logique mais jusqu'à le trouver
                         * => L'information du joueur qui a saisi l'objet peut prendre du temps à arriver */
                        else if (!activeCurses.Contains(Constants.SYNCHRONIZATION) && (curseEffect.Equals(Constants.SYNC_REFLECTION) || curseEffect.Equals(Constants.SYNC_CORE)) && GameNetworkManager.Instance.localPlayerController != __instance)
                        {
                            if (HasCursedScrapClone(ref GameNetworkManager.Instance.localPlayerController, ref grabbedScrap, true))
                            {
                                GameNetworkManager.Instance.localPlayerController.StartCoroutine(ApplySynchronizationCoroutine(GameNetworkManager.Instance.localPlayerController, grabbedScrap, false));
                            }
                        }
                        else if (curseEffect.Equals(Constants.DIMINUTIVE) && __instance.transform.localScale != DIMINUTIVE_SCALE)
                        {
                            ApplyDiminutive(true, ref __instance);
                        }
                        else if (curseEffect.Equals(Constants.COMMUNICATION) || curseEffect.Equals(Constants.COMM_CORE))
                        {
                            ApplyCommunicationFirstPart(ref grabbedScrap, ref __instance, ref curseEffect);
                        }
                        else if (curseEffect.Equals(Constants.COMM_REFLECTION))
                        {
                            ApplyCommunicationSecondPart(ref __instance);
                        }
                        // Malédictions locales
                        else if (__instance == GameNetworkManager.Instance.localPlayerController)
                        {
                            AddLocalCurse(curseEffect, true);
                        }
                    }
                    else
                    {
                        immunedPlayers.Remove(__instance);
                    }
                }
                else
                {
                    curseCounter++;
                }
            }
        }

        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.UpdateGameToMatchSettings))]
        [HarmonyPrefix]
        private static bool PreventRemoveWhenExitConfigs()
        {
            if (activeCurses.Contains(Constants.CONFUSION) || activeCurses.Contains(Constants.MUTE) || activeCurses.Contains(Constants.DEAFNESS))
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SetOption))]
        [HarmonyPrefix]
        private static bool PreventUpdateSettings(SettingsOptionType optionType)
        {
            if (ConfigManager.globalPrevent.Value &&
                ((activeCurses.Contains(Constants.MUTE) && (optionType == SettingsOptionType.MicEnabled || optionType == SettingsOptionType.MicDevice || optionType == SettingsOptionType.MicPushToTalk))
                 || (activeCurses.Contains(Constants.DEAFNESS) && (optionType == SettingsOptionType.MasterVolume || optionType == SettingsOptionType.MicDevice))))
            {
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.RebindKey))]
        [HarmonyPrefix]
        private static bool PreventUpdateRebinding(InputActionReference rebindableAction)
        {
            if (ConfigManager.globalPrevent.Value && activeCurses.Contains(Constants.CONFUSION) && rebindableAction.action.name.Equals("Move"))
            {
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static bool PreDropObject(ref PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer == lastGrabbedObject)
            {
                lastGrabbedObject = null;
            }

            string curseEffectHeld = GetCurseEffect(ref __instance.currentlyHeldObjectServer);
            if (!string.IsNullOrEmpty(curseEffectHeld))
            {
                if ((curseEffectHeld.Equals(Constants.CAPTIVE) || IsCoopCursed(curseEffectHeld))
                    && !__instance.isInHangarShipRoom)
                {
                    return false;
                }
                // S'il s'agit du joueur immobilisé, retirer la malédiction
                else if (activeCurses.Contains(Constants.COMMUNICATION)
                    && curseEffectHeld.Equals(Constants.COMM_CORE)
                    && actionsBlockedBy.Contains(Constants.COMMUNICATION))
                {
                    DesactiveCommunication(ref __instance);
                }
                else if (!activeCurses.Contains(Constants.SYNCHRONIZATION) && (curseEffectHeld == Constants.SYNC_REFLECTION || curseEffectHeld == Constants.SYNC_CORE))
                {
                    EnablePlayerActions(Constants.SYNCHRONIZATION, true);
                }
                else if (!activeCurses.Contains(Constants.COMMUNICATION) && (curseEffectHeld == Constants.COMM_REFLECTION || curseEffectHeld == Constants.COMM_CORE))
                {
                    EnablePlayerActions(Constants.COMMUNICATION, true);
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "ThrowObjectClientRpc")]
        [HarmonyPrefix]
        private static bool PreDropObjectSync(ref PlayerControllerB __instance)
        {
            if (!activeCurses.Contains(Constants.COMMUNICATION) && communicationPlayer != null && __instance == communicationPlayer)
            {
                DesactiveCommunication(ref GameNetworkManager.Instance.localPlayerController);
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPostfix]
        private static void PostDropObject(ref PlayerControllerB __instance)
        {
            string curseEffectHeld = GetCurseEffect(ref __instance.currentlyHeldObjectServer);
            if (!string.IsNullOrEmpty(curseEffectHeld))
            {
                if (!curseEffectHeld.Equals(Constants.CAPTIVE)
                    && activeCurses.Contains(Constants.ERRANT))
                {
                    TeleportPlayer(ref __instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "UpdatePlayerPositionClientRpc")]
        [HarmonyPostfix]
        private static void UpdatePlayerPosition(ref PlayerControllerB __instance)
        {
            if (__instance.isInHangarShipRoom)
            {
                if (activeCurses.Count > 0)
                {
                    RemoveCoopEffect(Constants.SYNCHRONIZATION, ref __instance, ref switchedPlayer);
                    RemoveCoopEffect(Constants.COMMUNICATION, ref __instance, ref communicationPlayer);

                    if (__instance == GameNetworkManager.Instance.localPlayerController)
                    {
                        RemoveAllLocalEffects();
                    }
                }

                if (__instance.transform.localScale == DIMINUTIVE_SCALE)
                {
                    ApplyDiminutive(false, ref __instance);
                }
            }
            
            if (activeCurses.Count > 0 && activeCurses.Contains(Constants.DIMINUTIVE) && __instance != GameNetworkManager.Instance.localPlayerController)
            {
                if (__instance.transform.localScale != DIMINUTIVE_SCALE && Vector3.Distance(__instance.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) <= 1f)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Crushing);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SetNightVisionEnabled")]
        [HarmonyPrefix]
        public static bool SetNightVisionEnabledOverride(ref bool isNotLocalClient, ref Light ___nightVision)
        {
            if (activeCurses.Contains(Constants.SYNCHRONIZATION) && switchedPlayer != null)
            {
                ___nightVision.enabled = false;
                if (!isNotLocalClient && switchedPlayer.isInsideFactory)
                {
                    ___nightVision.enabled = true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetInsideLightingDimness))]
        [HarmonyPostfix]
        public static void SwitchLightningSynchronization(ref TimeOfDay __instance)
        {
            if (__instance.sunDirect == null || __instance.sunIndirect == null)
            {
                return;
            }

            if (activeCurses.Contains(Constants.SYNCHRONIZATION) && switchedPlayer != null)
            {
                __instance.sunDirect.enabled = !switchedPlayer.isInsideFactory;
                __instance.sunIndirect.enabled = !switchedPlayer.isInsideFactory;
                __instance.insideLighting = switchedPlayer.isInsideFactory;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyPrefix]
        private static void PreDamagePlayer(ref int damageNumber, ref bool fallDamage, ref PlayerControllerB __instance)
        {
            // Si le joueur est réduit en taille et que ce ne sont pas des dommages de chute, le tuer au moindre dégât
            if (!fallDamage && activeCurses.Count > 0 && activeCurses.Contains(Constants.DIMINUTIVE))
            {
                damageNumber = __instance.health;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayerClientRpc")]
        [HarmonyPrefix]
        private static void PlayerDeath(ref int playerId, ref PlayerControllerB __instance)
        {
            PlayerControllerB player = __instance.playersManager.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (activeCurses.Count > 0)
            {
                /*if (activeCurses.Contains(Constants.SYNCHRONIZATION)
                    && switchedPlayer != null
                    && (player == GameNetworkManager.Instance.localPlayerController || player == switchedPlayer))
                {
                    if (player == switchedPlayer && HasCursedScrap(new string[] { Constants.SYNC_CORE, Constants.SYNC_REFLECTION }))
                    {
                        EnablePlayerActions(Constants.SYNCHRONIZATION, false);
                    }
                    AddActiveCurse(Constants.SYNCHRONIZATION, false);
                    SwitchPlayerCamera(ref switchedPlayer, false);
                }

                if (activeCurses.Contains(Constants.COMMUNICATION)
                    && communicationPlayer != null
                    && (player == GameNetworkManager.Instance.localPlayerController || player == communicationPlayer))
                {
                    if (player == communicationPlayer)
                    {
                        // Forcer l'autre partie de l'objet à être lâchée par le joueur en vie
                        GetCursedScrap(new string[] { Constants.COMM_CORE, Constants.COMM_REFLECTION })?.DiscardItemOnClient();
                    }

                    // Si joueur qui a déclenché la malédiction, supprimer la capacité de scanner le joueur sélectionné et l'objet à récupérer
                    GrabbableObject scrap = GetCursedScrap(new string[] { Constants.COMM_CORE });
                    if (scrap != null)
                    {
                        ScanNodeProperties playerNode = ((Component)(object)communicationPlayer).GetComponentInChildren<ScanNodeProperties>();
                        if (playerNode != null)
                        {
                            HUDManagerPatch.DestroyScanNode(ref playerNode);
                        }
                    }
                    AddActiveCurse(Constants.COMMUNICATION, false);
                    communicationPlayer = null;
                    trackedScrap = null;
                }*/
                DesactiveAllCoopEffects(ref player);

                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    RemoveAllLocalEffects();
                }
            }

            if (player.transform.localScale == DIMINUTIVE_SCALE)
            {
                ApplyDiminutive(false, ref player);
            }

            if (player == GameNetworkManager.Instance.localPlayerController && actionsBlockedBy.Contains(Constants.SYNCHRONIZATION))
            {
                EnablePlayerActions(Constants.SYNCHRONIZATION, true);
            }

            if (immunedPlayers.Contains(player))
            {
                immunedPlayers.RemoveAll(i => i == player);
            }
        }

        private static void AddLocalCurse(string curseEffect, bool enable)
        {
            if (!string.IsNullOrEmpty(curseEffect))
            {
                switch (curseEffect)
                {
                    case Constants.INHIBITION:
                        AddActiveCurse(Constants.INHIBITION, enable);
                        break;
                    case Constants.CONFUSION:
                        ApplyConfusion(enable);
                        break;
                    case Constants.BLURRY:
                        AddActiveCurse(Constants.BLURRY, enable);
                        break;
                    case Constants.MUTE:
                        ApplyMute(enable);
                        break;
                    case Constants.DEAFNESS:
                        ApplyDeafness(enable);
                        break;
                    case Constants.ERRANT:
                        ApplyErrant(enable);
                        break;
                    case Constants.PARALYSIS:
                        AddActiveCurse(Constants.PARALYSIS, enable);
                        break;
                    case Constants.SHADOW:
                        AddActiveCurse(Constants.SHADOW, enable);
                        break;
                    case Constants.EXPLORATION:
                        ApplyExploration(enable);
                        break;
                    default:
                        break;
                }
            }
        }

        internal static string GetCurseEffect(ref GrabbableObject grabbableObject)
        {
            if (grabbableObject != null)
            {
                ScanNodeProperties componentInChildren = ((Component)(object)grabbableObject).gameObject.GetComponentInChildren<ScanNodeProperties>();
                if (componentInChildren != null && componentInChildren.subText != null && componentInChildren.subText.Length > 1)
                {
                    string[] splitText = componentInChildren.subText.Split(new string[] { "\nCurse: " }, StringSplitOptions.None);
                    if (splitText.Length > 1)
                    {
                        return splitText[1];
                    }
                }
            }
            return null;
        }

        private static bool HasCursedScrap(string[] curses = null)
        {
            for (int i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
            {
                if (GameNetworkManager.Instance.localPlayerController.ItemSlots[i] != null)
                {
                    string curseEffect = GetCurseEffect(ref GameNetworkManager.Instance.localPlayerController.ItemSlots[i]);
                    if (!string.IsNullOrEmpty(curseEffect))
                    {
                        if (curses == null || (curses != null && curses.Contains(curseEffect)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsCoopCursed(string curseEffect)
        {
            if (hasCoopCurseActive
                && (curseEffect.Equals(Constants.SYNCHRONIZATION)
                    || curseEffect.Equals(Constants.SYNC_CORE)
                    || curseEffect.Equals(Constants.SYNC_REFLECTION)
                    || curseEffect.Equals(Constants.COMMUNICATION)
                    // S'il s'agit du joueur immobilisé, pouvoir retirer la malédiction
                    || (curseEffect.Equals(Constants.COMM_CORE) && !actionsBlockedBy.Contains(Constants.COMMUNICATION))
                    || curseEffect.Equals(Constants.COMM_REFLECTION)))
            {
                return true;
            }
            return false;
        }

        private static void DesactiveAllCoopEffects(ref PlayerControllerB player)
        {
            DesactiveSynchronization(ref player);
            DesactiveCommunication(ref player);
        }

        private static void RemoveCoopEffect(string curseEffect, ref PlayerControllerB movingPlayer, ref PlayerControllerB coopPlayer)
        {
            if (activeCurses.Contains(curseEffect)
                && ItemManagerPatch.scrapCoopToDestroy != null
                && coopPlayer != null
                && (movingPlayer == GameNetworkManager.Instance.localPlayerController || movingPlayer == coopPlayer))
            {
                ItemManagerPatch.scrapCoopToDestroy.DestroyObjectInHand(ItemManagerPatch.scrapCoopToDestroy.playerHeldBy);
                ItemManagerPatch.scrapCoopToDestroy = null;
                AddActiveCurse(curseEffect, false);

                if (curseEffect.Equals(Constants.SYNCHRONIZATION))
                {
                    SwitchPlayerCamera(ref switchedPlayer, false);
                }
                else if (curseEffect.Equals(Constants.COMMUNICATION))
                {
                    communicationPlayer = null;
                    HUDManagerPatch.forceEndChrono = true;
                }
            }
        }

        private static void RemoveAllLocalEffects()
        {
            foreach (string activeCurse in activeCurses.ToList())
            {
                AddLocalCurse(activeCurse, false);
            }
            // Mise à jour des paramètres
            IngamePlayerSettings.Instance.UpdateGameToMatchSettings();
        }

        private static void ApplyConfusion(bool enable)
        {
            AddActiveCurse(Constants.CONFUSION, enable);

            if (enable)
            {
                string moveUpKeyboard = null;
                string moveUpGamepad = null;
                string moveDownKeyboard = null;
                string moveDownGamepad = null;
                string moveLeftKeyboard = null;
                string moveLeftGamepad = null;
                string moveRightKeyboard = null;
                string moveRightGamepad = null;
                string jumpKeyboard = null;
                string jumpGamepad = null;
                string crouchKeyboard = null;
                string crouchGamepad = null;

                if (IngamePlayerSettings.Instance.settings != null && !string.IsNullOrEmpty(IngamePlayerSettings.Instance.settings.keyBindings))
                {
                    JObject jsonObject = JObject.Parse(IngamePlayerSettings.Instance.settings.keyBindings);
                    JArray bindings = (JArray)jsonObject?["bindings"];
                    if (bindings != null)
                    {
                        var filteredBindings = bindings.Where(b => ((string)b["action"]).Equals("Movement/Jump") || ((string)b["action"]).Equals("Movement/Crouch") || ((string)b["action"]).Equals("Movement/Move"));

                        foreach (JObject binding in filteredBindings.Cast<JObject>())
                        {
                            string action = (string)binding["action"];
                            string path = (string)binding["path"];

                            if (action.Equals("Movement/Move"))
                            {
                                // Les id ne semblent pas changer d'une personne à l'autre
                                string id = (string)binding["id"];
                                if (id.Equals("ae3df94a-dcc6-4177-b026-0938f8413a45") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveUpKeyboard = path;
                                else if (id.Equals("bb6e2ec9-7f02-4c1e-b136-f45218f65d48") && path.StartsWith("<Gamepad>")) moveUpGamepad = path;
                                else if (id.Equals("bc252037-120e-4c64-9671-40d365f856b3") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveDownKeyboard = path;
                                else if (id.Equals("717338d1-0b10-457a-a2ef-9b43135cbad6") && path.StartsWith("<Gamepad>")) moveDownGamepad = path;
                                else if (id.Equals("ad96b1ce-c0f3-4913-be03-acf077c11064") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveLeftKeyboard = path;
                                else if (id.Equals("1ffdc730-e9c8-4a8a-934f-98fa19bdca4d") && path.StartsWith("<Gamepad>")) moveLeftGamepad = path;
                                else if (id.Equals("756f3db2-a6e6-42d7-9580-70d42154cd11") && (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>"))) moveRightKeyboard = path;
                                else if (id.Equals("bc814a6c-e505-4ad6-988b-8e9ce3311a28") && path.StartsWith("<Gamepad>")) moveRightGamepad = path;
                            }
                            else if (action.Equals("Movement/Jump"))
                            {
                                if (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>")) jumpKeyboard = path;
                                else if (path.StartsWith("<Gamepad>")) jumpGamepad = path;
                            }
                            else if (action.Equals("Movement/Crouch"))
                            {
                                if (path.StartsWith("<Keyboard>") || path.StartsWith("<Mouse>")) crouchKeyboard = path;
                                else if (path.StartsWith("<Gamepad>")) crouchGamepad = path;
                            }
                        }
                    }
                }

                InputAction moveAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move");

                // Clavier
                if (!string.IsNullOrEmpty(moveDownKeyboard)) moveAction.ApplyBindingOverride(1, new InputBinding { overridePath = moveDownKeyboard });
                else moveAction.ApplyBindingOverride(1, new InputBinding { overridePath = moveAction.bindings[2].path });

                if (!string.IsNullOrEmpty(moveUpKeyboard)) moveAction.ApplyBindingOverride(2, new InputBinding { overridePath = moveUpKeyboard });
                else moveAction.ApplyBindingOverride(2, new InputBinding { overridePath = moveAction.bindings[1].path });

                if (!string.IsNullOrEmpty(moveRightKeyboard)) moveAction.ApplyBindingOverride(3, new InputBinding { overridePath = moveRightKeyboard });
                else moveAction.ApplyBindingOverride(3, new InputBinding { overridePath = moveAction.bindings[4].path });

                if (!string.IsNullOrEmpty(moveLeftKeyboard)) moveAction.ApplyBindingOverride(4, new InputBinding { overridePath = moveLeftKeyboard });
                else moveAction.ApplyBindingOverride(4, new InputBinding { overridePath = moveAction.bindings[3].path });

                // Manette
                if (!string.IsNullOrEmpty(moveDownGamepad)) moveAction.ApplyBindingOverride(6, new InputBinding { overridePath = moveDownGamepad });
                else moveAction.ApplyBindingOverride(6, new InputBinding { overridePath = moveAction.bindings[7].path });

                if (!string.IsNullOrEmpty(moveUpGamepad)) moveAction.ApplyBindingOverride(7, new InputBinding { overridePath = moveUpGamepad });
                else moveAction.ApplyBindingOverride(7, new InputBinding { overridePath = moveAction.bindings[6].path });

                if (!string.IsNullOrEmpty(moveRightGamepad)) moveAction.ApplyBindingOverride(8, new InputBinding { overridePath = moveRightGamepad });
                else moveAction.ApplyBindingOverride(8, new InputBinding { overridePath = moveAction.bindings[9].path });

                if (!string.IsNullOrEmpty(moveLeftGamepad)) moveAction.ApplyBindingOverride(9, new InputBinding { overridePath = moveLeftGamepad });
                else moveAction.ApplyBindingOverride(9, new InputBinding { overridePath = moveAction.bindings[8].path });

                InputAction jumpAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Jump");
                InputAction crouchAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch");

                // Clavier
                if (!string.IsNullOrEmpty(crouchKeyboard)) jumpAction.ApplyBindingOverride(0, new InputBinding { overridePath = crouchKeyboard });
                else jumpAction.ApplyBindingOverride(0, new InputBinding { overridePath = crouchAction.bindings[0].path });

                if (!string.IsNullOrEmpty(jumpKeyboard)) crouchAction.ApplyBindingOverride(0, new InputBinding { overridePath = jumpKeyboard });
                else crouchAction.ApplyBindingOverride(0, new InputBinding { overridePath = jumpAction.bindings[0].path });

                // Manette
                if (!string.IsNullOrEmpty(crouchGamepad)) jumpAction.ApplyBindingOverride(1, new InputBinding { overridePath = crouchGamepad });
                else jumpAction.ApplyBindingOverride(1, new InputBinding { overridePath = crouchAction.bindings[1].path });

                if (!string.IsNullOrEmpty(jumpGamepad)) crouchAction.ApplyBindingOverride(1, new InputBinding { overridePath = jumpGamepad });
                else crouchAction.ApplyBindingOverride(1, new InputBinding { overridePath = jumpAction.bindings[1].path });
            }
            else
            {
                IngamePlayerSettings.Instance.playerInput.actions.RemoveAllBindingOverrides();
                IngamePlayerSettings.Instance.LoadSettingsFromPrefs();
            }
        }

        private static void ApplyMute(bool enable)
        {
            AddActiveCurse(Constants.MUTE, enable);

            if (enable)
            {
                IngamePlayerSettings.Instance.unsavedSettings.micEnabled = false;
                IngamePlayerSettings.Instance.settings.micEnabled = false;

                foreach (SettingsOption setting in FindObjectsOfType<SettingsOption>(includeInactive: true).ToList().Where(s => s.optionType == SettingsOptionType.MicEnabled))
                {
                    setting.ToggleEnabledImage(4);
                }

                IngamePlayerSettings.Instance.SetMicrophoneEnabled();
            }
            else
            {
                IngamePlayerSettings.Instance.unsavedSettings.micEnabled = true;
                IngamePlayerSettings.Instance.settings.micEnabled = true;
            }
        }

        private static void ApplyDeafness(bool enable)
        {
            AddActiveCurse(Constants.DEAFNESS, enable);

            if (enable)
            {
                savedMasterVolume = (IngamePlayerSettings.Instance.settings.masterVolume == 0f ? savedMasterVolume : IngamePlayerSettings.Instance.settings.masterVolume);
                IngamePlayerSettings.Instance.ChangeMasterVolume(0);
            }
            else
            {
                if (savedMasterVolume != 0f)
                {
                    IngamePlayerSettings.Instance.ChangeMasterVolume((int)(savedMasterVolume * 100));
                }
            }
        }

        private static void ApplyErrant(bool enable)
        {
            AddActiveCurse(Constants.ERRANT, enable);
            TeleportPlayer(ref GameNetworkManager.Instance.localPlayerController);
        }

        internal static void ApplyDiminutive(bool enable, ref PlayerControllerB player)
        {
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                AddActiveCurse(Constants.DIMINUTIVE, enable);
                player.localVisor.gameObject.SetActive(!enable);
            }

            if (enable)
            {
                originalScale = player.transform.localScale;
                player.transform.localScale = DIMINUTIVE_SCALE;
                player.movementSpeed /= ConfigManager.diminutiveSpeed.Value;
                player.grabDistance /= ConfigManager.diminutiveGrab.Value;
            }
            else
            {
                player.transform.localScale = originalScale;
                player.movementSpeed *= ConfigManager.diminutiveSpeed.Value;
                player.grabDistance *= ConfigManager.diminutiveGrab.Value;
            }
        }

        internal static void ApplyExploration(bool enable)
        {
            // A faire avant AddActiveCurse pour ne pas effectuer ce code à chaque fois que l'objet avec cette malédiction est ramassé
            if (enable && !activeCurses.Contains(Constants.EXPLORATION))
            {
                HUDManagerPatch.ChangeRandomEntranceId(!GameNetworkManager.Instance.localPlayerController.isInsideFactory);
            }
            AddActiveCurse(Constants.EXPLORATION, enable);
        }

        private static void ApplyCommunicationFirstPart(ref GrabbableObject grabbedScrap, ref PlayerControllerB player, ref string curseEffect)
        {
            if (curseEffect.Equals(Constants.COMMUNICATION))
            {
                Vector3 position = ItemManagerPatch.GetFurthestPositionScrapSpawn(player.transform.position, ref grabbedScrap.itemProperties);
                ItemManagerPatch.CloneScrap(ref grabbedScrap, Constants.COMM_CORE, Constants.COMM_REFLECTION, ref position, ref player);
            }
            else if (trackedScrap == null && curseEffect.Equals(Constants.COMM_CORE) && GameNetworkManager.Instance.localPlayerController == player)
            {
                trackedScrap = ItemManagerPatch.GetCloneScrapFromCurse(grabbedScrap, Constants.COMM_REFLECTION, false);
            }

            // Sélection du joueur devant récupérer la seconde partie de l'objet
            PlayerControllerB grabbingPlayer = player;
            PlayerControllerB selectedPlayer = StartOfRound.Instance.allPlayerScripts
                .Where(p => p.isPlayerControlled && !p.isPlayerDead && p != grabbingPlayer)
                .OrderBy(p => Vector3.Distance(p.transform.position, grabbingPlayer.transform.position))
                .FirstOrDefault();

            if (selectedPlayer != null)
            {
                if (GameNetworkManager.Instance.localPlayerController == selectedPlayer)
                {
                    // Activer la malédiction dans la deuxième partie pour ce joueur => pour qu'il soit capable de récupérer la deuxième partie de l'objet
                    communicationPlayer = player;
                    HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, "A player has picked the first part of a cursed object, and you've been chosen to find the second part. Find it to set him free!");
                }
                else if (GameNetworkManager.Instance.localPlayerController == player)
                {
                    AddActiveCurse(Constants.COMMUNICATION, true);
                    communicationPlayer = selectedPlayer;

                    // Immobiliser le joueur
                    EnablePlayerActions(Constants.COMMUNICATION, false);

                    Debug.Log("StartTrackedScrapCoroutine");
                    HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartTrackedScrapCoroutine());
                }
            }
            else
            {
                if (GameNetworkManager.Instance.localPlayerController == player)
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, "No player could be found to share this curse with you.");
                    // Immobiliser le joueur
                    EnablePlayerActions(Constants.COMMUNICATION, false);
                }
            }
        }

        private static void ApplyCommunicationSecondPart(ref PlayerControllerB player)
        {
            if (activeCurses.Contains(Constants.COMMUNICATION) || player == GameNetworkManager.Instance.localPlayerController)
            {
                // Joueur immobilisé
                if (player != GameNetworkManager.Instance.localPlayerController)
                {
                    // Le joueur n'est plus immobilisé
                    EnablePlayerActions(Constants.COMMUNICATION, true);
                }
                else
                {
                    AddActiveCurse(Constants.COMMUNICATION, true);
                }
                trackedScrap = null;
                HUDManagerPatch.forceEndChrono = false;
                HUDManager.Instance.StartCoroutine(HUDManagerPatch.StartChronoCoroutine(ConfigManager.communicationChrono.Value));
            }
        }

        private static void DesactiveCommunication(ref PlayerControllerB player)
        {
            if (activeCurses.Contains(Constants.COMMUNICATION)
                && communicationPlayer != null
                && (player == GameNetworkManager.Instance.localPlayerController || player == communicationPlayer))
            {
                if (player == communicationPlayer)
                {
                    GameNetworkManager.Instance.localPlayerController.DropAllHeldItemsAndSync();
                }

                // Le joueur n'est plus immobilisé
                EnablePlayerActions(Constants.COMMUNICATION, true);

                AddActiveCurse(Constants.COMMUNICATION, false);
                communicationPlayer = null;
                trackedScrap = null;
                HUDManagerPatch.forceEndChrono = true;
            }
        }

        private static IEnumerator PlayerDoubleJump(PlayerControllerB player)
        {
            doubleJump = true;
            player.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
            if ((Coroutine)AccessTools.Field(typeof(PlayerControllerB), "jumpCoroutine").GetValue(player) != null)
            {
                player.StopCoroutine((Coroutine)AccessTools.Field(typeof(PlayerControllerB), "jumpCoroutine").GetValue(player));
            }
            AccessTools.Field(typeof(PlayerControllerB), "jumpCoroutine").SetValue(player, player.StartCoroutine((IEnumerator)AccessTools.Method(typeof(PlayerControllerB), "PlayerJump").Invoke(player, null)));

            yield return new WaitUntil(() => player.thisController.isGrounded);

            doubleJump = false;
        }

        private static void TeleportPlayer(ref PlayerControllerB player)
        {
            if (!player.isInHangarShipRoom)
            {
                Vector3 position = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                position = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position);
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = true;
                player.averageVelocity = 0f;
                player.velocityLastFrame = Vector3.zero;
                player.TeleportPlayer(position);
            }
        }

        internal static void Paralyze(ref PlayerControllerB __instance)
        {
            if (activeCurses.Contains(Constants.PARALYSIS))
            {
                __instance.JumpToFearLevel(0.6f);
                __instance.StartCoroutine(ParalyzeCoroutine());
            }
        }

        private static IEnumerator ParalyzeCoroutine()
        {
            EnablePlayerActions(Constants.PARALYSIS, false);
            yield return new WaitForSeconds(ConfigManager.paralysisTime.Value);
            EnablePlayerActions(Constants.PARALYSIS, true);
        }

        private static IEnumerator ApplySynchronizationCoroutine(PlayerControllerB player, GrabbableObject grabbedScrap, bool isLocalPlayerHolder)
        {
            while (!ApplySynchronization(ref player, ref grabbedScrap, isLocalPlayerHolder))
            {
                yield return new WaitForSeconds(1f);
            }
        }

        private static bool ApplySynchronization(ref PlayerControllerB player, ref GrabbableObject grabbedScrap, bool isLocalPlayerHolder = true)
        {
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i] != player && HasCursedScrapClone(ref StartOfRound.Instance.allPlayerScripts[i], ref grabbedScrap, isLocalPlayerHolder))
                {
                    AddActiveCurse(Constants.SYNCHRONIZATION, true);
                    EnablePlayerActions(Constants.SYNCHRONIZATION, true);
                    SwitchPlayerCamera(ref StartOfRound.Instance.allPlayerScripts[i], true);
                    return true;
                }
            }
            EnablePlayerActions(Constants.SYNCHRONIZATION, false);
            return false;
        }

        private static void DesactiveSynchronization(ref PlayerControllerB player)
        {
            if (activeCurses.Contains(Constants.SYNCHRONIZATION)
                && switchedPlayer != null
                && (player == GameNetworkManager.Instance.localPlayerController || player == switchedPlayer))
            {
                if (player == switchedPlayer && HasCursedScrap(new string[] { Constants.SYNC_CORE, Constants.SYNC_REFLECTION }))
                {
                    EnablePlayerActions(Constants.SYNCHRONIZATION, false);
                }
                AddActiveCurse(Constants.SYNCHRONIZATION, false);
                SwitchPlayerCamera(ref switchedPlayer, false);
            }
        }

        private static bool HasCursedScrapClone(ref PlayerControllerB player, ref GrabbableObject clonedScrap, bool isLocalPlayerHolder)
        {
            string clonedScrapCurse = GetCurseEffect(ref clonedScrap);
            if (!string.IsNullOrEmpty(clonedScrapCurse))
            {
                for (int i = 0; i < player.ItemSlots.Length; i++)
                {
                    if (player.ItemSlots[i] != null)
                    {
                        string curseEffect = GetCurseEffect(ref player.ItemSlots[i]);
                        if (!string.IsNullOrEmpty(curseEffect)
                            && ((!isLocalPlayerHolder && ((curseEffect.Equals(Constants.SYNC_REFLECTION) && clonedScrapCurse.Equals(Constants.SYNC_REFLECTION)) || (curseEffect.Equals(Constants.SYNC_CORE) && clonedScrapCurse.Equals(Constants.SYNC_CORE))))
                                || (isLocalPlayerHolder && ((curseEffect.Equals(Constants.SYNC_REFLECTION) && clonedScrapCurse.Equals(Constants.SYNC_CORE)) || (curseEffect.Equals(Constants.SYNC_CORE) && clonedScrapCurse.Equals(Constants.SYNC_REFLECTION)))))
                            && player.ItemSlots[i].itemProperties.spawnPrefab == clonedScrap.itemProperties.spawnPrefab
                            && player.ItemSlots[i].scrapValue == clonedScrap.scrapValue)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void SwitchPlayerCamera(ref PlayerControllerB player, bool isSwitched)
        {
            player.gameplayCamera.enabled = isSwitched;
            GameNetworkManager.Instance.localPlayerController.thisPlayerModelArms.enabled = !isSwitched;
            player.thisPlayerModelArms.enabled = isSwitched;

            (player.localVisorTargetPoint, GameNetworkManager.Instance.localPlayerController.localVisorTargetPoint) = (GameNetworkManager.Instance.localPlayerController.localVisorTargetPoint, player.localVisorTargetPoint);
            (player.nightVision, GameNetworkManager.Instance.localPlayerController.nightVision) = (GameNetworkManager.Instance.localPlayerController.nightVision, player.nightVision);
            (player.thisPlayerModel.gameObject.layer, GameNetworkManager.Instance.localPlayerController.thisPlayerModel.gameObject.layer) = (GameNetworkManager.Instance.localPlayerController.thisPlayerModel.gameObject.layer, player.thisPlayerModel.gameObject.layer);
            (player.thisPlayerModelArms.gameObject.layer, GameNetworkManager.Instance.localPlayerController.thisPlayerModelArms.gameObject.layer) = (GameNetworkManager.Instance.localPlayerController.thisPlayerModelArms.gameObject.layer, player.thisPlayerModelArms.gameObject.layer);

            if (isSwitched)
            {
                switchedPlayer = player;

                StartOfRound.Instance.activeCamera = player.gameplayCamera;
                GameNetworkManager.Instance.localPlayerController.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
                player.thisPlayerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                StartOfRound.Instance.activeCamera.transform.SetParent(player.gameplayCamera.transform, worldPositionStays: false);
            }
            else
            {
                StartOfRound.Instance.activeCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
                GameNetworkManager.Instance.localPlayerController.thisPlayerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                player.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
                StartOfRound.Instance.activeCamera.transform.SetParent(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform, worldPositionStays: false);

                switchedPlayer = null;
            }
        }

        private static void EnablePlayerActions(string curseEffect, bool enable)
        {
            string[] actionNames = { "Move", "Jump", "Crouch", "Interact", "ItemSecondaryUse", "ItemTertiaryUse", "ActivateItem", "SwitchItem", "InspectItem", "Emote1", "Emote2" };

            if (enable)
            {
                actionsBlockedBy.Remove(curseEffect);
            }
            else
            {
                if (!actionsBlockedBy.Contains(curseEffect))
                {
                    actionsBlockedBy.Add(curseEffect);
                }
            }

            foreach (string actionName in actionNames)
            {
                if (enable && actionsBlockedBy.Count == 0) IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Enable();
                else IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Disable();
            }
        }

        internal static void AddActiveCurse(string curseEffect, bool enable)
        {
            if (enable && !activeCurses.Contains(curseEffect))
            {
                if (curseEffect.Equals(Constants.SYNCHRONIZATION) || curseEffect.Equals(Constants.COMMUNICATION))
                {
                    hasCoopCurseActive = true;
                }
                activeCurses.Add(curseEffect);
            }
            else if (!enable)
            {
                if (curseEffect.Equals(Constants.SYNCHRONIZATION) || curseEffect.Equals(Constants.COMMUNICATION))
                {
                    hasCoopCurseActive = false;
                }
                activeCurses.Remove(curseEffect);
            }
        }
    }
}
