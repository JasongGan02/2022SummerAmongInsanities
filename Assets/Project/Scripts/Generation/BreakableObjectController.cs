using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public class BreakableObjectController : MonoBehaviour, IDamageable
{
    private IBreakableObject tile;
    private float healthPoint;
    private WorldGenerator terrainGeneration;
    private GameObject breaker;

    
    [HideInInspector] public bool isPlacedByPlayer;
    public void Initialize(TileObject tile, int hp, bool isPlacedByPlayer)
    {
        healthPoint = hp;
        this.tile = tile;
        this.isPlacedByPlayer = isPlacedByPlayer;
    }

    public void SetBreaker(GameObject breaker)
    {
        this.breaker = breaker;
    }
    
    private void OnObjectDestroyed(IDamageSource source)
    {
        AudioEmitter audioEmitter = null;
        if (breaker != null) //Player
        {
            audioEmitter = breaker.GetComponent<AudioEmitter>();
            audioEmitter.StopAudio();
        }
        else
        {
            audioEmitter = source.SourceGameObject.GetComponent<AudioEmitter>();
        }
        audioEmitter.PlayClipFromCategory("TileAfterBreaking");
        var drops = tile.GetDroppedGameObjects(isPlacedByPlayer, transform.position);
        Vector2Int chunkCoord = new Vector2Int(WorldGenerator.GetChunkCoordsFromPosition(transform.position), 0);
        Vector2Int worldPostion = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        Vector2Int localPosition = WorldGenerator.WorldToLocalCoords(worldPostion, chunkCoord.x);
        if (WorldGenerator.WorldData.ContainsKey(chunkCoord))
        {
            // Remove the tile entry from the dictionary
            WorldGenerator.WorldData[chunkCoord][localPosition.x, localPosition.y, ((TileObject)tile).TileLayer] = null;
        }
        if (((IGenerationObject)tile).NeedsBackground && !isPlacedByPlayer)
        {
            WorldGenerator.WorldData[chunkCoord][localPosition.x, localPosition.y, 0] = (TileObject)tile; 
            WorldGenerator.PlaceTile(((TileObject)tile), worldPostion.x, worldPostion.y, chunkCoord, true, false);
        }
        foreach (GameObject droppedItem in drops)
        {
            droppedItem.transform.parent = gameObject.transform.parent;
            droppedItem.transform.position = gameObject.transform.position;

            // Apply random torque
            float randomTorque = Random.Range(-20f, 20f); // Adjust the range as needed
            droppedItem.GetComponent<Rigidbody2D>().AddTorque(randomTorque);
        }
        WorldGenerator.Instance.RefreshChunkLight(chunkCoord, true);
    }

    public void TakeDamage(float amount, IDamageSource source)
    {
        healthPoint -= amount;
        if (healthPoint <= 0)
        {
            Destroy(gameObject);
            OnObjectDestroyed(source);
        }
    }

    public float CalculateDamage(float incomingAtkDamage, float attackerCritChance, float attackerCritDmgCoef)
    {
        // Determine if this is a critical hit
        bool isCritical = UnityEngine.Random.value < attackerCritChance;

        // Calculate base damage with critical hit consideration
        float baseDamage = isCritical ? incomingAtkDamage * attackerCritDmgCoef : incomingAtkDamage;
        
        return baseDamage;
    }
    
}
