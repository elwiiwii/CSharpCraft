using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftFilter;

namespace CSharpCraft.PcraftSeeded;

internal static class SeededDropManager
{
    internal static void AddItem(
        ItemDef mat, int minCount, int maxCount, F32 hitX, F32 hitY, List<Entity> entities,
        long nonGenSeed, Dictionary<string, int> harvestCounts,
        Dictionary<string, IDropFilter> dropFilters, bool useRelativeFacing,
        F32? playerFacing = null, double dropChance = 1.0)
    {
        int harvestIndex = harvestCounts.GetValueOrDefault(mat.Name, 0);
        harvestCounts[mat.Name] = harvestIndex + 1;

        var rng = new Random(SeedMixer.Combine(
            nonGenSeed,
            SeedMixer.HashString(mat.Name),
            harvestIndex));

        double roll = rng.NextDouble();
        dropFilters.TryGetValue(mat.Name, out IDropFilter? filter);
        if (filter is not null) roll = filter.NudgeRoll(roll, harvestIndex);
        if (roll >= dropChance) return;

        (int effMin, int effMax) = filter is not null
            ? filter.ModifyCount(minCount, maxCount, harvestIndex)
            : (minCount, maxCount);
        int count = rng.Next(effMin, effMax + 1);

        int tileX = F32.FloorToInt(hitX / F32.FromInt(16)) * 16;
        int tileY = F32.FloorToInt(hitY / F32.FromInt(16)) * 16;

        for (int k = 0; k < count; k++)
        {
            double dx = rng.NextDouble() * 14.0 - 7.0;
            double dy = rng.NextDouble() * 14.0 - 7.0;
            double vx = rng.NextDouble() * 3.0 - 1.5;
            double vy = rng.NextDouble() * 3.0 - 1.5;
            int timer = 110 + rng.Next(20);

            if (useRelativeFacing && playerFacing.HasValue)
            {
                int cardinalIndex = (int)Math.Round(playerFacing.Value.Double / 0.25) % 4;
                double snapped = cardinalIndex * 0.25;
                double cosA = Math.Cos(-2.0 * Math.PI * snapped);
                double sinA = Math.Sin(-2.0 * Math.PI * snapped);
                (dx, dy) = (dx * cosA - dy * sinA, dx * sinA + dy * cosA);
                (vx, vy) = (vx * cosA - vy * sinA, vx * sinA + vy * cosA);
            }

            F32 ex = F32.FromInt(tileX + 8) + F32.FromDouble(dx);
            F32 ey = F32.FromInt(tileY + 8) + F32.FromDouble(dy);
            entities.Add(new DroppedItemEntity(mat, ex, ey, F32.FromInt(timer), F32.FromDouble(vx), F32.FromDouble(vy)));
        }
    }
}
