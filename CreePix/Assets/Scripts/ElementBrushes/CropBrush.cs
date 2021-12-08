using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropBrush : InstanceBrush
{
    private float timer = 0.1F;

    [Range(1, 10)]
    public float separation = 3;

    [Range(0, 1)]
    public float noise = 0;

    public override void draw(float x, float z)
    {
        if (timer < 0.1F)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0;

        int n = Mathf.FloorToInt(1 + 2 * (radius / separation));
        float padding = radius - (n - 1) * (separation / 2);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                float xCoord = x - radius + padding + i * separation + Random.Range(-1.0F, 1.0F) * noise * (separation / 2);
                float zCoord = z - radius + padding + j * separation + Random.Range(-1.0F, 1.0F) * noise * (separation / 2);
                spawnObject(xCoord, zCoord);
            }
        }
    }
}
