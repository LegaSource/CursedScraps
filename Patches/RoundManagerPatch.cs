using CursedScraps.Behaviours;
using CursedScraps.Managers;
using HarmonyLib;
using System.Linq;

namespace CursedScraps.Patches
{
    internal class RoundManagerPatch
    {
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.CollectNewScrapForThisRound))]
        [HarmonyPrefix]
        private static bool CollectScrap(ref GrabbableObject scrapObject)
        {
            ObjectCSBehaviour objectBehaviour = scrapObject.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.FirstOrDefault(c => c.IsCoop) != null)
            {
                return CSObjectManager.IsCloneOnShip(ref scrapObject);
            }
            return true;
        }
    }
}
