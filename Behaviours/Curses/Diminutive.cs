using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Managers.NetworkManagers;
using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;
using Unity.Netcode;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Diminutive(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.DIMINUTIVE));
    private static readonly float grabFactor = 4f;

    public override void Apply(GameObject entity)
    {
        base.Apply(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (player == null) return;

        player.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        if (LFCUtilities.ShouldBeLocalPlayer(player))
        {
            LFCStatRegistry.AddModifier(LegaFusionCore.Constants.STAT_SPEED, $"{CursedScraps.modName}{Constants.DIMINUTIVE}", ConfigManager.diminutiveSpeed.Value);
            player.grabDistance /= grabFactor;
            player.localVisor.gameObject.SetActive(false);

            GrabbableObject grabbableObject = player.currentlyHeldObjectServer;
            if (grabbableObject != null)
                CursedScrapsNetworkManager.Instance.ScaleObjectEveryoneRpc(grabbableObject.GetComponent<NetworkObject>(), (int)player.playerClientId, false);
        }
    }

    public override void Expire(GameObject entity)
    {
        base.Expire(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (player == null) return;

        player.transform.localScale = new Vector3(1f, 1f, 1f);
        if (LFCUtilities.ShouldBeLocalPlayer(player))
        {
            LFCStatRegistry.RemoveModifier(LegaFusionCore.Constants.STAT_SPEED, $"{CursedScraps.modName}{Constants.DIMINUTIVE}");
            player.grabDistance *= grabFactor;
            player.localVisor.gameObject.SetActive(true);

            GrabbableObject grabbableObject = player.currentlyHeldObjectServer;
            if (grabbableObject != null)
                CursedScrapsNetworkManager.Instance.ScaleObjectEveryoneRpc(grabbableObject.GetComponent<NetworkObject>(), (int)player.playerClientId, true);
        }
    }

    public static void PlayerCollision(PlayerControllerB player)
    {
        if (!LFCUtilities.ShouldBeLocalPlayer(player) || HasCurse(player.gameObject, Constants.DIMINUTIVE)) return;

        foreach (Collider collider in Physics.OverlapSphere(player.transform.position, 0.65f, StartOfRound.Instance.playersMask))
        {
            PlayerControllerB pushedPlayer = collider.GetComponent<PlayerControllerB>();
            if (pushedPlayer == null || pushedPlayer == player || !HasCurse(pushedPlayer.gameObject, Constants.DIMINUTIVE)) continue;

            if (player.isFallingFromJump)
            {
                LFCNetworkManager.Instance.KillPlayerEveryoneRpc((int)pushedPlayer.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Crushing);
                return;
            }
            Vector3 direction = (pushedPlayer.transform.position - player.thisController.transform.position).normalized;
            CursedScrapsNetworkManager.Instance.PushPlayerEveryoneRpc((int)pushedPlayer.playerClientId, direction * player.thisController.velocity.magnitude * 0.2f);
        }
    }

    public static void ScaleObject(PlayerControllerB player, GrabbableObject grabbableObject, bool scale)
    {
        if (!HasCurse(player.gameObject, Constants.DIMINUTIVE)) return;
        grabbableObject.transform.localScale = grabbableObject.originalScale * (scale ? 5f : 0.2f);
    }
}
