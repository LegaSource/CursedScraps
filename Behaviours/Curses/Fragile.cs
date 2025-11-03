using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using System.Collections.Generic;
using Unity.Netcode;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Fragile(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.FRAGILE));

    public static bool PreDropObject(PlayerControllerB player, GrabbableObject grabbableObject)
        => player.isCrouching || grabbableObject.deactivated || !DestroyHeldObject(player, grabbableObject);

    public static void PlayerFall(PlayerControllerB player)
    {
        if (player.fallValueUncapped > -20f || player.isSpeedCheating) return;
        DestroyHeldObjects(player);
    }

    public static void DestroyHeldObjects(PlayerControllerB player)
    {
        if (!HasCurse(player.gameObject, Constants.FRAGILE)) return;

        HashSet<GrabbableObject> objectsToDestroy = [];
        for (int i = 0; i < player.ItemSlots.Length; i++)
        {
            if (player.ItemSlots[i] == null) continue;
            _ = objectsToDestroy.Add(player.ItemSlots[i]);
        }
        player.DropAllHeldItemsAndSync();
        foreach (GrabbableObject grabbableObject in objectsToDestroy)
        {
            if (string.IsNullOrEmpty(grabbableObject.itemProperties?.itemName)) continue;
            if (ConfigManager.fragileExclusions.Value.Contains(grabbableObject.itemProperties.itemName)) continue;

            LFCNetworkManager.Instance.DestroyObjectEveryoneRpc(grabbableObject.GetComponent<NetworkObject>());
        }
    }

    public static bool DestroyHeldObject(PlayerControllerB player, GrabbableObject grabbableObject)
    {
        if (grabbableObject == null || !HasCurse(player.gameObject, Constants.FRAGILE)) return false;
        if (string.IsNullOrEmpty(grabbableObject.itemProperties?.itemName)) return false;
        if (ConfigManager.fragileExclusions.Value.Contains(grabbableObject.itemProperties.itemName)) return false;

        LFCNetworkManager.Instance.DestroyObjectEveryoneRpc(grabbableObject.GetComponent<NetworkObject>());
        return true;
    }
}
