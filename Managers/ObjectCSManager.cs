using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Managers
{
    public class ObjectCSManager
    {
        public static int timeOut = 5;

        public static void SpawnNewItem(ref RoundManager roundManager, Item itemToSpawn)
        {
            try
            {
                System.Random random = new System.Random();
                List<RandomScrapSpawn> listRandomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList();

                if (listRandomScrapSpawn.Count <= 0) return;

                int indexRandomScrapSpawn = random.Next(0, listRandomScrapSpawn.Count);
                RandomScrapSpawn randomScrapSpawn = listRandomScrapSpawn[indexRandomScrapSpawn];
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    randomScrapSpawn.spawnUsed = true;
                    listRandomScrapSpawn.RemoveAt(indexRandomScrapSpawn);
                }
                else
                {
                    randomScrapSpawn.transform.position = roundManager.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, roundManager.navHit, roundManager.AnomalyRandom) + Vector3.up * itemToSpawn.verticalOffset;
                }

                Vector3 position = randomScrapSpawn.transform.position + Vector3.up * 0.5f;
                SpawnItem(ref itemToSpawn.spawnPrefab, ref position);
            }
            catch (Exception arg)
            {
                CursedScraps.mls.LogError($"Error in SpawnNewItem: {arg}");
            }
        }

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

        public static GrabbableObject SpawnItem(ref GameObject spawnPrefab, ref Vector3 position)
        {
            try
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(spawnPrefab, position, Quaternion.identity, StartOfRound.Instance.propsContainer);
                GrabbableObject grabbableObject = gameObject.GetComponent<GrabbableObject>();
                grabbableObject.fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
                return grabbableObject;
            }
            catch (Exception arg)
            {
                CursedScraps.mls.LogError($"Error in SpawnItem: {arg}");
            }
            return null;
        }

        public static bool HasItemByName(ref PlayerControllerB player, string itemName)
        {
            for (int i = 0; i < player.ItemSlots.Length; i++)
            {
                if (player.ItemSlots[i] != null && player.ItemSlots[i].itemProperties.itemName.Equals(itemName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
