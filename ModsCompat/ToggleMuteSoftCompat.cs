using CursedScraps.Registries;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Reflection;

namespace CursedScraps.ModsCompat;

public static class ToggleMuteSoftCompat
{
    public static void Patch(Harmony harmony)
    {
        Type toggleMuteType = Type.GetType("ToggleMute.ToggleMuteManager, ToggleMute");
        if (toggleMuteType != null)
        {
            MethodInfo onToggleMuteKeyPressed = AccessTools.Method(toggleMuteType, "OnToggleMuteKeyPressed");
            if (onToggleMuteKeyPressed != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(ToggleMuteSoftCompat), nameof(ToggleMute)));
                _ = harmony.Patch(onToggleMuteKeyPressed, prefix: prefix);
            }
        }
    }

    public static bool ToggleMute()
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player != null && CSCurseRegistry.HasCurse(player.gameObject, Constants.MUTE))
        {
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
            return false;
        }
        return true;
    }
}
