using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace CursedScraps.Patches
{
    internal class HUDManagerPatch
    {
        private static GameObject chrono;
        private static TextMeshProUGUI chronoText;
        internal static bool forceEndChrono = false;
        internal static int allowedEntrance;
        public static int timeOut = 5;

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPostfix]
        private static void Start(HUDManager __instance)
        {
            if (chrono == null)
            {
                chrono = UnityEngine.Object.Instantiate(__instance.weightCounterAnimator.gameObject, __instance.weightCounterAnimator.transform.parent);
                chrono.transform.localPosition += new Vector3(-85f, 185f, 0f);
                chrono.name = "ChronoUI";

                chronoText = chrono.GetComponentInChildren<TextMeshProUGUI>();
                chronoText.text = "";
                chronoText.alignment = TextAlignmentOptions.BottomLeft;
                chronoText.name = "Chrono";
            }
        }

        [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
        [HarmonyPostfix]
        private static void ScanManager(ref Dictionary<RectTransform, ScanNodeProperties>  ___scanNodes, ref RectTransform[] ___scanElements)
        {
            if (___scanElements != null)
            {
                foreach (RectTransform element in ___scanElements)
                {
                    try
                    {
                        if (___scanNodes != null && (___scanNodes.TryGetValue(element, out var value) && value.creatureScanID != -1))
                        {
                            PlayerManagerPatch.Paralyze(ref GameNetworkManager.Instance.localPlayerController);
                        }
                        else
                        {
                            TextMeshProUGUI[] scanElementText = element.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
                            if (scanElementText != null && scanElementText.Length > 1)
                            {
                                if (scanElementText[0].text != null && scanElementText[0].text.ToString().Equals(Constants.CURSE_PILLS))
                                {
                                    // nodeType = 0 --> scan bleu
                                    element.GetComponent<Animator>().SetInteger("colorNumber", 0);
                                }
                                else if (scanElementText[1].text != null)
                                {
                                    string[] splitText = scanElementText[1].text.Split(new string[] { "\nCurse: " }, StringSplitOptions.None);
                                    if (splitText.Length > 1)
                                    {
                                        if (ConfigManager.hidingMode.Value.Equals(Constants.HIDING_ALWAYS)
                                            || ((ConfigManager.hidingMode.Value.Equals(Constants.HIDING_COUNTER) || ConfigManager.hidingMode.Value.Equals(Constants.HIDING_COUNTER_NOT_NAMED))
                                                && PlayerManagerPatch.curseCounter >= (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost ? ConfigManager.hidingCounter.Value * 2 : ConfigManager.hidingCounter.Value)))
                                        {
                                            scanElementText[1].text = splitText[0];
                                        }
                                        else
                                        {
                                            if (ConfigManager.hidingMode.Value.Equals(Constants.HIDING_NEVER_NOT_NAMED) || ConfigManager.hidingMode.Value.Equals(Constants.HIDING_COUNTER_NOT_NAMED))
                                            {
                                                scanElementText[1].text = scanElementText[1].text.Substring(0, scanElementText[1].text.IndexOf("\nCurse: ")) + "\nCurse: ???";
                                            }
                                            // nodeType = 1 --> scan rouge
                                            element.GetComponent<Animator>().SetInteger("colorNumber", 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception arg)
                    {
                        Debug.LogError($"Error in ScanManager: {arg}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "SetScreenFilters")]
        [HarmonyPostfix]
        private static void UpdateScreenFilters(ref Volume ___drunknessFilter)
        {
            if (PlayerManagerPatch.activeCurses.Count > 0 && PlayerManagerPatch.activeCurses.Contains(Constants.BLURRY))
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
            if (PlayerManagerPatch.activeCurses.Count > 0 && PlayerManagerPatch.activeCurses.Contains(Constants.EXPLORATION))
            {
                InteractTrigger interactTrigger = GameNetworkManager.Instance.localPlayerController.hoveringOverTrigger;
                if (interactTrigger == null)
                {
                    return;
                }
                EntranceTeleport entranceTeleport = interactTrigger.gameObject.GetComponent<EntranceTeleport>();
                if (entranceTeleport != null)
                {
                    if (entranceTeleport.entranceId != allowedEntrance)
                    {
                        HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from using this doorway.");
                        __result = false;
                    }
                    else
                    {
                        ChangeRandomEntranceId(GameNetworkManager.Instance.localPlayerController.isInsideFactory);
                    }
                }
            }
        }

        internal static void ChangeRandomEntranceId(bool isEntrance)
        {
            List<EntranceTeleport> entrances = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>()
                .Where(e => e.isEntranceToBuilding == isEntrance)
                .ToList();

            EntranceTeleport entrance = entrances[new System.Random().Next(entrances.Count)];
            if (((Component)(object)entrance).GetComponentInChildren<ScanNodeProperties>() == null)
            {
                CreateScanNode(entrance.gameObject.transform, entrance.entranceId == 0 ? "Main entrance" : "Fire Exit");
            }
            HUDManager.Instance.StartCoroutine(ScanExplorationCoroutine(entrance));
            allowedEntrance = entrance.entranceId;
        }

        internal static void CreateScanNode(Transform parent, string headerText)
        {
            GameObject gameObject = new GameObject("ScanNode", typeof(ScanNodeProperties), typeof(BoxCollider));
            gameObject.layer = LayerMask.NameToLayer("ScanNode");
            gameObject.transform.localScale = Vector3.one * 1f;
            gameObject.transform.parent = parent;
            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            ScanNodeProperties component = gameObject.GetComponent<ScanNodeProperties>();
            // Scan bleu
            component.nodeType = 0;
            component.minRange = 1;
            component.maxRange = int.MaxValue;
            component.headerText = headerText;
            component.requiresLineOfSight = false;
        }

        private static IEnumerator ScanExplorationCoroutine(EntranceTeleport entrance)
        {
            // Forcer l'apparition du scan à l'écran si le joueur regarde dans la direction de la porte
            ScanNodeProperties node = ((Component)(object)entrance).GetComponentInChildren<ScanNodeProperties>();
            ForceAddNodeOnScreen(ref node);

            yield return new WaitForSeconds(ConfigManager.explorationTime.Value);

            // Suppression du scan de la porte et de la possibilité de la scanner
            DestroyScanNode(ref node);
        }

        private static void ForceAddNodeOnScreen(ref ScanNodeProperties node)
        {
            List<ScanNodeProperties> nodesOnScreen = (List<ScanNodeProperties>)AccessTools.Field(typeof(HUDManager), "nodesOnScreen").GetValue(HUDManager.Instance);
            if (node != null)
            {
                if (!nodesOnScreen.Contains(node))
                {
                    nodesOnScreen.Add(node);
                }
                MethodInfo methodInfo = typeof(HUDManager).GetMethod("AssignNodeToUIElement", BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo?.Invoke(HUDManager.Instance, new object[] { node });
            }
        }

        internal static void DestroyScanNode(ref ScanNodeProperties node)
        {
            List<ScanNodeProperties> nodesOnScreen = (List<ScanNodeProperties>)AccessTools.Field(typeof(HUDManager), "nodesOnScreen").GetValue(HUDManager.Instance);
            if (nodesOnScreen.Contains(node))
            {
                Dictionary<RectTransform, ScanNodeProperties> scanNodes = (Dictionary<RectTransform, ScanNodeProperties>)AccessTools.Field(typeof(HUDManager), "scanNodes").GetValue(HUDManager.Instance);
                nodesOnScreen.Remove(node);
                for (int i = 0; i < HUDManager.Instance.scanElements.Length; i++)
                {
                    if (scanNodes.Count > 0 && scanNodes.TryGetValue(HUDManager.Instance.scanElements[i], out var value) && value != null && value == node)
                    {
                        scanNodes.Remove(HUDManager.Instance.scanElements[i]);
                    }
                }
            }
            UnityEngine.Object.Destroy(node);
        }

        internal static IEnumerator StartTrackedScrapCoroutine()
        {
            int timePassed = 0;
            while (PlayerManagerPatch.trackedScrap == null)
            {
                yield return new WaitForSeconds(1f);
                timePassed++;

                if (timePassed >= timeOut) break;
            }

            while (IsTrackedEnded() != null)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        private static GrabbableObject IsTrackedEnded()
        {
            if (PlayerManagerPatch.communicationPlayer != null && PlayerManagerPatch.trackedScrap != null)
            {
                chronoText.text = Math.Round(Vector3.Distance(PlayerManagerPatch.communicationPlayer.transform.position, PlayerManagerPatch.trackedScrap.transform.position), 1).ToString();
            }
            else
            {
                chronoText.text = "";
            }
            return PlayerManagerPatch.trackedScrap;
        }

        internal static IEnumerator StartChronoCoroutine(int seconds)
        {
            while (!IsChronoEnded(seconds))
            {
                seconds--;
                yield return new WaitForSeconds(1f);
            }
        }

        private static bool IsChronoEnded(int totalSeconds)
        {
            int minutes = (int)Math.Floor(totalSeconds / 60.0);
            int seconds = (int)Math.Floor(totalSeconds % 60.0);

            chronoText.text = $"{minutes:D2}:{seconds:D2}";

            if (forceEndChrono || (minutes == 0 && seconds == 0))
            {
                if (!forceEndChrono)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                }
                chronoText.text = "";
                return true;
            }
            return false;
        }
    }
}
