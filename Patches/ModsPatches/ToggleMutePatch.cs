using CursedScraps.Registries;
using GameNetcodeStuff;
using HarmonyLib;

namespace CursedScraps.Patches.ModsPatches;

[HarmonyPatch]
internal class ToggleMutePatch
{
    public static bool ToggleMute()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null || !CSCurseRegistry.HasCurse(player.gameObject, Constants.MUTE)) return true;

        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
        return false;
    }
}
