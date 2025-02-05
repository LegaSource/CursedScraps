using CursedScraps.Managers;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Blurry
    {
        public static bool IsBlurry(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.BLURRY))) return false;
            return true;
        }

        public static void UpdateScreenFilters(PlayerCSBehaviour playerBehaviour)
        {
            if (!IsBlurry(playerBehaviour)) return;
            HUDManager.Instance.drunknessFilter.weight = ConfigManager.blurryIntensity.Value;
        }
    }
}
