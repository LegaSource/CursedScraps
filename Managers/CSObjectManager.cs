using CursedScraps.Behaviours;
using GameNetcodeStuff;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Managers
{
    internal class CSObjectManager
    {
        public static int timeOut = 5;

        public static Vector3 GetFurthestPositionScrapSpawn(Vector3 position, ref Item itemToSpawn)
        {
            RandomScrapSpawn randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                .Where(p => !p.spawnUsed)
                .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                .FirstOrDefault();

            if (randomScrapSpawn == null)
            {
                // Au cas où, mieux vaut prendre un spawn déjà utilisé que de le faire apparaître devant le joueur
                randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                    .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                    .FirstOrDefault();
            }

            if (randomScrapSpawn.spawnedItemsCopyPosition)
            {
                randomScrapSpawn.spawnUsed = true;
            }
            else
            {
                randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * itemToSpawn.verticalOffset;
            }
            return randomScrapSpawn.transform.position + Vector3.up * 0.5f;
        }

        public static void CloneScrap(string name, string nameCore, string nameReflection, ref Vector3 position, ref PlayerControllerB player, ref ObjectCSBehaviour objectBehaviour)
        {
            if (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost)
            {
                GrabbableObject cloneScrap = SpawnScrap(ref objectBehaviour.objectProperties, ref position);
                if (cloneScrap != null)
                {
                    CursedScrapsNetworkManager.Instance.SetCloneScrapServerRpc(objectBehaviour.objectProperties.GetComponent<NetworkObject>(), cloneScrap.GetComponent<NetworkObject>(), (int)player.playerClientId, name, nameReflection);
                }
            }

            objectBehaviour.isSplitted = true;
            ScanNodeProperties scanNode = objectBehaviour.objectProperties.gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (scanNode != null)
            {
                scanNode.subText = scanNode.subText.Replace(name, nameCore);
            }
        }

        public static GrabbableObject GetCloneScrap(GrabbableObject grabbableObject)
        {
            ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
            GrabbableObject cloneScrap = null;
            if (objectBehaviour != null)
            {
                cloneScrap = objectBehaviour.mainPart;
                if (cloneScrap == null)
                {
                    ObjectCSBehaviour objectBehaviourClone;
                    cloneScrap = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList()
                        .Where(o => (objectBehaviourClone = o.GetComponent<ObjectCSBehaviour>()) != null && objectBehaviourClone.mainPart == grabbableObject)
                        .FirstOrDefault();
                }
            }
            return cloneScrap;
        }

        public static bool IsCloneOnShip(ref GrabbableObject grabbableObject)
        {
            GrabbableObject searchedScrap = GetCloneScrap(grabbableObject);
            if (!(searchedScrap != null && (searchedScrap.isInShipRoom || searchedScrap.isInElevator)))
            {
                return false;
            }
            return true;
        }

        public static GrabbableObject SpawnScrap(ref GrabbableObject grabbableObject, ref Vector3 position)
        {
            try
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(grabbableObject.itemProperties.spawnPrefab, position, Quaternion.identity, StartOfRound.Instance.propsContainer);
                GrabbableObject cloneScrap = gameObject.GetComponent<GrabbableObject>();
                cloneScrap.fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
                return cloneScrap;
            }
            catch (Exception arg)
            {
                CursedScraps.mls.LogError($"Error in SpawnScrap: {arg}");
            }
            return null;
        }

        public static void RemovePlayerOwner(ref PlayerControllerB player)
        {
            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                GrabbableObject grabbableObject = player.ItemSlots[i];
                if (grabbableObject != null)
                {
                    ObjectCSBehaviour objectCSBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
                    if (objectCSBehaviour != null)
                    {
                        objectCSBehaviour.playerOwner = null;
                    }
                }
            }
        }
    }
}
