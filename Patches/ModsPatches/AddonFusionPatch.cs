using CursedScraps.Behaviours;
using HarmonyLib;
using System.Linq;
using Unity.Netcode;

namespace CursedScraps.Patches.ModsPatches
{
    [HarmonyPatch]
    internal class AddonFusionPatch
    {
        public static bool PreGrabObject(ref object __instance, ref NetworkObjectReference itemObject)
        {
            if (itemObject.TryGet(out NetworkObject networkObject))
            {
                ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
                if (objectBehaviour != null && objectBehaviour.curseEffects.Count() > 0)
                {
                    GrabbableObject capsuleHoiPoi = __instance as GrabbableObject;
                    if (capsuleHoiPoi.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
