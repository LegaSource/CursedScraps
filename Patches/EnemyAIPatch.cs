using CursedScraps.Behaviours.Curses;
using HarmonyLib;

namespace CursedScraps.Patches
{
    internal class EnemyAIPatch
    {
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SetClientCalculatingAI))]
        [HarmonyPostfix]
        private static void CalculateIA(ref EnemyAI __instance) => Shadow.ApplyShadow(ref __instance);
    }
}
