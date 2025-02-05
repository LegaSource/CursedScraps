using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Behaviours.Items
{
    public class OldScroll : PhysicsProp
    {
        public PlayerControllerB assignedPlayer;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (!buttonDown) return;
            if (playerHeldBy == null) return;

            if (assignedPlayer != null)
            {
                PlayerCSBehaviour playerBehaviour = assignedPlayer.GetComponent<PlayerCSBehaviour>();
                if (playerHeldBy != assignedPlayer && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.COMMUNICATION)))
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "This item is assigned to another player");
                    return;
                }
                playerBehaviour.canEscape = true;
            }
            StartCoroutine(ShowEntrancesCoroutine(playerHeldBy));
            CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
        }

        public IEnumerator ShowEntrancesCoroutine(PlayerControllerB player)
        {
            CustomPassManager.SetupCustomPassForDoors(!player.isInsideFactory);
            yield return new WaitForSeconds(ConfigManager.oldScrollAura.Value);
            CustomPassManager.RemoveAuraFromDoor();
        }
    }
}
