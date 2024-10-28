using CursedScraps.Managers;
using System.Collections.Generic;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Exploration
    {
        public static void ApplyExploration(bool enable, ref PlayerCSBehaviour playerBehaviour)
        {
            if (enable && playerBehaviour.targetDoor == null)
            {
                ChangeRandomEntranceId(!playerBehaviour.playerProperties.isInsideFactory, ref playerBehaviour);
            }
            else if (!enable)
            {
                CustomPassManager.RemoveAuraFromDoor();
                playerBehaviour.isRendered = false;
                playerBehaviour.targetDoor = null;
            }
        }

        public static void ChangeRandomEntranceId(bool isEntrance, ref PlayerCSBehaviour playerBehaviour)
        {
            List<EntranceTeleport> entrances = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>()
                .Where(e => e.isEntranceToBuilding == isEntrance)
                .ToList();
            playerBehaviour.targetDoor = entrances[new System.Random().Next(entrances.Count)];
        }
    }
}
