namespace CursedScraps.Behaviours.Curses
{
    public class CurseEffect(string curseName, float multiplier, string weight)
    {
        public string CurseName { get; private set; } = curseName;
        public float Multiplier { get; private set; } = multiplier;
        public string Weight { get; private set; } = weight;
    }
}
