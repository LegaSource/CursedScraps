using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Deafness(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.DEAFNESS));
    public static float savedMasterVolume = 0f;

    public override void Apply(GameObject entity)
    {
        base.Apply(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player))
        {
            savedMasterVolume = IngamePlayerSettings.Instance.settings.masterVolume == 0f ? savedMasterVolume : IngamePlayerSettings.Instance.settings.masterVolume;
            IngamePlayerSettings.Instance.ChangeMasterVolume(0);
        }
    }

    public override void Expire(GameObject entity)
    {
        base.Expire(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player) && savedMasterVolume != 0f)
            IngamePlayerSettings.Instance.ChangeMasterVolume((int)(savedMasterVolume * 100));
    }
}