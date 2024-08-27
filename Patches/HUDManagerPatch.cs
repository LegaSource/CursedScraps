using CursedScraps.Behaviours;
using CursedScraps.Managers;
using HarmonyLib;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace CursedScraps.Patches
{
    internal class HUDManagerPatch
    {
        public static GameObject chrono;
        public static TextMeshProUGUI chronoText;

        [HarmonyPatch(typeof(HUDManager), "Start")]
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
            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            if (playerBehaviour != null && playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.PARALYSIS)) != null)
            {
                for (int i = 0; i < __instance.scanElements.Length; i++)
                {
                    if (__instance.scanNodes.TryAdd(__instance.scanElements[i], node) && node.nodeType == 1)
                    {
                        CSPlayerManager.Paralyze(ref playerBehaviour);
                        break;
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
    }
}
