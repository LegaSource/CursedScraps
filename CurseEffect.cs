namespace CursedScraps
{
    public class CurseEffect
    {
        public string Name { get; private set; }
        public float Multiplier { get; private set; }
        public string Weight { get; private set; }

        public CurseEffect(string name, float multiplier, string weight)
        {
            this.Name = name;
            this.Multiplier = multiplier;
            this.Weight = weight;
        }
    }
}
