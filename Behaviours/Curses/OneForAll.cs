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
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.ONE_FOR_ALL))) return false;
            return true;
        }

        public static void ApplyOneForAll(bool enable, PlayerCSBehaviour playerBehaviour)
        {
            if (!enable) return;
            if (!ConfigManager.isOneForAllInfoOn.Value) return;
            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController) return;
            
            HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"{playerBehaviour.playerProperties.playerUsername} has been afflicted by the {Constants.ONE_FOR_ALL} curse, defend them if you can!");
        }

        public static void KillPlayer(PlayerCSBehaviour playerBehaviour)
        {
            if (!IsOneForAll(playerBehaviour)) return;

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (!player.isPlayerControlled) continue;
                if (player.isPlayerDead) continue;
                if (player == playerBehaviour.playerProperties) continue;

                CursedScrapsNetworkManager.Instance.KillPlayerServerRpc((int)player.playerClientId, Vector3.zero, true, (int)CauseOfDeath.Unknown);
            }
        }
    }
}
