using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;


public class OrbitingCode : MonoBehaviour
{
    public Transform target;          // set by spawner (enemy)
    public float radius = 1.5f;       // circle radius
    public float angularSpeed = 180f; // deg/sec
    public float startAngleDeg = 0f;  // initial angle
    public float lifetime = 5f;       // <=0 = infinite
    
    float angleDeg;
    
    void Start()
    {
        angleDeg = startAngleDeg;
        if (lifetime > 0f) Destroy(gameObject, lifetime);
        if (!target) { Destroy(gameObject); return; }
        UpdatePosition();
    }
    
    void Update()
    {
        if (!target) { Destroy(gameObject); return; }
        angleDeg += angularSpeed * Time.deltaTime;
        UpdatePosition();
    
        // keep minion visually unrotated
        transform.rotation = Quaternion.identity;
    }
    
    void UpdatePosition()
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 center = target.position;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        transform.position = center + offset;
    }
        
        
    void OnCollisionEnter2D(Collision2D other)
    {
        Destroy(gameObject);
        if (other.gameObject.tag == "Player")
        {
            PlayerHealth.instance.TakeDamage(2.0f);
        }
    }
        
}
