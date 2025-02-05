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
            if (!enable)
            {
                playerBehaviour.trackedItem = null;
                playerBehaviour.canEscape = false;
                return;
            }

            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer.IsServer || localPlayer.IsHost)
            {
                // Si le joueur a déjà eu la malédiction mais est parvenu à la retirer, si la carte qui lui était affectée n'a pas été utilisée depuis on la reprend
                GrabbableObject grabbableObject = Object.FindObjectsOfType<OldScroll>()
                    .FirstOrDefault(o => !o.deactivated && o.assignedPlayer != null && o.assignedPlayer == playerBehaviour.playerProperties);
                if (grabbableObject == null)
                {
                    Item itemToSpawn = CursedScraps.customItems.FirstOrDefault(c => c.Item.itemName.Equals(Constants.OLD_SCROLL)).Item;
                    Vector3 position = ObjectCSManager.GetFurthestPositionScrapSpawn(playerBehaviour.playerProperties.transform.position, itemToSpawn);
                    grabbableObject = ObjectCSManager.SpawnItem(itemToSpawn.spawnPrefab, position);
                }
                CursedScrapsNetworkManager.Instance.AssignTrackedItemServerRpc((int)playerBehaviour.playerProperties.playerClientId, grabbableObject.GetComponent<NetworkObject>());
            }

            if (localPlayer == playerBehaviour.playerProperties) return;
            if (!localPlayer.isPlayerDead) return;
                
            ApplyCommunicationForDeadPlayer(playerBehaviour);
        }

        public static bool IsCommunication(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.COMMUNICATION))) return false;
            return true;
        }

        public static void ApplyCommunicationForDeadPlayer(PlayerCSBehaviour playerBehaviour)
        {
            if (!IsCommunication(playerBehaviour)) return;

            if (trackedItemCoroutine != null)
                HUDManager.Instance.StopCoroutine(trackedItemCoroutine);
            trackedItemCoroutine = HUDManager.Instance.StartCoroutine(HUDCSManager.StartTrackedItemCoroutine(playerBehaviour));
        }

        public static bool CanEscape(PlayerCSBehaviour playerBehaviour, string message)
        {
            if (!IsCommunication(playerBehaviour)) return true;
            if (playerBehaviour.canEscape) return true;

            if (playerBehaviour.playerProperties != GameNetworkManager.Instance.localPlayerController) return false;
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, message);
            return false;
        }
    }
}
