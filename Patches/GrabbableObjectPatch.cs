using CursedScraps.Behaviours;
using HarmonyLib;
using System.Linq;

namespace CursedScraps.Patches
{
    internal class GrabbableObjectPatch
    {
        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Update))]
        [HarmonyPostfix]
        private static void UpdateGrabbableObject(ref GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour == null) return;
            if (objectBehaviour.particleEffect == null) return;
            
            objectBehaviour.particleEffect.transform.localScale = __instance.transform.localScale;
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
        [HarmonyPrefix]
        private static bool PreventOverrideCurseValue(ref GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour == null) return true;
            if (!objectBehaviour.curseEffects.Any()) return true;
            return false;
        }
    }
}
