using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMotion : MonoBehaviour
{
    [Header("Goal")]
    public Transform goal; // The character will move towards this goal.

    // Settings relative to the root motion.
    [Header("Root Motion Settings")]
    public float turnSpeed;
    public float moveSpeed;
    public float moveBackwardFactor = 0.5f;
    [Space(20)]
    public float turnAcceleration;
    public float moveAcceleration;
    [Space(20)]
    public float minDistToGoal;
    public float maxDistToGoal;
    SmoothDamp.Vector3 currentVelocity;
    SmoothDamp.Float currentAngularVelocity;

    // Settings relative to body adaptation to the terrain.
    [Header("Body Adaptation Settings")]
    public Transform hips;
    public Transform footChecker;

    public float heightAcceleration;
    public Vector3 constantHipsPosition;
    public Vector3 constantHipsRotation;
    public Vector3 constantIKPosition;

    public Transform groundChecker;
  



    // Foot Steppers for each leg.
    [Header("Controllers for the steps")]
    public FootStepper LeftFoot;
    public FootStepper RightFoot;





    public Transform IK;








    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        StartCoroutine(Gait());

        BodyInitialize();
    }

    // Update is called every frame, if the MonoBehaviour is enabled.
    private void Update()
    {
        RootMotion();
    }

    // LateUpdate is called after all Update functions have been called.
    private void LateUpdate()
    {
        RootAdaptation();
    }

    #region Root Motion

    /// <summary>
    /// Method used to move the character towards the goal.
    /// </summary>
    private void RootMotion()
    {
        // Get the vector towards the goal and projectected it on the plane defined by the normal transform.up.
        Vector3 towardGoal = goal.position - transform.position;
        Vector3 towardGoalProjected = Vector3.ProjectOnPlane(towardGoal, transform.up);



        // Angles between the forward direction and the direction to the goal. 
        var angToGoal = Vector3.SignedAngle(transform.forward, towardGoalProjected, transform.up);





        // Get a perfectange of the turnSpeed to apply based on how far the goal is and its sign.
        float targetAngularVelocity = Mathf.Sign(angToGoal) * Mathf.InverseLerp(5f, 30f, Mathf.Abs(angToGoal)) * turnSpeed;

        // Step() smoothing function.
        currentAngularVelocity.Step(targetAngularVelocity, turnAcceleration);

        // Initialize zero root velocity.
        Vector3 targetVelocity = Vector3.zero;

        // Estimating targetVelocity.
        // Only translate if we are close facing to the goal.
        if (Mathf.Abs(angToGoal) < 90)
        {

            var distToGoal = towardGoalProjected.magnitude;

            // Move towards target if we are too far.
            if (distToGoal > maxDistToGoal)
            {
                targetVelocity = moveSpeed * towardGoalProjected.normalized;
            }
            // Move away from target if we are too close.
            else if (distToGoal < minDistToGoal)
            {
                targetVelocity = moveSpeed * -towardGoalProjected.normalized * moveBackwardFactor;
            }

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
        constantIKPosition = new Vector3(footChecker.position.x, footChecker.position.y, footChecker.position.z);
    }

    /// <summary>
    /// In LateUpdate, after moving the root body to the target, we perform the adaptation on a top-layer to place the animal parallel to the ground.
    /// </summary>
    private void RootAdaptation()
    {
        // Origin of the ray.
        Vector3 raycastOriginBody = groundChecker.position;

        Vector3 raycastOriginFoot = footChecker.position;
        
        float distanceHit;
        Vector3 posHit=Vector3.zero;
        Vector3 normalTerrain = Vector3.zero; 

        float distanceHitF;
        Vector3 posHitF=Vector3.zero;
        Vector3 normalTerrainF = Vector3.zero; ;

        // The ray information gives you where you hit and the normal of the terrain in that location.
        Debug.Log(groundChecker.name);
        if (Physics.Raycast(raycastOriginBody, -transform.up, out RaycastHit hit, Mathf.Infinity))
        {

            Debug.Log(hit.transform.gameObject.tag);
            if (hit.transform.gameObject.tag == "Ground")
            {
                posHit = hit.point;
                distanceHit = hit.distance;
                normalTerrain = hit.normal;
            }
        }





        // The ray information gives you where you hit and the normal of the terrain in that location.
        if (Physics.Raycast(raycastOriginFoot+new Vector3(0,4,0), -transform.up, out RaycastHit hitF, Mathf.Infinity))
        {
            if (hit.transform.gameObject.tag == "Ground")
            {
                posHitF = hitF.point;
                distanceHitF = hit.distance;
                normalTerrainF = hit.normal;
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

        //IK.position = new Vector3(IK.position.x, constantIKPosition.y + posHitF.y, IK.position.z); ;

        Quaternion grndTilt = Quaternion.FromToRotation(hips.up, normalTerrain);
        //hips.rotation = Quaternion.Slerp(hips.rotation, grndTilt * hips.rotation, 1 - Mathf.Exp(-heightAcceleration * Time.deltaTime));




        // END TODO ###################
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
                LeftFoot.MoveLeg();


                // Wait a frame
                yield return null;

            } while (LeftFoot.Moving);



            // Do the same thing for the other diagonal pair
            do
            {
                RightFoot.MoveLeg();


                // Wait a frame
                yield return null;

            } while (RightFoot.Moving);
        }
    }

    #endregion
}
