using CursedScraps.Behaviours;
using GameNetcodeStuff;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Managers
{
    internal class CursedScrapsNetworkManager : NetworkBehaviour
    {
        public static CursedScrapsNetworkManager Instance;

        public void Awake()
        {
            Instance = this;
        }

        // GLOBAL
        [ServerRpc(RequireOwnership = false)]
        public void SetScrapCurseEffectServerRpc(NetworkObjectReference obj, string curseName)
        {
            SetScrapCurseEffectClientRpc(obj, curseName);
        }

        [ClientRpc]
        private void SetScrapCurseEffectClientRpc(NetworkObjectReference obj, string curseName)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                SetScrapCurseEffect(ref grabbableObject, curseName, true);
            }
        }

        public void SetScrapCurseEffect(ref GrabbableObject grabbableObject, string curseName, bool applyValue)
        {
            CurseEffect curseEffect = CursedScraps.curseEffects.Where(c => c.CurseName.Equals(curseName)).FirstOrDefault();
            ScanNodeProperties scanNode = grabbableObject?.gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (scanNode != null && curseEffect != null)
            {
                ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>() ?? grabbableObject.gameObject.AddComponent<ObjectCSBehaviour>();
                if (objectBehaviour != null)
                {
                    objectBehaviour.objectProperties = grabbableObject;
                    objectBehaviour.curseEffects.Add(curseEffect);

                    if (applyValue)
                    {
                        grabbableObject.scrapValue = (int)(grabbableObject.scrapValue * curseEffect.Multiplier);
                    }
                    scanNode.scrapValue = grabbableObject.scrapValue;
                    scanNode.subText = GetNewSubText(ref objectBehaviour, grabbableObject.scrapValue);
                    if (ConfigManager.isRedScanOn.Value)
                    {
                        scanNode.nodeType = 1;
                    }

                    if (ConfigManager.isParticleOn.Value)
                    {
                        GameObject curseParticleEffect = Instantiate(CursedScraps.curseParticle, grabbableObject.transform.position, Quaternion.identity);
                        curseParticleEffect.transform.SetParent(grabbableObject.transform);
                        curseParticleEffect.transform.localScale = grabbableObject.transform.localScale;
                        objectBehaviour.particleEffect = curseParticleEffect;
                    }
                }
            }
        }

        public string GetNewSubText(ref ObjectCSBehaviour objectCSBehaviour, int scrapValue)
        {
            string curseText = "";
            foreach (string curseName in objectCSBehaviour.curseEffects.Select(c => c.CurseName))
            {
                curseText += $"\nCurse: {curseName}";
            }
            return $"Value: ${scrapValue}{curseText}";
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveAllScrapCurseEffectServerRpc(NetworkObjectReference obj)
        {
            RemoveAllScrapCurseEffectClientRpc(obj);
        }

        [ClientRpc]
        private void RemoveAllScrapCurseEffectClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
                ScanNodeProperties scanNode = objectBehaviour?.objectProperties.gameObject.GetComponentInChildren<ScanNodeProperties>();
                if (scanNode != null)
                {
                    scanNode.subText = $"Value: ${scanNode.scrapValue}";
                    scanNode.nodeType = 2;
                    objectBehaviour.curseEffects.Clear();

                    if (objectBehaviour.particleEffect != null)
                    {
                        Destroy(objectBehaviour.particleEffect);
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerCurseEffectServerRpc(int playerId, string curseName, bool enable)
        {
            SetPlayerCurseEffectClientRpc(playerId, curseName, enable);
        }

        [ClientRpc]
        private void SetPlayerCurseEffectClientRpc(int playerId, string curseName, bool enable)
        {
            CurseEffect curseEffect = CursedScraps.curseEffects.Where(c => c.CurseName.Equals(curseName)).FirstOrDefault();
            if (curseEffect != null)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                CSPlayerManager.SetPlayerCurseEffect(player, curseEffect, enable);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyObjectServerRpc(NetworkObjectReference obj)
        {
            DestroyObjectClientRpc(obj);
        }

        [ClientRpc]
        private void DestroyObjectClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                DestroyObject(networkObject.gameObject.GetComponentInChildren<GrabbableObject>());
            }
        }

        public void DestroyObject(GrabbableObject grabbableObject)
        {
            if (grabbableObject != null)
            {
                if (grabbableObject is FlashlightItem flashlight)
                {
                    if (flashlight.isBeingUsed)
                    {
                        flashlight.isBeingUsed = false;
                        flashlight.usingPlayerHelmetLight = false;
                        flashlight.flashlightBulbGlow.enabled = false;
                        flashlight.SwitchFlashlight(on: false);
                    }
                }
                ObjectCSBehaviour objectBehaviour = grabbableObject.GetComponent<ObjectCSBehaviour>();
                if (objectBehaviour != null && objectBehaviour.particleEffect != null)
                {
                    Destroy(objectBehaviour.particleEffect);
                }
                grabbableObject.DestroyObjectInHand(grabbableObject.playerHeldBy);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void EnableParticleServerRpc(NetworkObjectReference obj, bool enable)
        {
            EnableParticleClientRpc(obj, enable);
        }

        [ClientRpc]
        private void EnableParticleClientRpc(NetworkObjectReference obj, bool enable)
        {
            if (obj.TryGet(out var networkObject))
            {
                ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
                if (objectBehaviour != null && objectBehaviour.particleEffect != null)
                {
                    objectBehaviour.particleEffect.gameObject.SetActive(enable);
                }
            }
        }

        // DIMINUTIVE
        [ServerRpc(RequireOwnership = false)]
        public void PushPlayerServerRpc(int playerId, Vector3 pushVector)
        {
            PushPlayerClientRpc(playerId, pushVector);
        }

        [ClientRpc]
        private void PushPlayerClientRpc(int playerId, Vector3 pushVector)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                player.thisController.Move(pushVector);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void KillPlayerServerRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
        {
            KillPlayerClientRpc(playerId, velocity, spawnBody, causeOfDeath);
        }

        [ClientRpc]
        private void KillPlayerClientRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
            }
        }

        // COOP
        [ServerRpc(RequireOwnership = false)]
        public void SetCloneScrapServerRpc(NetworkObjectReference obj, NetworkObjectReference objClone, int playerId, string name, string nameReflection)
        {
            SetCloneScrapClientRpc(obj, objClone, playerId, name, nameReflection);
        }

        [ClientRpc]
        private void SetCloneScrapClientRpc(NetworkObjectReference obj, NetworkObjectReference objClone, int playerId, string name, string nameReflection)
        {
            if (obj.TryGet(out var networkObject) && objClone.TryGet(out var networkObjectClone))
            {
                GrabbableObject scrap = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                GrabbableObject cloneScrap = networkObjectClone.gameObject.GetComponentInChildren<GrabbableObject>();
                if (scrap != null && cloneScrap != null)
                {
                    SetScrapCurseEffect(ref cloneScrap, name, false);
                    ObjectCSBehaviour objectBehaviour = cloneScrap.GetComponent<ObjectCSBehaviour>();
                    if (objectBehaviour != null)
                    {
                        objectBehaviour.mainPart = scrap;
                        objectBehaviour.isSplitted = true;

                        ScanNodeProperties scanNode = cloneScrap.gameObject.GetComponentInChildren<ScanNodeProperties>();
                        if (scanNode != null)
                        {
                            cloneScrap.scrapValue = scrap.scrapValue;
                            scanNode.scrapValue = scrap.scrapValue;
                            scanNode.subText = scanNode.subText.Replace(name, nameReflection);

                            // Spécifique pour COMMUNICATION
                            PlayerCSBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerCSBehaviour>();
                            if (name.Equals(Constants.COMMUNICATION))
                            {
                                objectBehaviour.playerOwner = playerBehaviour.coopPlayer;
                                if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
                                {
                                    playerBehaviour.trackedScrap = cloneScrap;
                                }
                            }
                        }
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerOwnerScrapServerRpc(NetworkObjectReference obj, int playerId)
        {
            SetPlayerOwnerScrapClientRpc(obj, playerId);
        }

        [ClientRpc]
        private void SetPlayerOwnerScrapClientRpc(NetworkObjectReference obj, int playerId)
        {
            if (obj.TryGet(out var networkObject))
            {
                ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
                if (objectBehaviour != null)
                {
                    objectBehaviour.playerOwner = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void DesactiveCoopEffectServerRpc(int playerId, string curseName, bool isCoop)
        {
            DesactiveCoopEffectClientRpc(playerId, curseName, isCoop);
        }

        [ClientRpc]
        private void DesactiveCoopEffectClientRpc(int playerId, string curseName, bool isCoop)
        {
            CurseEffect curseEffect = CursedScraps.curseEffects.Where(c => c.CurseName.Equals(curseName)).FirstOrDefault();
            if (curseEffect != null)
            {
                PlayerCSBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerCSBehaviour>();
                if (playerBehaviour != null)
                {
                    CSObjectManager.RemovePlayerOwner(ref playerBehaviour.playerProperties);
                    if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController)
                    {
                        CSPlayerManager.EnablePlayerActions(ref curseEffect, true);
                        if (isCoop)
                        {
                            playerBehaviour.playerProperties.DropAllHeldItemsAndSync();
                        }
                    }
                    CSPlayerManager.SetPlayerCurseEffect(playerBehaviour.playerProperties, curseEffect, false);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveObjectCoopEffectServerRpc(NetworkObjectReference obj)
        {
            RemoveObjectCoopEffectClientRpc(obj);
        }

        [ClientRpc]
        private void RemoveObjectCoopEffectClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
                if (objectBehaviour != null)
                {
                    objectBehaviour.playerOwner = null;
                    objectBehaviour.mainPart = null;
                    objectBehaviour.isSplitted = false;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ForcePlayerRotationServerRpc(int playerId, Vector3 direction)
        {
            ForcePlayerRotationClientRpc(playerId, direction);
        }

        [ClientRpc]
        private void ForcePlayerRotationClientRpc(int playerId, Vector3 direction)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                player.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
