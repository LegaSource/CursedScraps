using CursedScraps.Managers;
using HarmonyLib;

namespace CursedScraps.Patches;

internal class BeltBagItemPatch
{
    [HarmonyPatch(typeof(BeltBagItem), nameof(BeltBagItem.PutObjectInBagLocalClient))]
    [HarmonyPostfix]
    private static void PreGrabObject(ref BeltBagItem __instance, ref GrabbableObject gObject) => ObjectCSManager.PostGrabObject(__instance.playerHeldBy, gObject);
}
