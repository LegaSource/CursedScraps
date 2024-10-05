using CursedScraps.Behaviours;
using CursedScraps.Managers;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace CursedScraps.Patches
{
    internal class HUDManagerPatch
    {
        public static GameObject chrono;
        public static TextMeshProUGUI chronoText;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
        [HarmonyPostfix]
        private static void Start(HUDManager __instance)
        {
            if (chrono == null)
            {
                chrono = Object.Instantiate(__instance.weightCounterAnimator.gameObject, __instance.weightCounterAnimator.transform.parent);
                chrono.transform.localPosition += new Vector3(-85f, 185f, 0f);
                chrono.name = "ChronoUI";

                chronoText = chrono.GetComponentInChildren<TextMeshProUGUI>();
                chronoText.text = "";
                chronoText.alignment = TextAlignmentOptions.BottomLeft;
                chronoText.name = "Chrono";
            }
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.AssignNodeToUIElement))]
        [HarmonyPostfix]
        private static void ScanPerformed(ref HUDManager __instance, ref ScanNodeProperties node)
        {
            if (__instance.scanNodes.ContainsValue(node))
            {
                PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
                if (playerBehaviour != null
                    && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.PARALYSIS)) != null
                    && node.nodeType == 1)
                {
                    CSPlayerManager.Paralyze(ref playerBehaviour);
                }

                // Penalty mode
                if (!string.IsNullOrEmpty(ConfigManager.penaltyMode.Value) && !ConfigManager.penaltyMode.Value.Equals(Constants.PENALTY_NONE))
                {
                    ObjectCSBehaviour objectBehaviour = node.GetComponentInParent<ObjectCSBehaviour>();
                    if (objectBehaviour != null && objectBehaviour.curseEffects.Count() > 0)
                    {
                        SORCSBehaviour sorBehaviour = StartOfRound.Instance.GetComponent<SORCSBehaviour>();
                        if (!sorBehaviour.scannedObjects.Contains(objectBehaviour.objectProperties))
                        {
                            CursedScrapsNetworkManager.Instance.IncrementPenaltyCounterServerRpc(objectBehaviour.objectProperties.GetComponent<NetworkObject>());
                        }
                        if (sorBehaviour.counter >= ConfigManager.penaltyCounter.Value)
                        {
                            foreach (CurseEffect curseEffect in objectBehaviour.curseEffects.Where(c => !c.IsCoop))
                            {
                                if (ConfigManager.penaltyMode.Value.Equals(Constants.PENALTY_HARD))
                                {
                                    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isPlayerDead))
                                    {
                                        CursedScrapsNetworkManager.Instance.SetPlayerCurseEffectServerRpc((int)player.playerClientId, curseEffect.CurseName, true);
                                    }
                                }
                                else if (ConfigManager.penaltyMode.Value.Equals(Constants.PENALTY_MEDIUM))
                                {
                                    CursedScrapsNetworkManager.Instance.SetPlayerCurseEffectServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, curseEffect.CurseName, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "SetScreenFilters")]
        [HarmonyPostfix]
        private static void UpdateScreenFilters(ref Volume ___drunknessFilter)
        {
            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.BLURRY)) != null)
            {
                ___drunknessFilter.weight = ConfigManager.blurryIntensity.Value;
            }
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.HoldInteractionFill))]
        [HarmonyPostfix]
        private static void EntranceInteraction(ref bool __result)
        {
            if (!__result)
            {
                return;
            }
            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null && playerBehaviour.activeCurses.FirstOrDefault(p => p.CurseName.Equals(Constants.EXPLORATION)) != null)
            {
                EntranceTeleport entranceTeleport = playerBehaviour.playerProperties.hoveringOverTrigger?.gameObject.GetComponent<EntranceTeleport>();
                if (entranceTeleport != null)
                {
                    if (entranceTeleport != playerBehaviour.targetDoor)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from using this doorway.");
                        __result = false;
                    }
                    else
                    {
                        CSPlayerManager.ChangeRandomEntranceId(playerBehaviour.playerProperties.isInsideFactory, ref playerBehaviour);
                    }
                }
            }
        }
    }
}
