using System.Collections;
using UnityEngine;

public class LevelFailTrigger : MonoBehaviour
{
    [Header("Fail Delay")]
    public float failDelay = 2f;

    private bool failed;

    private void OnTriggerEnter(Collider other)
    {
        // CHECK PLAYER TAG
        if (other.CompareTag("Player") && !failed)
        {
            failed = true;

            StartCoroutine(FailRoutine());
        }
    }

    IEnumerator FailRoutine()
    {
        yield return new WaitForSeconds(failDelay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelFailed();
        }
    }
}