using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class LightOverlayManager : MonoBehaviour
{
    // Adjusted in WorldGenerator or a new dedicated LightManager class
    private GameObject lightMapOverlay;
    [SerializeField] private GameObject lightMapOverlayPrefab;
    [SerializeField] private Texture2D lightMapTexture;
    [SerializeField] private Material lightMapMaterial;
    private LightGenerator lightGenerator;

    private void Awake()
    {
        lightGenerator = new LightGenerator(this);
    }

    public void UpdateLightOverlayBoundsAndPosition()
    {
        if (WorldGenerator.ActiveChunks.Count < InfiniteTerrainGenerator.RenderDistance) return;

        int minX = WorldGenerator.ActiveChunks.Keys.Min();
        int maxX = WorldGenerator.ActiveChunks.Keys.Max();

        int sizeX = (maxX - minX + 1) * WorldGenerator.ChunkSize.x;

        // Adjust the size and position of the lightMapOverlay accordingly
        if (lightMapOverlay == null)
        {
            lightMapTexture = new Texture2D(sizeX, WorldGenerator.ChunkSize.y);
            lightMapOverlay = Instantiate(lightMapOverlayPrefab);
            lightMapOverlay.transform.SetParent(this.transform, false);
            lightMapMaterial.SetTexture("_LightMap", lightMapTexture);
            lightMapOverlay.GetComponent<SpriteRenderer>().material = lightMapMaterial;
            lightMapTexture.filterMode = FilterMode.Point; //< remove this line for smooth lighting, keep it for tiled lighting
        }

     

        // Now regenerate or update the light map considering all active chunks
        StartCoroutine(RegenerateLightMapForActiveChunks());
    }

    IEnumerator RegenerateLightMapForActiveChunks()
    {
        
        float startTime = Time.realtimeSinceStartup; // Capture start time
        float[,] lightDataToApply = null;
        lightGenerator.QueueDataToGenerate(new LightGenerator.GenData
        {
            GenerationPoint = new Vector2Int(),
            OnComplete = x => lightDataToApply = x
        });

        yield return new WaitUntil(() => lightDataToApply != null);
        


        for (int x = 0; x < lightDataToApply.GetLength(0); x++)
        {
            for (int y = 0; y < lightDataToApply.GetLength(1); y++)
            {
                lightMapTexture.SetPixel(x, y, new Color(0, 0, 0, lightDataToApply[x, y]));

            }
        }
        lightMapTexture.Apply();
        int minX = WorldGenerator.ActiveChunks.Keys.Min();
        int maxX = WorldGenerator.ActiveChunks.Keys.Max();

        int sizeX = (maxX - minX + 1) * WorldGenerator.ChunkSize.x;
        lightMapOverlay.transform.position = new Vector3(minX * WorldGenerator.ChunkSize.x + sizeX / 2f, WorldGenerator.ChunkSize.y / 2f, 0);
        lightMapOverlay.transform.localScale = new Vector3(sizeX, WorldGenerator.ChunkSize.y, 1);

        float endTime = Time.realtimeSinceStartup; // Capture end time
        float elapsedTime = endTime - startTime; // Calculate elapsed time
        Debug.Log($"Light map generation and rendering took {elapsedTime} seconds.");

    }

}
