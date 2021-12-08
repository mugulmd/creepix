using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InstanceBrush : Brush
{
    private int prefab_idx;

    public override void callDraw(float x, float z)
    {
        if (terrain.object_prefab)
            prefab_idx = terrain.registerPrefab(terrain.object_prefab);
        else
        {
            prefab_idx = -1;
            return;
        }
        Vector3 grid = terrain.world2grid(x, z);
        draw(grid.x, grid.z);
    }

    public override void draw(int x, int z)
    {
        draw((float)x, (float)z);
    }

    public void spawnObject(float x, float z, int idx)
    {
        if (idx == -1)
        {
            return;
        }
        if (terrain.getSteepness(x, z) > terrain.max_steepness)
        {
            return;
        }
        float scale_diff = Mathf.Abs(terrain.max_scale - terrain.min_scale);
        float scale_min = Mathf.Min(terrain.max_scale, terrain.min_scale);
        float scale = (float)CustomTerrain.rnd.NextDouble() * scale_diff + scale_min;
        terrain.spawnObject(terrain.getInterp3(x, z), scale, idx);
    }

    public void spawnObject(float x, float z)
    {
        spawnObject(x, z, prefab_idx);
    }
}
