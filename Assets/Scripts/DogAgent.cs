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
        if (sheep != null) sheepRb = sheep.GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        if (sheep == null || penGoal == null) return;
        // Resetare Câine
        transform.localPosition = dogStartPos;
        transform.localRotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;

        // Resetare Oaie (Poziție random)
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
        // 1-3: Direcția relativă Câine -> Oaie (Normalizată)
        Vector3 toSheep = (sheep.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(toSheep); 

        // 4-6: Direcția relativă Oaie -> Țarc
        Vector3 sheepToPen = (penGoal.localPosition - sheep.localPosition).normalized;
        sensor.AddObservation(sheepToPen);

        // 7: Distanța Câine -> Oaie (Scalată 0-1)
        sensor.AddObservation(Vector3.Distance(transform.localPosition, sheep.localPosition) / 15f);

        // 8: Distanța Oaie -> Țarc (Scalată 0-1)
        sensor.AddObservation(Vector3.Distance(sheep.localPosition, penGoal.localPosition) / 15f);

        // 9: Viteza Câinelui (proiecție pe forward)
        sensor.AddObservation(Vector3.Dot(rb.linearVelocity, transform.forward));

        // 10-12: Poziția relativă a oii față de țarc (X, Y, Z)
        sensor.AddObservation(sheepToPen);

        // TOTAL: 12 Observații (Păstrăm Space Size 12 în Unity!)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // LOGICA DE MIȘCARE
        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];

        float rotateAmount = (turnAction == 1) ? -1f : (turnAction == 2 ? 1f : 0f);
        transform.Rotate(Vector3.up, rotateAmount * turnSpeed * Time.deltaTime);

        float moveAmount = (moveAction == 1) ? 1f : (moveAction == 2 ? -0.5f : 0f);
        Vector3 moveVec = transform.forward * moveAmount * moveSpeed;
        rb.linearVelocity = new Vector3(moveVec.x, rb.linearVelocity.y, moveVec.z);

        // RECOMPENSE
        float currentDistToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);
        float currentDistSheepToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);

        // 1. Recompensă pentru apropierea de oaie (îl învață să meargă la ea)
        if (currentDistToSheep < lastDistanceToSheep) AddReward(0.001f);

        // 2. RECOMPENSA PRINCIPALĂ: Oaia se mișcă spre țarc
        if (currentDistSheepToPen < lastDistanceToPen)
        {
            AddReward(0.01f); // Recompensă mare pentru progres real
        }
        else if (currentDistSheepToPen > lastDistanceToPen)
        {
            AddReward(-0.005f); // Pedeapsă dacă oaia fuge de țarc
        }

        // 3. Penalizare de timp (Grăbește-te!)
        AddReward(-0.0005f);
        
        // --- LOGICA DE RECOMPENSE ANTI-ROTAȚIE ---

        // 1. PENALIZARE PENTRU ROTAȚIE (Dacă rotește, scade puncte)
        if (turnAction != 0) 
        {
            AddReward(-0.001f); // O penalizare mică, dar constantă pentru fiecare frame de rotație
        }

        // 2. RECOMPENSĂ PENTRU MERS DREPT (Doar dacă merge înainte fără să rotească)
        if (moveAction == 1 && turnAction == 0)
        {
            AddReward(0.0005f); // Îl încurajăm să aibă traiectorii drepte
        }

        // 3. RECOMPENSĂ DE ALINIERE (Cea mai importantă)
        // Îi dăm puncte doar dacă se uită spre oaie ÎN TIMP ce merge
        Vector3 dirToSheep = (sheep.localPosition - transform.localPosition).normalized;
        float alignment = Vector3.Dot(transform.forward, dirToSheep);

        if (moveAction == 1 && alignment > 0.8f)
        {
            AddReward(0.005f); // Recompensă mare pentru mers direct către țintă
        }

        lastDistanceToPen = currentDistSheepToPen;
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