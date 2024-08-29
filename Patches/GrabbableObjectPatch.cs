﻿using CursedScraps.Behaviours;
using CursedScraps.Managers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class GrabbableObjectPatch
    {
        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Update))]
        [HarmonyPostfix]
        private static void UpdateGrabbableObject(GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.particleEffect != null)
            {
                objectBehaviour.particleEffect.transform.localScale = __instance.transform.localScale;
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.EquipItem))]
        [HarmonyPostfix]
        private static void ShowParticle(GrabbableObject __instance)
        {
            if (!ConfigManager.isParticleHideWhenGrabbing.Value && __instance.GetComponent<ObjectCSBehaviour>() != null)
            {
                CursedScrapsNetworkManager.Instance.EnableParticleServerRpc(__instance.GetComponent<NetworkObject>(), true);
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.PocketItem))]
        [HarmonyPostfix]
        private static void HideParticle(GrabbableObject __instance)
        {
            if (!ConfigManager.isParticleHideWhenGrabbing.Value && __instance.GetComponent<ObjectCSBehaviour>() != null)
            {
                CursedScrapsNetworkManager.Instance.EnableParticleServerRpc(__instance.GetComponent<NetworkObject>(), false);
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
        [HarmonyPrefix]
        private static bool PreventOverrideCurseValue(GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.Count > 0)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetScrapValue))]
        [HarmonyPostfix]
        private static void SetCurseObject(GrabbableObject __instance)
        {
            if (GameNetworkManager.Instance?.localPlayerController != null
                && (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer))
            {
                string planetName = new(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
                if ((string.IsNullOrEmpty(ConfigManager.scrapExclusions.Value) || !ConfigManager.scrapExclusions.Value.Contains(__instance.itemProperties.itemName))
                    && __instance.scrapValue > 0
                    && __instance.isInFactory
                    && IsCursed(planetName))
                {
                    CurseEffect curseEffect = GetRandomCurseEffect(planetName);
                    if (curseEffect != null)
                    {
                        NetworkObject networkObject = __instance.GetComponent<NetworkObject>();
                        if (networkObject != null && networkObject.IsSpawned)
                        {
                            CursedScrapsNetworkManager.Instance.SetScrapCurseEffectServerRpc(networkObject, curseEffect.CurseName);
                        }
                    }
                }
            }
        }

        private static bool IsCursed(string planetName)
        {
            if (new System.Random().Next(1, 100) <= GetValueFromPair(ConfigManager.globalChance.Value, planetName))
            {
                return true;
            }
            return false;
        }

        private static CurseEffect GetRandomCurseEffect(string planetName)
        {
            bool isMultiplayer = StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isTestingPlayer).Count() > 1;
            // Ajout des malédictions éligibles en fonction de leur valeur d'importance
            List<CurseEffect> eligibleEffects = new List<CurseEffect>();
            foreach (CurseEffect effect in CursedScraps.curseEffects.Where(c => isMultiplayer || !c.IsCoop))
            {
                for (int i = 0; i < GetValueFromPair(effect.Weight, planetName); i++)
                {
                    eligibleEffects.Add(effect);
                }
            }
            // Sélectionner un effet aléatoire parmi les effets éligibles
            if (eligibleEffects.Count > 0)
            {
                return eligibleEffects[new System.Random().Next(eligibleEffects.Count)];
            }
            else
            {
                return null;
            }
        }

        private static int GetValueFromPair(string valueByMoons, string planetName)
        {
            // Tableau contenant tous les ensembles clé/valeur pour les noms de planètes/valeurs d'importances
            string[] valuePairs = valueByMoons.Split(',');
            int defaultValue = 0;

            foreach (string valuePair in valuePairs)
            {
                // Ensemble clé/valeur pour nom de la planète/valeur d'importance
                string[] valueTab = valuePair.Split(':');
                if (valueTab.Length == 2 && valueTab[0] == planetName)
                {
                    // Retourner la valeur d'importance si la clé est trouvée
                    return int.Parse(valueTab[1]);
                }
                else if (valueTab.Length == 2 && valueTab[0] == "default")
                {
                    defaultValue = int.Parse(valueTab[1]);
                }
            }
            return defaultValue;
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.SetControlTipsForItem))]
        [HarmonyPostfix]
        private static void ChangeToolTip(ref GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.curseEffects.FirstOrDefault(c => c.CurseName.Equals(Constants.SYNCHRONIZATION)) != null)
            {
                HUDManager.Instance.ChangeControlTipMultiple(__instance.itemProperties.toolTips.Concat(["Reorient the camera : [E]"]).ToArray(), holdingItem: true, __instance.itemProperties);
            }
        }
    }
}
