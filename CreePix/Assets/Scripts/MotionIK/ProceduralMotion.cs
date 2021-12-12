using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMotion : MonoBehaviour
{
    // Settings relative to the root motion.
    [Header("Root Motion Settings")]
    public float turnSpeed;
    public float moveSpeed;
    public float moveBackwardFactor = 0.5f;
    [Space(20)]
    public float turnAcceleration;
    public float moveAcceleration;

    public Transform goal;
   
    //SmoothDamp.Vector3 currentVelocity;
    //SmoothDamp.Float currentAngularVelocity;

    // Settings relative to body adaptation to the terrain.
    [Header("Body Adaptation Settings")]
    public Transform hips;


    public float heightAcceleration;
    public Vector3 constantHipsPosition;
    public Vector3 constantHipsRotation;

    public LayerMask groundRaycastMask = ~0; // Ground layer that you need to detect by raycasting.

    public Transform groundChecker;
    public Transform spine;

    


    // Foot Steppers for each leg.
    [Header("Controllers for the steps")]
    public FootStepper LeftFoot;
    public FootStepper RightFoot;

    public int direction;

    protected Terrain terrain;
    protected CustomTerrain cterrain;
    protected float width, height;

    private Coroutine gaitCoroutine;

    private Vector3 currentGoalDirection;
    public Vector3 normalTerrain;
    private Vector3 targetVelocity;


    // Awake is called when the script instance is being loaded.
    void Start()
    {
        terrain = Terrain.activeTerrain;
        cterrain = terrain.GetComponent<CustomTerrain>();

        normalTerrain = transform.up;

        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;

        currentGoalDirection = transform.forward;
        targetVelocity = moveSpeed * transform.forward;
        gaitCoroutine = StartCoroutine(Gait());

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
        if (transform.position.x > 497 || transform.position.x < 2 || transform.position.z > 497 || transform.position.z < 2)
        {
            gameObject.GetComponent<Agent>().energy = -5;
        }
        RootAdaptation();
    }
    public void destroyFootSteps()
    {
        if (gaitCoroutine != null)
            StopCoroutine(gaitCoroutine);
        Destroy(LeftFoot.gameObject);
        Destroy(RightFoot.gameObject);
    }

    #region Root Motion

    /// <summary>
    /// Method used to move the character towards the goal.
    /// </summary>

    private void RootMotion()
    {

        // Get the vector towards the goal and projectected it on the plane defined by the normal transform.up.
        Vector2 next_goalInfo = gameObject.GetComponent<Agent>().getNextGoalInfo(); // (angle, noise)

        float newAngle = next_goalInfo[0];
        
        if (gameObject.GetComponent<Agent>().debugOn)
        {
            Debug.Log($"input {newAngle}");
        }
        if (Mathf.Abs(newAngle) < 5)
        {
            newAngle = 0;
        } else if (Mathf.Abs(newAngle) > 20)
        {
            newAngle = Mathf.Sign(newAngle) * 20;
        }

        float angleToPerform = (1 - next_goalInfo[1]) * newAngle;

        if (gameObject.GetComponent<Agent>().debugOn)
        {
            Debug.Log($"angleToPerform {angleToPerform}");
        }

        currentGoalDirection = Quaternion.Euler(0, angleToPerform, 0) * transform.forward;

        // Get the vector towards the goal and projectected it on the plane defined by the normal transform.up.
        Vector3 towardGoalProjected = Vector3.ProjectOnPlane(currentGoalDirection, normalTerrain);

        // Angles between the forward direction and the direction to the goal. 
        var angToGoal = Vector3.SignedAngle(transform.forward, towardGoalProjected, normalTerrain);

        if (gameObject.GetComponent<Agent>().debugOn)
        {
            Debug.DrawLine(transform.position + 3.5f * transform.up, transform.position + 3.5f * transform.up + 5 * towardGoalProjected, gameObject.GetComponent<Agent>().getRayColor(), 1);
        }

        // Get a perfectange of the turnSpeed to apply based on how far the goal is and its sign.
        float targetAngularVelocity = Mathf.Sign(angToGoal) * Mathf.InverseLerp(0f, 30f, Mathf.Abs(angToGoal)) * turnSpeed;

        // Initialize  root velocity.
        targetVelocity = moveSpeed * towardGoalProjected.normalized;

        // Limit velocity progressively as we approach max angular velocity.
        //targetVelocity *= Mathf.InverseLerp(turnSpeed, turnSpeed * 0.2f, Mathf.Abs(targetAngularVelocity));
      

        // Apply targetVelocity using Step() and applying.
        transform.position += targetVelocity * Time.deltaTime;

        // Apply rotation.
        transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * targetAngularVelocity, transform.up);
    }

    #endregion

    #region Root Adaptation

    /// <summary>
    /// Saves the initial position and rotation of the hips.
    /// </summary>
    private void BodyInitialize()
    {

        float distanceHit = 0;
        Vector3 posHit = Vector3.zero;
        Vector3 normalTerrain = Vector3.zero;

        Vector3 raycastOriginBody = groundChecker.position;

        if (Physics.Raycast(raycastOriginBody, -transform.up, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                posHit = hit.point;
                distanceHit = hit.distance;
                normalTerrain = hit.normal;
            }
        }
        transform.position = new Vector3(transform.position.x, posHit.y, transform.position.z);
        constantHipsPosition = new Vector3(hips.position.x, hips.position.y-posHit.y, hips.position.z);
        constantHipsRotation = new Vector3(hips.rotation.x, hips.rotation.y, hips.rotation.z);

    }

    /// <summary>
    /// In LateUpdate, after moving the root body to the target, we perform the adaptation on a top-layer to place the animal parallel to the ground.
    /// </summary>
    private void RootAdaptation()
    {
        // Origin of the ray.
        Vector3 raycastOriginBody = groundChecker.position;
        Vector3 rayCastOriginSpine = spine.position;
        float distanceHit=0;
        Vector3 posHit=Vector3.zero;
        normalTerrain = transform.up;





        // The ray information gives you where you hit and the normal of the terrain in that location.
        if (Physics.Raycast(raycastOriginBody, -transform.up, out RaycastHit hit, Mathf.Infinity, groundRaycastMask))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
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


        hips.position = new Vector3(hips.position.x, constantHipsPosition.y+posHit.y, hips.position.z);




  
        
        
        //Debug.DrawRay(rayCastOriginSpine, normalTerrain,Color.red);
        //Debug.DrawRay(rayCastOriginSpine, -spine.right,Color.white);



        if (Vector3.Dot(normalTerrain.normalized, transform.forward) < -0.5)
        {
            float angle =(90- Vector3.Angle(normalTerrain, transform.up))/4;
            Quaternion grndTilt = Quaternion.AngleAxis(angle, direction*spine.forward);
            spine.rotation = Quaternion.Slerp(spine.rotation, grndTilt * spine.rotation,  1 - Mathf.Exp(-heightAcceleration * Time.deltaTime));
        }
        else
        {
            Quaternion grndTilt = Quaternion.FromToRotation(-spine.right, transform.up);
            spine.rotation = Quaternion.Slerp(spine.rotation, grndTilt * spine.rotation, 1 - Mathf.Exp(-heightAcceleration * Time.deltaTime));
        }
        

      


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
