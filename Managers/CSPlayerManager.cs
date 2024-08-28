using CursedScraps.Behaviours;
using GameNetcodeStuff;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace CursedScraps.Managers
{
    internal class CSPlayerManager
    {
        // DEAFNESS
        private static float savedMasterVolume = 0f;

        public static void SetPlayerCurseEffect(PlayerControllerB player, CurseEffect curseEffect, bool enable)
        {
            PlayerCSBehaviour playerBehaviour = player.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                if (enable && !playerBehaviour.activeCurses.Contains(curseEffect))
                {
                    AddCurseEffect(ref playerBehaviour, curseEffect, enable);
                }
                else if (!enable && playerBehaviour.activeCurses.Contains(curseEffect))
                {
                    AddCurseEffect(ref playerBehaviour, curseEffect, enable);
                }
            }
        }

        public static void AddCurseEffect(ref PlayerCSBehaviour playerBehaviour, CurseEffect curseEffect, bool enable)
        {
            bool isReturnOk = true;
            if (!string.IsNullOrEmpty(curseEffect.CurseName))
            {
                // Effets locaux
                if (GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
                {
                    switch (curseEffect.CurseName)
                    {
                        case Constants.CONFUSION:
                            ApplyConfusion(enable);
                            break;
                        case Constants.MUTE:
                            ApplyMute(enable);
                            break;
                        case Constants.DEAFNESS:
                            ApplyDeafness(enable);
                            break;
                        case Constants.EXPLORATION:
                            //CSPlayer.ApplyExploration(enable);
                            break;
                        default:
                            break;
                    }
                }
                // Effets serveur
                switch (curseEffect.CurseName)
                {
                    case Constants.DIMINUTIVE:
                        ApplyDiminutive(enable, ref playerBehaviour);
                        break;
                    case Constants.COMMUNICATION:
                        isReturnOk = ApplyCommunication(enable, playerBehaviour, ref curseEffect);
                        break;
                    case Constants.SYNCHRONIZATION:
                        ApplySynchronization(enable, playerBehaviour, ref curseEffect);
                        break;
                    default:
                        break;
                }
                
                if (enable && isReturnOk)
                {
                    playerBehaviour.activeCurses.Add(curseEffect);
                    if (ConfigManager.isCurseInfoOn.Value && GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"You have just been affected by the curse {curseEffect.CurseName}");
                    }
                }
                else if (!enable && isReturnOk)
                {
                    playerBehaviour.activeCurses.Remove(curseEffect);
                }
            }
        }

        public static void ApplyConfusion(bool enable)
        {
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

        public static void ApplyMute(bool enable)
        {
            if (enable)
            {
                IngamePlayerSettings.Instance.unsavedSettings.micEnabled = false;
                IngamePlayerSettings.Instance.settings.micEnabled = false;

                foreach (SettingsOption setting in Object.FindObjectsOfType<SettingsOption>(includeInactive: true).ToList().Where(s => s.optionType == SettingsOptionType.MicEnabled))
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

        public static void ApplyDeafness(bool enable)
        {
            if (enable)
            {
                savedMasterVolume = IngamePlayerSettings.Instance.settings.masterVolume == 0f ? savedMasterVolume : IngamePlayerSettings.Instance.settings.masterVolume;
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

        public static void ApplyErrant()
        {
            TeleportPlayer(ref GameNetworkManager.Instance.localPlayerController);
        }

        public static void Paralyze(ref PlayerCSBehaviour playerBehaviour)
        {
            CurseEffect curseEffect = playerBehaviour.activeCurses.Where(c => c.CurseName.Equals(Constants.PARALYSIS)).FirstOrDefault();
            if (curseEffect != null)
            {
                playerBehaviour.playerProperties.JumpToFearLevel(0.6f);
                playerBehaviour.playerProperties.StartCoroutine(ParalyzeCoroutine(curseEffect));
            }
        }

        public static IEnumerator ParalyzeCoroutine(CurseEffect curseEffect)
        {
            EnablePlayerActions(ref curseEffect, false);
            yield return new WaitForSeconds(ConfigManager.paralysisTime.Value);
            EnablePlayerActions(ref curseEffect, true);
        }

        public static void TeleportPlayer(ref PlayerControllerB player)
        {
            if (!player.isInHangarShipRoom)
            {
                Vector3 position = RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                position = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position);
                player.isInElevator = false;
                player.isInHangarShipRoom = false;
                player.isInsideFactory = true;
                player.averageVelocity = 0f;
                player.velocityLastFrame = Vector3.zero;
                player.TeleportPlayer(position);
            }
        }

        public static void ApplyDiminutive(bool enable, ref PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
            {
                playerBehaviour.playerProperties.localVisor.gameObject.SetActive(!enable);
            }

            if (enable)
            {
                playerBehaviour.originalScale = playerBehaviour.playerProperties.transform.localScale;
                playerBehaviour.playerProperties.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                playerBehaviour.playerProperties.movementSpeed /= ConfigManager.diminutiveSpeed.Value;
                playerBehaviour.playerProperties.grabDistance /= ConfigManager.diminutiveGrab.Value;
            }
            else
            {
                playerBehaviour.playerProperties.transform.localScale = playerBehaviour.originalScale;
                playerBehaviour.playerProperties.movementSpeed *= ConfigManager.diminutiveSpeed.Value;
                playerBehaviour.playerProperties.grabDistance *= ConfigManager.diminutiveGrab.Value;
            }
        }

        public static IEnumerator PlayerDoubleJump(PlayerCSBehaviour playerBehaviour)
        {
            PlayerControllerB player = playerBehaviour.playerProperties;
            playerBehaviour.doubleJump = true;
            player.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
            if (player.jumpCoroutine != null)
            {
                player.StopCoroutine(player.jumpCoroutine);
            }
            player.jumpCoroutine = player.StartCoroutine(player.PlayerJump());

            yield return new WaitUntil(() => player.thisController.isGrounded);

            playerBehaviour.doubleJump = false;
        }

        public static bool ApplyCommunication(bool enable, PlayerCSBehaviour playerBehaviour, ref CurseEffect curseEffect)
        {
            // Test sur communication pour s'assurer du fait qu'on est bien sur le joueur qui ramasse l'objet
            ObjectCSBehaviour objectBehaviour = playerBehaviour.playerProperties.currentlyHeldObjectServer?.GetComponent<ObjectCSBehaviour>();
            if (enable && objectBehaviour != null && objectBehaviour.curseEffects.FirstOrDefault(c => c.CurseName.Equals(Constants.COMMUNICATION)) != null)
            {
                if (objectBehaviour.playerOwner == null && playerBehaviour.trackedScrap == null)
                {
                    return ApplyCommunicationFirstPart(playerBehaviour, ref objectBehaviour, ref curseEffect);
                }
                else
                {
                    ApplyCommunicationSecondPart(playerBehaviour, ref objectBehaviour, ref curseEffect);
                }
            }
            // Suppression de la malédiction
            else if (!enable && GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
            {
                playerBehaviour.trackedScrap = null;
                playerBehaviour.coopPlayer = null;
                CSHUDManager.forceEndChrono = true;
            }
            return true;
        }

        public static bool ApplyCommunicationFirstPart(PlayerCSBehaviour playerBehaviour, ref ObjectCSBehaviour objectBehaviour, ref CurseEffect curseEffect)
        {
            // Sélection du joueur devant récupérer la seconde partie de l'objet
            PlayerControllerB selectedPlayer = StartOfRound.Instance.allPlayerScripts
                .Where(p => p.isPlayerControlled && !p.isPlayerDead && p != playerBehaviour.playerProperties)
                .OrderBy(p => Vector3.Distance(p.transform.position, playerBehaviour.playerProperties.transform.position))
                .FirstOrDefault();

            if (selectedPlayer != null)
            {
                PlayerCSBehaviour behaviourSelectedPlayer = selectedPlayer.GetComponent<PlayerCSBehaviour>();
                if (behaviourSelectedPlayer != null)
                {
                    behaviourSelectedPlayer.coopPlayer = playerBehaviour.playerProperties;
                    playerBehaviour.coopPlayer = selectedPlayer;
                    if (GameNetworkManager.Instance.localPlayerController == selectedPlayer)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, "A player has picked the first part of a cursed object, and you've been chosen to find the second part. Find it to set him free!");
                    }
                    else if (GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
                    {
                        // Immobiliser le joueur
                        EnablePlayerActions(ref curseEffect, false);
                        HUDManager.Instance.StartCoroutine(CSHUDManager.StartTrackedScrapCoroutine());
                    }

                    // Récupération de l'objet à tracker
                    if (!objectBehaviour.isSplitted)
                    {
                        Vector3 position = CSObjectManager.GetFurthestPositionScrapSpawn(playerBehaviour.playerProperties.transform.position, ref objectBehaviour.objectProperties.itemProperties);
                        CSObjectManager.CloneScrap(Constants.COMMUNICATION,
                            Constants.COMM_CORE,
                            Constants.COMM_REFLECTION,
                            ref position,
                            ref playerBehaviour.playerProperties,
                            ref objectBehaviour);
                    }
                    else if (GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
                    {
                        playerBehaviour.trackedScrap = CSObjectManager.GetCloneScrap(objectBehaviour.objectProperties);
                        CursedScrapsNetworkManager.Instance.SetPlayerOwnerScrapServerRpc(playerBehaviour.trackedScrap.GetComponent<NetworkObject>(), (int)playerBehaviour.coopPlayer.playerClientId);
                    }
                }
            }
            else if (GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties)
            {
                HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, "No player could be found to share this curse with you.");
                playerBehaviour.trackedScrap = null;
                playerBehaviour.coopPlayer = null;
                // Immobiliser le joueur
                EnablePlayerActions(ref curseEffect, false);
                return false;
            }
            return true;
        }

        public static void ApplyCommunicationSecondPart(PlayerCSBehaviour playerBehaviour, ref ObjectCSBehaviour objectBehaviour, ref CurseEffect curseEffect)
        {
            ObjectCSBehaviour objectBehaviourClone = CSObjectManager.GetCloneScrap(objectBehaviour.objectProperties)?.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviourClone != null)
            {
                objectBehaviour.playerOwner = playerBehaviour.playerProperties;
                objectBehaviourClone.playerOwner = playerBehaviour.coopPlayer;

                if (playerBehaviour.coopPlayer != null
                    && (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController
                        || playerBehaviour.coopPlayer == GameNetworkManager.Instance.localPlayerController))
                {
                    if (playerBehaviour.coopPlayer == GameNetworkManager.Instance.localPlayerController)
                    {
                        // Le joueur n'est plus immobilisé
                        EnablePlayerActions(ref curseEffect, true);
                        playerBehaviour.coopPlayer.GetComponent<PlayerCSBehaviour>().trackedScrap = null;
                    }
                    HUDManager.Instance.StartCoroutine(CSHUDManager.StartChronoCoroutine(ConfigManager.communicationChrono.Value));
                }
            }
        }

        public static void ApplySynchronization(bool enable, PlayerCSBehaviour playerBehaviour, ref CurseEffect curseEffect)
        {
            // Test sur communication pour s'assurer du fait qu'on est bien sur le joueur qui ramasse l'objet
            ObjectCSBehaviour objectBehaviour = playerBehaviour.playerProperties.currentlyHeldObjectServer?.GetComponent<ObjectCSBehaviour>();
            if (enable && objectBehaviour != null)
            {
                // Récupération de l'objet à tracker
                if (!objectBehaviour.isSplitted)
                {
                    Vector3 position = objectBehaviour.objectProperties.transform.position;
                    CSObjectManager.CloneScrap(Constants.SYNCHRONIZATION,
                        Constants.SYNC_CORE,
                        Constants.SYNC_REFLECTION,
                        ref position,
                        ref playerBehaviour.playerProperties,
                        ref objectBehaviour);

                    if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
                    {
                        // Immobiliser le joueur
                        EnablePlayerActions(ref curseEffect, false);
                    }
                }
                else
                {
                    GrabbableObject grabbableObject = CSObjectManager.GetCloneScrap(objectBehaviour.objectProperties);
                    if (grabbableObject != null)
                    {
                        if (grabbableObject.isHeld && grabbableObject.playerHeldBy != null)
                        {
                            objectBehaviour.playerOwner = playerBehaviour.playerProperties;
                            grabbableObject.GetComponent<ObjectCSBehaviour>().playerOwner = grabbableObject.playerHeldBy;

                            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController
                                || grabbableObject.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                            {
                                if (grabbableObject.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                                {
                                    // Le joueur n'est plus immobilisé
                                    EnablePlayerActions(ref curseEffect, true);
                                    PlayerCSBehaviour playerBehaviourCoop = grabbableObject.playerHeldBy.GetComponent<PlayerCSBehaviour>();
                                    SwitchPlayerCamera(ref playerBehaviourCoop, playerBehaviour.playerProperties, true);
                                }
                                else
                                {
                                    SwitchPlayerCamera(ref playerBehaviour, grabbableObject.playerHeldBy, true);
                                }
                            }
                        }
                        else if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
                        {
                            // Immobiliser le joueur
                            EnablePlayerActions(ref curseEffect, false);
                        }
                    }
                }
            }
            // Suppression de la malédiction
            else if (!enable && GameNetworkManager.Instance.localPlayerController == playerBehaviour.playerProperties && playerBehaviour.coopPlayer != null)
            {
                SwitchPlayerCamera(ref playerBehaviour, playerBehaviour.coopPlayer, false);
            }
        }

        private static void SwitchPlayerCamera(ref PlayerCSBehaviour playerBehaviour, PlayerControllerB playerCoop, bool isSwitched)
        {
            playerCoop.gameplayCamera.enabled = isSwitched;
            playerBehaviour.playerProperties.thisPlayerModelArms.enabled = !isSwitched;
            playerCoop.thisPlayerModelArms.enabled = isSwitched;

            (playerCoop.localVisorTargetPoint, playerBehaviour.playerProperties.localVisorTargetPoint) = (playerBehaviour.playerProperties.localVisorTargetPoint, playerCoop.localVisorTargetPoint);
            (playerCoop.nightVision, playerBehaviour.playerProperties.nightVision) = (playerBehaviour.playerProperties.nightVision, playerCoop.nightVision);
            (playerCoop.thisPlayerModel.gameObject.layer, playerBehaviour.playerProperties.thisPlayerModel.gameObject.layer) = (playerBehaviour.playerProperties.thisPlayerModel.gameObject.layer, playerCoop.thisPlayerModel.gameObject.layer);
            (playerCoop.thisPlayerModelArms.gameObject.layer, playerBehaviour.playerProperties.thisPlayerModelArms.gameObject.layer) = (playerBehaviour.playerProperties.thisPlayerModelArms.gameObject.layer, playerCoop.thisPlayerModelArms.gameObject.layer);

            if (isSwitched)
            {
                playerBehaviour.coopPlayer = playerCoop;

                StartOfRound.Instance.activeCamera = playerCoop.gameplayCamera;
                playerBehaviour.playerProperties.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
                playerCoop.thisPlayerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                StartOfRound.Instance.activeCamera.transform.SetParent(playerCoop.gameplayCamera.transform, worldPositionStays: false);
            }
            else
            {
                StartOfRound.Instance.activeCamera = playerBehaviour.playerProperties.gameplayCamera;
                playerBehaviour.playerProperties.thisPlayerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                playerCoop.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
                StartOfRound.Instance.activeCamera.transform.SetParent(playerBehaviour.playerProperties.gameplayCamera.transform, worldPositionStays: false);

                playerBehaviour.coopPlayer = null;
            }
        }

        public static void DesactiveCoopEffect(ref PlayerCSBehaviour playerBehaviour, CurseEffect curseEffect)
        {
            // isPlayerGone - le joueur coop est mort - sinon les deux joueurs doivent se trouver dans le vaisseau
            if (playerBehaviour.coopPlayer != null)
            {
                // isCoop: est le joueur coop
                CursedScrapsNetworkManager.Instance.DesactiveCoopEffectServerRpc((int)playerBehaviour.coopPlayer.playerClientId, curseEffect.CurseName, isCoop: true);
                CursedScrapsNetworkManager.Instance.DesactiveCoopEffectServerRpc((int)playerBehaviour.playerProperties.playerClientId, curseEffect.CurseName, isCoop: false);
            }
            // Cas pour débloquer le joueur lorsqu'il ramasse la pièce mais que la seconde n'est pas encore ramassée
            else
            {
                EnablePlayerActions(ref curseEffect, true);
                CursedScrapsNetworkManager.Instance.SetPlayerCurseEffectServerRpc((int)playerBehaviour.playerProperties.playerClientId, curseEffect.CurseName, false);
            }
        }

        public static bool RemoveCoopEffects(ref ObjectCSBehaviour objectBehaviour)
        {
            // Ne peut avoir qu'une seule malédiction coop en même temps
            CurseEffect curseEffect = objectBehaviour.curseEffects.FirstOrDefault(c => c.IsCoop);
            if (curseEffect != null)
            {
                GrabbableObject searchedScrap = CSObjectManager.GetCloneScrap(objectBehaviour.objectProperties);
                if (searchedScrap != null && (searchedScrap.isInShipRoom || searchedScrap.isInElevator))
                {
                    CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(searchedScrap.GetComponent<NetworkObject>());
                    CursedScrapsNetworkManager.Instance.RemoveObjectCoopEffectServerRpc(objectBehaviour.objectProperties.GetComponent<NetworkObject>());
                    return true;
                }
                // C'est un objet coop mais la seconde partie est hors du vaisseau
                return false;
            }
            // Ce n'est pas un objet coop
            return true;
        }

        public static void EnablePlayerActions(ref CurseEffect curseEffect, bool enable)
        {
            PlayerCSBehaviour playerCSBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            if (playerCSBehaviour != null)
            {
                string[] actionNames = { "Move", "Jump", "Crouch", "Interact", "ItemSecondaryUse", "ItemTertiaryUse", "ActivateItem", "SwitchItem", "InspectItem", "Emote1", "Emote2" };

                if (enable)
                {
                    playerCSBehaviour.actionsBlockedBy.Remove(curseEffect);
                }
                else
                {
                    if (!playerCSBehaviour.actionsBlockedBy.Contains(curseEffect))
                    {
                        playerCSBehaviour.actionsBlockedBy.Add(curseEffect);
                    }
                }

                foreach (string actionName in actionNames)
                {
                    if (enable && playerCSBehaviour.actionsBlockedBy.Count == 0) IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Enable();
                    else IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Disable();
                }
            }
        }
    }
}
