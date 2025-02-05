using CursedScraps.Managers;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    public class Paralysis
    {
        public static void ApplyParalysis(PlayerCSBehaviour playerBehaviour)
        {
            CurseEffect curseEffect = playerBehaviour.activeCurses.FirstOrDefault(c => c.CurseName.Equals(Constants.PARALYSIS));
            if (curseEffect == null) return;

            playerBehaviour.playerProperties.JumpToFearLevel(0.6f);
            playerBehaviour.playerProperties.StartCoroutine(ParalyzeCoroutine(curseEffect));
        }

        public static IEnumerator ParalyzeCoroutine(CurseEffect curseEffect)
        {
            PlayerCSManager.EnablePlayerActions(curseEffect, false);
            yield return new WaitForSeconds(ConfigManager.paralysisTime.Value);
            PlayerCSManager.EnablePlayerActions(curseEffect, true);
        }

        public static bool IsParalysis(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.PARALYSIS))) return false;
            return true;
        }

        public static void ScanPerformed(PlayerCSBehaviour playerBehaviour, ScanNodeProperties scanNodeProperties)
        {
            if (!IsParalysis(playerBehaviour)) return;
            if (scanNodeProperties.nodeType != 1) return;
            
            ApplyParalysis(playerBehaviour);
        }
    }
}
