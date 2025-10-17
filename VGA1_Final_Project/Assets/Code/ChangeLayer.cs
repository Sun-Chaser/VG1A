using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChangeLayer : MonoBehaviour
{
    [Header("Tag Settings")]
    public string playerTag = "Player";
    public string enemyTag = "Enemy";

    [Header("Target Settings")]
    public SpriteRenderer targetRenderer;

    private string _originalLayer;

    private void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInParent<SpriteRenderer>();

        if (targetRenderer != null)
            _originalLayer = targetRenderer.sortingLayerName;
        else
            Debug.LogWarning("ChangeLayer: SpriteRenderer not found");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!(other.CompareTag(playerTag) || other.CompareTag(enemyTag)) || targetRenderer == null) return;

        string current = targetRenderer.sortingLayerName;
        string newLayer = current;

        switch (current)
        {
            case "L1_Mid": newLayer = "L1_Front"; break;
            case "L2_Mid": newLayer = "L2_Front"; break;
            case "L3_Mid": newLayer = "L3_Front"; break;
        }

        if (newLayer != current)
            targetRenderer.sortingLayerName = newLayer;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!(other.CompareTag(playerTag) || other.CompareTag(enemyTag)) || targetRenderer == null) return;

        if (!string.IsNullOrEmpty(_originalLayer))
            targetRenderer.sortingLayerName = _originalLayer;
    }
}