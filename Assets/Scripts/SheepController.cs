using UnityEngine;

public class SheepController : MonoBehaviour
{
    public Transform dogTransform;
    public float fleeDistance = 5.0f;
    public float speed = 3.0f;
    
    private Animator animator; 

    void Start() {
        animator = GetComponent<Animator>();
    }

    void Update() {
        float distance = Vector3.Distance(transform.position, dogTransform.position);

        if (distance < fleeDistance) {
            FleeFromDog();
        } else {
            animator.Play("stand_to_sit"); 
         
        }
    }

    void FleeFromDog() {
       
        Vector3 direction = (transform.position - dogTransform.position);
        
        direction.y = 0; 
        direction = direction.normalized;
        
        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = currentPosition + direction * speed * Time.deltaTime;
        
        nextPosition.y = currentPosition.y; 

        transform.position = nextPosition;
        
        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    
        animator.Play("run_forward");
    }
}