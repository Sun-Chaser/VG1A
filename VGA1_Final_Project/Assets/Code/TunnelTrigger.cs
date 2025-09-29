using System;
using UnityEngine;

/// Attach this to a GameObject with a 2D trigger collider that spans the tunnel interior.
/// Make sure the trigger collider has `isTrigger = true`.
public class TunnelTrigger : MonoBehaviour
{
    [Header("Ceiling renderers (TilemapRenderer or SpriteRenderer)")]
    [SerializeField] private Renderer[] ceilingRenderers;

    [Header("Sorting orders")]
    [SerializeField] private int ceilingOrderInside = 280;
    [SerializeField] private int ceilingOrderOutside = 200;

    [Header("Blockers to keep player inside while in tunnel")]
    [SerializeField] private Collider2D[] blockers;   // Disable by default in Inspector
    
    [Header("Wall at the end of the tunnel")]
    [SerializeField] private Collider2D[] wallAtEndPoint;
    [SerializeField] private Renderer[] wallRenderers;

    [Header("Player filtering")]
    [SerializeField] private string playerTag = "Player";

    // If you have triggers on both ends, we use a ref-count so Enter/Exit pairs are robust.
    private int _insideCount = 0;
    
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        // Ensure initial state (outside)
        SetCeilingOrder(ceilingOrderOutside);
        SetBlockers(false);
        _spriteRenderer = GameObject.FindGameObjectWithTag(playerTag).GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _spriteRenderer = GameObject.FindGameObjectWithTag(playerTag).GetComponent<SpriteRenderer>();
        // If not on the first floor, ignore the tunnel trigger and keep the original setting
        if (_spriteRenderer.sortingOrder != 230)
        {
            var tunnel = GameObject.FindWithTag("Tunnel");
            if (tunnel && tunnel.TryGetComponent(out Collider2D col))
            {
                col.enabled = false;   // or true to re-enable
            }

            SetBlockers(false);
        }
        else
        {
            var tunnel = GameObject.FindWithTag("Tunnel");
            if (tunnel && tunnel.TryGetComponent(out Collider2D col))
            {
                col.enabled = true;   // or true to re-enable
            }

            SetBlockers(true);
        }
        

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _insideCount++;
        if (_insideCount == 1)
        {
            SetCeilingOrder(ceilingOrderInside);
            SetBlockers(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _insideCount = Mathf.Max(0, _insideCount - 1);
        if (_insideCount == 0)
        {
            SetCeilingOrder(ceilingOrderOutside);
            SetBlockers(false);
        }
    }

    // In case player spawns inside the tunnel, this keeps state correct.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_insideCount <= 0)
        {
            _insideCount = 1;
            SetCeilingOrder(ceilingOrderInside);
            SetBlockers(true);
        }
    }

    private void OnDisable()
    {
        // Safe reset when scene disables this trigger.
        _insideCount = 0;
        SetCeilingOrder(ceilingOrderOutside);
        SetBlockers(false);
    }

    private void SetCeilingOrder(int order)
    {
        if (ceilingRenderers == null) return;
        for (int i = 0; i < ceilingRenderers.Length; i++)
        {
            if (ceilingRenderers[i] != null)
                ceilingRenderers[i].sortingOrder = order;
        }
    }

    private void SetBlockers(bool enable)
    {
        if (blockers == null) return;
        for (int i = 0; i < blockers.Length; i++)
        {
            if (blockers[i] != null)
                blockers[i].enabled = enable;
        }
        for (int i = 0; i < wallAtEndPoint.Length; i++)
        {
            if (wallAtEndPoint[i] != null)
            {
                wallAtEndPoint[i].enabled = !enable;
            }
        }

        for (int i = 0; i < wallRenderers.Length; i++)
        {
            if (_spriteRenderer.sortingOrder != 230)
            {
                wallRenderers[i].sortingOrder = 225;
            }
            else
            {
                wallRenderers[i].sortingOrder = 290;
            }
        }
    }
    
}
