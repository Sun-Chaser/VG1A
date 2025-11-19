using UnityEngine;
using System.Collections;
using Player;

public class ChestInteract : MonoBehaviour
{
    [Header("Refs")]
    public GameObject highlight;   
    public Sprite openedImage;  

    [Header("Config")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.F;
    public float destroyDelay = 0.5f;

    private bool _playerInRange = false;
    private bool _opened = false;
    
    [Header("Loot (equal probability)")]
    public GameObject[] items;                 // ← put your item prefabs here (e.g., Temporal Speed Up as one element)
    public bool includeDirectXP = false;        // ← adds an extra equally likely choice: grant XP instead of spawning
    public Vector2Int directXpRange = new Vector2Int(10, 25); // XP range when Direct XP is picked
    public Vector2 spawnOffset = new Vector2(0f, 0.2f);       // small offset so item isn’t hidden by chest

    void Start()
    {
        if (highlight != null)
        {
            highlight.GetComponent<SpriteRenderer>().sortingLayerName =
                GetComponent<SpriteRenderer>().sortingLayerName;
            highlight.SetActive(false);
        }
    }

    void Update()
    {
        if (_playerInRange && !_opened && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenChest());
        }
    }

    private IEnumerator OpenChest()
    {
        _opened = true;

        if (highlight != null) highlight.SetActive(false);
        SoundManager.instance.PlayChestOpenClip();

        if (openedImage != null)
            GetComponent<SpriteRenderer>().sprite = openedImage;

        // Optional: base XP for opening a chest (keep or remove)
        GameController.instance.AddXP(10);

        // ----------------- Equal-probability loot roll -----------------
        int prefabCount = (items != null) ? items.Length : 0;
        int options = prefabCount + (includeDirectXP ? 1 : 0);

        if (options > 0)
        {
            int pick = Random.Range(0, options);

            // Direct XP is treated as the last slot
            bool pickedDirectXP = includeDirectXP && (pick == options - 1);

            if (pickedDirectXP)
            {
                int amount = Random.Range(directXpRange.x, directXpRange.y + 1);
                GameController.instance.AddXP(amount);
            }
            else
            {
                // Spawn the selected prefab
                GameObject prefab = items[pick];
                if (prefab != null)
                {
                    Vector3 pos = transform.position + (Vector3)spawnOffset;
                    prefab.GetComponent<SpriteRenderer>().sortingLayerName = GetComponent<SpriteRenderer>().sortingLayerName;
                    Instantiate(prefab, pos, Quaternion.identity);
                }
            }
        }
        // ----------------------------------------------------------------

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_opened) return;
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (highlight != null) highlight.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_opened) return;
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (highlight != null) highlight.SetActive(false);
        }
    }
}
