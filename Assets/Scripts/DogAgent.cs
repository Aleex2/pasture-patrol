using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DogAgent : Agent
{
    public Transform sheep;
    public Transform penGoal;
    
    public float moveSpeed = 7f;
    public float turnSpeed = 200f;
    
    private Rigidbody rb;
    private Rigidbody sheepRb;
    private Vector3 dogStartPos;
    
    private float lastDistanceToPen;
    private float lastDistanceToSheep;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        dogStartPos = transform.localPosition;
        if (sheep != null) sheepRb = sheep.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        if (sheep == null || penGoal == null) return;
        
        transform.localPosition = dogStartPos;
        transform.localRotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        
        if (sheep != null)
        {
            sheep.localPosition = new Vector3(Random.Range(-3.5f, 3.5f), 0.5f, Random.Range(-3.5f, 3.5f));
            sheepRb.linearVelocity = Vector3.zero;
        }

        lastDistanceToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
        lastDistanceToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        Vector3 toSheep = (sheep.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(toSheep); 
        
        Vector3 sheepToPen = (penGoal.localPosition - sheep.localPosition).normalized;
        sensor.AddObservation(sheepToPen);
        
        sensor.AddObservation(Vector3.Distance(transform.localPosition, sheep.localPosition) / 15f);
        
        sensor.AddObservation(Vector3.Distance(sheep.localPosition, penGoal.localPosition) / 15f);
        
        sensor.AddObservation(Vector3.Dot(rb.linearVelocity, transform.forward));
        
        sensor.AddObservation(sheepToPen);
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];

        float rotateAmount = (turnAction == 1) ? -1f : (turnAction == 2 ? 1f : 0f);
        transform.Rotate(Vector3.up, rotateAmount * turnSpeed * Time.deltaTime);

        float moveAmount = (moveAction == 1) ? 1f : (moveAction == 2 ? -0.5f : 0f);
        Vector3 moveVec = transform.forward * moveAmount * moveSpeed;
        rb.linearVelocity = new Vector3(moveVec.x, rb.linearVelocity.y, moveVec.z);
        
        float currentDistToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);
        float currentDistSheepToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
        
        if (currentDistToSheep < lastDistanceToSheep) AddReward(0.001f);
        
        if (currentDistSheepToPen < lastDistanceToPen)
        {
            AddReward(0.01f); 
        }
        else if (currentDistSheepToPen > lastDistanceToPen)
        {
            AddReward(-0.005f); 
        }
        
        AddReward(-0.0005f);
        
        if (turnAction != 0) 
        {
            AddReward(-0.001f); 
        }
        
        if (moveAction == 1 && turnAction == 0)
        {
            AddReward(0.0005f); 
        }

        Vector3 dirToSheep = (sheep.localPosition - transform.localPosition).normalized;
        float alignment = Vector3.Dot(transform.forward, dirToSheep);

        if (moveAction == 1 && alignment > 0.8f)
        {
            AddReward(0.005f); 
        }

        lastDistanceToPen = currentDistSheepToPen;
        lastDistanceToSheep = currentDistToSheep;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        
        float v = Input.GetAxisRaw("Vertical");
        if (v > 0) discreteActions[0] = 1;
        else if (v < 0) discreteActions[0] = 2;
        else discreteActions[0] = 0;
        
        float h = Input.GetAxisRaw("Horizontal");
        if (h > 0) discreteActions[1] = 2;
        else if (h < 0) discreteActions[1] = 1;
        else discreteActions[1] = 0;
    }

    public void ScoredGoal()
    {
        SetReward(15.0f); 
        Debug.Log("succes");
        EndEpisode();
    }
}