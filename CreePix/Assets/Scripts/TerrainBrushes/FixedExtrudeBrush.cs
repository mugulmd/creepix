using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedExtrudeBrush : TerrainBrush
{
    [Range(1, 50)]
    public float height = 10;

    public bool circular = false;

    public override void draw(int x, int z)
    {
        for (int xi = -radius; xi <= radius; xi++)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                // check if pattern is either square or circle
                if (circular && xi * xi + zi * zi > radius * radius) 
                    continue;

                terrain.set(x + xi, z + zi, height);
            }
        }
    }
}
