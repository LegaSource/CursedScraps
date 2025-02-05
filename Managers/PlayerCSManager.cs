using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using GameNetcodeStuff;
using System.Linq;
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
                    AddCurseEffect(playerBehaviour, curseEffect, enable);
                else if (!enable && playerBehaviour.activeCurses.Contains(curseEffect))
                    AddCurseEffect(playerBehaviour, curseEffect, enable);
            }
        }

        public static void AddCurseEffect(PlayerCSBehaviour playerBehaviour, CurseEffect curseEffect, bool enable)
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
                            Exploration.ApplyExploration(enable, playerBehaviour);
                            break;
                        case Constants.INHIBITION:
                            Inhibition.ApplyInhibition(enable, playerBehaviour);
                            break;
                        default:
                            break;
                    }
                }
                // Effets serveur
                switch (curseEffect.CurseName)
                {
                    case Constants.DIMINUTIVE:
                        Diminutive.ApplyDiminutive(enable, playerBehaviour);
                        break;
                    case Constants.COMMUNICATION:
                        Communication.ApplyCommunication(enable, playerBehaviour);
                        break;
                    case Constants.ONE_FOR_ALL:
                        OneForAll.ApplyOneForAll(enable, playerBehaviour);
                        break;
                    case Constants.SACRIFICE:
                        Sacrifice.ApplySacrifice(enable, playerBehaviour);
                        break;
                    default:
                        break;
                }

                if (localPlayer != playerBehaviour.playerProperties && localPlayer.isPlayerDead)
                    HUDCSManager.RefreshCursesText(playerBehaviour);
                
                // Reset du compteur de pénalité
                SORCSBehaviour sorBehaviour = StartOfRound.Instance.GetComponent<SORCSBehaviour>();
                sorBehaviour.counter = 0;
                sorBehaviour.scannedObjects.Clear();
            }
        }

        public static void TeleportPlayer(PlayerControllerB player, Vector3 position, bool isInElevator, bool isInHangarShipRoom, bool isInsideFactory)
        {
            player.isInElevator = isInElevator;
            player.isInHangarShipRoom = isInHangarShipRoom;
            player.isInsideFactory = isInsideFactory;
            player.averageVelocity = 0f;
            player.velocityLastFrame = Vector3.zero;
            player.TeleportPlayer(position);
        }

        public static void EnablePlayerActions(CurseEffect curseEffect, bool enable)
        {
            PlayerCSBehaviour playerCSBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            if (playerCSBehaviour == null) return;

            string[] actionNames = { "Move", "Jump", "Crouch", "Interact", "ItemSecondaryUse", "ItemTertiaryUse", "ActivateItem", "SwitchItem", "InspectItem", "Emote1", "Emote2" };

            if (enable)
                playerCSBehaviour.actionsBlockedBy.Remove(curseEffect);
            else if (!playerCSBehaviour.actionsBlockedBy.Contains(curseEffect))
                playerCSBehaviour.actionsBlockedBy.Add(curseEffect);

            foreach (string actionName in actionNames)
            {
                if (enable && !playerCSBehaviour.actionsBlockedBy.Any()) IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Enable();
                else IngamePlayerSettings.Instance.playerInput.actions.FindAction(actionName, false).Disable();
            }
        }
    }
}
