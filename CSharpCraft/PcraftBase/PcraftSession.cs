using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase;

internal class PcraftSession
{
    private static PcraftSession? _current;

    internal static PcraftSession Current
        => _current ?? throw new InvalidOperationException("No active PcraftSession.");

    internal static void SetCurrent(PcraftSession session) => _current = session;

    internal PlayerEntity Player { get; }
    internal Level?       Cave   { get; set; }
    internal Level?       Island { get; set; }

    internal PcraftSession(PlayerEntity player)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
    }
}
