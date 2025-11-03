using CursedScraps.Managers;
using CursedScraps.Registries;
using HarmonyLib;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Patches.ModsPatches;

[HarmonyPatch]
internal class BagConfigPatch
{
    public static bool EmptyBag(BeltBagItem @this, ref IEnumerator __result)
    {
        if (@this.playerHeldBy == null) return true;

        if (CSCurseRegistry.HasCurse(@this.playerHeldBy.gameObject, Constants.CAPTIVE))
        {
            __result = EmptyCoroutine();
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you to do this action.");
            return false;
        }

        __result = EmptyBagCoroutine(@this);
        return false;
    }

    public static IEnumerator EmptyCoroutine()
    {
        yield break; // Coroutine vide
    }

    public static IEnumerator EmptyBagCoroutine(BeltBagItem beltBagItem)
    {
        while (beltBagItem.objectsInBag.Any())
        {
            for (int i = beltBagItem.objectsInBag.Count - 1; i >= 0; i--)
            {
                GrabbableObject grabbableObject = beltBagItem.objectsInBag[i];
                if (beltBagItem.playerHeldBy == null) continue;
                if (!ObjectCSManager.PreDropObject(beltBagItem.playerHeldBy, grabbableObject)) continue;

                beltBagItem.RemoveObjectFromBag(i);
                ObjectCSManager.PostDropObject(beltBagItem.playerHeldBy);
                break; // Recommence la vérification des objets après modification
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
