﻿using CursedScraps.Managers;
using HarmonyLib;
using Unity.Netcode;

namespace CursedScraps.Patches
{
    internal class BeltBagInventoryUIPatch
    {
        [HarmonyPatch(typeof(BeltBagInventoryUI), nameof(BeltBagInventoryUI.RemoveItemFromUI))]
        [HarmonyPrefix]
        private static bool PreDropObject(ref BeltBagInventoryUI __instance, int slot)
        {
            if (__instance.currentBeltBag != null && slot != -1)
            {
                GrabbableObject grabbableObject = __instance.currentBeltBag.objectsInBag[slot];
                if (__instance.currentBeltBag.objectsInBag.Count > slot
                    && grabbableObject != null
                    && !__instance.currentBeltBag.tryingAddToBag
                    && !ObjectCSManager.PreDropObject(ref __instance.currentBeltBag.playerHeldBy, ref grabbableObject))
                {
                    if (grabbableObject.deactivated)
                    {
                        CursedScrapsNetworkManager.Instance.RemoveFromBagServerRpc(grabbableObject.GetComponent<NetworkObject>(), __instance.currentBeltBag.GetComponent<NetworkObject>());
                        __instance.inventorySlotIcons[slot].enabled = false;
                        __instance.FillSlots(__instance.currentBeltBag);
                        __instance.grabbingItemSlot = -1;
                    }
                    HUDManager.Instance.SetMouseCursorSprite(HUDManager.Instance.handOpenCursorTex);
                    return false;
                }
                ObjectCSManager.PostDropObject(ref __instance.currentBeltBag.playerHeldBy);
            }
            return true;
        }
    }
}
