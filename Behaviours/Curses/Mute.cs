using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    internal class Mute
    {
        public static void ApplyMute(bool enable)
        {
            if (enable)
            {
                IngamePlayerSettings.Instance.unsavedSettings.micEnabled = false;
                IngamePlayerSettings.Instance.settings.micEnabled = false;

                foreach (SettingsOption setting in UnityEngine.Object.FindObjectsOfType<SettingsOption>(includeInactive: true).ToList().Where(s => s.optionType == SettingsOptionType.MicEnabled))
                {
                    setting.ToggleEnabledImage(4);
                }

                IngamePlayerSettings.Instance.SetMicrophoneEnabled();
            }
            else
            {
                IngamePlayerSettings.Instance.unsavedSettings.micEnabled = true;
                IngamePlayerSettings.Instance.settings.micEnabled = true;
            }
        }
    }
}
