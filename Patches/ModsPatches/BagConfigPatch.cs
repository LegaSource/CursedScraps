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
            if (playerBehaviour == null) return true;
            if (!playerBehaviour.activeCurses.Any()) return true;

            __result = Captive.IsCaptive(playerBehaviour, true)
                ? EmptyCoroutine()
                : EmptyBagCoroutine(@this);
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
}
