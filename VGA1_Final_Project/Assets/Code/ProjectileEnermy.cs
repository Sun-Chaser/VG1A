using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnermyTest
{
    public class ProjectileEnermy : MonoBehaviour
    {
        // Outlets
        float lifeTime = 5f;
        
        // Start is called before the first frame update
        void Start()
        {
            // Destroy automatically after lifeTime
            Destroy(gameObject, lifeTime);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            Destroy(gameObject);
        }
    }
}

