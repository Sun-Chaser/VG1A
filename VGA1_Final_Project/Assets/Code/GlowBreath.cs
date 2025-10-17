using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GlowPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float speed = 2f;        // Pulse speed
    public float minAlpha = 0.25f;  // Minimum transparency
    public float maxAlpha = 0.6f;   // Maximum transparency

    [Header("Color Settings")]
    public Color colorA = Color.white;   // Start color
    public Color colorB = Color.yellow;  // End color

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // t oscillates between 0 and 1
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;

        // Smoothly interpolate between colorA and colorB
        Color c = Color.Lerp(colorA, colorB, t);

        // Apply alpha pulse
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);

        // Set final color
        _sr.color = c;
    }
}