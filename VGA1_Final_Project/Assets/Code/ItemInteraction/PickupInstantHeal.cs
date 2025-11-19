using UnityEngine;
using System.Collections;
using Player;

public class PickupInstantHeal : MonoBehaviour
{
    [Header("Interaction")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.F;
    public GameObject promptOrHighlight;   // optional: e.g., an icon or "Press F" label

    [Header("FX (optional)")]
    public AudioClip pickupSfx;

    private bool _taken = false;
    private bool _playerInRange = false;

    void Awake()
    {
        if (promptOrHighlight) promptOrHighlight.SetActive(false);
    }

    void Update()
    {
        if (_taken) return;

        if (_playerInRange && Input.GetKeyDown(interactKey))
        {
            DoPickup();
        }
    }

    private void DoPickup()
    {
        _taken = true;
        if (promptOrHighlight) promptOrHighlight.SetActive(false);

        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position, 1f);

        if (PlayerHealth.instance)
            if (PlayerHealth.instance.MaxHealth - PlayerHealth.instance.Health < 3)
            {
                PlayerHealth.instance.SetHealth(PlayerHealth.instance.MaxHealth);
            }
            else
            {
                PlayerHealth.instance.SetHealth(PlayerHealth.instance.Health + 3);
            }
            

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_taken) return;
        if (!other.CompareTag(playerTag)) return;

        _playerInRange = true;
        if (promptOrHighlight) promptOrHighlight.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_taken) return;
        if (!other.CompareTag(playerTag)) return;

        _playerInRange = false;
        if (promptOrHighlight) promptOrHighlight.SetActive(false);
    }
}