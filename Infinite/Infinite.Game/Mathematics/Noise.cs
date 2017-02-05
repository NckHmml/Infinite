using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Mathematics
{
    /// <summary>
    /// SimplexNoise implementation
    /// </summary>
    public class Noise : INoise
    {
        private double Factor { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="Noise"/>
        /// </summary>
        public Noise(double factor)
        {
            Factor = factor;
        }

        /// <summary>
        /// Generates a 2D SimplexNoise
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Noise value ranging 0 to 1</returns>
        public double Generate(long x, long y)
        {
            double noise = SimplexNoise.Generate(x / Factor, y / Factor);
            noise += 1;
            noise *= 0.5;
            return noise;
        }

        /// <summary>
        /// Generates a 2D SimplexNoise
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>Noise value ranging 0 to 1</returns>
        public double Generate(double x, double y)
        {
            double noise = SimplexNoise.Generate(x / Factor, y / Factor);
            noise += 1;
            noise *= 0.5;
            return noise;
        }

        /// <summary>
        /// Generates a 3D SimplexNoise
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <returns>Noise value ranging 0 to 1</returns>
        public double Generate(long x, long y, long z)
        {
            double noise = SimplexNoise.Generate(x / Factor, y / Factor, z / Factor);
            noise += 1;
            noise *= 0.5;
            return noise;
        }

        /// <summary>
        /// Generates a 3D SimplexNoise
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <returns>Noise value ranging 0 to 1</returns>
        public double Generate(double x, double y, double z)
        {
            double noise = SimplexNoise.Generate(x / Factor, y / Factor, z / Factor);
            noise += 1;
            noise *= 0.5;
            return noise;
        }
    }
}
