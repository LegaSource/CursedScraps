using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace CursedScraps.Behaviours.Curses
{
    public class Fragile
    {
        public static bool PreDropObject(PlayerCSBehaviour playerBehaviour, GrabbableObject grabbableObject)
        {
            if (playerBehaviour.playerProperties.isCrouching) return true;
            if (grabbableObject.deactivated) return true;
            if (!DestroyHeldObject(playerBehaviour, grabbableObject)) return true;
            return false;
        }

        public static void PlayerFall(PlayerControllerB player)
        {
            if (player.fallValueUncapped > -20f) return;
            if (player.isSpeedCheating) return;
            DestroyHeldObjects(player.GetComponent<PlayerCSBehaviour>());
        }

        public static void DestroyHeldObjects(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.FRAGILE))) return;

            PlayerControllerB player = playerBehaviour.playerProperties;
            HashSet<GrabbableObject> objectsToDestroy = new HashSet<GrabbableObject>();
            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                if (player.ItemSlots[i] == null) continue;
                objectsToDestroy.Add(player.ItemSlots[i]);
            }
            player.DropAllHeldItemsAndSync();
            foreach (GrabbableObject grabbableObject in objectsToDestroy)
            {
                if (string.IsNullOrEmpty(grabbableObject.itemProperties?.itemName)) continue;
                if (ConfigManager.fragileExclusions.Value.Contains(grabbableObject.itemProperties.itemName)) continue;

                CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(grabbableObject.GetComponent<NetworkObject>());
            }
        }

        public static bool DestroyHeldObject(PlayerCSBehaviour playerBehaviour, GrabbableObject grabbableObject)
        {
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.FRAGILE))) return false;
            if (grabbableObject == null) return false;
            if (string.IsNullOrEmpty(grabbableObject.itemProperties?.itemName)) return false;
            if (ConfigManager.fragileExclusions.Value.Contains(grabbableObject.itemProperties.itemName)) return false;

            CursedScrapsNetworkManager.Instance.DestroyObjectServerRpc(grabbableObject.GetComponent<NetworkObject>());
            return true;
        }
    }
}
