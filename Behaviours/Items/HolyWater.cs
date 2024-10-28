using CursedScraps.Managers;
using System.Linq;
using Unity.Netcode;

namespace CursedScraps.Behaviours.Items
{
    public class HolyWater : PhysicsProp
    {
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (buttonDown && playerHeldBy != null)
            {
                PlayerCSBehaviour playerBehaviour = playerHeldBy.GetComponent<PlayerCSBehaviour>();
                if (playerBehaviour != null && playerBehaviour.activeCurses.Count() > 0)
                {
                    CursedScrapsNetworkManager.Instance.RemoveAllPlayerCurseEffectServerRpc((int)playerHeldBy.playerClientId);
                    playerHeldBy.DropAllHeldItemsAndSync();
                    CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(GetComponent<NetworkObject>());
                }
                else
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "You have no active curse.");
                }
            }
        }
    }
}
