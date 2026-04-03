using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public DogAgent agent; // Tragi câinele aici în Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sheep"))
        {
            Debug.Log("Oaia a intrat în țarc!");
            agent.SetReward(10f); // Îi dăm recompensa câinelui
            agent.EndEpisode();    // Îi spunem câinelui să reseteze totul
        }
    }
}