using UnityEngine;

public class DebugDisable : MonoBehaviour
{
    private void OnDisable()
    {
        Debug.Log("CoinTextHUD DISABLED by something!", this);
    }
}