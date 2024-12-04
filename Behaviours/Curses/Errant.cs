using CursedScraps.Managers;
using System.Linq;

namespace CursedScraps.Behaviours.Curses
{
    public class Errant
    {
        public static bool canBeTeleported = false;

        public static bool CanTeleport(PlayerCSBehaviour playerBehaviour, GrabbableObject grabbableObject, bool checkCaptive = false)
        {
            if (playerBehaviour == null) return false;

            if (!playerBehaviour.activeCurses.Any(c => c.CurseName.Equals(Constants.ERRANT))
                || ConfigManager.errantExclusions.Value.Contains(grabbableObject.itemProperties.itemName))
            {
                return false;
            }

            // Si le joueur possède la malédiction captive, l'objet ne peut pas être drop, on ne fait donc pas la tp
            if (checkCaptive && Captive.IsCaptive(playerBehaviour)) return false;

            return true;
        }

        // Téléportation après avoir attrapé un objet
        public static void PostGrabTeleport(PlayerCSBehaviour playerBehaviour, GrabbableObject grabbableObject)
        {
            if (CanTeleport(playerBehaviour, grabbableObject))
                PlayerCSManager.TeleportPlayer(ref playerBehaviour.playerProperties);
        }

        // Préparation pour la téléportation avant de déposer un objet
        public static void PreDropTeleport(PlayerCSBehaviour playerBehaviour, GrabbableObject grabbableObject)
        {
            if (CanTeleport(playerBehaviour, grabbableObject, checkCaptive: true))
                canBeTeleported = true;
        }

        // Téléportation après avoir déposé un objet
        public static void PostDropTeleport(PlayerCSBehaviour playerBehaviour)
        {
            if (canBeTeleported)
            {
                canBeTeleported = false;
                PlayerCSManager.TeleportPlayer(ref playerBehaviour.playerProperties);
            }
        }
    }
}
