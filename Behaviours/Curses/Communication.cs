using CursedScraps.Behaviours.Items;
using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    public class Communication
    {
        public static Coroutine trackedItemCoroutine;

        public static void ApplyCommunication(bool enable, PlayerCSBehaviour playerBehaviour)
        {
            if (enable)
            {
                PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (localPlayer.IsServer || localPlayer.IsHost)
                {
                    // Si le joueur a déjà eu la malédiction mais est parvenu à la retirer, si la carte qui lui était affectée n'a pas été utilisée depuis on la reprend
                    GrabbableObject grabbableObject = Object.FindObjectsOfType<OldScroll>()
                        .FirstOrDefault(o => !o.deactivated && o.assignedPlayer != null && o.assignedPlayer == playerBehaviour.playerProperties);
                    if (grabbableObject == null)
                    {
                        Item itemToSpawn = CursedScraps.customItems.FirstOrDefault(c => c.Item.itemName.Equals(Constants.OLD_SCROLL)).Item;
                        Vector3 position = ObjectCSManager.GetFurthestPositionScrapSpawn(playerBehaviour.playerProperties.transform.position, ref itemToSpawn);
                        grabbableObject = ObjectCSManager.SpawnItem(ref itemToSpawn.spawnPrefab, ref position);
                    }
                    CursedScrapsNetworkManager.Instance.AssignTrackedItemServerRpc((int)playerBehaviour.playerProperties.playerClientId, grabbableObject.GetComponent<NetworkObject>());
                }

                if (localPlayer != playerBehaviour.playerProperties && localPlayer.isPlayerDead)
                {
                    ApplyCommunicationForDeadPlayer(ref playerBehaviour);
                }
            }
            else
            {
                playerBehaviour.trackedItem = null;
                playerBehaviour.canEscape = false;
            }
        }

        public static void ApplyCommunicationForDeadPlayer(ref PlayerCSBehaviour playerBehaviour)
        {
            if (trackedItemCoroutine != null)
            {
                HUDManager.Instance.StopCoroutine(trackedItemCoroutine);
            }
            trackedItemCoroutine = HUDManager.Instance.StartCoroutine(HUDCSManager.StartTrackedItemCoroutine(playerBehaviour));
        }

        public static bool CanEscape(ref PlayerCSBehaviour playerBehaviour, string message)
        {
            if (playerBehaviour.activeCurses.FirstOrDefault(p => p.CurseName.Equals(Constants.COMMUNICATION)) != null && !playerBehaviour.canEscape)
            {
                if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, message);
                }
                return false;
            }
            return true;
        }
    }
}
