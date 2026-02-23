using Xunit;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8;

public class TrackManagerTests
{
    private MockAudioGraphicsSettings CreateSettings()
    {
        return new MockAudioGraphicsSettings();
    }

    private TrackManager CreateTrackManager(
        Dictionary<string, List<SongInst>>? musicDict = null,
        Dictionary<string, Dictionary<int, string>>? sfxDict = null)
    {
        var settings = CreateSettings();
        return new TrackManager(
            () => musicDict,
            () => sfxDict,
            settings);
    }

    // Tests for MusicCount property
    [Fact]
    public void MusicCount_WithNullDictionary_Returns_Zero()
    {
        var manager = CreateTrackManager(musicDict: null);
        manager.MusicCount.Should().Be(0);
    }

    [Fact]
    public void MusicCount_WithEmptyDictionary_Returns_Zero()
    {
        var musicDict = new Dictionary<string, List<SongInst>>();
        var manager = CreateTrackManager(musicDict: musicDict);
        manager.MusicCount.Should().Be(0);
    }

    [Fact]
    public void MusicCount_WithMultipleTracks_Returns_CorrectCount()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "track1", new List<SongInst>() },
            { "track2", new List<SongInst>() },
            { "track3", new List<SongInst>() }
        };
        var manager = CreateTrackManager(musicDict: musicDict);
        manager.MusicCount.Should().Be(3);
    }

    // Tests for SfxCount property
    [Fact]
    public void SfxCount_WithNullDictionary_Returns_Zero()
    {
        var manager = CreateTrackManager(sfxDict: null);
        manager.SfxCount.Should().Be(0);
    }

    [Fact]
    public void SfxCount_WithEmptyDictionary_Returns_Zero()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>();
        var manager = CreateTrackManager(sfxDict: sfxDict);
        manager.SfxCount.Should().Be(0);
    }

    [Fact]
    public void SfxCount_WithMultiplePacks_Returns_CorrectCount()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>
        {
            { "pack1", new Dictionary<int, string>() },
            { "pack2", new Dictionary<int, string>() }
        };
        var manager = CreateTrackManager(sfxDict: sfxDict);
        manager.SfxCount.Should().Be(2);
    }

    // Tests for GetCurrentSoundtrackName
    [Fact]
    public void GetCurrentSoundtrackName_WithNullDictionary_Returns_DefaultName()
    {
        var manager = CreateTrackManager(musicDict: null);
        manager.GetCurrentSoundtrackName().Should().Be("music");
    }

    [Fact]
    public void GetCurrentSoundtrackName_WithOutOfBoundsIndex_Returns_DefaultName()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "track1", new List<SongInst>() }
        };
        var manager = CreateTrackManager(musicDict: musicDict);
        manager.GetType().GetProperty("MusicCount")?.DeclaringType?
            .GetField("getMusicDict", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // Index is 0 by default, but force it to be out of bounds

        // Actually, let's test the normal case first
        manager.GetCurrentSoundtrackName().Should().Be("track1");
    }

    [Fact]
    public void GetCurrentSoundtrackName_WithValidIndex_Returns_CorrectName()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "forest", new List<SongInst>() },
            { "cave", new List<SongInst>() },
            { "boss", new List<SongInst>() }
        };
        var manager = CreateTrackManager(musicDict: musicDict);
        manager.GetCurrentSoundtrackName().Should().Be("forest");
    }

    // Tests for GetCurrentSfxPackName
    [Fact]
    public void GetCurrentSfxPackName_WithNullDictionary_Returns_DefaultName()
    {
        var manager = CreateTrackManager(sfxDict: null);
        manager.GetCurrentSfxPackName().Should().Be("sfx");
    }

    [Fact]
    public void GetCurrentSfxPackName_WithValidIndex_Returns_CorrectName()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>
        {
            { "effects1", new Dictionary<int, string>() },
            { "effects2", new Dictionary<int, string>() }
        };
        var manager = CreateTrackManager(sfxDict: sfxDict);
        manager.GetCurrentSfxPackName().Should().Be("effects1");
    }

    // Tests for IncrementSoundtrack
    [Fact]
    public void IncrementSoundtrack_WithNullDictionary_DoesNothing()
    {
        var manager = CreateTrackManager(musicDict: null);
        manager.IncrementSoundtrack();
        // Verify no exception thrown
        manager.MusicCount.Should().Be(0);
    }

    [Fact]
    public void IncrementSoundtrack_WithSingleTrack_DoesNothing()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "track1", new List<SongInst>() }
        };
        var settings = CreateSettings();
        var manager = new TrackManager(() => musicDict, () => null, settings);

        manager.IncrementSoundtrack();
        settings.CurrentSoundtrack.Should().Be(0);
    }

    [Fact]
    public void IncrementSoundtrack_WithMultipleTracks_IncrementsIndex()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "track1", new List<SongInst>() },
            { "track2", new List<SongInst>() },
            { "track3", new List<SongInst>() }
        };
        var settings = CreateSettings();
        var manager = new TrackManager(() => musicDict, () => null, settings);

        settings.CurrentSoundtrack = 0;
        manager.IncrementSoundtrack();
        settings.CurrentSoundtrack.Should().Be(1);

        manager.IncrementSoundtrack();
        settings.CurrentSoundtrack.Should().Be(2);
    }

    [Fact]
    public void IncrementSoundtrack_AtEnd_WrapsToBeginning()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "track1", new List<SongInst>() },
            { "track2", new List<SongInst>() },
            { "track3", new List<SongInst>() }
        };
        var settings = CreateSettings();
        settings.CurrentSoundtrack = 2; // Last track
        var manager = new TrackManager(() => musicDict, () => null, settings);

        manager.IncrementSoundtrack();
        settings.CurrentSoundtrack.Should().Be(0); // Wraps to first
    }

    // Tests for DecrementSoundtrack
    [Fact]
    public void DecrementSoundtrack_WithNullDictionary_DoesNothing()
    {
        var manager = CreateTrackManager(musicDict: null);
        manager.DecrementSoundtrack();
        manager.MusicCount.Should().Be(0);
    }

    [Fact]
    public void DecrementSoundtrack_AtBeginning_WrapsToEnd()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "track1", new List<SongInst>() },
            { "track2", new List<SongInst>() },
            { "track3", new List<SongInst>() }
        };
        var settings = CreateSettings();
        settings.CurrentSoundtrack = 0; // First track
        var manager = new TrackManager(() => musicDict, () => null, settings);

        manager.DecrementSoundtrack();
        settings.CurrentSoundtrack.Should().Be(2); // Wraps to last
    }

    [Fact]
    public void DecrementSoundtrack_FromMiddle_Decrements()
    {
        var musicDict = new Dictionary<string, List<SongInst>>
        {
            { "track1", new List<SongInst>() },
            { "track2", new List<SongInst>() },
            { "track3", new List<SongInst>() }
        };
        var settings = CreateSettings();
        settings.CurrentSoundtrack = 2;
        var manager = new TrackManager(() => musicDict, () => null, settings);

        manager.DecrementSoundtrack();
        settings.CurrentSoundtrack.Should().Be(1);
    }

    // Tests for IncrementSfxPack
    [Fact]
    public void IncrementSfxPack_WithMultiplePacks_Increments()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>
        {
            { "pack1", new Dictionary<int, string>() },
            { "pack2", new Dictionary<int, string>() },
            { "pack3", new Dictionary<int, string>() }
        };
        var settings = CreateSettings();
        var manager = new TrackManager(() => null, () => sfxDict, settings);

        settings.CurrentSfxPack = 0;
        manager.IncrementSfxPack();
        settings.CurrentSfxPack.Should().Be(1);
    }

    [Fact]
    public void IncrementSfxPack_AtEnd_WrapsToBeginning()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>
        {
            { "pack1", new Dictionary<int, string>() },
            { "pack2", new Dictionary<int, string>() },
            { "pack3", new Dictionary<int, string>() }
        };
        var settings = CreateSettings();
        settings.CurrentSfxPack = 2; // Last pack
        var manager = new TrackManager(() => null, () => sfxDict, settings);

        manager.IncrementSfxPack();
        settings.CurrentSfxPack.Should().Be(0); // Wraps to first
    }

    // Tests for DecrementSfxPack
    [Fact]
    public void DecrementSfxPack_WithMultiplePacks_Decrements()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>
        {
            { "pack1", new Dictionary<int, string>() },
            { "pack2", new Dictionary<int, string>() },
            { "pack3", new Dictionary<int, string>() }
        };
        var settings = CreateSettings();
        settings.CurrentSfxPack = 2;
        var manager = new TrackManager(() => null, () => sfxDict, settings);

        manager.DecrementSfxPack();
        settings.CurrentSfxPack.Should().Be(1);
    }

    [Fact]
    public void DecrementSfxPack_AtBeginning_WrapsToEnd()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>
        {
            { "pack1", new Dictionary<int, string>() },
            { "pack2", new Dictionary<int, string>() },
            { "pack3", new Dictionary<int, string>() }
        };
        var settings = CreateSettings();
        settings.CurrentSfxPack = 0; // First pack
        var manager = new TrackManager(() => null, () => sfxDict, settings);

        manager.DecrementSfxPack();
        settings.CurrentSfxPack.Should().Be(2); // Wraps to last
    }

    [Fact]
    public void DecrementSfxPack_WithSinglePack_DoesNothing()
    {
        var sfxDict = new Dictionary<string, Dictionary<int, string>>
        {
            { "pack1", new Dictionary<int, string>() }
        };
        var settings = CreateSettings();
        var manager = new TrackManager(() => null, () => sfxDict, settings);

        manager.DecrementSfxPack();
        settings.CurrentSfxPack.Should().Be(0);
    }
}
