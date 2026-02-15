using GameNetcodeStuff;
using LegaFusionCore.Utilities;
using UnityEngine;
using static CursedScraps.Registries.CSCurseRegistry;

namespace CursedScraps.Behaviours.Curses;

public class Mute(int playerWhoHit, int duration, System.Action onApply, System.Action onExpire, System.Action onUpdate)
    : CurseEffect(Type, playerWhoHit, duration, onApply, onExpire, onUpdate)
{
    private static readonly CurseEffectType Type = curseEffectTypes.Find(t => t.Name.Equals(Constants.MUTE));

    public override void Apply(GameObject entity)
    {
        base.Apply(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (!LFCUtilities.ShouldBeLocalPlayer(player)) return;

        IngamePlayerSettings.Instance.unsavedSettings.micEnabled = false;
        IngamePlayerSettings.Instance.settings.micEnabled = false;

        foreach (SettingsOption setting in Object.FindObjectsOfType<SettingsOption>(includeInactive: true))
        {
            if (setting.optionType != SettingsOptionType.MicEnabled) continue;
            setting.ToggleEnabledImage(4);
        }

        IngamePlayerSettings.Instance.SetMicrophoneEnabled();
    }

    public override void Expire(GameObject entity)
    {
        base.Expire(entity);

        PlayerControllerB player = LFCUtilities.GetSafeComponent<PlayerControllerB>(entity);
        if (LFCUtilities.ShouldBeLocalPlayer(player))
        {
            IngamePlayerSettings.Instance.unsavedSettings.micEnabled = true;
            IngamePlayerSettings.Instance.settings.micEnabled = true;
        }
    }
}