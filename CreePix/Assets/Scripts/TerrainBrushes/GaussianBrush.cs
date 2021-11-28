using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussianBrush : TerrainBrush
{
    [Range(0, 1)]
    public float strength = 0.35F;

    public bool push = false;

    public override void draw(int x, int z)
    {
        for (int xi = -radius; xi <= radius; xi++)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                // circular pattern
                if (xi + zi * zi > radius * radius)
                    continue;

                float height = terrain.get(x + xi, z + zi);

                float gaussian_factor = Mathf.Exp(-(xi*xi+zi*zi)/(2*radius));

                // push or pull with given strength
                if (push)
                    terrain.set(x + xi, z + zi, height - strength*gaussian_factor);
                else
                    terrain.set(x + xi, z + zi, height + strength*gaussian_factor);
            }
        }
    }
}
