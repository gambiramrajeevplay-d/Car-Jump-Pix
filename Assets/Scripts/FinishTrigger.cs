using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private bool finished = false;

    private void OnTriggerEnter(Collider other)
    {
        // PREVENT MULTIPLE TRIGGERS
        if (finished)
            return;

        // CHECK PLAYER
        if (other.CompareTag("Player"))
        {
            finished = true;

            // SHOW LEVEL PASS
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelPassed();
            }
        }
    }
}