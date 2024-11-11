using CursedScraps.Behaviours;
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
        public void SetScrapCurseEffectServerRpc(NetworkObjectReference obj, string curseName) => SetScrapCurseEffectClientRpc(obj, curseName);

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
                    scanNode.subText = GetNewSubText(ref objectBehaviour, grabbableObject.scrapValue.ToString());
                    if (ConfigManager.isRedScanOn.Value)
                    {
                        scanNode.nodeType = 1;
                    }

                    if (ConfigManager.isParticleOn.Value)
                    {
                        float maxValue = Mathf.Max(grabbableObject.transform.localScale.x, Mathf.Max(grabbableObject.transform.localScale.y, grabbableObject.transform.localScale.z));
                        if (maxValue < ConfigManager.minParticleScale.Value) maxValue = ConfigManager.minParticleScale.Value;
                        else if (maxValue > ConfigManager.maxParticleScale.Value) maxValue = ConfigManager.maxParticleScale.Value;

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
                }
            }
        }

        public string GetNewSubText(ref ObjectCSBehaviour objectCSBehaviour, string scrapValue)
        {
            if (ConfigManager.isHideValue.Value)
            {
                scrapValue = "???";
            }
            string curseText = "";
            if (!ConfigManager.isHideLine.Value)
            {
                foreach (CurseEffect curseEffect in objectCSBehaviour.curseEffects)
                {
                    string curseName = curseEffect.CurseName;
                    if (ConfigManager.isHideName.Value)
                    {
                        curseName = "???";
                    }
                    curseText += $"\nCurse: {curseName}";
                }
            }
            return $"Value: ${scrapValue}{curseText}";
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveAllScrapCurseEffectServerRpc(NetworkObjectReference obj) => RemoveAllScrapCurseEffectClientRpc(obj);

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
        public void SetPlayerCurseEffectServerRpc(int playerId, string curseName, bool enable) => SetPlayerCurseEffectClientRpc(playerId, curseName, enable);

        [ClientRpc]
        private void SetPlayerCurseEffectClientRpc(int playerId, string curseName, bool enable)
        {
            CurseEffect curseEffect = CursedScraps.curseEffects.Where(c => c.CurseName.Equals(curseName)).FirstOrDefault();
            if (curseEffect != null)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                PlayerCSManager.SetPlayerCurseEffect(player, curseEffect, enable);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveAllPlayerCurseEffectServerRpc(int playerId) => RemoveAllPlayerCurseEffectClientRpc(playerId);

        [ClientRpc]
        private void RemoveAllPlayerCurseEffectClientRpc(int playerId)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            PlayerCSBehaviour playerBehaviour = player.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null)
            {
                foreach (CurseEffect curseEffect in playerBehaviour.activeCurses.ToList())
                {
                    PlayerCSManager.SetPlayerCurseEffect(player, curseEffect, false);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyObjectServerRpc(NetworkObjectReference obj) => DestroyObjectClientRpc(obj);

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
                else if (grabbableObject is BeltBagItem beltBagItem)
                {
                    SkinnedMeshRenderer[] skinnedMeshRenderers = beltBagItem.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers.ToList())
                    {
                        Destroy(skinnedMeshRenderer);
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
        public void EnableParticleServerRpc(NetworkObjectReference obj, bool enable) => EnableParticleClientRpc(obj, enable);

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
        public void PushPlayerServerRpc(int playerId, Vector3 pushVector) => PushPlayerClientRpc(playerId, pushVector);

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
        public void KillPlayerServerRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath) => KillPlayerClientRpc(playerId, velocity, spawnBody, causeOfDeath);

        [ClientRpc]
        private void KillPlayerClientRpc(int playerId, Vector3 velocity, bool spawnBody, int causeOfDeath)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
            if (player == GameNetworkManager.Instance.localPlayerController)
            {
                player.KillPlayer(velocity, spawnBody, (CauseOfDeath)causeOfDeath);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AssignTrackedItemServerRpc(int playerId, NetworkObjectReference obj) => AssignTrackedItemClientRpc(playerId, obj);

        [ClientRpc]
        private void AssignTrackedItemClientRpc(int playerId, NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                PlayerCSBehaviour playerBehaviour = StartOfRound.Instance.allPlayerObjects[playerId].GetComponentInChildren<PlayerCSBehaviour>();
                playerBehaviour.trackedItem = grabbableObject;

                if (grabbableObject is OldScroll oldScroll)
                {
                    oldScroll.assignedPlayer = playerBehaviour.playerProperties;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnParticleServerRpc(int playerId, bool isHot) => SpawnParticleClientRpc(playerId, isHot);

        [ClientRpc]
        public void SpawnParticleClientRpc(int playerId, bool isHot)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();

            GameObject spawnObject;
            if (isHot)
            {
                spawnObject = Instantiate(CursedScraps.hotParticle, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);
            }
            else
            {
                spawnObject = Instantiate(CursedScraps.coldParticle, player.gameplayCamera.transform.position, player.gameplayCamera.transform.rotation);
            }
            spawnObject.transform.SetParent(player.transform);
            spawnObject.transform.localScale = player.transform.localScale;

            ParticleSystem particleSystem = spawnObject.GetComponent<ParticleSystem>();
            Destroy(spawnObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
        }

        [ServerRpc(RequireOwnership = false)]
        public void IncrementPenaltyCounterServerRpc(NetworkObjectReference obj) => IncrementPenaltyCounterClientRpc(obj);

        [ClientRpc]
        private void IncrementPenaltyCounterClientRpc(NetworkObjectReference obj)
        {
            if (obj.TryGet(out var networkObject))
            {
                ObjectCSBehaviour objectBehaviour = networkObject.gameObject.GetComponentInChildren<ObjectCSBehaviour>();
                if (objectBehaviour != null)
                {
                    SORCSBehaviour sorBehaviour = StartOfRound.Instance.GetComponent<SORCSBehaviour>();
                    sorBehaviour.counter++;
                    sorBehaviour.scannedObjects.Add(objectBehaviour.objectProperties);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveFromBagServerRpc(NetworkObjectReference obj, NetworkObjectReference bagObj) => RemoveFromBagClientRpc(obj, bagObj);

        [ClientRpc]
        private void RemoveFromBagClientRpc(NetworkObjectReference obj, NetworkObjectReference bagObj)
        {
            if (obj.TryGet(out var networkObject) && bagObj.TryGet(out var bagNetworkObject))
            {
                GrabbableObject grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
                BeltBagItem beltBagItem = bagNetworkObject.gameObject.GetComponentInChildren<GrabbableObject>() as BeltBagItem;
                if (grabbableObject != null && beltBagItem != null)
                {
                    beltBagItem.objectsInBag.Remove(grabbableObject);
                }
            }
        }
    }
}
