namespace CursedScraps.Behaviours.Curses
{
    public class CurseEffect
    {
        public string CurseName { get; private set; }
        public float Multiplier { get; private set; }
        public string Weight { get; private set; }

        public CurseEffect(string curseName, float multiplier, string weight)
        {
            CurseName = curseName;
            Multiplier = multiplier;
            Weight = weight;
        }
    }
}
