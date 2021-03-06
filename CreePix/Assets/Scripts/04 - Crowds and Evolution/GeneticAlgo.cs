using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class GeneticAlgo : MonoBehaviour {

    [Header("Genetic algorithm parameters")]
    public int pop_size = 100;
    public int pop_max = 200;
    public GameObject prefab;

    protected List<GameObject> animals;

    protected Terrain terrain;
    protected CustomTerrain cterrain;
    protected float width, height;

    void Start() {
        terrain = Terrain.activeTerrain;
        cterrain = gameObject.GetComponent<CustomTerrain>();

        animals = new List<GameObject>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;
        for (int i = 0; i < pop_size; i++) {
            GameObject animal = makeAnimal();
            animals.Add(animal);
        }
    }

    public GameObject makeAnimal(Vector3 position) {
        GameObject animal = Instantiate(prefab, position, Quaternion.Euler(0.0f, UnityEngine.Random.value * 360.0f, 0.0f), transform);
        animal.GetComponent<Agent>().setup(cterrain, this);
        return animal;
    }
    public GameObject makeAnimal() {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = (0.00005f + UnityEngine.Random.Range(0, 0.9009f)) * width / scale.x;
        float z = (0.00005f + UnityEngine.Random.Range(0, 0.9009f)) * width / scale.z;
        


        float y = terrain.terrainData.GetInterpolatedHeight(x / terrain.terrainData.heightmapResolution,
                                                  z / terrain.terrainData.heightmapResolution);
        return makeAnimal(new Vector3(x, y + 100, z));
    }

    public void addOffspring(Agent parent) {
        if (animals.Count > pop_max)
        {
            return;
        }
        GameObject animal = makeAnimal(parent.transform.position + 100*Vector3.up);
        animal.GetComponent<Agent>().inheritBrain(parent, true);
        animal.GetComponent<Agent>().setGeneration(parent.getGeneration() + 1);
        animals.Add(animal);
    }

    public void removeAnimal(Agent animal, bool destroy=true) {
        animals.Remove(animal.transform.gameObject);
        if (destroy)
        {
            animal.gameObject.GetComponent<ProceduralMotion>().destroyFootSteps();
            Destroy(animal.transform.gameObject);
        }

    }

}
