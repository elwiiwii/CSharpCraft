namespace CSharpCraft.Pcraft;

public class Ground
{
    public int Gr { get; set; }
    public int Id { get; set; }
    public bool IsTree { get; set; }
    public int Life { get; set; }
    public Material? Mat { get; set; }
    public int[]? Pal { get; set; }
    public Ground? Tile { get; set; }
    public int MinedCount { get; set; } = 0;
    public (int a, int b) DroppedCount { get; set; } = (0, 0);
}
