using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace CursedScraps.Patches
{
    internal class HUDManagerPatch
    {
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
                                        // nodeType = 1 --> scan rouge
                                        element.GetComponent<Animator>().SetInteger("colorNumber", 1);
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
        private static void UpdateScreenFilters(ref Volume ___drunknessFilter, ref Volume ___flashbangScreenFilter)
        {
            if (PlayerManagerPatch.activeCurses.Contains(Constants.BLURRY))
            {
                ___drunknessFilter.weight = 1f;
            }
        }
    }
}
