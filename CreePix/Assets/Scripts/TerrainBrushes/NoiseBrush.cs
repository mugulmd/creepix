using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseBrush : TerrainBrush
{
    [Range(1, 50)]
    public float height = 10;

    [Range(1, 200)]
    public float scale = 50;

    public override void draw(int x, int z)
    {
        for (int xi = -radius; xi <= radius; xi++)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                float xCoord = (float)(x + xi) / (float)(terrain.getWidth()) * scale;
                float zCoord = (float)(z + zi) / (float)(terrain.getHeight()) * scale;
                float noise_factor = Mathf.PerlinNoise(xCoord, zCoord);
                terrain.set(x + xi, z + zi, height * noise_factor);
            }
        }
    }
}
