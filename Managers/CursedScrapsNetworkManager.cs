﻿using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.Behaviours.Items;
using GameNetcodeStuff;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace CursedScraps.Managers
{
    public class CursedScrapsNetworkManager : NetworkBehaviour
    {
        public static CursedScrapsNetworkManager Instance;

        public void Awake() => Instance = this;

        // GLOBAL
        [ServerRpc(RequireOwnership = false)]
        public void SetScrapCurseEffectServerRpc(NetworkObjectReference obj, string curseName)
            => SetScrapCurseEffectClientRpc(obj, curseName);

        [ClientRpc]
        private void SetScrapCurseEffectClientRpc(NetworkObjectReference obj, string curseName)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                SetScrapCurseEffect(grabbableObject, curseName, true);
            }
        }

        public void SetScrapCurseEffect(GrabbableObject grabbableObject, string curseName, bool applyValue)
        {
            CurseEffect curseEffect = CursedScraps.curseEffects.FirstOrDefault(c => c.CurseName.Equals(curseName));
            if (curseEffect == null || grabbableObject == null) return;

            ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>() ?? grabbableObject.gameObject.AddComponent<ObjectCSBehaviour>();
            objectBehaviour.objectProperties = grabbableObject;
            objectBehaviour.curseEffects.Add(curseEffect);
            if (grabbableObject.itemProperties.isScrap && applyValue)
                grabbableObject.scrapValue = (int)(grabbableObject.scrapValue * curseEffect.Multiplier);

            if (ConfigManager.isParticleOn.Value)
            {
                float maxValue = Mathf.Max(grabbableObject.transform.localScale.x, Mathf.Max(grabbableObject.transform.localScale.y, grabbableObject.transform.localScale.z));
                if (maxValue < ConfigManager.minParticleScale.Value) maxValue = ConfigManager.minParticleScale.Value;
                else if (maxValue > ConfigManager.maxParticleScale.Value) maxValue = ConfigManager.maxParticleScale.Value;

                if (objectBehaviour.particleEffect == null)
                {
                    GameObject curseParticleEffect = Instantiate(CursedScraps.curseParticle, grabbableObject.transform.position, Quaternion.identity);
                    curseParticleEffect.transform.SetParent(grabbableObject.transform);
                    objectBehaviour.particleEffect = curseParticleEffect;

                    ParticleSystem particleSystem = curseParticleEffect.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        ParticleSystem.MainModule mainModule = particleSystem.main;
                        mainModule.startSize = maxValue;
                        mainModule.startSpeed = maxValue / 3f;
                    }
                }

                if (grabbableObject.isHeld)
                    EnableParticle(objectBehaviour, false);
            }

            ScanNodeProperties scanNode = grabbableObject.gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (scanNode != null)
            {
                if (grabbableObject.itemProperties.isScrap)
                    scanNode.scrapValue = grabbableObject.scrapValue;

                scanNode.subText = GetNewSubText(objectBehaviour, grabbableObject.scrapValue.ToString());

                if (ConfigManager.isRedScanOn.Value)
                    scanNode.nodeType = 1;
            }
        }

        public string GetNewSubText(ObjectCSBehaviour objectCSBehaviour, string scrapValue)
        {
            if (ConfigManager.isHideValue.Value)
                scrapValue = "???";

            string curseText = "";
            bool isScrap = objectCSBehaviour.objectProperties.itemProperties.isScrap;
            bool firstCurse = true;

            if (!ConfigManager.isHideLine.Value)
            {
                foreach (CurseEffect curseEffect in objectCSBehaviour.curseEffects)
                {
                    string curseName = ConfigManager.isHideName.Value ? "???" : curseEffect.CurseName;
                    curseText += (firstCurse && !isScrap ? "" : "\n") + $"Curse: {curseName}";
                    firstCurse = false;
                }
            }
            return isScrap ? $"Value: ${scrapValue}{curseText}" : curseText;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveAllScrapCurseEffectServerRpc(NetworkObjectReference obj)
            => RemoveAllScrapCurseEffectClientRpc(obj);

        [ClientRpc]
        private void RemoveAllScrapCurseEffectClientRpc(NetworkObjectReference obj)
        {
            if (!obj.TryGet(out var networkObject)) return;

            ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
            if (objectBehaviour == null) return;

            objectBehaviour.curseEffects.Clear();

            if (objectBehaviour.particleEffect != null)
                Destroy(objectBehaviour.particleEffect);

            ScanNodeProperties scanNode = objectBehaviour.objectProperties.gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (scanNode == null) return;

            scanNode.subText = $"Value: ${scanNode.scrapValue}";
            scanNode.nodeType = (objectBehaviour.objectProperties.itemProperties.isScrap ? 2 : 0);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerCurseEffectServerRpc(int playerId, string curseName, bool enable)
            => SetPlayerCurseEffectClientRpc(playerId, curseName, enable);

        [ClientRpc]
        private void SetPlayerCurseEffectClientRpc(int playerId, string curseName, bool enable)
        {
            CurseEffect curseEffect = CursedScraps.curseEffects.FirstOrDefault(c => c.CurseName.Equals(curseName));
            if (curseEffect == null) return;

            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            PlayerCSManager.SetPlayerCurseEffect(player, curseEffect, enable);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveAllPlayerCurseEffectServerRpc(int playerId)
            => RemoveAllPlayerCurseEffectClientRpc(playerId);

        [ClientRpc]
        private void RemoveAllPlayerCurseEffectClientRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            PlayerCSBehaviour playerBehaviour = player.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                foreach (CurseEffect curseEffect in playerBehaviour.activeCurses.ToList())
                    PlayerCSManager.SetPlayerCurseEffect(player, curseEffect, false);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyObjectServerRpc(NetworkObjectReference obj)
            => DestroyObjectClientRpc(obj);

        [ClientRpc]
        private void DestroyObjectClientRpc(NetworkObjectReference obj)
        {
            if (!obj.TryGet(out var networkObject)) return;
            DestroyObject(networkObject.gameObject.GetComponentInChildren<GrabbableObject>());
        }

        public void DestroyObject(GrabbableObject grabbableObject)
        {
            if (grabbableObject == null) return;

            if (grabbableObject is FlashlightItem flashlight && flashlight.isBeingUsed)
            {
                flashlight.isBeingUsed = false;
                flashlight.usingPlayerHelmetLight = false;
                flashlight.flashlightBulbGlow.enabled = false;
                flashlight.SwitchFlashlight(on: false);
            }
            else if (grabbableObject is BeltBagItem beltBagItem)
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers = beltBagItem.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers.ToList())
                    Destroy(skinnedMeshRenderer);
            }
            ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.particleEffect != null)
                Destroy(objectBehaviour.particleEffect);
            grabbableObject.DestroyObjectInHand(grabbableObject.playerHeldBy);
        }

        [ServerRpc(RequireOwnership = false)]
        public void EnableParticleServerRpc(NetworkObjectReference obj, bool enable)
            => EnableParticleClientRpc(obj, enable);

        [ClientRpc]
        private void EnableParticleClientRpc(NetworkObjectReference obj, bool enable)
        {
            if (!obj.TryGet(out var networkObject)) return;

            ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
            EnableParticle(objectBehaviour, enable);
        }

        public void EnableParticle(ObjectCSBehaviour objectBehaviour, bool enable)
        {
            if (objectBehaviour == null) return;
            if (objectBehaviour.particleEffect == null) return;
            
            objectBehaviour.particleEffect.gameObject.SetActive(enable);
        }

        // DIMINUTIVE
        [ServerRpc(RequireOwnership = false)]
        public void PushPlayerServerRpc(int playerId, Vector3 pushVector)
            => PushPlayerClientRpc(playerId, pushVector);

        [ClientRpc]
        private void PushPlayerClientRpc(int playerId, Vector3 pushVector)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player != GameNetworkManager.Instance.localPlayerController) return;
            
            player.thisController.Move(pushVector);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerServerRpc(int playerId, Vector3 position, bool isInElevator, bool isInHangarShipRoom, bool isInsideFactory)
            => TeleportPlayerClientRpc(playerId, position, isInElevator, isInHangarShipRoom, isInsideFactory);

        [ClientRpc]
        private void TeleportPlayerClientRpc(int playerId, Vector3 position, bool isInElevator, bool isInHangarShipRoom, bool isInsideFactory)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player != GameNetworkManager.Instance.localPlayerController) return;
            
            PlayerCSManager.TeleportPlayer(player, position, isInElevator, isInHangarShipRoom, isInsideFactory);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DamagePlayerServerRpc(int playerId, int damageNumber)
            => DamagePlayerClientRpc(playerId, damageNumber);

        [ClientRpc]
        private void DamagePlayerClientRpc(int playerId, int damageNumber)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player != GameNetworkManager.Instance.localPlayerController) return;
            
            player.DamagePlayer(damageNumber, hasDamageSFX: true, callRPC: true, CauseOfDeath.Unknown);
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillPlayerServerRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
            => KillPlayerClientRpc(playerId, velocity, spawnBody, causeOfDeath);

        [ClientRpc]
        private void KillPlayerClientRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player != GameNetworkManager.Instance.localPlayerController) return;
            
            player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AssignTrackedItemServerRpc(int playerId, NetworkObjectReference obj)
            => AssignTrackedItemClientRpc(playerId, obj);

        [ClientRpc]
        private void AssignTrackedItemClientRpc(int playerId, NetworkObjectReference obj)
        {
            if (!obj.TryGet(out var networkObject)) return;

            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            PlayerCSBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerCSBehaviour>();
            playerBehaviour.trackedItem = grabbableObject;

            if (grabbableObject is not OldScroll oldScroll) return;
            oldScroll.assignedPlayer = playerBehaviour.playerProperties;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnParticleServerRpc(int playerId, bool isHot)
            => SpawnParticleClientRpc(playerId, isHot);

        [ClientRpc]
        public void SpawnParticleClientRpc(int playerId, bool isHot)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            GameObject spawnObject;

            if (isHot) spawnObject = Instantiate(CursedScraps.hotParticle, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);
            else spawnObject = Instantiate(CursedScraps.coldParticle, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);

            spawnObject.transform.SetParent(player.transform);
            spawnObject.transform.localScale = player.transform.localScale;

            ParticleSystem particleSystem = spawnObject.GetComponent<ParticleSystem>();
            Destroy(spawnObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
        }

        [ServerRpc(RequireOwnership = false)]
        public void IncrementPenaltyCounterServerRpc(NetworkObjectReference obj)
            => IncrementPenaltyCounterClientRpc(obj);

        [ClientRpc]
        private void IncrementPenaltyCounterClientRpc(NetworkObjectReference obj)
        {
            if (!obj.TryGet(out var networkObject)) return;

            ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
            if (objectBehaviour == null) return;

            SORCSBehaviour sorBehaviour = StartOfRound.Instance.GetComponent<SORCSBehaviour>();
            sorBehaviour.counter++;
            sorBehaviour.scannedObjects.Add(objectBehaviour.objectProperties);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveFromBagServerRpc(NetworkObjectReference obj, NetworkObjectReference bagObj)
            => RemoveFromBagClientRpc(obj, bagObj);

        [ClientRpc]
        private void RemoveFromBagClientRpc(NetworkObjectReference obj, NetworkObjectReference bagObj)
        {
            if (!obj.TryGet(out var networkObject)) return;
            if (!bagObj.TryGet(out var bagNetworkObject)) return;

            GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
            if (grabbableObject == null) return;

            BeltBagItem beltBagItem = bagNetworkObject.gameObject.GetComponentInChildren<GrabbableObject>() as BeltBagItem;
            if (beltBagItem == null) return;
            
            beltBagItem.objectsInBag.Remove(grabbableObject);
        }
    }
}
