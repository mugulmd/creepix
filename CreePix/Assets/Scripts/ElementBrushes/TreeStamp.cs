using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeStamp : InstanceBrush
{
    private float timer = 0.1F;
    private int[,] hist = null;

    public override void draw(float x, float z)
    {
        if (timer < 0.1F)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            hist = new int[terrain.getPrototypeCount(), radius];
            for (int idx = 0; idx < terrain.getPrototypeCount(); idx++)
            {
                for (int r = 0; r < radius; r++)
                {
                    hist[idx, r] = 0;
                }
            }

            Vector3 mouse = terrain.get3(x, z);
            for (int i = 0; i < terrain.getObjectCount(); i++)
            {
                TreeInstance instance = terrain.getObject(i);
                float dist = Vector3.Distance(mouse, terrain.getObjectLoc(i));
                if (dist < radius)
                {
                    int slot = Mathf.FloorToInt(dist);
                    hist[instance.prototypeIndex, slot]++;
                }
            }
        } 
        else if (hist != null)
        {
            for (int idx = 0; idx < terrain.getPrototypeCount(); idx++)
            {
                for (int r = 0; r < radius; r++)
                {
                    for (int n = 0; n < hist[idx, r]; n++)
                    {
                        float randDist = Random.Range((float)r, (float)(r + 1));
                        float randAngle = Random.Range(0.0F, 6.28F);
                        float randX = x + Mathf.Cos(randAngle) * randDist;
                        float randZ = z + Mathf.Sin(randAngle) * randDist;
                        spawnObject(randX, randZ, idx);
                    }
                }
            }
        }
    }
}
