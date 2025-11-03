using CursedScraps.Behaviours.Curses;
using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Managers;

public class CursedScrapsNetworkManager : NetworkBehaviour
{
    public static CursedScrapsNetworkManager Instance;

    public void Awake() => Instance = this;

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ScaleObjectEveryoneRpc(NetworkObjectReference obj, int playerId, bool scale)
    {
        if (obj.TryGet(out NetworkObject networkObject))
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            Diminutive.ScaleObject(player, networkObject.gameObject.GetComponentInChildren<GrabbableObject>(), scale);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyObjectCurseEveryoneRpc(NetworkObjectReference obj, string curseName)
    {
        if (obj.TryGet(out NetworkObject networkObject))
        {
            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            ApplyCurse(grabbableObject.gameObject, curseName, -1, -1);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void RemoveObjectCurseEveryoneRpc(NetworkObjectReference obj, string curseName)
    {
        if (obj.TryGet(out NetworkObject networkObject))
        {
            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            RemoveCurse(grabbableObject.gameObject, curseName);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ApplyPlayerCurseEveryoneRpc(int playerId, string curseName, int duration)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        ApplyCurse(player.gameObject, curseName, -1, duration);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void RemovePlayerCurseEveryoneRpc(int playerId, string curseName)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        RemoveCurse(player.gameObject, curseName);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ClearPlayerCursesEveryoneRpc(int playerId) => ClearCurses(StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>().gameObject);

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushPlayerEveryoneRpc(int playerId, Vector3 pushVector)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        if (LFCUtilities.ShouldBeLocalPlayer(player)) _ = player.thisController.Move(pushVector);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void RemoveFromBagEveryoneRpc(NetworkObjectReference obj, NetworkObjectReference bagObj)
    {
        if (obj.TryGet(out NetworkObject networkObject) && bagObj.TryGet(out NetworkObject bagNetworkObject))
        {
            BeltBagItem beltBagItem = bagNetworkObject.gameObject.GetComponentInChildren<BeltBagItem>();
            _ = (beltBagItem?.objectsInBag.Remove(networkObject.gameObject.GetComponentInChildren<GrabbableObject>()));
        }
    }
}
