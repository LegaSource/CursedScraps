using CursedScraps.Behaviours;
using HarmonyLib;

namespace CursedScraps.Patches
{
    internal class GrabbableObjectPatch
    {
        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Update))]
        [HarmonyPostfix]
        private static void UpdateGrabbableObject(ref GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.particleEffect != null)
            {
                objectBehaviour.particleEffect.transform.localScale = __instance.transform.localScale;
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
        [HarmonyPrefix]
        private static bool PreventOverrideCurseValue(ref GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.Count > 0)
            {
                return false;
            }
            return true;
        }
    }
}
