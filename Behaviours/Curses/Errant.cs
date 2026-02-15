using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Errant(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.ERRANT));
    public static bool canBeTeleported = false;

    public static bool CanTeleport(PlayerControllerB player, GrabbableObject grabbableObject, bool checkCaptive = false)
    {
        if (!HasCurse(player.gameObject, Constants.ERRANT)) return false;
        if (string.IsNullOrEmpty(grabbableObject.itemProperties?.itemName)) return false;
        if (ConfigManager.errantExclusions.Value.Contains(grabbableObject.itemProperties.itemName)) return false;
        // Si le joueur possède la malédiction captive, l'objet ne peut pas être drop, on ne fait donc pas la tp
        return !checkCaptive || !HasCurse(player.gameObject, Constants.CAPTIVE);
    }

    // Téléportation après avoir attrapé un objet
    public static void PostGrabTeleport(PlayerControllerB player, GrabbableObject grabbableObject)
    {
        if (!CanTeleport(player, grabbableObject)) return;
        TeleportPlayer(player);
    }

    // Préparation pour la téléportation avant de déposer un objet
    public static void PreDropTeleport(PlayerControllerB player, GrabbableObject grabbableObject)
        => canBeTeleported = CanTeleport(player, grabbableObject, checkCaptive: true);

    // Téléportation après avoir déposé un objet
    public static void PostDropTeleport(PlayerControllerB player)
    {
        if (!canBeTeleported) return;
        TeleportPlayer(player);
    }

    public static void TeleportPlayer(PlayerControllerB player)
    {
        if (!player.isInHangarShipRoom)
        {
            canBeTeleported = false;
            Vector3 position = RoundManager.Instance.insideAINodes[Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
            position = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position);
            LFCNetworkManager.Instance.TeleportPlayerEveryoneRpc((int)player.playerClientId, position, false, false, true);
        }
    }
}
