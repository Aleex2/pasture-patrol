using UnityEngine;

public class SheepController : MonoBehaviour
{
    public Transform dogTransform;
    public float fleeDistance = 5.0f;
    public float speed = 3.0f;
    
    private Animator animator; // Direct reference to the Animator component

    void Start() {
        // Get the Animator component attached to this sheep
        animator = GetComponent<Animator>();
    }

    void Update() {
        float distance = Vector3.Distance(transform.position, dogTransform.position);

        if (distance < fleeDistance) {
            FleeFromDog();
        } else {
            // Use the string name exactly as it appeared in the other script
            animator.Play("stand_to_sit"); 
         
        }
    }

    void FleeFromDog() {
        // 1. Calculăm direcția de fugă (Departe de câine)
        Vector3 direction = (transform.position - dogTransform.position);
    
        // 2. Tăiem axa Y complet din calculul direcției
        direction.y = 0; 
        direction = direction.normalized;

        // 3. Calculăm noua poziție, dar păstrăm Y-ul neschimbat
        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = currentPosition + direction * speed * Time.deltaTime;
    
        // Forțăm Y-ul să rămână la nivelul solului (de exemplu 0 sau cât e la tine)
        nextPosition.y = currentPosition.y; 

        transform.position = nextPosition;

        // 4. Rotația: Oaia se uită unde fuge, dar fără să se încline în sus/jos
        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    
        animator.Play("run_forward");
    }
}