using System;

namespace CursedScraps.Values
{
    public class CustomItem(Type type, Item item, bool isSpawnable, int minSpawn, int maxSpawn, int rarity, int value = 0)
    {
        public Type Type { get; internal set; } = type;
        public Item Item { get; internal set; } = item;
        public bool IsSpawnable { get; internal set; } = isSpawnable;
        public int MinSpawn { get; private set; } = minSpawn;
        public int MaxSpawn { get; private set; } = maxSpawn;
        public int Rarity { get; internal set; } = rarity;
        public int Value { get; internal set; } = value;
    }
}
