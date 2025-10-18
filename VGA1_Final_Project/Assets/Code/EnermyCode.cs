using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace EnermyTest
{
    public class EnermyCode : MonoBehaviour
    {
        public int maxHealth = 100;
        public int currentHealth;
        private EnermyHealthBar healthBar;
        
        // Outlet
        public float moveSpeed = 2f;
        public float shootInterval = 2f;   // time between shots
        public GameObject projectilePrefab;
        public float projectileSpeed = 5f;
        public float radialCount;
        public float detectionRadius = 6f; // start chasing/shooting inside this
        public float stopDistance = 1.2f;  // stop moving when this close
        public float burstDelay = 0.2f;    // spacing between shots inside a burst
        private bool isBursting = false;   // prevent overlapping bursts

        private Transform player;
        private float shootTimer;
        
        // Attacking Mode
        public bool trackEnermy;
        public bool shootradial;
        public int burstCount;
        
        
        // Start is called before the first frame update
        void Start()
        {
            healthBar = GetComponentInChildren<EnermyHealthBar>();
            currentHealth = maxHealth;
            healthBar.SetMaxHealth(maxHealth);
            
            player = GameObject.FindGameObjectWithTag("Player").transform;
            shootTimer = shootInterval;
        }

        // Update is called once per frame
        void Update()
        {
            if (!player) return;

            // Only act if player is close enough
            float dist = Vector2.Distance(player.position, transform.position);
            bool inRange = dist <= detectionRadius;

            if (inRange)
            {
                // Move toward player until close enough
                if (dist > stopDistance)
                {
                    Vector3 dir = (player.position - transform.position).normalized;
                    transform.position += dir * (moveSpeed * Time.deltaTime);
                }

                // Shoot on a timer (don’t start a new burst while one is running)
                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0f && !isBursting)
                {
                    StartCoroutine(ShootBurst(burstCount, burstDelay));
                    shootTimer = shootInterval;
                }
            }
            // else: out of range → idle (no move, no shoot)
            
            
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void Shoot(Vector3 direction)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<SpriteRenderer>()
                .sortingLayerName = this.GetComponent<SpriteRenderer>().sortingLayerName;
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
            isBursting = true;
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
            isBursting = false;
        }

        void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                healthBar.SetMaxHealth(0);
                Destroy(gameObject);
            }
            healthBar.SetHealth(currentHealth);
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<FireBall>())
            {
                GameController.AddXP(5);
                TakeDamage(10);
            }
        }
    }
}

