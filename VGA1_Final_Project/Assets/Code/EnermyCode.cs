using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace EnermyTest
{
    public class EnermyCode : MonoBehaviour
    {
        // Outlet
        public float moveSpeed = 2f;
        public float shootInterval = 2f;   // time between shots
        public GameObject projectilePrefab;
        public float projectileSpeed = 5f;

        private Transform player;
        private float shootTimer;
        
        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            shootTimer = shootInterval;

        }

        // Update is called once per frame
        void Update()
        {
            if (!player) return;
            
            // Shooting
            shootTimer  -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Vector3 direction = player.position - transform.position;
                ShootAtPlayer(direction);
                shootTimer = shootInterval;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void ShootAtPlayer(Vector3 direction)
        {
            // Small offset so bullet spawns outside the enemy’s collider
            Vector3 spawnPos = transform.position + (Vector3)(direction * 0.15f);

            GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * projectileSpeed;
            }
        }
    }
}

