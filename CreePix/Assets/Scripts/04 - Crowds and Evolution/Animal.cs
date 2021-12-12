using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animal : Agent
{
    static public int max_generation = 0;
    public static SimpleNeuralNet best_brain = null;
    public float split_energy = 180f;
    public float energy_gain = 2.0f;

    private int predatorLayerMask;

    public override int getMaxGeneration()
    {
        return max_generation;
    }
    public override void setMaxGeneration(int new_max_generation)
    {
        max_generation = new_max_generation;
    }
    public override SimpleNeuralNet getBestBrain()
    {
        return best_brain;
    }
    public override void setBestBrain()
    {
        best_brain = new SimpleNeuralNet(brain);
    }
    public override string getType(){
        return "Animal";
    }
    public override Color getRayColor()
    {
        return Color.magenta;
    }
    public override void inheritBrain(Agent other, bool mutate)
    {
        if (brain != null)
        {
            Debug.Log("inheritance failed");
        }
        brain = new SimpleNeuralNet(other.getBrain());
        if (mutate)
        {
            brain.mutate(swap_rate, mutate_rate, swap_strength, mutate_strength);
        }
    }

    private void Awake()
    {
        predatorLayerMask = 1 << LayerMask.NameToLayer("Predator");
        baseColor = Color.blue;
        network_struct = new int[] {nb_eyes, 16, 2 };
        vision = new float[nb_eyes];
        angle_step = 2 * max_angle / (nb_eyes - 1);
        energy = split_energy/3;
    }

    void Update() {
        if (brain == null)
        {
            if (best_brain == null)
            {
                brain = new SimpleNeuralNet(network_struct);
            }
            else
            {
                brain = new SimpleNeuralNet(best_brain);
                brain.mutate(swap_rate, 2 * mutate_rate, swap_strength * 2, mutate_strength * 2);
            }
        }
        if (terrain == null)
            return;
        if (details == null) {
            updateSetup();
            return;
        }

        if (printOn)
        {
            brain.writeToDebug("human");
            printOn = false;
        }


        int dx = (int)((transform.position.x / terrain_sz.x) * detail_sz.x);
        int dy = (int)((transform.position.z / terrain_sz.y) * detail_sz.y);
        if (debugOn)
            Debug.Log($"terrain {dx} to {details.GetLength(0)} / {dy} to {details.GetLength(1)} ");

        if (energy > split_energy) {
            energy = split_energy / 2;
            genetic_algo.addOffspring(this);
        }

        else if (energy < 0) {
            energy = 0.0f;
            genetic_algo.removeAnimal(this);
        }

        // Update receptor
        updateVision();
        // Use brain
        float[] output = brain.getOutput(vision);
        
        // Act using actuators
        float angle = (output[0] * 2.0f - 1.0f) * action_angle;
        float noise = output[1];
        nextGoalInfo = new Vector2(angle, noise);
    }

    private void updateVision()
    {
        if (debugOn)
        {
            Debug.Log("Enter");
        }

        energy -= energy_gain/10;

        for (int i = 0; i < nb_eyes; i++)
        {
            Vector3 normalTerrain = gameObject.GetComponent<ProceduralMotion>().normalTerrain;
            Quaternion rot = transform.rotation * Quaternion.AngleAxis(-max_angle + angle_step * i, normalTerrain);

            Vector3 v = rot * Vector3.forward;

            vision[i] = 1.0f;

            bool didHit = false;
            if (Physics.Raycast(transform.position + 3.5f * transform.up, v, out RaycastHit hit, max_vision, predatorLayerMask))
            {
                didHit = true;
                if (hit.collider.tag != "Predator")
                    Debug.Log("Problemo");

                vision[i] = hit.distance / max_vision;

                if (debugOn)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.blue);
                    Debug.Log($"avoid {i} {vision[i]}");
                }
            }
            if (didHit)
                energy += energy_gain;
        }
    }

}

