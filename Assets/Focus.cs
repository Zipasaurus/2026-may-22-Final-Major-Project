using UnityEngine;

public class Focus : MonoBehaviour
{
    [Tooltip("The empty GameObject to focus on (camera will orbit around this point)")]
    public Transform focusPoint;

    [Tooltip("The player's transform to match rotation")]
    public Transform playerTransform;

    [Tooltip("Speed of camera rotation with mouse")]
    public float rotationSpeed = 3f;

    [Tooltip("Maximum pitch angle (up/down) in degrees")]
    public float maxPitch = 80f;

    [Tooltip("Minimum pitch angle (up/down) in degrees")]
    public float minPitch = -80f;

    private Camera cam;
    private SoftFollow softFollow;
    private bool isFocusing = false;
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private float baseYaw = 0f;
    private float distance;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        softFollow = cam.GetComponent<SoftFollow>();

        if (focusPoint == null)
        {
            Debug.LogError("Focus point not assigned!");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player transform not assigned!");
            return;
        }

        initialPosition = cam.transform.position;
        initialRotation = cam.transform.rotation;
        distance = Vector3.Distance(cam.transform.position, focusPoint.position);
    }

    void Update()
    {
        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            if (!isFocusing)
            {
                // Snap to focus point position and rotation
                cam.transform.position = focusPoint.position;
                cam.transform.rotation = focusPoint.rotation;
                // Initialize yaw and pitch from the snapped rotation
                Vector3 euler = cam.transform.eulerAngles;
                currentYaw = euler.y;
                currentPitch = euler.x;
                if (softFollow != null) softFollow.enabled = false;
                isFocusing = true;
            }

            // Handle mouse input for rotation
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentYaw += mouseX;
            currentPitch -= mouseY; // Invert Y for natural feel
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

            // Set rotation
            cam.transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            // Keep position at focus point
            cam.transform.position = focusPoint.position;
        }
        else
        {
            if (isFocusing)
            {
                // Reset camera to initial position and rotation
                cam.transform.position = initialPosition;
                cam.transform.rotation = initialRotation;
                if (softFollow != null) softFollow.enabled = true;
                isFocusing = false;
            }
        }
    }
}