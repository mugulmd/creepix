using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PredatorGeneticAlgo : GeneticAlgo {
    void Update()
    {
        while (animals.Count < pop_size / 2)
        {
            animals.Add(makeAnimal());
        }
    }
}
