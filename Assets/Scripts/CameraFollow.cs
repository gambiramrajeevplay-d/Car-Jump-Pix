using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 6f, -8f);

    [Header("Follow")]
    public float followSpeed = 8f;
    public float rotationSpeed = 5f;

    [Header("Look At")]
    public Vector3 lookOffset = new Vector3(0f, 2f, 0f);

    [Header("Camera Angle")]
    public Vector3 cameraRotation = new Vector3(25f, 0f, 0f);

    void LateUpdate()
    {
        if (!target)
            return;

        // ONLY FOLLOW Y ROTATION
        Quaternion flatRotation =
            Quaternion.Euler(
                0f,
                target.eulerAngles.y,
                0f
            );

        // CAMERA POSITION
        Vector3 desiredPosition =
            target.position +
            flatRotation * offset;

        // SMOOTH FOLLOW
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // LOOK TARGET
        Vector3 lookPoint =
            target.position + lookOffset;

        // LOOK ROTATION
        Quaternion lookRotation =
            Quaternion.LookRotation(
                lookPoint - transform.position
            );

        // OPTIONAL CAMERA ANGLE OFFSET
        Quaternion desiredRotation =
            lookRotation *
            Quaternion.Euler(cameraRotation);

        // SMOOTH ROTATION
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}