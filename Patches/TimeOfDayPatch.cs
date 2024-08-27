using CursedScraps.Behaviours;
using HarmonyLib;
using System.Linq;

namespace CursedScraps.Patches
{
    internal class TimeOfDayPatch
    {
        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetInsideLightingDimness))]
        [HarmonyPostfix]
        public static void SwitchLightningSynchronization(ref TimeOfDay __instance)
        {
            if (__instance.sunDirect == null || __instance.sunIndirect == null)
            {
                return;
            }

            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null
                && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.SYNCHRONIZATION)) != null
                && playerBehaviour.coopPlayer != null)
            {
                __instance.sunDirect.enabled = !playerBehaviour.coopPlayer.isInsideFactory;
                __instance.sunIndirect.enabled = !playerBehaviour.coopPlayer.isInsideFactory;
                __instance.insideLighting = playerBehaviour.coopPlayer.isInsideFactory;
            }
        }
    }
}
