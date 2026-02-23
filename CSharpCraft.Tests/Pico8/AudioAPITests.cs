using Xunit;
using FluentAssertions;
using CSharpCraft.Pico8;
using Moq;

namespace CSharpCraft.Tests.Pico8;

public class AudioAPITests
{
    [Fact]
    public void AudioAPI_Implements_IAudioAPI_Interface()
    {
        // Verify AudioAPI implements IAudioAPI contract
        typeof(AudioAPI).Should().Implement(typeof(IAudioAPI));
    }

    [Fact]
    public void IAudioAPI_Has_Sfx_Method()
    {
        // Verify Sfx method exists with correct parameters
        var method = typeof(IAudioAPI).GetMethod("Sfx", new[] { typeof(double), typeof(double), typeof(double), typeof(double) });
        method.Should().NotBeNull();
    }

    [Fact]
    public void IAudioAPI_Has_Music_Method()
    {
        // Verify Music method exists with correct parameters
        var method = typeof(IAudioAPI).GetMethod("Music", new[] { typeof(int), typeof(double) });
        method.Should().NotBeNull();
    }

    [Fact]
    public void IAudioAPI_Has_Mute_Method()
    {
        // Verify Mute method exists
        var method = typeof(IAudioAPI).GetMethod("Mute", System.Type.EmptyTypes);
        method.Should().NotBeNull();
    }

    [Fact]
    public void IAudioAPI_Interface_Has_Seven_Methods()
    {
        // Verify the interface has exactly 7 public methods (Sfx, Music, Mute, Pause, Resume, StopAll, Update)
        var methods = typeof(IAudioAPI).GetMethods(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
        );
        
        methods.Should().HaveCount(7);
    }

    [Fact]
    public void AudioAPI_Is_Public()
    {
        // Verify AudioAPI is public and accessible
        typeof(AudioAPI).IsPublic.Should().BeTrue();
    }
}
