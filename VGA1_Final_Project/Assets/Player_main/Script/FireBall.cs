using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float lifetime = 3f; 
    public int damage = 1;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        Destroy(gameObject);

    }


        void Update()
    {
        
    }
}
