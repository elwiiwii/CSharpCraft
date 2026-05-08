using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftPreview;
using PSharp8.Audio;
using PSharp8.Scene;

namespace CSharpCraft.PcraftFilter;

internal class PcraftFilterBase : PcraftSceneBase, IScene
{
    public override string? Name => "Pcraft Filter Base";

    // Layout constants
    // Resolution: 4px margin | 512px map | 4px gap | 512px map | 4px margin = 1036px wide
    //             10px header | 512px map | 10px footer = 532px tall
    private const int MarginX  = 4;
    private const int GapX     = 4;
    private const int MapPx    = 64 * 14;   // 64 tiles × 12px/tile
    private const int HeaderY  = 10;
    private const int FooterY  = HeaderY + MapPx;   // HeaderY + MapPx = 10 + 512
    private const int Left0X   = MarginX;
    private const int Right0X  = MarginX + MapPx + GapX;  // 520

    public override void Init(ISceneSetup setup)
    {
        // Don't call base.Init() — no game session, music, or player needed.
        setup.Resolution = (MapPx * 2 + 12, FooterY + 10);

        var filters = new FilterSet([
            new TileCountFilter(TileId: 3, MinimumCount: 80),
            new TileCountFilter(TileId: 4, MinimumCount: 80),
        ]);

        long          seed         = 0L;
        SampleResult? baseline     = null;
        SampleResult? filtered     = null;
        bool          dirty        = true;
        bool          previewDirty = true;

        void Regenerate()
        {
            baseline     = PcraftWorldSampler.Sample(seed, radius: 32,
                forceCenterX: 32, forceCenterY: 32);
            filtered     = FilteredWorldSampler.Sample(seed, radius: 32, filters,
                forceCenterX: 32, forceCenterY: 32);
            dirty        = false;
            previewDirty = true;
        }

        setup.RegisterUpdate(() =>
        {
            if (dirty) Regenerate();

            if (Pico8.Btnp(4) || Pico8.Btnp(5))
            {
                seed  = Random.Shared.NextInt64();
                dirty = true;
            }
        }, fps: 30);

        setup.RegisterDraw(() =>
        {
            if (baseline is null || filtered is null) return;
            if (!previewDirty) return;

            Pico8.Cls(13);

            // Panel labels
            Pico8.Print("baseline", Left0X,  2, 7);
            Pico8.Print("filtered", Right0X, 2, 7);

            // Maps
            FilterPreviewDrawer.Draw(baseline, Left0X,  HeaderY);
            FilterPreviewDrawer.Draw(filtered, Right0X, HeaderY);

            // Footer: seed value + hint
            Pico8.Print($"seed:{seed.ToString("X16").ToLower()}  x:new", Left0X, FooterY, 6);

            previewDirty = false;
        }, fps: 30);
    }
}

