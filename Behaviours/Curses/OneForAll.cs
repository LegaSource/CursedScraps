using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Utilities;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class OneForAll(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.ONE_FOR_ALL));

    public override void Apply(GameObject entity)
    {
        base.Apply(entity);
        if (!ConfigManager.isOneForAllInfoOn.Value) return;

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldNotBeLocalPlayer(player))
            HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"{player.playerUsername} has been afflicted by the {EffectType.Name} curse, defend them if you can!");
    }

    public static void KillPlayer(PlayerControllerB player)
    {
        if (!HasCurse(player.gameObject, Constants.ONE_FOR_ALL)) return;

        foreach (PlayerControllerB otherPlayer in StartOfRound.Instance.allPlayerScripts)
        {
            if (!otherPlayer.isPlayerControlled || otherPlayer.isPlayerDead || otherPlayer == player) continue;
            LFCNetworkManager.Instance.KillPlayerEveryoneRpc((int)otherPlayer.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
        }
    }
}