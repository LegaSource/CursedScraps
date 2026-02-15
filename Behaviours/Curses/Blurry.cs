using CursedScraps.Managers;
using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Blurry(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.BLURRY));

    public override void Update(GameObject entity)
    {
        base.Update(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player))
            HUDManager.Instance.drunknessFilter.weight = ConfigManager.blurryIntensity.Value;
    }
}
