using CursedScraps.Behaviours;
using CursedScraps.Patches;
using System;
using System.Collections;
using UnityEngine;

namespace CursedScraps.Managers
{
    internal class CSHUDManager
    {
        public static int timeOut = 5;
        public static bool forceEndChrono = false;

        public static IEnumerator StartTrackedScrapCoroutine()
        {
            PlayerCSBehaviour playerBehaviour = GameNetworkManager.Instance.localPlayerController.GetComponent<PlayerCSBehaviour>();
            int timePassed = 0;
            while (playerBehaviour.trackedScrap == null)
            {
                yield return new WaitForSeconds(1f);
                timePassed++;

                if (timePassed >= timeOut) break;
            }

            while (IsTrackedEnded(ref playerBehaviour))
            {
                yield return new WaitForSeconds(1f);
            }
        }

        private static bool IsTrackedEnded(ref PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour.coopPlayer != null && playerBehaviour.trackedScrap != null)
            {
                HUDManagerPatch.chronoText.text = Math.Round(Vector3.Distance(playerBehaviour.coopPlayer.transform.position, playerBehaviour.trackedScrap.transform.position), 1).ToString();
            }
            else
            {
                HUDManagerPatch.chronoText.text = "";
            }
            return playerBehaviour.trackedScrap != null;
        }

        public static IEnumerator StartChronoCoroutine(int seconds)
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
        }
    }
}
