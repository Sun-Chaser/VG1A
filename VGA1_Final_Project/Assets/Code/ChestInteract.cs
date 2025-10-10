using UnityEngine;
using System.Collections;
using Player;

public class ChestSimple : MonoBehaviour
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

    void Start()
    {
        if (highlight != null)
        {
            highlight.GetComponent<SpriteRenderer>().sortingLayerName = this.GetComponent<SpriteRenderer>().sortingLayerName;
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

        if (highlight != null)
            highlight.SetActive(false);

        if (openedImage != null)
        {
            this.GetComponent<SpriteRenderer>().sprite = openedImage;
            GameController.AddXP(10);
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_opened) return;
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (highlight != null)
                highlight.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_opened) return;
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (highlight != null)
                highlight.SetActive(false);
        }
    }
}