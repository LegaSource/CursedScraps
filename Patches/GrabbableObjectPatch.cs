using CursedScraps.Behaviours;
using HarmonyLib;
using System.Linq;

namespace CursedScraps.Patches
{
    internal class GrabbableObjectPatch
    {
        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Update))]
        [HarmonyPostfix]
        private static void UpdateGrabbableObject(GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.particleEffect != null)
            {
                objectBehaviour.particleEffect.transform.localScale = __instance.transform.localScale;
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
        [HarmonyPrefix]
        private static bool PreventOverrideCurseValue(GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.Count > 0)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetControlTipsForItem))]
        [HarmonyPostfix]
        private static void ChangeToolTip(ref GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.FirstOrDefault(c => c.CurseName.Equals(Constants.SYNCHRONIZATION)) != null)
            {
                HUDManager.Instance.ChangeControlTipMultiple(__instance.itemProperties.toolTips.Concat(["Reorient the camera : [E]"]).ToArray(), holdingItem: true, __instance.itemProperties);
            }
        }
    }
}
