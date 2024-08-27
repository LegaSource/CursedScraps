using CursedScraps.Behaviours;
using CursedScraps.Managers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

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
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.particleEffect != null)
            {
                objectBehaviour.particleEffect.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.PocketItem))]
        [HarmonyPostfix]
        private static void HideParticle(GrabbableObject __instance)
        {
            ObjectCSBehaviour objectBehaviour = __instance.GetComponent<ObjectCSBehaviour>();
            if (objectBehaviour != null && objectBehaviour.particleEffect != null)
            {
                objectBehaviour.particleEffect.gameObject.SetActive(false);
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
            if (GameNetworkManager.Instance.localPlayerController.IsHost || GameNetworkManager.Instance.localPlayerController.IsServer)
            {
                string planetName = new(StartOfRound.Instance.currentLevel.PlanetName.SkipWhile((char c) => !char.IsLetter(c)).ToArray());
                if (__instance.scrapValue > 0 && IsCursed(planetName))
                {
                    CurseEffect curseEffect = GetRandomCurseEffect(planetName);
                    if (curseEffect != null)
                    {
                        CursedScrapsNetworkManager.Instance.SetScrapCurseEffectServerRpc(__instance.GetComponent<NetworkObject>(), curseEffect.CurseName);
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
            // Ajout des malédictions éligibles en fonction de leur valeur d'importance
            List<CurseEffect> eligibleEffects = new List<CurseEffect>();
            foreach (CurseEffect effect in CursedScraps.curseEffects)
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
