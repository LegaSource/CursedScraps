namespace CursedScraps.Behaviours.Curses
{
    public class Deafness
    {
        public static float savedMasterVolume = 0f;

        public static void ApplyDeafness(bool enable)
        {
            if (enable)
            {
                savedMasterVolume = IngamePlayerSettings.Instance.settings.masterVolume == 0f ? savedMasterVolume : IngamePlayerSettings.Instance.settings.masterVolume;
                IngamePlayerSettings.Instance.ChangeMasterVolume(0);
            }
            else if (savedMasterVolume != 0f)
            {
                IngamePlayerSettings.Instance.ChangeMasterVolume((int)(savedMasterVolume * 100));
            }
        }
    }
}
