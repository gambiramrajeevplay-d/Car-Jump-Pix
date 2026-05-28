using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFallManager : MonoBehaviour
{
    [Header("Tiles Order")]
    public List<FallingTile> tiles = new List<FallingTile>();

    [Header("Settings")]
    public float delayBetweenTiles = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip breakClip;
    [SerializeField] private float volume = 1f;

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
            // Break tile
            tile.Fall();

            // 🔊 Play break sound
            if (breakClip != null)
            {
                GameObject audioObj = new GameObject("TileBreakSound");

                AudioSource source = audioObj.AddComponent<AudioSource>();

                source.clip = breakClip;
                source.volume = volume;

                // IMPORTANT
                source.spatialBlend = 0f; // 2D sound

                source.Play();

                Destroy(audioObj, breakClip.length);
            }

            yield return new WaitForSeconds(delayBetweenTiles);
        }
    }
}