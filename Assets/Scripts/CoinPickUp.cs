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

        // AUDIO
        if (pickupClip != null)
        {
            GameObject audioObj = new GameObject("CoinPickupSound");
            audioObj.transform.position = transform.position;
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = pickupClip;
            source.volume = volume;
            source.spatialBlend = 0f;
            source.Play();
            Destroy(audioObj, pickupClip.length);
        }

        // PARTICLE
        if (pickupEffect != null)
        {
            pickupEffect.transform.SetParent(null);
            pickupEffect.Play();
            Destroy(pickupEffect.gameObject, 2f);
        }

        // ADD COIN TO SESSION
        GameManager.Instance.AddSessionCoin(coinValue);

        Destroy(gameObject);
    }
}