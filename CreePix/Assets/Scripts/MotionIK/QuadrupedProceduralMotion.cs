using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedProceduralMotion : MonoBehaviour
{
    // Settings relative to the root motion.
    [Header("Root Motion Settings")]
    public float turnSpeed;
    public float moveSpeed;
    public float moveBackwardFactor = 0.5f;
    [Space(20)]
    public float turnAcceleration;
    public float moveAcceleration;
    [Space(20)]

    SmoothDamp.Vector3 currentVelocity;
    SmoothDamp.Float currentAngularVelocity;

    // Settings relative to body adaptation to the terrain.
    [Header("Body Adaptation Settings")]
    public Transform hips;
    public float heightAcceleration;
    public Vector3 constantHipsPosition;
    public Vector3 constantHipsRotation;
    public Transform groundChecker;
    public Vector3 normalTerrain;
    public float distanceHit;
    public Vector3 posHit;

    // Settings relative to the tail.
    [Header("Tail Settings")]
    public Transform[] tailBones;
    public float tailTurnMultiplier;
    public float tailTurnSpeed;
    Quaternion[] tailHomeLocalRotation;
    SmoothDamp.Float tailRotation;

    // Settings relative to the head.
    [Header("Head Settings")]
    public Transform headBone;
    public float speedHead = 1f;
    public bool headDebug = false;
    public float angleHeadLimit = 90f;

    // Foot Steppers for each leg.
    private Coroutine gaitCoroutine;
    
    [Header("Controllers for the steps")]
    public FootStepper frontLeftFoot;
    public FootStepper frontRightFoot;
    public FootStepper backLeftFoot;
    public FootStepper backRightFoot;

    protected Terrain terrain;
    protected CustomTerrain cterrain;
    protected float width, height;

    private Vector3 currentGoal_Position;
    private float currentGoalImportance;

    public float minReachableDistance = 2f;
    public float maxReachableDistance = 8f;

    // Awake is called when the script instance is being loaded.
    void Start()
    {
        terrain = Terrain.activeTerrain;
        cterrain = terrain.GetComponent<CustomTerrain>();

        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;

        currentGoal_Position = hips.position;
        currentGoalImportance = 0;

        gaitCoroutine = StartCoroutine(Gait());

        TailInitialize();
        BodyInitialize();
    }

    public Vector3 getScaledCurrentVelocity()
    {
        return new Vector3(currentVelocity.x/ moveSpeed, currentVelocity.y/ moveSpeed, currentVelocity.z/ moveSpeed);
    }

    // Update is called every frame, if the MonoBehaviour is enabled.
    private void Update()
    {
        RootMotion();
    }

    // LateUpdate is called after all Update functions have been called.
    private void LateUpdate()
    {
        //TrackHead();
        TailUpdate();
        RootAdaptation();
        if (transform.position.x > 497 || transform.position.x < 2 || transform.position.z > 497 || transform.position.z < 2)
        {
            Vector3 loc = transform.position;
            loc.x = (loc.x < 1) ? loc.x + 494 : ((loc.x > 498) ? loc.x - 494 : loc.x);
            loc.z = (loc.z < 1) ? loc.z + 494 : ((loc.z > 498) ? loc.z - 494 : loc.z);

            transform.position = loc;
        }
    }

    public void destroyFootSteps()
    {
        if (gaitCoroutine != null)
            StopCoroutine(gaitCoroutine);
        Destroy(frontLeftFoot.gameObject);
        Destroy(frontRightFoot.gameObject);
        Destroy(backLeftFoot.gameObject);
        Destroy(backRightFoot.gameObject);
    }

    #region Root Motion

    /// <summary>
    /// Method used to move the character towards the goal.
    /// </summary>
    private void RootMotion()
    {
        Vector3 next_goalInfo = gameObject.GetComponent<Agent>().getNextGoalInfo(); // (angle, distToGoal, importance)

        if (currentGoalImportance * 1.1 < next_goalInfo[2]
            || Vector3.Distance(currentGoal_Position, hips.position) < minReachableDistance
            || Vector3.Distance(currentGoal_Position, hips.position) > maxReachableDistance)
        {
            currentGoal_Position = hips.position + Quaternion.Euler(0, next_goalInfo[0], 0) * (next_goalInfo[1] * hips.forward);
            currentGoalImportance = next_goalInfo[2];
        }

        Vector3 noise = 0.2f * Mathf.Pow(1 - currentGoalImportance, 3) * currentGoal_Position.magnitude * new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        currentGoal_Position += noise;
        // Get the vector towards the goal and projectected it on the plane defined by the normal transform.up.
       
        Vector3 towardGoalProjected = Vector3.ProjectOnPlane(currentGoal_Position - hips.position, hips.up);

        // Angles between the forward direction and the direction to the goal. 
        var angToGoal = Vector3.SignedAngle(hips.forward, towardGoalProjected, hips.up);


        if (gameObject.GetComponent<Agent>().debugOn)
        {
            Debug.DrawLine(hips.position, hips.position + towardGoalProjected, gameObject.GetComponent<Agent>().getRayColor(), 1);
        }

        // Get a perfectange of the turnSpeed to apply based on how far the goal is and its sign.

        float targetAngularVelocity = Mathf.Sign(angToGoal) * Mathf.InverseLerp(0f, 180f, Mathf.Abs(angToGoal)) * turnSpeed;

        // Step() smoothing function.
        currentAngularVelocity.Step(targetAngularVelocity, turnAcceleration);

        // Initialize zero root velocity.
        Vector3 targetVelocity = Vector3.zero;

        // Estimating targetVelocity.
        // Only translate if we are close facing to the goal.
        if (Mathf.Abs(angToGoal) < 120)
        {
            targetVelocity = moveSpeed * towardGoalProjected.normalized;

            // Limit velocity progressively as we approach max angular velocity.
            targetVelocity *= Mathf.InverseLerp(turnSpeed, turnSpeed * 0.2f, Mathf.Abs(currentAngularVelocity));
        }

        // Apply targetVelocity using Step() and applying.
        currentVelocity.Step(targetVelocity, moveAcceleration);
        transform.position += currentVelocity.currentValue * Time.deltaTime;
        // Apply rotation.
        transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * currentAngularVelocity, transform.up);
    }

    #endregion

    #region Root Adaptation

    /// <summary>
    /// Saves the initial position and rotation of the hips.
    /// </summary>
    private void BodyInitialize()
    {
        constantHipsPosition = new Vector3(hips.position.x, hips.position.y, hips.position.z);
        constantHipsRotation = new Vector3(hips.rotation.x, hips.rotation.y, hips.rotation.z);
    }

    /// <summary>
    /// In LateUpdate, after moving the root body to the target, we perform the adaptation on a top-layer to place the animal parallel to the ground.
    /// </summary>
    private void RootAdaptation()
    {
        // Origin of the ray.
        Vector3 raycastOrigin = groundChecker.position;

        // The ray information gives you where you hit and the normal of the terrain in that location.
        if (Physics.Raycast(raycastOrigin, -transform.up, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.tag == "Ground")
            {
                posHit = hit.point;
                distanceHit = hit.distance;
                normalTerrain = hit.normal;
            }
        }

        /*
         * In this layer, we need to refine the position and rotation of the hips based on the ground. Without this part, the animal would not lift its root body when walking on high terrains.
         * First, try to use the hit information to modify hips.position and move it up when you are in a higher ground.
         * Then, use also this information (normalTerrain) to rotate the root body and place it parallel to the ground. You can use Quaternion.FromToRotation() for that.
         * When you have the angle that you want to have in your root body, you can place it directly, or use some interpolation technique to go smoothly to that value, in order to have less drastical movements.
         */

        // START TODO ###################

        hips.position = new Vector3(hips.position.x, constantHipsPosition.y + posHit.y, hips.position.z);

        Quaternion grndTilt = Quaternion.FromToRotation(hips.up, normalTerrain);
        hips.rotation = Quaternion.Slerp(hips.rotation, grndTilt * hips.rotation, 1 - Mathf.Exp(-heightAcceleration * Time.deltaTime));

        // END TODO ###################
    }

    #endregion

    #region Tail Motion

    /// <summary>
    /// Initialize all the bones of the tail.
    /// </summary>
    void TailInitialize()
    {
        // Store the default rotation of the tail bones.
        tailHomeLocalRotation = new Quaternion[tailBones.Length];
        for (int i = 0; i < tailHomeLocalRotation.Length; i++)
        {
            tailHomeLocalRotation[i] = tailBones[i].localRotation;
        }
    }


    /// <summary>
    /// Rotate the tail as a function of the angular velocity of the root body.
    /// </summary>
    void TailUpdate()
    {
        // Tail rotates opposite to the current angular velocity to have some inertia effect.
        tailRotation.Step(-currentAngularVelocity / turnSpeed * tailTurnMultiplier, tailTurnSpeed);

        for (int i = 0; i < tailBones.Length; i++)
        {
            // Convert to Euler and apply the rotation my multiplying to the current local quaternion.
            Quaternion rotation = Quaternion.Euler(0, tailRotation, 0);
            tailBones[i].localRotation = rotation * tailHomeLocalRotation[i];
        }
    }

    #endregion

    

    #region Gait

    /// <summary>
    /// Coroutine that describes how the gait of the character will be.
    /// It calls the MoveLeg() function for each leg in a defined moment. In this case, we want the diagonal legs pair move simultaneiously, while the other pair of diagonal legs stays in place.
    /// This is necessary as we do not have any kinematic animation - our character moves purely with IK and procedural functions.
    /// Other complex behaviors might be created.
    /// </summary>
    /// <returns></returns>
    IEnumerator Gait()
    {
        while (true)
        {

            do
            {
                frontLeftFoot.MoveLeg();

                backRightFoot.MoveLeg();

                // Wait a frame
                yield return null;

            } while (backRightFoot.Moving || frontLeftFoot.Moving);

            // Do the same thing for the other diagonal pair
            do
            {
                frontRightFoot.MoveLeg();
                backLeftFoot.MoveLeg();

                // Wait a frame
                yield return null;

            } while (backLeftFoot.Moving || frontRightFoot.Moving);
        }
    }

    #endregion
}

