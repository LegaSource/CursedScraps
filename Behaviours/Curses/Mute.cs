using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Mute
    {
        public static bool IsMute(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.MUTE))) return false;
            return true;
        }

        public static void ApplyMute(bool enable)
        {
            IngamePlayerSettings.Instance.unsavedSettings.micEnabled = !enable;
            IngamePlayerSettings.Instance.settings.micEnabled = !enable;

            if (!enable) return;

            foreach (SettingsOption setting in UnityEngine.Object.FindObjectsOfType<SettingsOption>(includeInactive: true))
            {
                if (setting.optionType != SettingsOptionType.MicEnabled) continue;
                setting.ToggleEnabledImage(4);
            }  

            IngamePlayerSettings.Instance.SetMicrophoneEnabled();
        }
    }
}
