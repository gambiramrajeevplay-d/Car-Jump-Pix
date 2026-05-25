using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingTile : MonoBehaviour
{
    private Rigidbody rb;

    private bool hasFallen = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
    }

    public void Fall()
    {
        if (hasFallen)
            return;

        hasFallen = true;

        transform.parent = null;

        rb.isKinematic = false;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;
    }
}