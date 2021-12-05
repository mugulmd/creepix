using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBrush : InstanceBrush
{
    [Range(1, 50)]
    public float min_dist = 10;

    public override void draw(float x, float z)
    {
        Vector3 mouse = terrain.get3(x, z);
        for(int i = 0; i < terrain.getObjectCount(); i++)
        {
            Vector3 loc = terrain.getObjectLoc(i);
            if (Vector3.Distance(mouse, loc) < min_dist)
                return;
        }
        spawnObject(x, z);
    }
}
