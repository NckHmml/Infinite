namespace Infinite.Mathematics
{
    /// <summary>
    /// SimplexNoise implementation using one coordinate as seed
    /// </summary>
    public class SeededNoise: INoise
    {
        private double Seed { get; set; }
        private double Factor { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="SeededNoise"/>
        /// </summary>
        public SeededNoise(double seed, double factor)
        {
            Seed = seed;
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
            double noise = SimplexNoise.Generate(x / Factor, y / Factor, Seed);
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
            double noise = SimplexNoise.Generate(x / Factor, y / Factor, Seed);
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
            double noise = SimplexNoise.Generate(x / Factor, y / Factor, z / Factor, Seed);
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
            double noise = SimplexNoise.Generate(x / Factor, y / Factor, z / Factor, Seed);
            noise += 1;
            noise *= 0.5;
            return noise;
        }
    }
}
