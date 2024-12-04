using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Captive
    {
        public static bool IsCaptive(PlayerCSBehaviour playerBehaviour, bool isMessage = false)
        {
            if (playerBehaviour != null && playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.CAPTIVE)))
            {
                if (isMessage)
                    HUDManager.Instance.DisplayTip(Constants.IMPOSSIBLE_ACTION, "A curse prevents you to do this action.");
                return true;
            }
            return false;
        }
    }
}
