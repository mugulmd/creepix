using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeforestBrush : Brush
{
    public override void callDraw(float x, float z)
    {
        Vector3 grid = terrain.world2grid(x, z);
        draw(grid.x, grid.z);
    }

    public override void draw(int x, int z)
    {
        draw((float)x, (float)z);
    }

    public override void draw(float x, float z)
    {
        List<TreeInstance> instances = terrain.getObjects();

        Vector3 mouse = terrain.get3(x, z);
        Predicate<TreeInstance> predicate = delegate (TreeInstance instance) {
            return Vector3.Distance(mouse, terrain.getObjectLoc(instance)) < radius;
        };
        instances.RemoveAll(predicate);

        terrain.setObjects(instances);
    }
}
