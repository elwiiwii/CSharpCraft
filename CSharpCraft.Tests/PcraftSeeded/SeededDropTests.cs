using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftSeeded;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftSeeded;

// ---------------------------------------------------------------------------
// Tests for Phase 3 SeededServices drop behaviour.
// These tests MUST FAIL TO COMPILE until Green phase because:
//   - "worldSeed" named parameter does not exist (current param is "seed")
//   - "nonGenSeed" named parameter does not exist
//   - UseRelativeFacing property does not exist
// ---------------------------------------------------------------------------

public sealed class SeededServicesDropTests
{
    // --------------------------------------------------------------------------
    #region Determinism
    // --------------------------------------------------------------------------

    [Fact]
    public void OnAddItem_ProducesSameDrops_GivenSameSeedAndHarvestOrder()
    {
        List<Entity> entities1 = [];
        SeededServices svc1 = new(worldSeed: 42L);
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities1, dropChance: 1.0);

        List<Entity> entities2 = [];
        SeededServices svc2 = new(worldSeed: 42L);
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities2, dropChance: 1.0);

        entities1.Should().HaveCount(entities2.Count);

        for (int i = 0; i < entities1.Count; i++)
        {
            DroppedItemEntity d1 = (DroppedItemEntity)entities1[i];
            DroppedItemEntity d2 = (DroppedItemEntity)entities2[i];

            d1.X.Should().Be(d2.X, because: "same seed must produce same X for item {0}", i);
            d1.Y.Should().Be(d2.Y, because: "same seed must produce same Y for item {0}", i);
            d1.Type.Should().Be(d2.Type, because: "same seed must produce same Type for item {0}", i);
            d1.Timer.Should().Be(d2.Timer, because: "same seed must produce same Timer for item {0}", i);
        }
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Counter independence
    // --------------------------------------------------------------------------

    [Fact]
    public void OnAddItem_WoodHarvestIndex_IsUnaffectedBy_StoneHarvests()
    {
        // svc1: harvest Wood at index 0 for Wood
        List<Entity> entities1 = [];
        SeededServices svc1 = new(worldSeed: 42L);
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities1, dropChance: 1.0);

        // svc2: harvest Stone 3 times (indices 0,1,2 for Stone), then Wood (index 0 for Wood)
        List<Entity> stoneIgnored = [];
        SeededServices svc2 = new(worldSeed: 42L);
        PcraftServices.AddItem(PcraftData.Stone, 2, 2, F32.FromInt(32), F32.FromInt(32), stoneIgnored, dropChance: 1.0);
        PcraftServices.AddItem(PcraftData.Stone, 2, 2, F32.FromInt(32), F32.FromInt(32), stoneIgnored, dropChance: 1.0);
        PcraftServices.AddItem(PcraftData.Stone, 2, 2, F32.FromInt(32), F32.FromInt(32), stoneIgnored, dropChance: 1.0);

        List<Entity> entities2 = [];
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities2, dropChance: 1.0);

        entities1.Should().HaveCount(entities2.Count,
            because: "Wood harvest counter must be independent of Stone harvest counter");

        for (int i = 0; i < entities1.Count; i++)
        {
            DroppedItemEntity d1 = (DroppedItemEntity)entities1[i];
            DroppedItemEntity d2 = (DroppedItemEntity)entities2[i];

            d1.X.Should().Be(d2.X, because: "Wood drop X must not be affected by prior Stone harvests (item {0})", i);
            d1.Y.Should().Be(d2.Y, because: "Wood drop Y must not be affected by prior Stone harvests (item {0})", i);
            d1.Timer.Should().Be(d2.Timer, because: "Wood drop Timer must not be affected by prior Stone harvests (item {0})", i);
        }
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region nonGenSeed independence
    // --------------------------------------------------------------------------

    [Fact]
    public void OnAddItem_ProducesDifferentDrops_GivenDifferentNonGenSeeds()
    {
        List<Entity> entities1 = [];
        SeededServices svc1 = new(worldSeed: 42L, nonGenSeed: 100L);
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities1, dropChance: 1.0);

        List<Entity> entities2 = [];
        SeededServices svc2 = new(worldSeed: 42L, nonGenSeed: 200L);
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities2, dropChance: 1.0);

        entities1.Should().NotBeEmpty();
        entities2.Should().NotBeEmpty();

        DroppedItemEntity d1 = (DroppedItemEntity)entities1[0];
        DroppedItemEntity d2 = (DroppedItemEntity)entities2[0];

        bool differ = d1.X != d2.X || d1.Y != d2.Y;
        differ.Should().BeTrue(
            because: "different nonGenSeed must produce different scatter positions");
    }

    [Fact]
    public void OnAddItem_ProducesDifferentDrops_WhenNonGenSeedDiffersFromWorldSeed()
    {
        // svc1: nonGenSeed defaults to worldSeed (42L)
        List<Entity> entities1 = [];
        SeededServices svc1 = new(worldSeed: 42L);
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities1, dropChance: 1.0);

        // svc2: same worldSeed but different nonGenSeed
        List<Entity> entities2 = [];
        SeededServices svc2 = new(worldSeed: 42L, nonGenSeed: 999L);
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities2, dropChance: 1.0);

        entities1.Should().NotBeEmpty();
        entities2.Should().NotBeEmpty();

        DroppedItemEntity d1 = (DroppedItemEntity)entities1[0];
        DroppedItemEntity d2 = (DroppedItemEntity)entities2[0];

        bool differ = d1.X != d2.X || d1.Y != d2.Y;
        differ.Should().BeTrue(
            because: "nonGenSeed 999L must decouple drops from worldSeed-defaulted drops");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Drop chance
    // --------------------------------------------------------------------------

    [Fact]
    public void OnAddItem_DropsApproximately30Percent_GivenDropChance0Point3()
    {
        int nonEmptyCount = 0;

        for (int i = 0; i < 300; i++)
        {
            List<Entity> entities = [];
            SeededServices svc = new(worldSeed: (long)i);
            PcraftServices.AddItem(PcraftData.Apple, 1, 1, F32.FromInt(32), F32.FromInt(32), entities, dropChance: 0.3);
            if (entities.Count > 0)
                nonEmptyCount++;
        }

        nonEmptyCount.Should().BeInRange(60, 120,
            because: "30% drop chance should produce ~30% drop rate over 300 trials");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region UseRelativeFacing
    // --------------------------------------------------------------------------

    [Fact]
    public void OnAddItem_MirrorsDrop_WhenFacingChangesBy0Point5()
    {
        // hitX=32, hitY=32 → tileX=32/8=4 * 8+8 = ... tile centre = (tileX*8+8, tileY*8+8)
        // With tileX = floor(32/8) = 4, tileY = floor(32/8) = 4:
        // tile centre = (4*8+8, 4*8+8) = (40, 40) → mirror axis at x=40, y=40

        List<Entity> entities1 = [];
        SeededServices svc1 = new(worldSeed: 42L) { UseRelativeFacing = true };
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities1,
            playerFacing: F32.Zero, dropChance: 1.0);

        List<Entity> entities2 = [];
        SeededServices svc2 = new(worldSeed: 42L) { UseRelativeFacing = true };
        PcraftServices.AddItem(PcraftData.Wood, 2, 2, F32.FromInt(32), F32.FromInt(32), entities2,
            playerFacing: F32.FromDouble(0.5), dropChance: 1.0);

        entities1.Should().HaveCount(entities2.Count,
            because: "mirrored drop must produce same number of entities");

        for (int i = 0; i < entities1.Count; i++)
        {
            DroppedItemEntity d1 = (DroppedItemEntity)entities1[i];
            DroppedItemEntity d2 = (DroppedItemEntity)entities2[i];

            (d1.X + d2.X).Should().Be(F32.FromInt(80),
                because: "drops must mirror through tile-centre x=40 when facing changes by 0.5 (item {0})", i);
            (d1.Y + d2.Y).Should().Be(F32.FromInt(80),
                because: "drops must mirror through tile-centre y=40 when facing changes by 0.5 (item {0})", i);
        }
    }
    
    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Golden values (cross-run stability of drop timers)
    // --------------------------------------------------------------------------

    // These constants must never be changed. If any assertion fails it means
    // something introduced .NET-runtime-randomised hashing into the drop seed path.
    // nonGenSeed=99L was chosen as a neutral probe value with no game meaning.

    [Theory]
    [InlineData("sand", 114)]  // SeedMixer.Combine(99L, HashString("sand"), 0) → rng seed  1101804343
    [InlineData("iron", 125)]  // SeedMixer.Combine(99L, HashString("iron"), 0) → rng seed  -213176139
    [InlineData("gold", 120)]  // SeedMixer.Combine(99L, HashString("gold"), 0) → rng seed  1976284451
    [InlineData("gem", 128)]   // SeedMixer.Combine(99L, HashString("gem"),  0) → rng seed  2082340032
    public void OnAddItem_Timer_MatchesGoldenValue_GivenKnownNonGenSeedAndMaterial(
        string materialName, int expectedTimer)
    {
        ItemDef mat = materialName switch
        {
            "sand" => PcraftData.Sand,
            "iron" => PcraftData.Iron,
            "gold" => PcraftData.Gold,
            "gem" => PcraftData.Gem,
            _ => throw new ArgumentException(materialName)
        };

        List<Entity> entities = [];
        SeededServices svc = new(worldSeed: 1L, nonGenSeed: 99L);
        PcraftServices.AddItem(mat, 1, 1, F32.FromInt(32), F32.FromInt(32), entities, dropChance: 1.0);

        entities.Should().ContainSingle(because: "dropChance=1.0 and count range [1,1]");
        DroppedItemEntity drop = (DroppedItemEntity)entities[0];

        drop.Timer.Should().Be(F32.FromInt(expectedTimer),
            because: $"timer for '{materialName}' at nonGenSeed=99 harvestIndex=0 must be stable across process restarts");
    }

    // --------------------------------------------------------------------------
    #endregion
}
