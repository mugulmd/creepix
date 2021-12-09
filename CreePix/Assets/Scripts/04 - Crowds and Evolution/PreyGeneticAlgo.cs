using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreyGeneticAlgo : GeneticAlgo
{
    [Header("Dynamic elements")]
    public float vegetation_growth_rate = 1.0f;
    protected float curr_growth = 0.0f;

    public void updateResources()
    {
        Vector2 detail_sz = cterrain.detailSize();
        int[,] details = cterrain.getDetails();
        curr_growth += vegetation_growth_rate;
        while (curr_growth > 1.0f)
        {
            float flipCoin = UnityEngine.Random.value;
            int x;
            int y;
            if (flipCoin < 0.5f)
            {
                x = (int)((0.001 + UnityEngine.Random.value / 2) * detail_sz.x);
                y = (int)((0.001 + UnityEngine.Random.value / 2) * detail_sz.y);
            }
            else
            {
                x = (int)((0.49 + UnityEngine.Random.value / 2) * detail_sz.x);
                y = (int)((0.49 + UnityEngine.Random.value / 2) * detail_sz.y);
            }

            details[y, x] = 1;
            curr_growth -= 1.0f;
        }

        gameObject.GetComponent<CustomTerrain>().saveDetails();
    }

    void Update()
    {
        while (animals.Count < pop_size / 2)
        {
            animals.Add(makeAnimal());
        }
        updateResources();
    }
}
