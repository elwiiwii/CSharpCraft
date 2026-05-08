using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase;

internal class PcraftSession
{
    internal static PcraftSession Current { get => field ?? throw new InvalidOperationException("No active PcraftSession."); private set; }

    internal static void SetCurrent(PcraftSession session)
    {
        Current = session;
    }

    internal PlayerEntity Player { get; }
    internal Level? Cave { get; set; }
    internal Level? Island { get; set; }

    internal PcraftSession(PlayerEntity player)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
    }
}
