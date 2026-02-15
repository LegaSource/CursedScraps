using CursedScraps.Managers;
using CursedScraps.Registries;
using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CursedScraps.ModsCompat;

public static class BagConfigSoftCompat
{
    public static void Patch(Harmony harmony)
    {
        Type beltBagType = Type.GetType("BagConfig.Patches.BeltBagPatch, BagConfig");
        if (beltBagType != null)
        {
            MethodInfo emptyBagCoroutine = AccessTools.Method(beltBagType, "EmptyBagCoroutine");
            if (emptyBagCoroutine != null)
            {
                HarmonyMethod prefix = new HarmonyMethod(AccessTools.Method(typeof(BagConfigSoftCompat), nameof(EmptyBag)));
                _ = harmony.Patch(emptyBagCoroutine, prefix: prefix);
            }
        }
    }

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
