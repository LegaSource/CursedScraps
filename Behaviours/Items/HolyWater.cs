using CursedScraps.Managers;
using CursedScraps.Registries;
using LegaFusionCore.Managers.NetworkManagers;
using Unity.Netcode;

namespace CursedScraps.Behaviours.Items;

public class HolyWater : PhysicsProp
{
    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (buttonDown && playerHeldBy != null)
        {
            if (CSCurseRegistry.HasCurse(playerHeldBy.gameObject))
            {
                CursedScrapsNetworkManager.Instance.ClearPlayerCursesEveryoneRpc((int)playerHeldBy.playerClientId);
                LFCNetworkManager.Instance.DestroyObjectEveryoneRpc(GetComponent<NetworkObject>());
                return;
            }
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "You have no active curse.");
        }
    }
}
