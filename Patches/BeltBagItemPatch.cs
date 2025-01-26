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
            if (__instance.playerHeldBy != null)
            {
                ObjectCSManager.PostGrabObject(__instance.playerHeldBy, gObject);
                ObjectCSBehaviour objectBehaviour = gObject.GetComponent<ObjectCSBehaviour>();
                if (objectBehaviour != null)
                {
                    foreach (CurseEffect curseEffect in objectBehaviour.curseEffects)
                        CursedScrapsNetworkManager.Instance.SetScrapCurseEffectServerRpc(__instance.GetComponent<NetworkObject>(), curseEffect.CurseName);
                }
            }
        }

        [HarmonyPatch(typeof(BeltBagItem), nameof(BeltBagItem.DiscardItem))]
        [HarmonyPrefix]
        private static void DropItem(ref BeltBagItem __instance)
        {
            if (__instance.playerHeldBy != null && __instance.playerHeldBy.isInHangarShipRoom)
            {
                foreach (GrabbableObject grabbableObject in __instance.objectsInBag)
                {
                    ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
                    if (objectBehaviour != null
                        && objectBehaviour.curseEffects.Any())
                    {
                        CursedScrapsNetworkManager.Instance.RemoveAllScrapCurseEffectServerRpc(grabbableObject.GetComponent<NetworkObject>());
                    }
                }
            }
        }
    }
}
