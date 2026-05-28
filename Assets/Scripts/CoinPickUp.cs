using UnityEngine;

public class CoinPickUp : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;

    [Header("Effects")]
    [SerializeField] private ParticleSystem pickupEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 🔊 Create temporary AudioSource and play clip
        if (pickupClip != null)
        {
            GameObject audioObj = new GameObject("CoinPickupSound");

            // Spawn at coin position
            audioObj.transform.position = transform.position;

            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = pickupClip;
            source.volume = volume;
            source.spatialBlend = 0f; // 0 = 2D sound, 1 = 3D sound

            source.Play();

            // Destroy after clip finishes
            Destroy(audioObj, pickupClip.length);
        }

        // ✨ Play pickup particle
        if (pickupEffect != null)
        {
            pickupEffect.transform.SetParent(null);
            pickupEffect.Play();

            Destroy(pickupEffect.gameObject, 2f);
        }

        // 💰 Add coins here if needed
        // CoinManager.Instance.AddCoins(coinValue);

        Destroy(gameObject);
    }
}