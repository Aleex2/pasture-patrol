using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DogAgent2 : Agent
{
    public Transform sheep;
    public Transform penGoal;
    public float moveSpeed = 7f;
    public float turnSpeed = 250f;
    
    private Rigidbody rb;
    private Vector3 dogStartPos;
    private float lastDistanceToPen;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        dogStartPos = transform.localPosition;
    }

 public override void OnEpisodeBegin()
{
    transform.localPosition = dogStartPos;
    transform.localRotation = Quaternion.identity;
    if(rb != null) rb.linearVelocity = Vector3.zero;

    if (sheep == null) sheep = GameObject.Find("Sheep")?.transform;
    if (penGoal == null) penGoal = GameObject.Find("PenGoal")?.transform;

    if (sheep != null)
    {
        //respawn la oita
        sheep.localPosition = new Vector3(Random.Range(-2f, 2f), 0.5f, Random.Range(-2f, 2f));
        Rigidbody sRb = sheep.GetComponent<Rigidbody>();
        if (sRb != null) sRb.linearVelocity = Vector3.zero;
        
        if(penGoal != null)
            lastDistanceToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
            
        // distanta caine-oaie
        lastDistanceToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);
    }
}
 
private float lastDistanceToSheep;

public override void OnActionReceived(ActionBuffers actions)
{
    if (sheep == null || penGoal == null) return;

    // miscare agent
    int moveAction = actions.DiscreteActions[0];
    int turnAction = actions.DiscreteActions[1];

    float rotateAmount = (turnAction == 1) ? -1f : (turnAction == 2 ? 1f : 0f);
    transform.Rotate(Vector3.up, rotateAmount * turnSpeed * Time.deltaTime);

    float moveAmount = (moveAction == 1) ? 1f : (moveAction == 2 ? -0.5f : 0);
    Vector3 moveVec = transform.forward * moveAmount * moveSpeed;
    rb.linearVelocity = new Vector3(moveVec.x, rb.linearVelocity.y, moveVec.z);

    // recompensa
    float currentDistToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);
    float currentDistToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
    
    // aliniere caine-oaie-tarc
    Vector3 dirToSheep = (sheep.localPosition - transform.localPosition).normalized;
    Vector3 dirSheepToPen = (penGoal.localPosition - sheep.localPosition).normalized;
    float dotAlignment = Vector3.Dot(dirToSheep, dirSheepToPen);
    
    if (currentDistToSheep < 2.0f && dotAlignment > 0.8f)
    {
        AddReward(0.005f); 
    }

    // apropiere de oaie recompensata
    if (currentDistToSheep < lastDistanceToSheep) {
        AddReward(0.001f); 
    }

    // daca e foarte aproape
    if (currentDistToSheep < 1.5f) {
        AddReward(0.002f);
    }

   //oaie - tarc
    float progress = lastDistanceToPen - currentDistToPen;

    if (progress > 0.02f) 
    {
        AddReward(0.1f); 
    }
    else if (progress < -0.02f) 
    {
        AddReward(-0.05f); //penalizare pentru indepartare oaie de tarc
    }

    // penalizare pentru rotatie daca nu se uita spre oaie
    Vector3 toSheep = (sheep.localPosition - transform.localPosition).normalized;
    float alignment = Vector3.Dot(transform.forward, toSheep);
    if (turnAction != 0 && alignment > 0.9f) {
        AddReward(-0.002f);
    }

    
    AddReward(-0.001f); //penzalizare de timp
    
    if (StepCount > 4000) 
    {
        AddReward(-1.0f); 
        EndEpisode();    
    }

    lastDistanceToPen = currentDistToPen;
    lastDistanceToSheep = currentDistToSheep;
}

    public override void CollectObservations(VectorSensor sensor)
    {
       
        if (sheep == null || penGoal == null || rb == null) 
        {
            for(int i=0; i<12; i++) sensor.AddObservation(0f);
            return;
        }

        Vector3 toSheep = (sheep.localPosition - transform.localPosition).normalized;
        Vector3 sheepToPen = (penGoal.localPosition - sheep.localPosition).normalized;

        sensor.AddObservation(toSheep); 
        sensor.AddObservation(sheepToPen); 
        sensor.AddObservation(Vector3.Distance(transform.localPosition, sheep.localPosition) / 15f); 
        sensor.AddObservation(Vector3.Distance(sheep.localPosition, penGoal.localPosition) / 15f); 
        sensor.AddObservation(Vector3.Dot(transform.forward, toSheep)); 
        sensor.AddObservation(rb.linearVelocity / moveSpeed); 
        
        float alignmentGoal = Vector3.Dot(toSheep, sheepToPen);
        sensor.AddObservation(alignmentGoal); 
    
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetAxisRaw("Vertical") > 0 ? 1 : (Input.GetAxisRaw("Vertical") < 0 ? 2 : 0);
        discreteActions[1] = Input.GetAxisRaw("Horizontal") > 0 ? 2 : (Input.GetAxisRaw("Horizontal") < 0 ? 1 : 0);
    }

    public void ScoredGoal()
    {
        SetReward(100.0f);
        Debug.Log("Succes");
        EndEpisode();
    }
}