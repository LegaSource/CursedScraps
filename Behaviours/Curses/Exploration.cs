using CursedScraps.Managers;
using System.Collections.Generic;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Exploration
    {
        public static void ApplyExploration(bool enable, PlayerCSBehaviour playerBehaviour)
        {
            if (!enable)
            {
                CustomPassManager.RemoveAuraFromDoor();
                playerBehaviour.isRendered = false;
                playerBehaviour.targetDoor = null;
                return;
            }

            if (playerBehaviour.targetDoor != null) return;
            ChangeRandomEntranceId(!playerBehaviour.playerProperties.isInsideFactory, playerBehaviour);
        }

        public static bool IsExploration(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.EXPLORATION))) return false;
            return true;
        }

        public static bool EntranceInteraction(PlayerCSBehaviour playerBehaviour, EntranceTeleport entranceTeleport)
        {
            if (!IsExploration(playerBehaviour)) return true;

            if (entranceTeleport != playerBehaviour.targetDoor)
            {
                HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you from using this doorway.");
                return false;
            }
            ChangeRandomEntranceId(playerBehaviour.playerProperties.isInsideFactory, playerBehaviour);
            return true;
        }

        public static void ChangeRandomEntranceId(bool isEntrance, PlayerCSBehaviour playerBehaviour)
        {
            List<EntranceTeleport> entrances = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>()
                .Where(e => e.isEntranceToBuilding == isEntrance)
                .ToList();
            playerBehaviour.targetDoor = entrances[new System.Random().Next(entrances.Count)];
        }
    }
}
