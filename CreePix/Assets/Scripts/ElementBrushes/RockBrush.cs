using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBrush : DetailBrush
{
    public override void draw(int x, int z)
    {
        int density = terrain.getDetailDensity(x, z);
        terrain.setDetailDensity(x, z, density+1);
    }
}
