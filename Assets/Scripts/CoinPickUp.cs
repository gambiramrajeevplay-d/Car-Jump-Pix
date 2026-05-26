using UnityEngine;

public class CoinPickUp : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private ParticleSystem pickupEffect;

    private AudioSource pickupSound;

    private void Awake()
    {
        // 🔊 Find AudioSource with tag "PickUp"
        GameObject audioObj = GameObject.FindGameObjectWithTag("PickUp");

        if (audioObj != null)
        {
            pickupSound = audioObj.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("[CoinPickUp] No GameObject found with tag 'PickUp'");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;


        // 🔊 Play pickup sound
        if (pickupSound != null)
            pickupSound.Play();

        // ✨ Play pickup particle
        if (pickupEffect)
        {
            pickupEffect.transform.SetParent(null);
            pickupEffect.Play();
            Destroy(pickupEffect.gameObject, 2f);
        }

        Destroy(gameObject);
    }
}
