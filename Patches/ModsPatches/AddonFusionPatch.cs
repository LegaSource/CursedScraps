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
            if (!itemObject.TryGet(out NetworkObject networkObject)) return true;

            ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
            if (objectBehaviour == null) return true;
            if (!objectBehaviour.curseEffects.Any()) return true;

            GrabbableObject capsuleHoiPoi = __instance as GrabbableObject;
            if (capsuleHoiPoi.playerHeldBy != GameNetworkManager.Instance.localPlayerController) return false;
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from performing this action.");
            return false;
        }
    }
}
