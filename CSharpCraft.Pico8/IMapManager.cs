namespace CSharpCraft.Pico8;

/// <summary>
/// Defines map data management for tile and flag access.
/// Provides bounds-checked access to the Pico-8 map and flag data.
/// </summary>
public interface IMapManager
{
    /// <summary>
    /// Gets the sprite number at map cell (celx, cely).
    /// Returns 0 if the cell is out of bounds.
    /// </summary>
    int Mget(double celx, double cely);

    /// <summary>
    /// Sets the sprite number at map cell (celx, cely).
    /// Does nothing if the cell is out of bounds.
    /// </summary>
    void Mset(double celx, double cely, double snum = 0);

    /// <summary>
    /// Gets the flag bits for sprite number n.
    /// Returns 0 if the sprite is out of bounds.
    /// </summary>
    int Fget(int n);
}
