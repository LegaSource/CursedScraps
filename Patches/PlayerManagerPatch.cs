using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine.Rendering;
using System;

namespace CursedScraps.Patches
{
    internal class PlayerManagerPatch
    {
        public static float savedMasterVolume = 0f;
        public static bool isCursed = false;
        public static bool isConfusion = false;
        public static bool isMute = false;
        public static bool isDeafness = false;
        public static bool isErrant = false;
        public static bool isParalysis = false;

        [HarmonyPatch(typeof(PlayerControllerB), "ActivateItem_performed")]
        [HarmonyPostfix]
        private static void UseItem(ref PlayerControllerB __instance)
        {
            if ((bool)AccessTools.Method(typeof(PlayerControllerB), "CanUseItem").Invoke(__instance, null) && __instance.currentlyHeldObjectServer.itemProperties.itemName.Equals(Constants.CURSE_PILLS))
            {
                if (isCursed && !HasCursedScrap(ref __instance))
                {
                    __instance.ItemSlots[__instance.currentItemSlot].DestroyObjectInHand(__instance);
                    RemoveAllEffects();
                }
                else if (isCursed)
                {
                    HUDManager.Instance.DisplayTip("Impossible action", "You must not possess cursed items to use a pill.");
                }
                else
                {
                    HUDManager.Instance.DisplayTip("Impossible action", "You have no active curses.");
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "GrabObject")]
        [HarmonyPostfix]
        private static void AddEffect(ref bool ___grabInvalidated, ref GrabbableObject ___currentlyGrabbingObject, ref PlayerControllerB __instance)
        {
            if (___grabInvalidated)
            {
                return;
            }

            string curseEffect = GetCurseEffect(ref ___currentlyGrabbingObject);
            if (!string.IsNullOrEmpty(curseEffect))
            {
                isCursed = true;

                switch (curseEffect)
                {
                    case Constants.INHIBITION:
                        IngamePlayerSettings.Instance.playerInput.actions.FindAction("Jump", false).Disable();
                        IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch", false).Disable();
                        break;
                    case Constants.CONFUSION:
                        isConfusion = true;
                        ApplyConfusion();
                        break;
                    case Constants.BLURRY:
                        HUDManagerPatch.isBlurry = true;
                        break;
                    case Constants.MUTE:
                        isMute = true;
                        IngamePlayerSettings.Instance.unsavedSettings.micEnabled = false;
                        IngamePlayerSettings.Instance.settings.micEnabled = false;
                        IngamePlayerSettings.Instance.UpdateGameToMatchSettings();
                        break;
                    case Constants.DEAFNESS:
                        isDeafness = true;
                        savedMasterVolume = (IngamePlayerSettings.Instance.settings.masterVolume == 0f ? savedMasterVolume : IngamePlayerSettings.Instance.settings.masterVolume);
                        IngamePlayerSettings.Instance.ChangeMasterVolume(0);
                        break;
                    case Constants.ERRANT:
                        isErrant = true;
                        break;
                    case Constants.PARALYSIS:
                        isParalysis = true;
                        break;
                    case Constants.SHADOW:
                        EnemyAIPatch.isShadow = true;
                        break;
                    default:
                        break;
                }
            }

            if (isErrant)
            {
                TeleportPlayer(ref __instance);
            }
        }

        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.DiscardChangedSettings))]
        [HarmonyPrefix]
        private static bool PreventRemoveWhenExitConfigs()
        {
            if (isConfusion || isMute || isDeafness)
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
                ((isMute && optionType == SettingsOptionType.MicEnabled)
                 || (isDeafness && optionType == SettingsOptionType.MasterVolume)))
            {
                HUDManager.Instance.DisplayTip("Impossible action", "A curse prevents you from performing this action.");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.RebindKey))]
        [HarmonyPrefix]
        private static bool PreventUpdateRebinding(InputActionReference rebindableAction)
        {
            if (ConfigManager.globalPrevent.Value && isConfusion && rebindableAction.action.name.Equals("Move"))
            {
                HUDManager.Instance.DisplayTip("Impossible action", "A curse prevents you from performing this action.");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPrefix]
        private static bool PreDropObject(ref PlayerControllerB __instance)
        {
            string curseEffectHeld = GetCurseEffect(ref __instance.currentlyHeldObjectServer);
            if (!string.IsNullOrEmpty(curseEffectHeld) && curseEffectHeld == Constants.CAPTIVE && !__instance.isInHangarShipRoom)
            {
                return false;
            }
            /*else if (!string.IsNullOrEmpty(curseEffectHeld) && curseEffectHeld != Constants.CAPTIVE)
            {
                RemovePlayerEffect(ref curseEffectHeld, ref __instance);
            }*/
            return true;
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DiscardHeldObject))]
        [HarmonyPostfix]
        private static void PostDropObject(ref PlayerControllerB __instance)
        {
            if (isErrant)
            {
                TeleportPlayer(ref __instance);
            }
        }
        
        [HarmonyPatch(typeof(PlayerControllerB), "UpdatePlayerPositionClientRpc")]
        [HarmonyPostfix]
        private static void UpdatePlayerPosition(ref PlayerControllerB __instance)
        {
            if (__instance.isInHangarShipRoom && isCursed)
            {
                if (!HasCursedScrap(ref __instance))
                {
                    RemoveAllEffects();
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
        [HarmonyPostfix]
        private static void PlayerDeath()
        {
            if (isCursed)
            {
                RemoveAllEffects();
            }
        }

        internal static string GetCurseEffect(ref GrabbableObject grabbableObject)
        {
            if (grabbableObject != null)
            {
                ScanNodeProperties componentInChildren = ((Component)(object)grabbableObject).gameObject.GetComponentInChildren<ScanNodeProperties>();
                if (componentInChildren != null && componentInChildren.subText.Length > 1)
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

        private static bool HasCursedScrap(ref PlayerControllerB player)
        {
            bool hasCursedScrap = false;
            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                if (player.ItemSlots[i] != null)
                {
                    if (!string.IsNullOrEmpty(GetCurseEffect(ref player.ItemSlots[i])))
                    {
                        hasCursedScrap = true;
                        return hasCursedScrap;
                    }
                }
            }
            return hasCursedScrap;
        }

        private static void RemoveAllEffects()
        {
            isCursed = false;
            // INHIBITION
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("Jump", false).Enable();
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch", false).Enable();
            // CONFUSION
            isConfusion = false;
            IngamePlayerSettings.Instance.playerInput.actions.RemoveAllBindingOverrides();
            IngamePlayerSettings.Instance.LoadSettingsFromPrefs();
            // MUTE
            isMute = false;
            IngamePlayerSettings.Instance.unsavedSettings.micEnabled = true;
            IngamePlayerSettings.Instance.settings.micEnabled = true;
            // DEAFNESS
            isDeafness = false;
            if (savedMasterVolume != 0f)
            {
                IngamePlayerSettings.Instance.ChangeMasterVolume((int)(savedMasterVolume * 100));
            }
            // BLURRY
            HUDManagerPatch.isBlurry = false;
            // ERRANT
            isErrant = false;
            // PARALYSIS
            isParalysis = false;
            // SHADOW
            EnemyAIPatch.isShadow = false;
            // Mise à jour des paramètres
            IngamePlayerSettings.Instance.UpdateGameToMatchSettings();
        }

        /*internal static void RemovePlayerEffect(ref string curseEffect, ref PlayerControllerB player)
        {
            if (!string.IsNullOrEmpty(curseEffect))
            {
                bool isUniqueEffect = true;
                for (int i = 0; i < player.ItemSlots.Length; i++)
                {
                    if (player.ItemSlots[i] != null && player.currentlyHeldObjectServer != player.ItemSlots[i])
                    {
                        string curseEffectSlot = GetCurseEffect(ref player.ItemSlots[i]);
                        if (!string.IsNullOrEmpty(curseEffectSlot) && curseEffect == curseEffectSlot)
                        {
                            isUniqueEffect = false;
                            break;
                        }
                    }
                }
                if (isUniqueEffect)
                {
                    switch (curseEffect)
                    {
                        case Constants.INHIBITION:
                            string jumpMethod = "Jump_performed", jumpAction = "Jump";
                            EnablePlayerAction(ref jumpMethod, ref jumpAction, ref player);
                            string crouchMethod = "Crouch_performed", crouchAction = "Crouch";
                            EnablePlayerAction(ref crouchMethod, ref crouchAction, ref player);
                            break;
                        case Constants.CONFUSION:
                            isConfusion = false;
                            IngamePlayerSettings.Instance.playerInput.actions.RemoveAllBindingOverrides();
                            if (!string.IsNullOrEmpty(IngamePlayerSettings.Instance.settings.keyBindings))
                            {
                                IngamePlayerSettings.Instance.playerInput.actions.LoadBindingOverridesFromJson(IngamePlayerSettings.Instance.settings.keyBindings);
                            }
                            break;
                        case Constants.BLURRY:
                            HUDManagerPatch.isBlurry = false;
                            break;
                        case Constants.MUTE:
                            isMute = false;
                            IngamePlayerSettings.Instance.unsavedSettings.micEnabled = true;
                            IngamePlayerSettings.Instance.settings.micEnabled = true;
                            IngamePlayerSettings.Instance.UpdateGameToMatchSettings();
                            break;
                        case Constants.DEAFNESS:
                            isDeafness = false;
                            IngamePlayerSettings.Instance.ChangeMasterVolume((int)(masterVolumePref * 100));
                            break;
                        case Constants.ERRANT:
                            isErrant = false;
                            break;
                        default:
                            break;
                    }
                }
            }
        }*/

        private static void ApplyConfusion()
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

        private static void TeleportPlayer(ref PlayerControllerB player)
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

        internal static void Paralyze(ref PlayerControllerB __instance)
        {
            if (isParalysis)
            {
                __instance.JumpToFearLevel(0.6f);
                __instance.StartCoroutine(ParalyzeCoroutine());
            }
        }

        private static IEnumerator ParalyzeCoroutine()
        {
            EnablePlayerActions(false);
            yield return new WaitForSeconds(ConfigManager.paralysisTime.Value);
            EnablePlayerActions(true);
        }

        private static void EnablePlayerActions(bool enable)
        {
            string[] actionNames = { "Move", "Jump", "Crouch", "Interact", "ItemSecondaryUse", "ItemTertiaryUse", "ActivateItem", "SwitchItem", "InspectItem", "Emote1", "Emote2" };

            foreach (string actionName in actionNames)
            {
                if (enable) IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Enable();
                else IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Disable();
            }
        }
    }
}
