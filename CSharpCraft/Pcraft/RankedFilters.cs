using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Pcraft
{
    public class RankedFilters
    {
        public List<DensityCheck> Surface1Checks { get; } = [
            new() {
                Radius = (1, 5),
                Tiles = [0],
                Density = (0.1, 1)
            },

            new() {
                Radius = (1, 5),
                Tiles = [1],
                Density = (0, 5)
            },

            new() {
                Radius = (1, 5),
                Tiles = [3],
                Density = (30, 40)
            },

            new() {
                Radius = (1, 5),
                Tiles = [4],
                Density = (25, 40)
            }
            ];

        public List<DensityComparison> Surface1Comps { get; } = [
            
            ];



        public List<DensityCheck> Surface2Checks { get; } = [
            new() {
                Radius = (1, 5),
                Tiles = [0],
                Density = (0.1, 1)
            },

            new() {
                Radius = (1, 5),
                Tiles = [1],
                Density = (0, 5)
            },

            new() {
                Radius = (1, 4),
                Tiles = [3],
                Density = (10, 15)
            },

            new() {
                Radius = (1, 5),
                Tiles = [4],
                Density = (25, 40)
            }
            ];

        public List<DensityComparison> Surface2Comps { get; } = [
        
            ];



        public List<DensityCheck> Surface3Checks { get; } = [
            new() {
                Radius = (1, 5),
                Tiles = [0],
                Density = (0.1, 1)
            },

            new() {
                Radius = (1, 5),
                Tiles = [1],
                Density = (0, 5)
            },

            new() {
                Radius = (1, 5),
                Tiles = [3],
                Density = (0, 1)
            },

            new() {
                Radius = (1, 5),
                Tiles = [4],
                Density = (25, 40)
            }
            ];

        public List<DensityComparison> Surface3Comps { get; } = [
            
            ];



        public List<DensityCheck> Surface4Checks { get; } = [
            new() {
                Radius = (1, 6),
                Tiles = [0],
                Density = (0.1, 7)
            },

            new() {
                Radius = (1, 6),
                Tiles = [1],
                Density = (21, 100)
            },

            new() {
                Radius = (1, 5),
                Tiles = [4],
                Density = (22, 40)
            }
            ];

        public List<DensityComparison> Surface4Comps { get; } = [
            
            ];



        public List<DensityCheck> Surface5Checks { get; } = [
            
            ];

        public List<DensityComparison> Surface5Comps { get; } = [
            
            ];



        public List<DensityCheck> Cave1Checks { get; } = [
            new() {
                Radius = (1, 5),
                Tiles = [1],
                Density = (65, 100)
            },

            new() {
                Radius = (1, 2),
                Tiles = [1],
                Density = (40, 85)
            },

            new() {
                Radius = (1, 3),
                Tiles = [9],
                Density = (0, 0)
            },

            new() {
                Radius = (1, 5),
                Tiles = [3],
                Density = (4, 100)
            }
            ];

        public List<DensityComparison> Cave1Comps { get; } = [
            
            ];



        public List<DensityCheck> Cave2Checks { get; } = [
            
            ];

        public List<DensityComparison> Cave2Comps { get; } = [
            ];



        public List<DensityCheck> Cave3Checks { get; } = [
            
            ];

        public List<DensityComparison> Cave3Comps { get; } = [
            
            ];



        public List<DensityCheck> Cave4Checks { get; } = [
            
            ];

        public List<DensityComparison> Cave4Comps { get; } = [
            
            ];



        public List<DensityCheck> Cave5Checks { get; } = [
            
            ];

        public List<DensityComparison> Cave5Comps { get; } = [
            
            ];
    }
}
