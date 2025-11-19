using UnityEngine;

public class EnemyLootDropper : MonoBehaviour
{
    [Header("Chest Drop")]
    [Range(0f,1f)] public float chestChance = 0.20f;   // 20% by default
    public GameObject chestPrefab;                     // drag your Chest prefab here
    public Vector2 spawnJitter = new Vector2(0.2f, 0.1f); // small random offset

    /// <summary>Roll and spawn a chest at position. Returns true if dropped.</summary>
    public bool TryDrop(Vector3 position)
    {
        if (chestPrefab == null) return false;
        if (Random.value > chestChance) return false;

        Vector3 dropPos = position + new Vector3(
            Random.Range(-spawnJitter.x, spawnJitter.x),
            Random.Range(-spawnJitter.y, spawnJitter.y),
            0f
        );

        var chest = Object.Instantiate(chestPrefab, dropPos, Quaternion.identity);

        // Optional: match sorting layer
        var sr = chest.GetComponent<SpriteRenderer>();
        if (sr)
            sr.sortingLayerName = GetComponent<SpriteRenderer>().sortingLayerName;

        return true;
    }
}