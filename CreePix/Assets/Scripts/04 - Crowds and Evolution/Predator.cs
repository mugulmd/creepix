using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Predator : Agent
{
    static public int max_generation = 0;
    public static SimpleNeuralNet best_brain = null;
    private int preyLayerMask;
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
    public override string getType()
    {
        return "Predator";
    }
    public override Color getRayColor()
    {
        return Color.cyan;
    }
    public override void inheritBrain(Agent other, bool mutate)
    {
        brain = new SimpleNeuralNet(other.getBrain());
        if (mutate)
            brain.mutate(swap_rate, mutate_rate, swap_strength, mutate_strength);
    }
    private void Awake()
    {
        preyLayerMask = 1 << LayerMask.NameToLayer("Prey");
        baseColor = Color.red;
        vision = new float[2 * nb_eyes];
        network_struct = new int[] { 2*nb_eyes, 32, 16, 8, 2 };
        angle_step = 2 * max_angle / (nb_eyes - 1);
    }
    void Update()
    {
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
        if (details == null)
        {
            updateSetup();
            return;
        }

        energy -= energy_loss;
        int dx = (int)((transform.position.x / terrain_sz.x) * detail_sz.x);
        int dy = (int)((transform.position.z / terrain_sz.y) * detail_sz.y);
        // If over grass, eat it, gain energy and spawn offspring
        if (dx >= 0 && dx < details.GetLength(1) &&
            dy >= 0 && dy < details.GetLength(0))
        {



            Collider[] hitColliders = Physics.OverlapSphere(transform.position + 3.5f * transform.up, 2.5f, preyLayerMask);


            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.tag != "Prey")
                {
                    Debug.Log("PROBLEM");
                }

                Agent prey = hitCollider.transform.parent.gameObject.GetComponent<Agent>();
                terrain.gameObject.GetComponent<PreyGeneticAlgo>().removeAnimal(prey);
            }
            if (hitColliders.Length > 0)
            {
                energy += energy_gain * hitColliders.Length;
                genetic_algo.addOffspring(this);
            }

            if (energy > max_energy)
                energy = max_energy;
        }
        // Die when out of energy
        if (energy < 0)
        {
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

        for (int i = 0; i < nb_eyes; i++)
        {
            Vector3 normalTerrain = gameObject.GetComponent<ProceduralMotion>().normalTerrain;

            Quaternion rot = transform.rotation * Quaternion.AngleAxis(-max_angle + angle_step * i, normalTerrain);

            Vector3 v = rot * Vector3.forward;

            vision[i] = 1.0f;


            if (Physics.Raycast(transform.position + 3.5f * transform.up, v, out RaycastHit hit, max_vision, preyLayerMask))
            {
                if (hit.collider.tag != "Prey")
                    Debug.Log("Problemo");
                vision[i] = hit.distance / max_vision;
                Vector3 preyrWorldVelocity = hit.collider.transform.parent.gameObject.GetComponent<ProceduralMotion>().getScaledCurrentVelocity();
                vision[nb_eyes + i] = (Vector3.Dot(preyrWorldVelocity.normalized, transform.forward) + 1) / 2;

                if (debugOn)
                {
                    Debug.DrawLine(transform.position + 3.5f * transform.up, hit.point, Color.yellow);
                    Debug.DrawRay(hit.collider.transform.position + 3.5f * hit.collider.transform.up, 5 * preyrWorldVelocity, Color.red);
                    Debug.Log($"{i} {vision[i]}");
                }
            }
        }
    }

}
