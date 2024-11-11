using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace CursedScraps.Behaviours.Curses
{
    public class Fragile
    {
        public static void DestroyHeldObjects(ref PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.FRAGILE)))
            {
                PlayerControllerB player = playerBehaviour.playerProperties;
                HashSet<GrabbableObject> objectsToDestroy = new HashSet<GrabbableObject>();
                for (int i = 0; i < player.ItemSlots.Length; i++)
                {
                    if (player.ItemSlots[i] != null)
                    {
                        objectsToDestroy.Add(player.ItemSlots[i]);
                    }
                }
                player.DropAllHeldItemsAndSync();
                foreach (GrabbableObject grabbableObject in objectsToDestroy.Where(o => !ConfigManager.fragileExclusions.Value.Contains(o.itemProperties.itemName)))
                {
                    CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(grabbableObject.GetComponent<NetworkObject>());
                }
            }
        }
        public static bool DestroyHeldObject(ref PlayerCSBehaviour playerBehaviour, ref GrabbableObject grabbableObject)
        {
            if (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.FRAGILE)))
            {
                if (grabbableObject != null && !ConfigManager.fragileExclusions.Value.Contains(grabbableObject.itemProperties.itemName))
                {
                    CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(grabbableObject.GetComponent<NetworkObject>());
                    return true;
                }
            }
            return false;
        }
    }
}
