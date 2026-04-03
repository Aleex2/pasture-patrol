using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DogAgent : Agent
{
    [Header("Referințe Obiecte")]
    public Transform sheep;
    public Transform penGoal;

    [Header("Setări Mișcare")]
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
        
        if (sheep != null)
        {
            sheepRb = sheep.GetComponent<Rigidbody>();
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = dogStartPos;
        transform.localRotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (sheep != null)
        {
            // Spawn random în teren
            sheep.localPosition = new Vector3(Random.Range(-3.5f, 3.5f), 0.5f, Random.Range(-3.5f, 3.5f));
            if (sheepRb != null)
            {
                sheepRb.linearVelocity = Vector3.zero;
                sheepRb.angularVelocity = Vector3.zero;
            }
        }

        lastDistanceToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
        lastDistanceToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (sheep == null || penGoal == null) return;

        // 1-3: Poziție Câine
        sensor.AddObservation(transform.localPosition);
        // 4-6: Poziție Oaie 
        sensor.AddObservation(sheep.localPosition);
        // 7-9: Poziție Țarc
        sensor.AddObservation(penGoal.localPosition);

        // 10: Direcția "Privirii" față de Oaie (Dot Product)
        // Spune AI-ului dacă se uită spre oaie (1) sau e cu spatele (-1)
        Vector3 dirToSheep = (sheep.localPosition - transform.localPosition).normalized;
        float lookAtSheep = Vector3.Dot(transform.forward, dirToSheep);
        sensor.AddObservation(lookAtSheep);

        // 11-12: Viteza (X și Z)
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // CITIRE ACȚIUNI DISCRETE
        int moveAction = actions.DiscreteActions[0]; // 0, 1, 2
        int turnAction = actions.DiscreteActions[1]; // 0, 1, 2

        // Logica Mișcare
        float moveForward = 0f;
        if (moveAction == 1) moveForward = 1f;  // Înainte
        if (moveAction == 2) moveForward = -1f; // Înapoi

        // Logica Rotație
        float turn = 0f;
        if (turnAction == 1) turn = -1f; // Stânga
        if (turnAction == 2) turn = 1f;  // Dreapta

        // Aplicare Fizică
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
        Vector3 moveVec = transform.forward * moveForward * moveSpeed;
        rb.linearVelocity = new Vector3(moveVec.x, rb.linearVelocity.y, moveVec.z);

        // RECOMPENSE
        float currentDistToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
        float currentDistToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);

        // 1. Pedeapsă serioasă pentru rotație inutilă
        if (turnAction != 0) { AddReward(-0.001f); }

        // 2. Recompensă pentru mers înainte (încurajăm activitatea)
        if (moveAction == 1) { AddReward(0.0005f); }

        // 3. Recompensă dacă se uită spre oaie și merge spre ea
        Vector3 dirToSheep = (sheep.localPosition - transform.localPosition).normalized;
        if (Vector3.Dot(transform.forward, dirToSheep) > 0.8f && moveAction == 1)
        {
            AddReward(0.001f);
        }

        // 4. Recompensă: Oaia se apropie de Țarc
        if (currentDistToPen < lastDistanceToPen)
        {
            AddReward(0.002f);
        }
        else if (currentDistToPen > lastDistanceToPen)
        {
            AddReward(-0.001f);
        }

        // Pedeapsă de timp
        AddReward(-0.0001f);

        lastDistanceToPen = currentDistToPen;
        lastDistanceToSheep = currentDistToSheep;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        
        // Vertical (W/S)
        float v = Input.GetAxisRaw("Vertical");
        if (v > 0) discreteActions[0] = 1;
        else if (v < 0) discreteActions[0] = 2;
        else discreteActions[0] = 0;

        // Horizontal (A/D)
        float h = Input.GetAxisRaw("Horizontal");
        if (h > 0) discreteActions[1] = 2;
        else if (h < 0) discreteActions[1] = 1;
        else discreteActions[1] = 0;
    }

    public void ScoredGoal()
    {
        SetReward(15.0f); // Recompensă mare pentru succes!
        Debug.Log("GOL! Succes total.");
        EndEpisode();
    }
}