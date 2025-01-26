using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Linq;
using UnityEngine;

namespace CursedScraps.Behaviours.Curses
{
    public class OneForAll
    {
        public static bool IsOneForAll(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour != null && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.ONE_FOR_ALL)))
                return true;
            return false;
        }

        public static void ApplyOneForAll(bool enable, PlayerCSBehaviour playerBehaviour)
        {
            if (enable && ConfigManager.isOneForAllInfoOn.Value && playerBehaviour.playerProperties != GameNetworkManager.Instance.localPlayerController)
                HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"{playerBehaviour.playerProperties.playerUsername} has been afflicted by the {Constants.ONE_FOR_ALL} curse, defend them if you can!");
        }

        public static void KillPlayer(PlayerCSBehaviour playerBehaviour)
        {
            if (IsOneForAll(playerBehaviour))
            {
                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts.Where(p => p.isPlayerControlled && !p.isPlayerDead))
                    CursedScrapsNetworkManager.Instance.KillPlayerServerRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
            }
        }
    }
}
