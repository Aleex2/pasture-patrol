using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public DogAgent2 agent; 

    private void OnTriggerEnter(Collider other)
    {
        
        Debug.Log("Tarcul a fost atins de: " + other.name + " cu tag-ul: " + other.tag);

        if (other.CompareTag("sheep"))
        {
            Debug.Log("SUCCES!");
            agent.ScoredGoal(); 
        }
    }
}