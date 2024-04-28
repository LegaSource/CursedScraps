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
        internal static int allowedEntrance;

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
                                if (scanElementText[1].text != null && scanElementText[0].text.ToString().Equals(Constants.CURSE_PILLS))
                                {
                                    // nodeType = 0 --> scan bleu
                                    element.GetComponent<Animator>().SetInteger("colorNumber", 0);
                                }
                                else if (scanElementText[1].text != null)
                                {
                                    string[] splitText = scanElementText[1].text.Split(new string[] { "\nCurse: " }, StringSplitOptions.None);
                                    if (splitText.Length > 1)
                                    {
                                        if (!ConfigManager.hidingMode.Value.Equals(Constants.HIDING_NEVER)
                                            && (ConfigManager.hidingMode.Value.Equals(Constants.HIDING_ALWAYS)
                                                || (ConfigManager.hidingMode.Value.Equals(Constants.HIDING_COUNTER)
                                                    && PlayerManagerPatch.curseCounter >= (GameNetworkManager.Instance.localPlayerController.IsServer || GameNetworkManager.Instance.localPlayerController.IsHost ? ConfigManager.hidingCounter.Value * 2 : ConfigManager.hidingCounter.Value))))
                                        {
                                            scanElementText[1].text = splitText[0];
                                        }
                                        else
                                        {
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
                ___drunknessFilter.weight = 1f;
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
                GameObject gameObject = new GameObject("ScanNode", typeof(ScanNodeProperties), typeof(BoxCollider));
                gameObject.layer = LayerMask.NameToLayer("ScanNode");
                gameObject.transform.localScale = Vector3.one * 1f;
                gameObject.transform.parent = entrance.gameObject.transform;
                gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                ScanNodeProperties component = gameObject.GetComponent<ScanNodeProperties>();
                // Scan bleu
                component.nodeType = 0;
                component.minRange = 1;
                component.maxRange = 9999;
                component.headerText = (entrance.entranceId == 0 ? "Main entrance" : "Fire Exit");
                component.requiresLineOfSight = false;
            }

            HUDManager.Instance.StartCoroutine(ScanCoroutine(entrance));
            
            allowedEntrance = entrance.entranceId;
        }

        private static IEnumerator ScanCoroutine(EntranceTeleport entrance)
        {
            List<ScanNodeProperties> nodesOnScreen = (List<ScanNodeProperties>)AccessTools.Field(typeof(HUDManager), "nodesOnScreen").GetValue(HUDManager.Instance);
            ScanNodeProperties node = ((Component)(object)entrance).GetComponentInChildren<ScanNodeProperties>();
            if (node != null)
            {
                if (!nodesOnScreen.Contains(node))
                {
                    nodesOnScreen.Add(node);
                }
                MethodInfo methodInfo = typeof(HUDManager).GetMethod("AssignNodeToUIElement", BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo?.Invoke(HUDManager.Instance, new object[] { node });
            }

            yield return new WaitForSeconds(ConfigManager.explorationTime.Value);

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
            UnityEngine.Object.Destroy(node.gameObject);
        }
    }
}
