using UnityEngine;

public class TileFallTrigger : MonoBehaviour
{
    public TileFallManager manager;

    private bool triggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (triggered)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        triggered = true;

        manager.StartFalling();
    }
}