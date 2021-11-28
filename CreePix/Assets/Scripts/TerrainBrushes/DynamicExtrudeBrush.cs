using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicExtrudeBrush : TerrainBrush
{
    [Range(0, 1)]
    public float strength = 0.2F;

    public bool circular = false;
    public bool push = false;

    public override void draw(int x, int z)
    {
        for (int xi = -radius; xi <= radius; xi++)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                // check if pattern is either square or circle
                if (circular && xi * xi + zi * zi > radius * radius)
                    continue;

                float height = terrain.get(x + xi, z + zi);

                // push or pull with given strength
                if (push)
                    terrain.set(x + xi, z + zi, height - strength);
                else
                    terrain.set(x + xi, z + zi, height + strength);
            }
        }
    }
}
