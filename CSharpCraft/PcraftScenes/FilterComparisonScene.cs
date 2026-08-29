using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftFilter;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftSeeded;
using PSharp8.Graphics;
using PSharp8.Input;
using PSharp8.Scene;

namespace CSharpCraft.PcraftScenes;

internal class FilterComparisonScene : PcraftSceneBase
{
    public override string Name => "Filter Comparison";

    // Layout constants
    // Resolution: 4px margin | 512px map | 4px gap | 512px map | 4px margin = 1036px wide
    //             10px header | 512px map | 10px footer = 532px tall
    private const int MarginX = 4;
    private const int GapX = 4;
    private const int MapPx = 64 * 14;   // 64 tiles × 14px/tile
    private const int HeaderY = 10;
    private const int FooterY = HeaderY + MapPx;
    private const int Left0X = MarginX;
    private const int Right0X = MarginX + MapPx + GapX;

    private readonly FilterSet _filters = new([
        new TileCountFilter(TileId: (int)TileId.Rock, MinimumCount: 80),
        new TileCountFilter(TileId: (int)TileId.Sand, MinimumCount: 80),
    ]);
    private long _seed;
    private SampleResult? _baseline;
    private SampleResult? _filtered;
    private bool _dirty = true;
    private bool _previewDirty = true;

    public override void Init(ISceneSetup setup)
    {
        // Don't call base.Init() — no game session, music, or player needed.
        setup.Resolution = ((MapPx * 2) + 12, FooterY + 10);
        _seed = 0L;
        _dirty = true;
        _previewDirty = true;
    }

    public override void Update()
    {
        if (_dirty) Regenerate();

        if (Pico8.Btnp(PicoButton.Primary) || Pico8.Btnp(PicoButton.Secondary))
        {
            _seed = Random.Shared.NextInt64();
            _dirty = true;
        }
    }

    public override void Draw()
    {
        if (_baseline is null || _filtered is null) return;
        if (!_previewDirty) return;

        Pico8.Cls(PicoColor._13Lavender);

        // Panel labels
        Pico8.Print("baseline", Left0X, 2, PicoColor._07White);
        Pico8.Print("filtered", Right0X, 2, PicoColor._07White);

        // Maps
        FilterPreviewDrawer.Draw(_baseline, Left0X, HeaderY);
        FilterPreviewDrawer.Draw(_filtered, Right0X, HeaderY);

        // Footer: seed value + hint
        Pico8.Print($"seed:{_seed.ToString("X16").ToLower()}  x:new", Left0X, FooterY, PicoColor._06LightGrey);

        _previewDirty = false;
    }

    private void Regenerate()
    {
        _baseline = PcraftWorldSampler.Sample(_seed, radius: 32,
            forceCenterX: 32, forceCenterY: 32);
        _filtered = FilteredWorldSampler.Sample(_seed, radius: 32, _filters,
            forceCenterX: 32, forceCenterY: 32);
        _dirty = false;
        _previewDirty = true;
    }
}
