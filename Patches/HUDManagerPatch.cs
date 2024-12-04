using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
using CursedScraps.Managers;
using GameNetcodeStuff;
using HarmonyLib;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class HUDManagerPatch
    {
        //public static TextMeshProUGUI chronoText;
        public static TextMeshProUGUI distanceText;
        public static TextMeshProUGUI cursesText;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
        [HarmonyPostfix]
        private static void StartHUDManager()
        {
            /*GameObject chrono = Object.Instantiate(__instance.weightCounterAnimator.gameObject, __instance.weightCounterAnimator.transform.parent);
            chrono.transform.localPosition += new Vector3(-85f, 185f, 0f);
            chrono.name = "ChronoUI";

            chronoText = chrono.GetComponentInChildren<TextMeshProUGUI>();
            chronoText.text = "";
            chronoText.alignment = TextAlignmentOptions.BottomLeft;
            chronoText.name = "Chrono";*/
            
            distanceText = HUDCSManager.CreateUIElement(
                name: "DistanceUI",
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(0f, 1f),
                pivot: new Vector2(0f, 1f),
                anchoredPosition: new Vector2(ConfigManager.communicationDistancePosX.Value, ConfigManager.communicationDistancePosY.Value),
                alignment: TextAlignmentOptions.TopLeft
            );

            cursesText = HUDCSManager.CreateUIElement(
                name: "CursesUI",
                anchorMin: new Vector2(1f, 0f),
                anchorMax: new Vector2(1f, 0f),
                pivot: new Vector2(1f, 0f),
                anchoredPosition: new Vector2(ConfigManager.deadCursesPosX.Value, ConfigManager.deadCursesPosY.Value),
                alignment: TextAlignmentOptions.BottomRight
            );
        }

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.AssignNodeToUIElement))]
        [HarmonyPostfix]
        private static void ScanPerformed(ref HUDManager __instance, ref ScanNodeProperties node)
        {
            if (__instance.scanNodes.ContainsValue(node))
            {
                PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
                Paralysis.ScanPerformed(playerBehaviour, node);

                // Penalty mode
                if (!string.IsNullOrEmpty(ConfigManager.penaltyMode.Value) && !ConfigManager.penaltyMode.Value.Equals(Constants.PENALTY_NONE))
                {
                    ObjectCSBehaviour objectBehaviour = node.GetComponentInParent<ObjectCSBehaviour>();
                    if (objectBehaviour != null && objectBehaviour.curseEffects.Count() > 0)
                    {
                        SORCSBehaviour sorBehaviour = StartOfRound.Instance.GetComponent<SORCSBehaviour>();

                        if (!sorBehaviour.scannedObjects.Contains(objectBehaviour.objectProperties))
                            CursedScrapsNetworkManager.Instance.IncrementPenaltyCounterServerRpc(objectBehaviour.objectProperties.GetComponent<NetworkObject>());

                        if (sorBehaviour.counter >= ConfigManager.penaltyCounter.Value)
                        {
                            foreach (CurseEffect curseEffect in objectBehaviour.curseEffects)
                            {
                                if (ConfigManager.penaltyMode.Value.Equals(Constants.PENALTY_HARD))
                                {
                                    foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isPlayerDead))
                                        CursedScrapsNetworkManager.Instance.SetPlayerCurseEffectServerRpc((int)player.playerClientId, curseEffect.CurseName, true);
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

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SetScreenFilters))]
        [HarmonyPostfix]
        private static void UpdateScreenFilters() => Blurry.UpdateScreenFilters(GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>());

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.HoldInteractionFill))]
        [HarmonyPostfix]
        private static void EntranceInteraction(ref bool __result)
        {
            if (!__result) return;

            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            EntranceTeleport entranceTeleport = playerBehaviour.playerProperties.hoveringOverTrigger?.gameObject.GetComponent<EntranceTeleport>();
            if (entranceTeleport != null)
            {
                if (!Exploration.EntranceInteraction(playerBehaviour, entranceTeleport)
                    || !Communication.CanEscape(playerBehaviour, "A curse prevents you from using this doorway."))
                {
                    __result = false;
                }
            }
        }
    }
}
