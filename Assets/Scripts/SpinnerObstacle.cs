using UnityEngine;

public class SpinnerObstacle : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 250f;

    void Update()
    {
        transform.Rotate(
            Vector3.up * rotationSpeed * Time.deltaTime,
            Space.World);
    }
}