using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
using HarmonyLib;
using System.Linq;
using Unity.Netcode;

namespace CursedScraps.Patches
{
    internal class BeltBagItemPatch
    {
        [HarmonyPatch(typeof(BeltBagItem), nameof(BeltBagItem.PutObjectInBagLocalClient))]
        [HarmonyPostfix]
        private static void PreGrabObject(ref BeltBagItem __instance, ref GrabbableObject gObject)
        {
            if (__instance.playerHeldBy == null) return;

            ObjectCSManager.PostGrabObject(__instance.playerHeldBy, gObject);
            ObjectCSBehaviour objectBehaviour = gObject.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour == null) return;

            foreach (CurseEffect curseEffect in objectBehaviour.curseEffects)
                CursedScrapsNetworkManager.Instance.SetScrapCurseEffectServerRpc(__instance.GetComponent<NetworkObject>(), curseEffect.CurseName);
        }

        [HarmonyPatch(typeof(BeltBagItem), nameof(BeltBagItem.DiscardItem))]
        [HarmonyPrefix]
        private static void DropItem(ref BeltBagItem __instance)
        {
            if (__instance.playerHeldBy == null) return;
            if (!__instance.playerHeldBy.isInHangarShipRoom) return;

            foreach (GrabbableObject grabbableObject in __instance.objectsInBag)
            {
                ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
                if (objectBehaviour == null) continue;
                if (!objectBehaviour.curseEffects.Any()) continue;
                
                CursedScrapsNetworkManager.Instance.RemoveAllScrapCurseEffectServerRpc(grabbableObject.GetComponent<NetworkObject>());
            }
        }
    }
}
