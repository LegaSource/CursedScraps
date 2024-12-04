using CursedScraps.Managers;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    public class Paralysis
    {
        public static void ApplyParalysis(ref PlayerCSBehaviour playerBehaviour)
        {
            CurseEffect curseEffect = playerBehaviour.activeCurses.Where(c => c.CurseName.Equals(Constants.PARALYSIS)).FirstOrDefault();
            if (curseEffect != null)
            {
                playerBehaviour.playerProperties.JumpToFearLevel(0.6f);
                playerBehaviour.playerProperties.StartCoroutine(ParalyzeCoroutine(curseEffect));
            }
        }

        public static IEnumerator ParalyzeCoroutine(CurseEffect curseEffect)
        {
            PlayerCSManager.EnablePlayerActions(ref curseEffect, false);
            yield return new WaitForSeconds(ConfigManager.paralysisTime.Value);
            PlayerCSManager.EnablePlayerActions(ref curseEffect, true);
        }

        public static bool IsParalysis(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour != null && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.PARALYSIS)))
                return true;
            return false;
        }

        public static void ScanPerformed(PlayerCSBehaviour playerBehaviour, ScanNodeProperties scanNodeProperties)
        {
            if (IsParalysis(playerBehaviour) && scanNodeProperties.nodeType == 1)
                ApplyParalysis(ref playerBehaviour);
        }
    }
}
