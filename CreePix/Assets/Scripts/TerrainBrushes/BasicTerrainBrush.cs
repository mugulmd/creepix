using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTerrainBrush : TerrainBrush
{
    public override void draw(int x, int z)
    {
        for(int xi = -radius; xi <= radius; xi++)
        {
            for(int zi = -radius; zi <= radius; zi++)
            {
                terrain.set(x+xi, z+zi, 10);
            }
        }
    }
}
