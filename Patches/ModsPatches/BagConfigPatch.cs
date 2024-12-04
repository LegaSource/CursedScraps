using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
using HarmonyLib;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Patches.ModsPatches
{
    [HarmonyPatch]
    internal class BagConfigPatch
    {
        public static bool EmptyBag(BeltBagItem @this, ref IEnumerator __result)
        {
            PlayerCSBehaviour playerBehaviour = @this.playerHeldBy?.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null && playerBehaviour.activeCurses.Any())
            {
                __result = Captive.IsCaptive(playerBehaviour, true)
                    ? EmptyCoroutine()
                    : EmptyBagCoroutine(@this);
                return false;
            }
            return true;
        }

        public static IEnumerator EmptyCoroutine()
        {
            yield break; // Coroutine vide
        }

        public static IEnumerator EmptyBagCoroutine(BeltBagItem beltBagItem)
        {
            while (beltBagItem.objectsInBag.Count > 0)
            {
                for (int i = beltBagItem.objectsInBag.Count - 1; i >= 0; i--)
                {
                    GrabbableObject grabbableObject = beltBagItem.objectsInBag[i];
                    if (beltBagItem.playerHeldBy != null
                        && ObjectCSManager.PreDropObject(beltBagItem.playerHeldBy, grabbableObject))
                    {
                        beltBagItem.RemoveObjectFromBag(i);
                        ObjectCSManager.PostDropObject(beltBagItem.playerHeldBy);
                        break; // Recommence la vérification des objets après modification
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
