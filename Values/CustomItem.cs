using System;

namespace CursedScraps.Values
{
    public class CustomItem
    {
        public bool IsEnabled { get; internal set; }
        public Type Type { get; internal set; }
        public Item Item { get; internal set; }
        public bool IsSpawnable { get; internal set; }
        public int MinSpawn { get; private set; }
        public int MaxSpawn { get; private set; }
        public int Rarity { get; internal set; }

        public CustomItem(bool isEnabled, Type type, Item item, bool isSpawnable, int minSpawn, int maxSpawn, int rarity)
        {
            IsEnabled = isEnabled;
            Type = type;
            Item = item;
            IsSpawnable = isSpawnable;
            MaxSpawn = maxSpawn;
            Rarity = rarity;
        }
    }
}
