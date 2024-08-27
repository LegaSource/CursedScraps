namespace CursedScraps.Behaviours
{
    internal class CurseEffect
    {
        public string CurseName { get; private set; }
        public float Multiplier { get; private set; }
        public string Weight { get; private set; }
        public bool IsCoop { get; private set; }

        public CurseEffect(string curseName, float multiplier, string weight, bool isCoop)
        {
            this.CurseName = curseName;
            this.Multiplier = multiplier;
            this.Weight = weight;
            this.IsCoop = isCoop;
        }
    }
}
