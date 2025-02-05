using CursedScraps.Behaviours.Curses;
using CursedScraps.Behaviours;
using CursedScraps.Values;
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

        public static void AddNewItems(RoundManager roundManager)
        {
            foreach (CustomItem customItem in CursedScraps.customItems)
            {
                if (!customItem.IsSpawnable) continue;

                for (int i = 0; i < customItem.MaxSpawn; i++)
                {
                    if (i < customItem.MinSpawn || new System.Random().Next(1, 100) <= customItem.Rarity)
                        SpawnNewItem(roundManager, customItem.Item);
                }
            }
        }

        public static void SpawnNewItem(RoundManager roundManager, Item itemToSpawn)
        {
            try
            {
                System.Random random = new System.Random();
                List<RandomScrapSpawn> listRandomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(s => !s.spawnUsed).ToList();

                if (!listRandomScrapSpawn.Any()) return;

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
                SpawnItem(itemToSpawn.spawnPrefab, position);
            }
            catch (Exception arg)
            {
                CursedScraps.mls.LogError($"Error in SpawnNewItem: {arg}");
            }
        }

        public static Vector3 GetFurthestPositionScrapSpawn(Vector3 position, Item itemToSpawn)
        {
            RandomScrapSpawn randomScrapSpawn = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                .Where(p => !p.spawnUsed)
                .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                .FirstOrDefault()
                    ?? UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                        .OrderByDescending(p => Vector3.Distance(position, p.transform.position))
                        .FirstOrDefault();

            if (randomScrapSpawn.spawnedItemsCopyPosition)
                randomScrapSpawn.spawnUsed = true;
            else
                randomScrapSpawn.transform.position = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit, RoundManager.Instance.AnomalyRandom) + Vector3.up * itemToSpawn.verticalOffset;
            return randomScrapSpawn.transform.position + Vector3.up * 0.5f;
        }

        public static GrabbableObject SpawnItem(GameObject spawnPrefab, Vector3 position)
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

        public static void PostGrabObject(PlayerControllerB player, GrabbableObject grabbableObject)
        {
            if (grabbableObject == null) return;

            // Comportements spécifiques pour les malédictions au moment du grab
            PlayerCSBehaviour playerBehaviour = player.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour == null) return;

            Errant.PostGrabTeleport(playerBehaviour, grabbableObject);
            Diminutive.ScaleObject(playerBehaviour, grabbableObject, false);

            ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour == null) return;
            if (!objectBehaviour.curseEffects.Any()) return;

            CursedScrapsNetworkManager.Instance.EnableParticleServerRpc(grabbableObject.GetComponent<NetworkObject>(), false);

            // Affectation des malédictions au joueur
            foreach (CurseEffect curseEffect in objectBehaviour.curseEffects)
                PlayerCSManager.SetPlayerCurseEffect(player, curseEffect, true);
        }

        public static bool PreDropObject(PlayerControllerB player, GrabbableObject grabbableObject)
        {
            PlayerCSBehaviour playerBehaviour = player.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour == null) return true;

            if (!player.isInHangarShipRoom)
            {
                if (Captive.IsCaptive(playerBehaviour, true)) return false;
                if (!Fragile.PreDropObject(playerBehaviour, grabbableObject)) return false;
                Errant.PreDropTeleport(playerBehaviour, grabbableObject);
            }

            ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour == null) return true;
            if (!objectBehaviour.curseEffects.Any()) return true;

            // Suppression des malédictions
            if (player.isInHangarShipRoom) CursedScrapsNetworkManager.Instance.RemoveAllScrapCurseEffectServerRpc(grabbableObject.GetComponent<NetworkObject>());
            else CursedScrapsNetworkManager.Instance.EnableParticleServerRpc(grabbableObject.GetComponent<NetworkObject>(), true);
            return true;
        }

        public static void PostDropObject(PlayerControllerB player)
            => Errant.PostDropTeleport(player.GetComponent<PlayerCSBehaviour>());
    }
}
