using CursedScraps.Managers;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Blurry
    {
        public static bool IsBlurry(PlayerCSBehaviour playerBehaviour)
        {
            if (playerBehaviour != null && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.BLURRY)))
                return true;
            return false;
        }

        public static void UpdateScreenFilters(PlayerCSBehaviour playerBehaviour)
        {
            if (IsBlurry(playerBehaviour))
                HUDManager.Instance.drunknessFilter.weight = ConfigManager.blurryIntensity.Value;
        }
    }
}
