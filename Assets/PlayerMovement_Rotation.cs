using UnityEngine;

public class PlayerMovement_Rotation : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private bool rotationInverted = false;
    [SerializeField] private float rotationOffset = 0f;

    [Tooltip("How quickly the player rotates to the target angle")]
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleMovementAndRotation();
    }

    void HandleMovementAndRotation()
    {
        // Get input
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;

        // If there's input, move and rotate
        if (horizontal != 0f || vertical != 0f)
        {
            // Calculate target angle
            float angle = Mathf.Atan2(vertical, horizontal) * Mathf.Rad2Deg;

            // Apply rotation offset
            angle += rotationOffset;

            // Invert rotation if enabled
            if (rotationInverted)
                angle *= -1f;

            // Round to nearest 45 degrees for 8-axis rotation
            angle = Mathf.Round(angle / 45f) * 45f;

            // Create target rotation
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);

            // Smoothly rotate toward target
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Move in facing direction
            Vector3 moveDirection = transform.forward;

            rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y,
                moveDirection.z * moveSpeed
            );
        }
        else
        {
            // Stop movement when no input
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
}