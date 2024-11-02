using CursedScraps.Behaviours;
using HarmonyLib;

namespace CursedScraps.Patches
{
    internal class BeltBagItemPatch
    {
        [HarmonyPatch(typeof(BeltBagItem), nameof(BeltBagItem.PutObjectInBagLocalClient))]
        [HarmonyPrefix]
        private static bool PreGrabObject(ref BeltBagItem __instance, ref GrabbableObject gObject)
        {
            ObjectCSBehaviour objectBehaviour = gObject.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.Count > 0)
            {
                if (__instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
                }
                return false;
            }
            return true;
        }
    }
}
