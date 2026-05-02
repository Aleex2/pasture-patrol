using UnityEngine;
using System.Collections.Generic;

public class HerdingSpawnManager : MonoBehaviour
{
    public GameObject pen;
    public GameObject sheep;
    public GameObject dog;

    // testare generare harta:
    private bool useTimer = false;
    private float repeatRate = 3.0f;

    public float boundaryLimit = 99f;
    public int currentGeneration = 0;
    private float maxGap = 50f;
    //private int respawnEveryXEpisodes = 10; // TRAINING
    private int respawnEveryXEpisodes = 3; // TESTING
    private int episodeCounter = 0;

    // Pozitiile generate pentru a respawna in acelasi loc (respawnEveryXEpisodes)
    private Vector3 activePenPos;
    private Vector3 activeSheepPos;
    private Vector3 activeDogPos;

    // dimensiuni:
    private Vector2 penSize = new Vector2(40, 40);
    private Vector2 animalSize = new Vector2(3, 3);
    private float penRadius = 20f;
    private float animalRadius = 1.5f;

    void Start()
    {
        episodeCounter = respawnEveryXEpisodes;
        RespawnForEpisode();

        // testare generare harta:
        if (useTimer)
        {
            InvokeRepeating(nameof(RespawnForEpisode), 0f, repeatRate);
        }
    }

    public void RespawnForEpisode()
    {
        if (episodeCounter >= respawnEveryXEpisodes)
        {
            GenerateNewLayout();
            episodeCounter = 0;
            //currentGeneration++; // TRAINING
            currentGeneration += 2; // TESTING
        }
        else
        {
            ResetToActiveLayout();
        }

        episodeCounter++;
    }

    private void GenerateNewLayout()
    {
        float baseGap = 10f + (Mathf.Floor(currentGeneration / 5f) * 1f);
        baseGap = Mathf.Min(baseGap, maxGap);

        bool success = false;
        int attempts = 0;

        while (!success && attempts < 100)
        {
            attempts++;
            Vector3 penPos = GetRandomPosInBounds(penSize);
            Vector2 randomDir2D = Random.insideUnitCircle.normalized;
            Vector3 lineDir = new Vector3(randomDir2D.x, 0, randomDir2D.y);

            // TRAINING:
            // Aseaza in linie caine - oaie - tarc
            //float distToSheep = penRadius + animalRadius + baseGap;
            //Vector3 sheepPos = penPos + (lineDir * distToSheep);
            //float distToDog = animalRadius + animalRadius + baseGap;
            //Vector3 dogPos = sheepPos + (lineDir * distToDog);

            // TESTING:
            // Aseaza in linie oaie - caine - tarc

            float distToDog = penRadius + animalRadius + baseGap;
            Vector3 dogPos = penPos + (lineDir * distToDog);
            float distToSheep = animalRadius + animalRadius + baseGap;
            Vector3 sheepPos = dogPos + (lineDir * distToDog);


            if (IsInsideBounds(sheepPos, animalSize) && IsInsideBounds(dogPos, animalSize))
            {
                activePenPos = penPos;
                activeSheepPos = sheepPos;
                activeDogPos = dogPos;
                success = true;
            }
        }

        if (!success)
        {
            SetFallbackActivePositions(baseGap);
        }

        ApplyPositions();
        Debug.Log($"<color=green>New Layout Generated</color> | Gap: {baseGap} | Gen: {currentGeneration}");
    }

    private void ResetToActiveLayout()
    {
        ApplyPositions();
        Debug.Log($"Resetting to Layout (Episode {episodeCounter}/{respawnEveryXEpisodes})");
    }

    private void ApplyPositions()
    {
        pen.transform.position = activePenPos;

        sheep.transform.position = activeSheepPos;
        ResetRigidbody(sheep);

        dog.transform.position = activeDogPos;
        ResetRigidbody(dog);
    }

    private void SetFallbackActivePositions(float gap)
    {
        activePenPos = GetRandomPosInBounds(penSize);
        float distToSheep = penRadius + animalRadius + gap;
        activeSheepPos = CalculateRelativeFallback(animalSize, activePenPos, distToSheep);
        float distToDog = animalRadius + animalRadius + gap;
        activeDogPos = CalculateRelativeFallback(animalSize, activeSheepPos, distToDog);
    }

    private Vector3 CalculateRelativeFallback(Vector2 size, Vector3 anchor, float dist)
    {
        for (int i = 0; i < 50; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 pos = anchor + new Vector3(dir.x * dist, 0, dir.y * dist);
            if (IsInsideBounds(pos, size)) return pos;
        }
        return GetRandomPosInBounds(size);
    }

    private void ResetRigidbody(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
    }

    private Vector3 GetRandomPosInBounds(Vector2 size)
    {
        float x = Random.Range(-boundaryLimit + (size.x / 2f), boundaryLimit - (size.x / 2f));
        float z = Random.Range(-boundaryLimit + (size.y / 2f), boundaryLimit - (size.y / 2f));
        return new Vector3(x, 0.5f, z);
    }

    private bool IsInsideBounds(Vector3 pos, Vector2 size)
    {
        float hX = size.x / 2f;
        float hZ = size.y / 2f;
        return (pos.x - hX >= -boundaryLimit) && (pos.x + hX <= boundaryLimit) &&
               (pos.z - hZ >= -boundaryLimit) && (pos.z + hZ <= boundaryLimit);
    }
}