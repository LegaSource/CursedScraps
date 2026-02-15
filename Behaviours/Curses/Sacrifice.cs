using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Utilities;
using System.Linq;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Sacrifice(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.SACRIFICE));

    public override void Apply(GameObject entity)
    {
        base.Apply(entity);

        if (ConfigManager.isSacrificeInfoOn.Value)
        {
            PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
            if (LFCUtilities.ShouldNotBeLocalPlayer(player))
                HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"{player.playerUsername} has been afflicted by the {Constants.SACRIFICE} curse!");
        }
    }
    public static PlayerControllerB GetSacrificePlayer()
        => StartOfRound.Instance.allPlayerScripts.FirstOrDefault(p => HasCurse(p.gameObject, Constants.SACRIFICE));

    public static bool DamagePlayer(PlayerControllerB player, int damage)
    {
        PlayerControllerB sacrificePlayer = GetSacrificePlayer();
        if (sacrificePlayer != null && sacrificePlayer != player)
        {
            LFCNetworkManager.Instance.DamagePlayerEveryoneRpc((int)sacrificePlayer.playerClientId, damage);
            return true;
        }
        return false;
    }

    public static bool KillPlayer(PlayerControllerB player)
    {
        PlayerControllerB sacrificePlayer = GetSacrificePlayer();
        if (sacrificePlayer != null && sacrificePlayer != player)
        {
            SwapPlayers(player, sacrificePlayer);
            return true;
        }
        return false;
    }

    public static void SwapPlayers(PlayerControllerB player1, PlayerControllerB player2)
    {
        LFCNetworkManager.Instance.TeleportPlayerEveryoneRpc((int)player1.playerClientId, player2.transform.position, player2.isInElevator, player2.isInHangarShipRoom, player2.isInsideFactory);
        LFCNetworkManager.Instance.TeleportPlayerEveryoneRpc((int)player2.playerClientId, player1.transform.position, player1.isInElevator, player1.isInHangarShipRoom, player1.isInsideFactory);
    }
}
