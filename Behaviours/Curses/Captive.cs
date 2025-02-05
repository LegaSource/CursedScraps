using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Captive
    {
        public static bool IsCaptive(PlayerCSBehaviour playerBehaviour, bool isMessage = false)
        {
            if (playerBehaviour == null) return false;
            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.CAPTIVE))) return false;

            if (!isMessage) return true;
            HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you to do this action.");
            return true;
        }
    }
}
