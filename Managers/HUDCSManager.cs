using CursedScraps.Behaviours;
using CursedScraps.Patches;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CursedScraps.Managers
{
    public class HUDCSManager
    {
        public static int timeOut = 5;
        //public static bool forceEndChrono = false;

        public static TextMeshProUGUI CreateUIElement(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, TextAlignmentOptions alignment)
        {
            Transform parent = GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").transform;
            GameObject uiObject = new GameObject(name);
            uiObject.transform.localPosition = Vector3.zero;

            RectTransform rectTransform = uiObject.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, worldPositionStays: false);
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(300f, 300f);

            TextMeshProUGUI textMesh = uiObject.AddComponent<TextMeshProUGUI>();
            textMesh.alignment = alignment;
            textMesh.font = HUDManager.Instance.controlTipLines[0].font;
            textMesh.fontSize = 14f;

            return textMesh;
        }

        public static IEnumerator StartTrackedItemCoroutine(PlayerCSBehaviour playerBehaviour)
        {
            int timePassed = 0;
            while (playerBehaviour.trackedItem == null)
            {
                yield return new WaitForSeconds(1f);
                timePassed++;

                if (timePassed >= timeOut) break;
            }

            while (IsTrackedEnded(ref playerBehaviour))
                yield return new WaitForSeconds(1f);
        }

        private static bool IsTrackedEnded(ref PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour.trackedItem != null)
                HUDManagerPatch.distanceText.text = Math.Round(Vector3.Distance(playerBehaviour.playerProperties.transform.position, playerBehaviour.trackedItem.transform.position), 1).ToString();
            else
                HUDManagerPatch.distanceText.text = "";
            return playerBehaviour.trackedItem != null;
        }

        /*public static IEnumerator StartChronoCoroutine(int seconds)
        {
            // Reset du chrono si déjà utilisé avant: si fait après avec l'attente de 1 seconde on risque de ne jamais quitter cette coroutine
            forceEndChrono = false;

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

            HUDManagerPatch.chronoText.text = $"{minutes:D2}:{seconds:D2}";

            if (forceEndChrono || (minutes == 0 && seconds == 0))
            {
                if (!forceEndChrono)
                {
                    GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero, spawnBody: true, CauseOfDeath.Unknown);
                }
                HUDManagerPatch.chronoText.text = "";
                return true;
            }
            return false;
        }*/

        public static void RefreshCursesText(ref PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour != null)
            {
                string cursesName = null;
                foreach (string curseName in playerBehaviour.activeCurses.Select(c => c.CurseName))
                {
                    if (!string.IsNullOrEmpty(cursesName))
                        cursesName += "\n";
                    cursesName += curseName;
                }

                if (!string.IsNullOrEmpty(cursesName))
                    HUDManagerPatch.cursesText.text = cursesName;
                else
                    HUDManagerPatch.cursesText.text = "";
            }
        }
    }
}
