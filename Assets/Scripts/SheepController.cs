using UnityEngine;

public class SheepController : MonoBehaviour
{
    public Transform dogTransform;

    private float fleeDistance = 12f;
    private float speed = 6.0f;

    private float sitDelay = 2f;
    private float sitTimer = 0f;

    private Animator animator; 

    void Start() {
        animator = GetComponent<Animator>();
    }

    void Update() {
        float distance = Vector3.Distance(transform.position, dogTransform.position);

        if (distance < fleeDistance) {
            sitTimer = 0f;
            FleeFromDog();
        } else {

            sitTimer += Time.deltaTime;
            if (sitTimer >= sitDelay)
            {
                animator.Play("stand_to_sit");
            } else if (sitTimer > 0.2f)
            {
                animator.Play("idle");
            }

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
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 180f * Time.deltaTime);
        }
    
        animator.Play("run_forward");
    }
}