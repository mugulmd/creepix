using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncrementalTerrainBrush : TerrainBrush
{
    public override void draw(int x, int z)
    {
        for (int xi = -radius; xi <= radius; xi++)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                float height = terrain.get(x + xi, z + zi);
                terrain.set(x + xi, z + zi, height+0.05F);
            }
        }
    }
}
