using CursedScraps.Behaviours.Curses;
using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using Unity.Netcode;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Managers;

public class ObjectCSManager
{
    public static void PostGrabObject(PlayerControllerB player, GrabbableObject grabbableObject)
    {
        if (player == null || grabbableObject == null) return;

        Errant.PostGrabTeleport(player, grabbableObject);
        CursedScrapsNetworkManager.Instance.ScaleObjectEveryoneRpc(grabbableObject.GetComponent<NetworkObject>(), (int)player.playerClientId, false);

        foreach (CurseEffectType curseType in GetCurses(grabbableObject.gameObject))
        {
            CursedScrapsNetworkManager.Instance.ApplyPlayerCurseEveryoneRpc((int)player.playerClientId, curseType.Name, curseType.Duration);
            LFCNetworkManager.Instance.SetScrapValueEveryoneRpc(grabbableObject.GetComponent<NetworkObject>(), (int)(grabbableObject.scrapValue * curseType.Multiplier));
            CursedScrapsNetworkManager.Instance.RemoveObjectCurseEveryoneRpc(grabbableObject.GetComponent<NetworkObject>(), curseType.Name);
        }
    }

    public static bool PreDropObject(PlayerControllerB __instance, GrabbableObject grabbableObject)
    {
        if (HasCurse(__instance.gameObject, Constants.CAPTIVE))
        {
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you to do this action.");
            return false;
        }
        if (!Fragile.PreDropObject(__instance, grabbableObject)) return false;
        Errant.PreDropTeleport(__instance, grabbableObject);
        CursedScrapsNetworkManager.Instance.ScaleObjectEveryoneRpc(grabbableObject.GetComponent<NetworkObject>(), (int)__instance.playerClientId, true);

        return true;
    }

    public static void PostDropObject(PlayerControllerB __instance) => Errant.PostDropTeleport(__instance);
}
