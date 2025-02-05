using CursedScraps.Managers;
using GameNetcodeStuff;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Sacrifice
    {
        public static bool IsSacrifice(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.SACRIFICE))) return false;
            return true;
        }

        public static PlayerControllerB GetSacrificePlayer()
            => StartOfRound.Instance.allPlayerScripts.FirstOrDefault(p => IsSacrifice(p.GetComponent<PlayerCSBehaviour>()));

        public static void ApplySacrifice(bool enable, PlayerCSBehaviour playerBehaviour)
        {
            if (!enable) return;
            if (!ConfigManager.isSacrificeInfoOn.Value) return;
            if (playerBehaviour.playerProperties == GameNetworkManager.Instance.localPlayerController) return;
            
            HUDManager.Instance.DisplayTip(Constants.IMPORTANT_INFORMATION, $"{playerBehaviour.playerProperties.playerUsername} has been afflicted by the {Constants.SACRIFICE} curse!");
        }

        public static bool KillPlayer(PlayerCSBehaviour playerBehaviour)
        {
            PlayerControllerB sacrificePlayer = GetSacrificePlayer();
            if (sacrificePlayer == null) return false;
            if (IsSacrifice(playerBehaviour)) return false;

            SwapPlayersPositions(playerBehaviour.playerProperties, sacrificePlayer);
            return true;
        }

        public static void SwapPlayersPositions(PlayerControllerB localPlayer, PlayerControllerB sacrificePlayer)
        {
            CursedScrapsNetworkManager.Instance.TeleportPlayerServerRpc((int)sacrificePlayer.playerClientId, localPlayer.transform.position, localPlayer.isInElevator, localPlayer.isInHangarShipRoom, localPlayer.isInsideFactory);
            PlayerCSManager.TeleportPlayer(localPlayer, sacrificePlayer.transform.position, sacrificePlayer.isInElevator, sacrificePlayer.isInHangarShipRoom, sacrificePlayer.isInsideFactory);
        }

        public static bool DamagePlayer(PlayerCSBehaviour playerBehaviour, int damageNumber)
        {
            PlayerControllerB sacrificePlayer = GetSacrificePlayer();
            if (sacrificePlayer == null) return false;
            if (IsSacrifice(playerBehaviour)) return false;

            CursedScrapsNetworkManager.Instance.DamagePlayerServerRpc((int)sacrificePlayer.playerClientId, damageNumber);
            return true;
        }
    }
}
