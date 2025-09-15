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
        public float radialCount;

        private Transform player;
        private float shootTimer;
        
        // Attacking Mode
        public bool trackEnermy;
        public bool shootradial;
        public int burstCount;
        
        
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
                StartCoroutine(ShootBurst(burstCount, 0.2f));
                
                shootTimer = shootInterval;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void Shoot(Vector3 direction)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * projectileSpeed;
            }
        }
        
        void ShootRadial()
        {
            float angleStep = 360f / radialCount;
            float angle = 0f;

            for (int i = 0; i < radialCount; i++)
            {
                float dirX = Mathf.Cos(angle * Mathf.Deg2Rad);
                float dirY = Mathf.Sin(angle * Mathf.Deg2Rad);

                Vector2 direction = new Vector2(dirX, dirY).normalized;
                Shoot(direction);

                angle += angleStep;
            }
        }
        
        IEnumerator ShootBurst(int count, float delay)
        {
            for (int i = 0; i < count; i++)
            {
                if (trackEnermy)
                {
                    Vector3 direction = player.position - transform.position;
                    Shoot(direction.normalized);
                }
                if (shootradial)
                {
                    ShootRadial();
                }
                yield return new WaitForSeconds(delay);
            }
        }
    }
}

