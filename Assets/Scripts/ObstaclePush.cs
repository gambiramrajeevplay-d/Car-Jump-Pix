using UnityEngine;

public class ObstaclePush : MonoBehaviour
{
    public float pushForce = 15f;
    public float upwardForce = 4f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

        if (playerRb == null)
            return;

        // Direction from obstacle center to player
        Vector3 pushDirection =
            (collision.transform.position - transform.position).normalized;

        // Add slight upward force
        pushDirection.y = 0.3f;

        playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }
}