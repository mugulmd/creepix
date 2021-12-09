using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animal : Agent
{
    static public int max_generation = 0;
    public static SimpleNeuralNet best_brain = null;
    public static SimpleNeuralNet best_sub_brain_food = null;
    public static SimpleNeuralNet best_sub_brain_avoid = null;

    const int hiddenLayersize = 8;

    private SimpleNeuralNet sub_brain_avoid = null;
    private SimpleNeuralNet sub_brain_food = null;
    protected int[] food_network_struct;
    protected int[] avoid_network_struct;
    protected float[] food_vision;
    protected float[] avoid_vision;

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
        best_sub_brain_food = new SimpleNeuralNet(sub_brain_food);
        best_sub_brain_avoid = new SimpleNeuralNet(sub_brain_avoid);

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
        sub_brain_food = new SimpleNeuralNet(((Animal)other).getSubBrainFood());
        sub_brain_avoid = new SimpleNeuralNet(((Animal)other).getSubBrainAvoid());

        if (mutate)
        {
            brain.mutate(swap_rate, mutate_rate, swap_strength, mutate_strength);
            sub_brain_food.mutate(swap_rate, mutate_rate, swap_strength, mutate_strength);
            sub_brain_avoid.mutate(swap_rate, mutate_rate, swap_strength, mutate_strength);
        }
    }
    public SimpleNeuralNet getSubBrainFood()
    {
        return sub_brain_food;
    }
    public SimpleNeuralNet getSubBrainAvoid()
    {
        return sub_brain_avoid;
    }

    private void Awake()
    {
        predatorLayerMask = 1 << LayerMask.NameToLayer("Predator");
        baseColor = Color.blue;
        network_struct = new int[] { 2 * hiddenLayersize + 2, 16, 3 };
        food_network_struct = new int[] { nb_eyes, hiddenLayersize };
        avoid_network_struct = new int[]{ 2*nb_eyes, hiddenLayersize };
        vision = new float[2* hiddenLayersize + 2];
        food_vision = new float[nb_eyes];
        avoid_vision = new float[2*nb_eyes];
        angle_step = 2 * max_angle / (nb_eyes - 1);
    }

    void Update() {
        if (brain == null)
        {
            if (best_brain == null)
            {
                sub_brain_food = new SimpleNeuralNet(food_network_struct);
                sub_brain_avoid = new SimpleNeuralNet(avoid_network_struct);
                brain = new SimpleNeuralNet(network_struct);
            }
            else
            {
                brain = new SimpleNeuralNet(best_brain);
                sub_brain_food = new SimpleNeuralNet(best_sub_brain_food);
                sub_brain_avoid = new SimpleNeuralNet(best_sub_brain_avoid);
                brain.mutate(swap_rate, 0.5f, swap_strength * 2, mutate_strength * 2);
                sub_brain_food.mutate(swap_rate, 0.5f, swap_strength * 2, mutate_strength * 2);
                sub_brain_avoid.mutate(swap_rate, 0.5f, swap_strength * 2, mutate_strength * 2);
            }
        }
        if (terrain == null)
            return;
        if (details == null) {
            updateSetup();
            return;
        }

        energy -= energy_loss;
        int dx = (int)((transform.position.x / terrain_sz.x) * detail_sz.x);
        int dy = (int)((transform.position.z / terrain_sz.y) * detail_sz.y);
        // If over grass, eat it, gain energy and spawn offspring
        if (dx >= 0 && dx < details.GetLength(1) &&
            dy >= 0 && dy < details.GetLength(0) &&
            details[dy, dx] > 0) {
            details[dy, dx] = 0;
            energy += energy_gain;
            if (energy > max_energy)
                energy = max_energy;
            genetic_algo.addOffspring(this);
        }
        // Die when out of energy
        if (energy < 0) {
            energy = 0.0f;
            genetic_algo.removeAnimal(this);
        }
        /*foreach (Material mat in materials)
        {
            if (mat != null)
                mat.color = baseColor * (energy / max_energy);
        }*/

        // Update receptor
        updateVision();

        // Use brain
        float[] output = brain.getOutput(vision);
        
        if (debugOn)
        {
            Debug.Log($"importance {output[2]}");
        }
        // Act using actuators
        float angle = (output[0] * 2.0f - 1.0f) * max_angle;
        float distToGoal = 0.2f + output[1] * max_vision;
        nextGoalInfo = new Vector3(angle, distToGoal, output[2]);
    }

    private void updateVision() {
        if (debugOn)
        {
            Debug.Log("Enter");
        }
        updateSubVision();
        setUpVision();
    }

    private void updateSubVision()
    {
        Vector2 ratio = detail_sz / terrain_sz;

        for (int i = 0; i < nb_eyes; i++)
        {
            Quaternion rot = transform.rotation * Quaternion.Euler(0.0f, -max_angle + angle_step * i, 0.0f);

            Vector3 v = rot * Vector3.forward;
            float sx = transform.position.x * ratio.x;
            float sy = transform.position.z * ratio.y;
            food_vision[i] = 1.0f;
            for (float d = 1.0f; d < max_vision; d += 0.5f)
            {
                float px = (sx + d * v.x * ratio.x);
                float py = (sy + d * v.z * ratio.y);
                if (px < 0)
                    px += detail_sz.x;
                else if (px >= detail_sz.x)
                    px -= detail_sz.x;
                if (py < 0)
                    py += detail_sz.y;
                else if (py >= detail_sz.y)
                    py -= detail_sz.y;

                if ((int)px >= 0 && (int)px < details.GetLength(1) &&
                    (int)py >= 0 && (int)py < details.GetLength(0) &&
                    details[(int)py, (int)px] > 0)
                {
                    food_vision[i] = d / max_vision;
                    if (debugOn)
                    {
                        Debug.DrawLine(transform.position + 3.5f * transform.up, transform.position + d * v, Color.white);
                        Debug.Log($"food {i} {food_vision[i]}");
                    }
                    break;
                }
            }

            avoid_vision[i] = 1.0f;
            avoid_vision[nb_eyes + i] = 1.0f;
            if (Physics.Raycast(transform.position + 3.5f * transform.up, v, out RaycastHit hit, max_vision, predatorLayerMask))
            {
                if (hit.collider.tag != "Predator")
                    Debug.Log("Problemo");


                Vector3 predatorWorldVelocity = hit.collider.transform.parent.gameObject.GetComponent<ProceduralMotion>().getScaledCurrentVelocity();

                avoid_vision[i] = hit.distance / max_vision;
                avoid_vision[nb_eyes + i] = (Vector3.Dot(predatorWorldVelocity.normalized, transform.forward) + 1) / 2;

                if (debugOn)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.yellow);
                    Debug.Log($"avoid {i} {avoid_vision[i]}  / {avoid_vision[nb_eyes + i]}");
                }
            }

        }
    }

    private void setUpVision()
    {
        
        float[] output_food = sub_brain_food.getOutput(food_vision);
        float[] output_avoid = sub_brain_avoid.getOutput(avoid_vision);

        for (int i = 0; i< hiddenLayersize; i++)
        {
            vision[i] = output_food[i];
            vision[hiddenLayersize + i] = output_avoid[i];
        }

        Vector3 animalScaledVelocity = gameObject.GetComponent<ProceduralMotion>().getScaledCurrentVelocity();
        vision[2 * hiddenLayersize] = (Vector3.Dot(animalScaledVelocity.normalized, transform.forward) + 1) / 2;
        vision[2 * hiddenLayersize + 1] = gameObject.GetComponent<ProceduralMotion>().getCurrentImportance();
    }
}

