using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DogAgent : Agent
{
    public HerdingSpawnManager spawnManager;

    public Transform sheep;
    public Transform penGoal;

    private float moveSpeed = 10f;
    private float turnSpeed = 100f;
    
    private Rigidbody rb;
    private Vector3 dogStartPos;
    private float lastDistanceToPen;

    private Animator anim;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        dogStartPos = transform.localPosition;

        if (anim != null)
        {
            anim.SetFloat("State", 0.6f);
            anim.SetFloat("Vert", 1);
        }
    }

    public override void OnEpisodeBegin()
    {

        if (spawnManager != null)
        {
            // Resetam harta:
            spawnManager.RespawnForEpisode();

            lastDistanceToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
            lastDistanceToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);

            transform.localRotation = Quaternion.identity;

            // Pozitionam cainele intr-o rotatie aleatorie
            transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }
    }

    private float lastDistanceToSheep;
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (sheep == null || penGoal == null) return;

        // Miscare agent
        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];

        // 1 = stanga, 2 = dreapta, 0 = nimic
        float rotateAmount = (turnAction == 1) ? -1f : (turnAction == 2 ? 1f : 0f);
        transform.Rotate(Vector3.up, rotateAmount * turnSpeed * Time.deltaTime);

        // 1 = inainte, 2 = inapoi, 0 = nimic
        float moveAmount = (moveAction == 1) ? 1f : (moveAction == 2 ? -0.5f : 0);
        Vector3 moveVec = transform.forward * moveAmount * moveSpeed;
        rb.linearVelocity = new Vector3(moveVec.x, rb.linearVelocity.y, moveVec.z);

        // Recompensa:

        // distante
        float currentDistToSheep = Vector3.Distance(transform.localPosition, sheep.localPosition);
        float currentDistToPen = Vector3.Distance(sheep.localPosition, penGoal.localPosition);
        
        // directii:
        Vector3 dirToSheep = (sheep.localPosition - transform.localPosition).normalized;
        Vector3 dirSheepToPen = (penGoal.localPosition - sheep.localPosition).normalized;

        // pozitia ideala pentru ghidat oaia:
        float behindSheepOffset = 8.0f;
        Vector3 idealSteeringPos = sheep.localPosition - (dirSheepToPen * behindSheepOffset);

        // distanta fata de pozitia ideala:
        float distToSteeringPos = Vector3.Distance(transform.localPosition, idealSteeringPos);

        // reward daca cainele aproape de pozitia ideala
        if (distToSteeringPos < 5.0f)
        {
            AddReward(0.001f); // Incentivizes circling to the right side
        }

        // reward daca cainele aproape de pozitia ideala si se uita in directia corecta
        float dogFaceAlignment = Vector3.Dot(transform.forward, dirSheepToPen);
        if (distToSteeringPos < 5.0f && dogFaceAlignment > 0.6f)
        {
            AddReward(0.005f);
        }

        // reward daca cainele directioneza oaia corect spre tarc
        float dotAlignment = Vector3.Dot(dirToSheep, dirSheepToPen);
        if (currentDistToSheep < 13.0f && dotAlignment > 0.8f)
        {
            AddReward(0.005f);
        }

        // apropiere de oaie recompensata, penalizare pentru departare
        float distanceDelta = lastDistanceToSheep - currentDistToSheep;
        AddReward(distanceDelta * 0.02f);

        // recompesa/ penalizare oaie - tarc
        float progress = lastDistanceToPen - currentDistToPen;

        if (progress > 0.02f)
        {
            AddReward(0.1f);
        }
        else if (progress < -0.02f)
        {
            AddReward(-0.05f);
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
            for (int i = 0; i < 14; i++) sensor.AddObservation(0f);
            return;
        }

        // Directii relative in local space-ul cainelui
        Vector3 dirToSheep = (sheep.localPosition - transform.localPosition).normalized;
        Vector3 toSheepLocal = transform.InverseTransformDirection(dirToSheep);
        sensor.AddObservation(toSheepLocal); // 3 obs

        Vector3 dirSheepToPen = (penGoal.localPosition - sheep.localPosition).normalized;
        Vector3 sheepToPenLocal = transform.InverseTransformDirection(dirSheepToPen);
        sensor.AddObservation(sheepToPenLocal); // 3 obs

        // Cross product pentru a sti daca oaia e in stanga sau dreapta
        Vector3 cross = Vector3.Cross(dirToSheep, dirSheepToPen);
        sensor.AddObservation(cross.y); // 1 obs: pozitiv - stanga, negativ - dreapta.

        // Distante normalizate
        sensor.AddObservation(Vector3.Distance(transform.localPosition, sheep.localPosition) / 250f); // 1 obs
        sensor.AddObservation(Vector3.Distance(sheep.localPosition, penGoal.localPosition) / 250f); // 1 obs

        // Alinierea cainelui cu oaia
        sensor.AddObservation(Vector3.Dot(transform.forward, dirToSheep)); // 1 obs

        // Viteza locala a cainelui
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        sensor.AddObservation(localVelocity / moveSpeed); // 3 obs

        // Alinierea caine - oaie - tarc (1 obs)
        float alignmentGoal = Vector3.Dot(dirToSheep, dirSheepToPen);
        sensor.AddObservation(alignmentGoal); // 1 obs
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