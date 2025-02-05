using CursedScraps.Behaviours;
using CursedScraps.Managers;
using HarmonyLib;
using System;
using System.Linq;

namespace CursedScraps.Patches
{
    internal class IngamePlayerSettingsPatch
    {
        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.DiscardChangedSettings))]
        [HarmonyPrefix]
        private static bool PreventRemoveWhenDiscardSettings(ref IngamePlayerSettings __instance)
            => DiscardSettings(__instance);

        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SaveChangedSettings))]
        [HarmonyPrefix]
        private static bool PreventRemoveWhenConfirmSettings(ref IngamePlayerSettings __instance)
            => DiscardSettings(__instance);

        public static bool DiscardSettings(IngamePlayerSettings ingamePlayerSettings)
        {
            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance?.localPlayerController?.GetComponent<PlayerCSBehaviour>();
            if (!ConfigManager.globalPrevent.Value) return true;
            if (playerBehaviour == null) return true;

            var activeCurses = playerBehaviour.activeCurses.Select(c => c.CurseName);
            if (activeCurses.Contains(Constants.CONFUSION) || activeCurses.Contains(Constants.MUTE) || activeCurses.Contains(Constants.DEAFNESS))
            {
                ingamePlayerSettings.SetChangesNotAppliedTextVisible(visible: false);
                ingamePlayerSettings.unsavedSettings.CopySettings(ingamePlayerSettings.settings);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.SetOption))]
        [HarmonyPrefix]
        private static bool PreventUpdateSettings(ref SettingsOptionType optionType)
        {
            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance?.localPlayerController?.GetComponent<PlayerCSBehaviour>();
            if (!ConfigManager.globalPrevent.Value) return true;
            if (playerBehaviour == null) return true;

            var activeCurses = playerBehaviour.activeCurses.Select(c => c.CurseName);
            if ((activeCurses.Contains(Constants.MUTE) && (optionType == SettingsOptionType.MicEnabled || optionType == SettingsOptionType.MicDevice || optionType == SettingsOptionType.MicPushToTalk))
                    || (activeCurses.Contains(Constants.DEAFNESS) && (optionType == SettingsOptionType.MasterVolume || optionType == SettingsOptionType.MicDevice)))
            {
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
                return false;
            }
            return true;
        }
    }
}
