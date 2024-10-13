using CursedScraps.Managers;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    internal class Paralyze
    {
        public static void ApplyParalyze(ref PlayerCSBehaviour playerBehaviour)
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
    }
}
