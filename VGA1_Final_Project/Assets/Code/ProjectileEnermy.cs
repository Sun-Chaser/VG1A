using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace EnermyTest
{
    public class ProjectileEnermy : MonoBehaviour
    {
        // Outlets
        float lifeTime = 3f;
        
        // Start is called before the first frame update
        void Start()
        {
            // Destroy automatically after lifeTime
            Destroy(gameObject, lifeTime);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            Destroy(gameObject);
            if (other.gameObject.tag == "Player")
            {
                PlayerHealth.Instance.TakeDamage(1.0f);
            }
        }
    }
}

