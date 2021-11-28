using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothBrush : TerrainBrush
{
    public override void draw(int x, int z)
    {
        for (int xi = -radius; xi <= radius; xi++)
        {
            // compute average height in neighborhood
            for (int zi = -radius; zi <= radius; zi++)
            {
                float avg = 0;
                for(int i = -1; i <= 1; i++)
                {
                    for(int j = -1; j <= 1; j++)
                    {
                        avg += terrain.get(x + xi + i, z + zi + j);
                    }
                }
                avg /= 9;

                terrain.set(x + xi, z + zi, avg);
            }
        }
    }
}
