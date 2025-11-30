using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class HammerBall : MonoBehaviour
{
    public float lifetime = 1f;
    public string EnemyProjectile = "EnemyProjectile";

    Quaternion rot;
    void Awake() { rot = transform.rotation; }
    void LateUpdate() { transform.rotation = rot; }

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
 
        GameObject hit = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;

        if (hit.CompareTag(EnemyProjectile))
        {
            Destroy(hit);     
        }
    }


}
