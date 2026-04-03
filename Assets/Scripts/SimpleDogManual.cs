using UnityEngine;

public class SimpleDogManual : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 720f;
    
    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // 1. Get Input (Standard WASD / Arrows)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // 2. Rotate the Dog to face the direction of movement
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 3. Move the Dog
            rb.MovePosition(transform.position + direction * speed * Time.fixedDeltaTime);

            // 4. Trigger Run Animation (Using the string from your prefab)
            animator.Play("run_forward"); 
        }
        else
        {
            // 5. If not moving, stay in "Sit" or "Idle"
            animator.Play("stand_to_sit");
        }
    }
}