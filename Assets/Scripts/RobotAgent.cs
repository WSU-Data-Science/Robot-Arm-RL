using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RobotAgent : Agent
{
    public GameObject robot;
    public GameObject hand;
    public GameObject target;
    private RobotController controller;
    private TargetDetector detector;

    public void Start()
    {
        controller = robot.GetComponent<RobotController>();
        detector = hand.GetComponent<TargetDetector>();
        //Debug.Log("Number of children: " + this.transform.childCount);
    }
    
    public override void OnEpisodeBegin()
    {
        //for (int i = 0; i < controller.joints.Length; i++)
        //{
        //    ArticulationJointController joint = controller.joints[i].robotPart.GetComponent<ArticulationJointController>();
        //    joint.RotateTo(0f);

        //}

        controller.ResetRotation();
        detector.hasTouched = false;

        // Reset Target Motion
        Rigidbody targetRigidBody = target.GetComponent<Rigidbody>();
        targetRigidBody.angularVelocity = Vector3.zero;
        targetRigidBody.velocity = Vector3.zero;

        // Reset Target Position

        float radius = Mathf.Sqrt(Mathf.Pow(0.485f,2) + Mathf.Pow(-0.17f, 2));

        // 2D
        //Vector2 center = Vector2.zero;
        //Vector2 randomPoint = center + Random.insideUnitCircle * radius;
        //target.transform.position = new Vector3(randomPoint.x, 0.778f, randomPoint.y);

        //Debug.Log("New Position" + target.transform.position);

        // 3D
        Vector3 center = new Vector3(0f, 0.778f, 0f);
        Vector3 point = center + Random.insideUnitSphere * radius;
        target.transform.position = point;

        //Debug.Log("New Position" + target.transform.position);


        // Fixed
        //target.transform.position = new Vector3(0.485f, 0.778f, -0.17f);

        //Debug.Log("Bounding mesh size y: " + this.GetComponent < MeshFilter > ().mesh.bounds.extents.y);
        //Debug.Log("Bounding mesh size x: " + this.GetComponent<MeshFilter>().mesh.bounds.extents.x);
        //Debug.Log("Bounding mesh size z: " + this.GetComponent<MeshFilter>().mesh.bounds.extents.z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(0f);

        // Obtain rotation axis in radians for each joint
        for (int i = 0; i < controller.joints.Length; i++)
        {
            ArticulationJointController joint = controller.joints[i].robotPart.GetComponent<ArticulationJointController>();
            float rotation = joint.CurrentPrimaryAxisRotation();
            sensor.AddObservation(rotation);
        }

        // Observe target location
        sensor.AddObservation(target.transform.position);
        //Debug.Log("Observations : " + sensor.ToString());


    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log("Length of action: " + actions.DiscreteActions.Length);
        //Debug.Log("First Action: " + actions.DiscreteActions[0]);
        
        for (int i = 0; i < actions.DiscreteActions.Length; i++)
        {
            int actionValue = (int)(actions.DiscreteActions[i] - 1); // Turns 0,1,2 to -1,0,1 respectively
            RotationDirection direction = (RotationDirection)(actionValue); // Convert to enumerator
            controller.RotateJoint(i, direction);
            //controller.RotateJoint(i, actionValue);

            // Check for episode reset
            if (Input.GetKey(KeyCode.Space) || detector.hasTouched)
            {
                AddReward(10f);
                EndEpisode();
            }

        }
        //controller.StopAllJointRotations();

        AddReward(-0.01f);


    }

    // Manual Input from keyboard
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        for (int i = 0; i < actions.Length; i++)
        {
            float inputVal = Input.GetAxis(controller.joints[i].inputAxis);
            actions[i] = GetActionIndex(inputVal) + 1;
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("Collided with a " + collision.collider.gameObject.tag + " by a " + collision.GetContact(0).thisCollider.gameObject.name);
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            Debug.Log("Touched the Target !!");
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("" + collision.gameObject.name);
    //}

    private void Update()
    {
        

        //Vector3.dis
        //Transform lastChild = this.transform.GetChild(this.transform.childCount - 1);
        //Debug.Log("Last Child is: " + lastChild.name);
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    EndEpisode();
        //}
    }
    


    // Converts keyboard input to action index
    static int GetActionIndex(float inputVal)
    {
        if (inputVal > 0)
        {
            return 1;
        }
        else if (inputVal < 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }




}