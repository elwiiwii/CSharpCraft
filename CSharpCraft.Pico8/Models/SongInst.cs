namespace CSharpCraft.Pico8.Models;

/// <summary>
/// Represents a song definition: a list of named tracks with loop settings.
/// </summary>
public record SongInst(List<(string name, bool loop)> Tracks, int Group);
