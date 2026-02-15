using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Registries;
using LegaFusionCore.Utilities;
using System.Linq;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Inhibition(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.INHIBITION));
    private static float inhibitionTimer;
    private static string lockedAction;

    public override void Apply(GameObject entity)
    {
        base.Apply(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player))
            inhibitionTimer = ConfigManager.inhibitionCooldown.Value;
    }

    public override void Update(GameObject entity)
    {
        base.Update(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (!LFCUtilities.ShouldBeLocalPlayer(player)) return;

        inhibitionTimer += Time.deltaTime;
        if (inhibitionTimer >= ConfigManager.inhibitionCooldown.Value)
        {
            inhibitionTimer = 0f;
            if (!string.IsNullOrEmpty(lockedAction)) LFCPlayerActionRegistry.RemoveLock(lockedAction, $"{CursedScraps.modName}{EffectType.Name}");

            string[] actions = ConfigManager.inhibitionActions.Value
                .Split(',')
                .Where(a => string.IsNullOrEmpty(lockedAction) || !a.Equals(lockedAction))
                .ToArray();
            lockedAction = actions[new System.Random().Next(actions.Length)];

            LFCPlayerActionRegistry.AddLock(lockedAction, $"{CursedScraps.modName}{EffectType.Name}");
            if (ConfigManager.isInhibitionTip.Value)
                HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"One of your actions has been locked by the {Constants.INHIBITION} curse.");
        }
    }

    public override void Expire(GameObject entity)
    {
        base.Expire(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player) && !string.IsNullOrEmpty(lockedAction))
            LFCPlayerActionRegistry.RemoveLock(lockedAction, $"{CursedScraps.modName}{EffectType.Name}");
    }
}
