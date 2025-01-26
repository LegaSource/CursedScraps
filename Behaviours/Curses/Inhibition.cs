using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    public class Inhibition
    {
        public static Coroutine inhibitionCoroutine;

        public static void ApplyInhibition(bool enable, PlayerCSBehaviour playerBehaviour)
        {
            PlayerControllerB player = playerBehaviour.playerProperties;
            if (enable)
            {
                inhibitionCoroutine ??= player.StartCoroutine(InhibitionCoroutine(playerBehaviour));
            }
            else
            {
                if (inhibitionCoroutine != null)
                {
                    player.StopCoroutine(inhibitionCoroutine);
                    inhibitionCoroutine = null;
                }

                if (!string.IsNullOrEmpty(playerBehaviour.blockedAction))
                    IngamePlayerSettings.Instance.playerInput.actions.FindAction(playerBehaviour.blockedAction, false).Enable();

                playerBehaviour.blockedAction = null;
            }
        }

        public static IEnumerator InhibitionCoroutine(PlayerCSBehaviour playerBehaviour)
        {
            while (playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.INHIBITION)))
            {
                if (!string.IsNullOrEmpty(playerBehaviour.blockedAction))
                    IngamePlayerSettings.Instance.playerInput.actions.FindAction(playerBehaviour.blockedAction, false).Enable();

                string[] actions = ConfigManager.inhibitionActions.Value.Split(',').Where(a => string.IsNullOrEmpty(playerBehaviour.blockedAction) || !a.Equals(playerBehaviour.blockedAction)).ToArray();
                playerBehaviour.blockedAction = actions[new System.Random().Next(actions.Length)];
                IngamePlayerSettings.Instance.playerInput.actions.FindAction(playerBehaviour.blockedAction, false).Disable();
                if (ConfigManager.isInhibitionTip.Value) HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"One of your actions has been blocked by the {Constants.INHIBITION} curse.");

                yield return new WaitForSeconds(ConfigManager.inhibitionCooldown.Value);
            }
            inhibitionCoroutine = null;
        }
    }
}
