using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public float swap_rate = 0.01f;
    public float mutate_rate = 0.01f;
    public float swap_strength = 10.0f;
    public float mutate_strength = 0.5f;
    public float max_angle = 10.0f;
    public float max_energy = 10.0f;
    public float energy_loss = 0.1f;
    public float energy_gain = 10.0f;
    public float energy;

    public float max_vision = 20.0f;
    protected float angle_step;
    public int nb_eyes = 6;

    protected Color baseColor;
    public bool debugOn = false;

    protected int[] network_struct;
    protected SimpleNeuralNet brain = null;
    protected GeneticAlgo genetic_algo = null;


    protected CustomTerrain terrain = null;
    protected int[,] details = null;
    protected Vector2 detail_sz;
    protected Vector2 terrain_sz;

    protected float[] vision;
    
    protected int generation = 0;
    protected Vector2 nextGoalInfo;

    public int getGeneration()
    {
        return generation;
    }

    public abstract int getMaxGeneration();
    public abstract void setMaxGeneration(int new_max_generation);
    public abstract SimpleNeuralNet getBestBrain();
    public abstract void setBestBrain();
    public abstract string getType();
    public abstract Color getRayColor();

    public void setGeneration(int gen)
    {
        generation = gen;
        if (generation > getMaxGeneration())
        {
            if (generation > 15)
            {
                Debug.Log($"{getType()} best brain set {generation}");
                setBestBrain();
            }
            setMaxGeneration(gen);
        }

        if (generation > 20 && generation <= 40)
        {
            max_energy += 20f;
        }
        else if (generation > 40)
        {
            max_energy += 60f;
        }
    }
    public Vector2 getNextGoalInfo()
    {
        return nextGoalInfo;
    }

    void Start()
    {
        name = $"{getType()} gen{generation} {(int)(UnityEngine.Random.value * 10000000)}";
        nextGoalInfo = new Vector2(0, 1);
        energy = max_energy;

    }
    public void setup(CustomTerrain ct, GeneticAlgo ga)
    {
        terrain = ct;
        genetic_algo = ga;
        updateSetup();
    }
    protected void updateSetup()
    {
        detail_sz = terrain.detailSize();
        Vector3 gsz = terrain.terrainSize();
        terrain_sz = new Vector2(gsz.x, gsz.z);
        details = terrain.getDetails();
    }

    public abstract void inheritBrain(Agent other, bool mutate);
    public SimpleNeuralNet getBrain()
    {
        return brain;
    }
    public float getHealth()
    {
        return energy / max_energy;
    }
}
