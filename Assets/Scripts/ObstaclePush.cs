using UnityEngine;

public class ObstaclePush : MonoBehaviour
{
    [Header("Push Settings")]
    public float pushForce = 15f;
    public float upwardForce = 4f;

    [Header("Audio")]
    [SerializeField] private AudioClip pushClip;
    [SerializeField] private float volume = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

        if (playerRb == null)
            return;

        // Direction from obstacle to player
        Vector3 pushDirection =
            (collision.transform.position - transform.position).normalized;

        // Add upward force
        pushDirection.y = 0.3f;

        // Apply push
        playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);

        // 🔊 Play push sound
        if (pushClip != null)
        {
            GameObject audioObj = new GameObject("ObstaclePushSound");

            audioObj.transform.position = transform.position;

            AudioSource source = audioObj.AddComponent<AudioSource>();

            source.clip = pushClip;
            source.volume = volume;
            source.spatialBlend = 1f; // 3D sound
            source.Play();

            // Destroy after clip finishes
            Destroy(audioObj, pushClip.length);
        }
    }
}