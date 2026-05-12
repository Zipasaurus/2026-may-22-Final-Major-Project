using UnityEngine;

public class SoftFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -8f);

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;

    [Header("Rotation Settings")]
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private float rotationSpeed = 5f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Desired position
        Vector3 desiredPosition = target.position + offset;

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Optional smooth look at
        if (lookAtTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(
                target.position - transform.position
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}