namespace CursedScraps.Patches
{
    internal class MenuManagerPatch
    {
        private static void AwakePatch()
        {
            PlayerManagerPatch.activeCurses = null;
            PlayerManagerPatch.actionsBlockedBy = null;
            PlayerManagerPatch.immunedPlayers = null;
            PlayerManagerPatch.hasCoopCurseActive = false;
        }
    }
}
