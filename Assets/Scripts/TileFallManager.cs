using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFallManager : MonoBehaviour
{
    [Header("Tiles Order")]
    public List<FallingTile> tiles = new List<FallingTile>();

    [Header("Settings")]
    public float delayBetweenTiles = 0.5f;

    private bool started = false;

    public void StartFalling()
    {
        if (started)
            return;

        started = true;

        StartCoroutine(FallTilesCoroutine());
    }

    IEnumerator FallTilesCoroutine()
    {
        foreach (FallingTile tile in tiles)
        {
            tile.Fall();

            yield return new WaitForSeconds(delayBetweenTiles);
        }
    }
}