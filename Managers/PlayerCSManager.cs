using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using GameNetcodeStuff;
using UnityEngine;

namespace CursedScraps.Managers
{
    public class PlayerCSManager
    {
        public static void SetPlayerCurseEffect(PlayerControllerB player, CurseEffect curseEffect, bool enable)
        {
            PlayerCSBehaviour playerBehaviour = player.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                if (enable && !playerBehaviour.activeCurses.Contains(curseEffect))
                    AddCurseEffect(ref playerBehaviour, curseEffect, enable);
                else if (!enable && playerBehaviour.activeCurses.Contains(curseEffect))
                    AddCurseEffect(ref playerBehaviour, curseEffect, enable);
            }
        }

        public static void AddCurseEffect(ref PlayerCSBehaviour playerBehaviour, CurseEffect curseEffect, bool enable)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (!string.IsNullOrEmpty(curseEffect.CurseName))
            {
                if (enable)
                {
                    playerBehaviour.activeCurses.Add(curseEffect);
                    if (ConfigManager.isCurseInfoOn.Value && localPlayer == playerBehaviour.playerProperties)
                        HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"You have just been affected by the curse {curseEffect.CurseName}");
                }
                else
                {
                    playerBehaviour.activeCurses.Remove(curseEffect);
                }

                // Effets locaux
                if (localPlayer == playerBehaviour.playerProperties)
                {
                    switch (curseEffect.CurseName)
                    {
                        case Constants.CONFUSION:
                            Confusion.ApplyConfusion(enable);
                            break;
                        case Constants.MUTE:
                            Mute.ApplyMute(enable);
                            break;
                        case Constants.DEAFNESS:
                            Deafness.ApplyDeafness(enable);
                            break;
                        case Constants.EXPLORATION:
                            Exploration.ApplyExploration(enable, ref playerBehaviour);
                            break;
                        case Constants.INHIBITION:
                            Inhibition.ApplyInhibition(enable, ref playerBehaviour);
                            break;
                        default:
                            break;
                    }
                }
                // Effets serveur
                switch (curseEffect.CurseName)
                {
                    case Constants.DIMINUTIVE:
                        Diminutive.ApplyDiminutive(enable, ref playerBehaviour);
                        break;
                    case Constants.COMMUNICATION:
                        Communication.ApplyCommunication(enable, playerBehaviour);
                        break;
                    default:
                        break;
                }

                if (localPlayer != playerBehaviour.playerProperties && localPlayer.isPlayerDead)
                {
                    HUDCSManager.RefreshCursesText(ref playerBehaviour);
                }
                
                // Reset du compteur de pénalité
                SORCSBehaviour sorBehaviour = StartOfRound.Instance.GetComponent<SORCSBehaviour>();
                sorBehaviour.counter = 0;
                sorBehaviour.scannedObjects.Clear();
            }
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

        public static void EnablePlayerActions(ref CurseEffect curseEffect, bool enable)
        {
            PlayerCSBehaviour playerCSBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            if (playerCSBehaviour != null)
            {
                string[] actionNames = { "Move", "Jump", "Crouch", "Interact", "ItemSecondaryUse", "ItemTertiaryUse", "ActivateItem", "SwitchItem", "InspectItem", "Emote1", "Emote2" };

                if (enable)
                    playerCSBehaviour.actionsBlockedBy.Remove(curseEffect);
                else if (!playerCSBehaviour.actionsBlockedBy.Contains(curseEffect))
                    playerCSBehaviour.actionsBlockedBy.Add(curseEffect);

                foreach (string actionName in actionNames)
                {
                    if (enable && playerCSBehaviour.actionsBlockedBy.Count == 0) IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Enable();
                    else IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Disable();
                }
            }
        }
    }
}
