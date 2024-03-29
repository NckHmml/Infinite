﻿using Infinite.Mathematics;
using SiliconStudio.Core.Mathematics;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Infinite.Terrain
{
    public static class TerrainGenerator
    {
        const double seed = 2001;
        const int maxHeight = 60;
        const double valleyGap = 0.2;
        private static INoise HeightNoise1 { get; } = new SeededNoise(seed, 125);
        private static INoise HeightNoise2 { get; } = new Noise(25);
        private static INoise CaveNoise1 { get; } = new SeededNoise(seed, 15);
        private static INoise CaveNoise2 { get; } = new Noise(15);
        private static INoise CaveNoise3 { get; } = new SeededNoise(seed, 250);
        private static INoise TreeNoise1 { get; } = new SeededNoise(seed, 50);
        private static INoise TreeNoise2 { get; } = new SeededNoise(seed, 5);

        public static Chunk GenerateChunk(GenericVector3<int> chunkPosition)
        {
            // Variables
            int iY, maxY;
            long diff, diff1, diff2, diff3, diff4, diffY;
            long cX, cY, cZ;
            double noise;
            // Blocks buffer
            var blocks = new Dictionary<GenericVector3<byte>, Block>();
            var entities = new Dictionary<GenericVector3<byte>, EntitySpawn>();

            long chunkBottom = chunkPosition.Y * Chunk.Size;
            long chunkCeiling = chunkBottom + Chunk.Size;

            var heightMap = new HeightMap<int>(Chunk.Size);
            var treeMap = new HeightMap<bool>(Chunk.Size);
            // There is no surface below 0
            if (chunkBottom >= 0 && chunkBottom <= maxHeight)
            {
                // Generate the height map
                heightMap.Fill((x, z) =>
                {
                    // Variables for readabilty 
                    cX = chunkPosition.X * Chunk.Size + x;
                    cZ = chunkPosition.Z * Chunk.Size + z;

                    noise = .93 * HeightNoise1.Generate(cX, cZ);
                    noise += .07 * HeightNoise2.Generate(cX, cZ);

                    // Flatten valley
                    noise -= valleyGap;
                    noise *= 1 / (1 - valleyGap);
                    noise = noise < 0 ? 0 : noise;

                    return (int)(1 + noise * maxHeight);
                });


                // Generate the tree maps
                treeMap.Fill((x, z) =>
                {
                    // No tree checking around chunk borders
                    if (x < 0 || z < 0 || x >= Chunk.Size || z >= Chunk.Size)
                        return false;

                    maxY = heightMap[x, z];
                    if (maxY == 1)
                        return false;
                    // Variables for readabilty 
                    cX = chunkPosition.X * Chunk.Size + x;
                    cZ = chunkPosition.Z * Chunk.Size + z;

                    noise = TreeNoise2.Generate(cX, cZ);
                    noise = Math.Round(noise, 1);
                    return noise == 1;
                });

                // Combine nearby possibilities to a single spawn point
                for (int x = 0; x < Chunk.Size; x++)
                {
                    for (int z = 0; z < Chunk.Size; z++)
                    {
                        if (treeMap[x, z])
                        {
                            var vectors = new List<Vector2>();
                            vectors.Add(Vector2.Zero);
                            if (treeMap[x - 1, z])
                                vectors.Add(new Vector2(-1, 0));
                            if (treeMap[x - 1, z - 1])
                                vectors.Add(new Vector2(-1, -1));
                            if (treeMap[x, z - 1])
                                vectors.Add(new Vector2(0, -1));
                            if (treeMap[x + 1, z])
                                vectors.Add(new Vector2(1, 0));
                            if (treeMap[x + 1, z + 1])
                                vectors.Add(new Vector2(1, 1));
                            if (treeMap[x, z + 1])
                                vectors.Add(new Vector2(0, 1));
                            if (treeMap[x + 1, z - 1])
                                vectors.Add(new Vector2(1, -1));
                            if (treeMap[x - 1, z + 1])
                                vectors.Add(new Vector2(-1, 1));

                            foreach (Vector2 vector in vectors)
                                treeMap[x - (int)vector.X, z - (int)vector.Y] = false;

                            Vector2 average = vectors.Aggregate((a, b) => a + b) / vectors.Count;
                            treeMap[x - (int)Math.Round(average.X), z - (int)Math.Round(average.Y)] = true;
                        }
                    }
                }
            }



            // Generate cave map
            var caveMap = new HeightMap3D<bool>(Chunk.Size);
            caveMap.Fill((x, y, z) =>
            {
                // Get surface height
                maxY = heightMap[x, z];
                if (chunkBottom + y > maxY)
                    return false;

                // Variables for readabilty 
                cX = chunkPosition.X * Chunk.Size + x;
                cY = chunkPosition.Y * Chunk.Size + y;
                cZ = chunkPosition.Z * Chunk.Size + z;

                // Combine 2 3D noises to make caves
                noise = Math.Round(CaveNoise1.Generate(cX, cY, cZ));
                noise *= Math.Round(CaveNoise2.Generate(cX, cY, cZ));
                // The deeper, the more common
                double pow = (cY + 128d) / 128d;
                if (pow < 0.0)
                    pow = 0;
                if (pow > 1.0)
                    pow = 1;
                pow = 1 - pow;

                // Powing makes for a more solid 'border'
                noise *= Math.Pow(CaveNoise3.Generate(cX, cZ), 2 - pow);

                return noise > 0.8;
            });

            // Generate the surface
            for (int x = 0; x < Chunk.Size; x++)
            {
                for (int z = 0; z < Chunk.Size; z++)
                {
                    // Get surface height
                    maxY = heightMap[x, z];

                    #region Surface
                    // There is no surface below 0
                    if (chunkBottom >= 0 && chunkBottom <= maxHeight)
                    {
                        if (maxY <= chunkCeiling && maxY >= chunkBottom)
                        {
                            // Create surface blocks
                            iY = (int)(maxY - chunkBottom);
                            if (!caveMap[x, iY, z])
                            {
                                var position = new GenericVector3<byte>((byte)x, (byte)iY, (byte)z);

                                if (maxY == 1)
                                {
                                    AddSide(blocks, position, GetSides(position, heightMap, chunkBottom, Block.Adjecent.Top), Block.MaterialType.Soil);
                                    // ToDo: Figure out why alpha channels don't blend
                                    var above = new GenericVector3<byte>((byte)x, (byte)(iY + 1), (byte)z);
                                    AddSide(blocks, above, Block.Adjecent.Top, Block.MaterialType.Water);
                                }
                                else
                                {
                                    AddSide(blocks, position, GetSides(position, heightMap, chunkBottom, Block.Adjecent.Top), Block.MaterialType.Grass);
                                }
                                if (caveMap[x - 1, iY, z])
                                    AddSide(blocks, position, Block.Adjecent.Left);
                                if (caveMap[x + 1, iY, z])
                                    AddSide(blocks, position, Block.Adjecent.Right);
                                if (caveMap[x, iY - 1, z])
                                    AddSide(blocks, position, Block.Adjecent.Bottom);
                                if (iY < Chunk.Size && caveMap[x, iY + 1, z])
                                    AddSide(blocks, position, Block.Adjecent.Top);
                                if (caveMap[x, iY, z - 1])
                                    AddSide(blocks, position, Block.Adjecent.Back);
                                if (caveMap[x, iY, z + 1])
                                    AddSide(blocks, position, Block.Adjecent.Front);

                                if (treeMap[x, z])
                                {
                                    cX = chunkPosition.X * Chunk.Size + x;
                                    cZ = chunkPosition.Z * Chunk.Size + z;

                                    // ToDo: check if there aren't trees nearby?
                                    entities.Add(position, new EntitySpawn()
                                    {
                                        Rotation = (float)TreeNoise1.Generate(cX, cZ) * MathUtil.PiOverTwo,
                                        Type = EntitySpawn.EntityType.Tree
                                    });
                                }
                            }
                        }

                        // Calculate adjecent heights
                        diff1 = maxY - heightMap[x - 1, z] - 1;
                        diff2 = maxY - heightMap[x + 1, z] - 1;
                        diff3 = maxY - heightMap[x, z - 1] - 1;
                        diff4 = maxY - heightMap[x, z + 1] - 1;
                        // Merge diffs to get the heighest
                        diff = Math.Max(Math.Max(diff1, diff2), Math.Max(diff3, diff4));
                        // Get the difference between the top and max
                        diffY = Math.Min(maxY, chunkCeiling + 1);
                        diff -= maxY - diffY;

                        for (int i = 1; i <= diff; i++)
                        {
                            iY = (int)(diffY - chunkBottom - i);
                            if (iY > 0 && !caveMap[x, iY, z])
                            {
                                var position = new GenericVector3<byte>((byte)x, (byte)iY, (byte)z);
                                Block.MaterialType material = i < 3 ? Block.MaterialType.Grass : Block.MaterialType.Stone;
                                AddSide(blocks, position, GetSides(position, heightMap, chunkBottom), material);
                                if (caveMap[x - 1, iY, z])
                                    AddSide(blocks, position, Block.Adjecent.Left);
                                if (caveMap[x + 1, iY, z])
                                    AddSide(blocks, position, Block.Adjecent.Right);
                                if (caveMap[x, iY - 1, z])
                                    AddSide(blocks, position, Block.Adjecent.Bottom);
                                if (iY < Chunk.Size && caveMap[x, iY + 1, z])
                                    AddSide(blocks, position, Block.Adjecent.Top);
                                if (caveMap[x, iY, z - 1])
                                    AddSide(blocks, position, Block.Adjecent.Back);
                                if (caveMap[x, iY, z + 1])
                                    AddSide(blocks, position, Block.Adjecent.Front);
                            }
                        }
                    }
                    #endregion

                    // Render cave
                    for (int y = 0; y < Chunk.Size && y + chunkBottom < maxY; y++)
                    {
                        var position = new GenericVector3<byte>((byte)x, (byte)y, (byte)z);

                        if (caveMap[x, y, z])
                            AddSide(blocks, position, Block.Adjecent.None);
                        if (!caveMap[x, y, z] && caveMap[x - 1, y, z])
                            AddSide(blocks, position, Block.Adjecent.Left);
                        if (!caveMap[x, y, z] && caveMap[x + 1, y, z])
                            AddSide(blocks, position, Block.Adjecent.Right);
                        if (!caveMap[x, y, z] && caveMap[x, y - 1, z])
                            AddSide(blocks, position, Block.Adjecent.Bottom);
                        if (!caveMap[x, y, z] && caveMap[x, y + 1, z])
                            AddSide(blocks, position, Block.Adjecent.Top);
                        if (!caveMap[x, y, z] && caveMap[x, y, z - 1])
                            AddSide(blocks, position, Block.Adjecent.Back);
                        if (!caveMap[x, y, z] && caveMap[x, y, z + 1])
                            AddSide(blocks, position, Block.Adjecent.Front);
                    }
                }
            }

            return new Chunk(chunkPosition, blocks, entities);
        }

        private static void AddSide(Dictionary<GenericVector3<byte>, Block> blocks, GenericVector3<byte> position, Block.Adjecent side, Block.MaterialType material = Block.MaterialType.Stone)
        {
            Block block;
            if (blocks.ContainsKey(position))
            {
                block = blocks[position];
                block.Sides |= side;
                blocks[position] = block;
            }
            else
            {
                block = new Block
                {
                    Material = material,
                    Sides = side
                };
                blocks.Add(position, block);
            }
        }

        private static Block.Adjecent GetSides(GenericVector3<byte> position, HeightMap<int> heightMap, long chunkBottom, Block.Adjecent sides = Block.Adjecent.None)
        {
            // Get sides
            if (position.Y > heightMap[position.X - 1, position.Z] - chunkBottom)
                sides |= Block.Adjecent.Left;
            if (position.Y > heightMap[position.X + 1, position.Z] - chunkBottom)
                sides |= Block.Adjecent.Right;
            if (position.Y > heightMap[position.X, position.Z + 1] - chunkBottom)
                sides |= Block.Adjecent.Front;
            if (position.Y > heightMap[position.X, position.Z - 1] - chunkBottom)
                sides |= Block.Adjecent.Back;

            return sides;
        }
    }
}
