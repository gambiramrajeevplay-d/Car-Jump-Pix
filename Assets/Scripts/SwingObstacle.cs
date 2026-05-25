using UnityEngine;

public class SwingObstacle : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingAngle = 60f;

    public float swingSpeed = 2f;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float angle =
            Mathf.Sin(Time.time * swingSpeed) * swingAngle;

        // SWING ON Z AXIS
        transform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, angle);
    }
}