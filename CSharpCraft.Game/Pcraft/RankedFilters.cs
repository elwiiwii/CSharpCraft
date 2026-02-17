using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Pcraft;

public static class RankedFilters
{
    private static List<DensityCheck> Surface1Checks { get; } = [
        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [0],
            Density = (0.1, 1)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [1],
            Density = (0, 5)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [3],
            Density = (30, 40)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [4],
            Density = (25, 40)
        }
        ];

    private static List<DensityComparison> Surface1Comps { get; } = [

        ];



    private static List<DensityCheck> Surface2Checks { get; } = [
        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [0],
            Density = (0.1, 1)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [1],
            Density = (0, 5)
        },

        new() {
            IsCave = false,
            Radius = (1, 4),
            Tiles = [3],
            Density = (10, 15)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [4],
            Density = (25, 40)
        }
        ];

    private static List<DensityComparison> Surface2Comps { get; } = [

        ];



    private static List<DensityCheck> Surface3Checks { get; } = [
        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [0],
            Density = (0.1, 1)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [1],
            Density = (0, 5)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [3],
            Density = (0, 1)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [4],
            Density = (25, 40)
        }
        ];

    private static List<DensityComparison> Surface3Comps { get; } = [

        ];



    private static List<DensityCheck> Surface4Checks { get; } = [
        new() {
            IsCave = false,
            Radius = (1, 6),
            Tiles = [0],
            Density = (0.1, 7)
        },

        new() {
            IsCave = false,
            Radius = (1, 6),
            Tiles = [1],
            Density = (21, 100)
        },

        new() {
            IsCave = false,
            Radius = (1, 5),
            Tiles = [4],
            Density = (22, 40)
        }
        ];

    private static List<DensityComparison> Surface4Comps { get; } = [

        ];



    private static List<DensityCheck> Surface5Checks { get; } = [

        ];

    private static List<DensityComparison> Surface5Comps { get; } = [

        ];



    private static List<DensityCheck> Cave1Checks { get; } = [
        new() {
            IsCave = true,
            Radius = (1, 4),
            Tiles = [1],
            Density = (65, 100)
        },

        new() {
            IsCave = true,
            Radius = (1, 7),
            Tiles = [1],
            Density = (62, 100)
        },

        new() {
            IsCave = true,
            Radius = (1, 2),
            Tiles = [1],
            Density = (40, 85)
        },

        new() {
            IsCave = true,
            Radius = (1, 3),
            Tiles = [9],
            Density = (0, 0.1)
        },

        new() {
            IsCave = true,
            Radius = (1, 5),
            Tiles = [3],
            Density = (2, 100)
        }
        ];

    private static List<DensityComparison> Cave1Comps { get; } = [

        ];



    private static List<DensityCheck> Cave2Checks { get; } = [
        new() {
            IsCave = true,
            Radius = (1, 5),
            Tiles = [1],
            Density = (45, 50)
        },

        new() {
            IsCave = true,
            Radius = (5, 7),
            Tiles = [1],
            Density = (0, 10)
        },

        new() {
            IsCave = true,
            Radius = (1, 2),
            Tiles = [1],
            Density = (40, 85)
        },

        new() {
            IsCave = true,
            Radius = (1, 3),
            Tiles = [9],
            Density = (0, 0.1)
        },

        new() {
            IsCave = true,
            Radius = (1, 5),
            Tiles = [3],
            Density = (2, 100)
        }
        ];

    private static List<DensityComparison> Cave2Comps { get; } = [
        new() {
            IsCave = true,
            Radius1 = (1, 5),
            Tiles1 = [10],
            Radius2 = (1, 5),
            Tiles2 = [3, 8],
            Mag = 15,
            Opr = "<"
        }
        ];



    private static List<DensityCheck> Cave3Checks { get; } = [
        new() {
            IsCave = true,
            Radius = (1, 4),
            Tiles = [1],
            Density = (32, 37)
        },

        new() {
            IsCave = true,
            Radius = (4, 6),
            Tiles = [1],
            Density = (0, 10)
        },

        new() {
            IsCave = true,
            Radius = (1, 2),
            Tiles = [1],
            Density = (40, 85)
        },

        new() {
            IsCave = true,
            Radius = (1, 3),
            Tiles = [9],
            Density = (0, 0.1)
        },

        new() {
            IsCave = true,
            Radius = (1, 5),
            Tiles = [3],
            Density = (2, 100)
        }
        ];

    private static List<DensityComparison> Cave3Comps { get; } = [
        new() {
            IsCave = true,
            Radius1 = (1, 5),
            Tiles1 = [10],
            Radius2 = (1, 5),
            Tiles2 = [3, 8],
            Mag = 15,
            Opr = "<"
        }
        ];



    private static List<DensityCheck> Cave4Checks { get; } = [
        new() {
            IsCave = true,
            Radius = (1, 7),
            Tiles = [1],
            Density = (0, 10)
        },

        new() {
            IsCave = true,
            Radius = (1, 3),
            Tiles = [10],
            Density = (8, 40)
        },

        new() {
            IsCave = true,
            Radius = (1, 5),
            Tiles = [9],
            Density = (0, 15)
        }
        ];

    private static List<DensityComparison> Cave4Comps { get; } = [

        ];



    private static List<DensityCheck> Cave5Checks { get; } = [

        ];

    private static List<DensityComparison> Cave5Comps { get; } = [

        ];



    public static List<List<DensityCheck>> RankedSurfaceChecks { get; } = [
        Surface1Checks,
        Surface2Checks,
        Surface3Checks,
        Surface4Checks,
        Surface5Checks
    ];

    public static List<List<DensityComparison>> RankedSurfaceComps { get; } = [
        Surface1Comps,
        Surface2Comps,
        Surface3Comps,
        Surface4Comps,
        Surface5Comps
    ];

    public static List<List<DensityCheck>> RankedCaveChecks { get; } = [
        Cave1Checks,
        Cave2Checks,
        Cave3Checks,
        Cave4Checks,
        Cave5Checks
    ];

    public static List<List<DensityComparison>> RankedCaveComps { get; } = [
        Cave1Comps,
        Cave2Comps,
        Cave3Comps,
        Cave4Comps,
        Cave5Comps
    ];
}
