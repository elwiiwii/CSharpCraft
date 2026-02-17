namespace CSharpCraft.Pico8;

/// <summary>
/// Utility functions for Pico8 engine.
/// Provides common mathematical and utility functions.
/// </summary>
public static class Pico8MathUtils
{
    /// <summary>
    /// Loop value using modulo with proper handling of negative numbers.
    /// Equivalent to Lua's modulo operation.
    /// </summary>
    public static int Loop(int value, int count)
    {
        if (count <= 0) return 0;
        return ((value % count) + count) % count;
    }

    /// <summary>
    /// Loop value within a list's bounds.
    /// </summary>
    public static int Loop<T>(int value, List<T> list)
    {
        return Loop(value, list.Count);
    }
}
