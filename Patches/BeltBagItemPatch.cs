using CursedScraps.Managers;
using HarmonyLib;

namespace CursedScraps.Patches
{
    internal class BeltBagItemPatch
    {
        [HarmonyPatch(typeof(BeltBagItem), nameof(BeltBagItem.PutObjectInBagLocalClient))]
        [HarmonyPostfix]
        private static void PreGrabObject(ref BeltBagItem __instance, ref GrabbableObject gObject)
        {
            if (__instance.playerHeldBy != null)
            {
                ObjectCSManager.PostGrabObject(ref __instance.playerHeldBy, ref gObject);
            }
        }
    }
}
