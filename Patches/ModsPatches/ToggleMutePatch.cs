using CursedScraps.Behaviours.Curses;
using CursedScraps.Behaviours;
using HarmonyLib;

namespace CursedScraps.Patches.ModsPatches
{
    [HarmonyPatch]
    internal class ToggleMutePatch
    {
        public static bool ToggleMute()
        {
            if (!Mute.IsMute(GameNetworkManager.Instance.localPlayerController?.GetComponent<PlayerCSBehaviour>())) return true;

            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
            return false;
        }
    }
}
