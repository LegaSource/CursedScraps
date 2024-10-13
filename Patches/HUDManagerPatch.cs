using CursedScraps.Behaviours;
using CursedScraps.Behaviours.Curses;
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
        public static TextMeshProUGUI chronoText;
        public static TextMeshProUGUI distanceText;
        public static TextMeshProUGUI cursesText;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.Start))]
        [HarmonyPostfix]
        private static void StartHUDManager(HUDManager __instance)
        {
            GameObject chrono = Object.Instantiate(__instance.weightCounterAnimator.gameObject, __instance.weightCounterAnimator.transform.parent);
            chrono.transform.localPosition += new Vector3(-85f, 185f, 0f);
            chrono.name = "ChronoUI";

            chronoText = chrono.GetComponentInChildren<TextMeshProUGUI>();
            chronoText.text = "";
            chronoText.alignment = TextAlignmentOptions.BottomLeft;
            chronoText.name = "Chrono";


            GameObject distance = new GameObject("DistanceUI");
            distance.transform.localPosition = new Vector3(0f, 0f, 0f);
            distance.AddComponent<RectTransform>();

            TextMeshProUGUI textMeshDistance = distance.AddComponent<TextMeshProUGUI>();
            RectTransform rectTransformDistance = textMeshDistance.rectTransform;
            rectTransformDistance.SetParent(GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").transform, worldPositionStays: false);
            rectTransformDistance.anchorMin = new Vector2(0f, 1f);
            rectTransformDistance.anchorMax = new Vector2(0f, 1f);
            rectTransformDistance.pivot = new Vector2(0f, 1f);
            rectTransformDistance.anchoredPosition = new Vector2(30f, -50f);
            rectTransformDistance.sizeDelta = new Vector2(300f, 300f);
            textMeshDistance.alignment = TextAlignmentOptions.TopLeft;
            textMeshDistance.font = __instance.controlTipLines[0].font;
            textMeshDistance.fontSize = 14f;
            distanceText = textMeshDistance;


            GameObject curses = new GameObject("CursesUI");
            curses.transform.localPosition = new Vector3(0f, 0f, 0f);
            curses.AddComponent<RectTransform>();

            TextMeshProUGUI textMeshCurses = curses.AddComponent<TextMeshProUGUI>();
            RectTransform rectTransformCurses = textMeshCurses.rectTransform;
            rectTransformCurses.SetParent(GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").transform, worldPositionStays: false);
            rectTransformCurses.anchorMin = new Vector2(1f, 0f);
            rectTransformCurses.anchorMax = new Vector2(1f, 0f);
            rectTransformCurses.pivot = new Vector2(1f, 0f);
            rectTransformCurses.anchoredPosition = new Vector2(-30f, 50f);
            rectTransformCurses.sizeDelta = new Vector2(300f, 300f);
            textMeshCurses.alignment = TextAlignmentOptions.BottomRight;
            textMeshCurses.font = __instance.controlTipLines[0].font;
            textMeshCurses.fontSize = 14f;
            cursesText = textMeshCurses;
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
                    Paralyze.ApplyParalyze(ref playerBehaviour);
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
                            foreach (CurseEffect curseEffect in objectBehaviour.curseEffects)
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

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SetScreenFilters))]
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
            EntranceTeleport entranceTeleport = playerBehaviour.playerProperties.hoveringOverTrigger?.gameObject.GetComponent<EntranceTeleport>();
            if (entranceTeleport != null)
            {
                // EXPLORATION
                if (playerBehaviour.activeCurses.FirstOrDefault(p => p.CurseName.Equals(Constants.EXPLORATION)) != null)
                {
                    if (entranceTeleport != playerBehaviour.targetDoor)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from using this doorway.");
                        __result = false;
                    }
                    else
                    {
                        Exploration.ChangeRandomEntranceId(playerBehaviour.playerProperties.isInsideFactory, ref playerBehaviour);
                    }
                }
                // COMMUNICATION
                __result = Communication.CanEscape(ref playerBehaviour, "A curse prevents you from using this doorway.");
            }
        }
    }
}
