using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace EnermyTest
{
    public class BossCode : MonoBehaviour
    {
        private static readonly int MovingLeft = Animator.StringToHash("MovingLeft");
        private static readonly int Level = Animator.StringToHash("Level");
        public int maxHealth = 900;
        public int currentHealth;
        private EnermyHealthBar healthBar;
        
        // Outlet
        public float moveSpeed = 1f;
        public float shootInterval = 3f;   // time between shots
        public GameObject projectilePrefab;
        public float projectileSpeed = 3f;
        public float radialCount;
        public float detectionRadius = 10f; // start chasing/shooting inside this
        public float stopDistance = 2f;  // stop moving when this close
        public float burstDelay = 0.2f;    // spacing between shots inside a burst
        private Animator animator;

        private Transform player;
        private float shootTimer;
        
        private Coroutine healRoutine;
        public float healTickInterval = 0.1f; // seconds between ticks
        
        // Attacking Mode
        public bool trackEnermy;
        public bool shootradial;
        public int burstCount; 
        public GameObject summonPrefab;
        private int summonTime = 1;
        public GameObject orbitPrefab;

        
        
        // Start is called before the first frame update
        void Start()
        {
            healthBar = GetComponentInChildren<EnermyHealthBar>();
            currentHealth = maxHealth;
            healthBar.SetMaxHealth(maxHealth);
            
            player = GameObject.FindGameObjectWithTag("Player").transform;
            shootTimer = shootInterval;
            
            animator = GetComponent<Animator>();
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
                    animator.SetBool(MovingLeft, dir.x < 0);
                    transform.position += dir * (moveSpeed * Time.deltaTime);
                }

                // Shoot on a timer (don’t start a new burst while one is running)
                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0f)
                {
                    StartCoroutine(ShootBurst(burstCount, burstDelay));
                    shootTimer = shootInterval;
                }
            }
            // else: out of range → idle (no move, no shoot)
            
            if (currentHealth >= maxHealth * 0.67)
            {
                animator.SetInteger(Level, 1);

            }
            else if (currentHealth >= maxHealth * 0.33)
            {
                animator.SetInteger(Level, 2);
                StartHealing();
            }
            else
            {
                animator.SetInteger(Level, 3);

            }
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

        void Level1()
        {
            ShootRadial();
        }

        void Level2()
        {
            if(summonTime-- > 0)
                SummonCircle();
            else
                ShootRadial();
                
        }

        void Level3()
        {
            StartOrbitAttack();
        }
        
        IEnumerator ShootBurst(int count, float delay)
        {
            for (int i = 0; i < count; i++)
            {
                if (currentHealth >= maxHealth * 0.67)
                {
                    Level1();
                }
                else if (currentHealth >= maxHealth * 0.33)
                {
                    Level2();
                }
                else
                {
                    Level3();
                }
                yield return new WaitForSeconds(delay);
            }
        }

        void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                GameController.instance.AddXP(200);
                healthBar.SetMaxHealth(0);
                Destroy(gameObject);
                GameController.instance.AddXP(100);
            }
            healthBar.SetHealth(currentHealth);
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (((other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject).CompareTag("PlayerProjectile")))
            {
                TakeDamage(PlayerMovement.instance.fireDamage);
            }
        }

        public void StartHealing()
        {
            if (currentHealth >= maxHealth * 0.67) return;
            if (healRoutine != null) return;        // already healing
            healRoutine = StartCoroutine(healing());
        }

        private System.Collections.IEnumerator healing()
        {
            while (currentHealth < maxHealth)
            {
                // at least 1 HP per tick so int division doesn't stall
                int step = 2;
                currentHealth = Mathf.Min((int)(maxHealth * 0.67f), currentHealth + step);

                if (healthBar) healthBar.SetHealth(currentHealth);

                // wait for next tick
                yield return new WaitForSeconds(healTickInterval);
            }
            healRoutine = null;
        }
        
        public void SummonCircle(int count = 4, float radius = 1.5f, float startAngleDeg = 0f)
        {
            if (!summonPrefab || count <= 0) return;
        
            Vector3 center = transform.position;
            float step = 360f / count;
        
            for (int i = 0; i < count; i++)
            {
                float ang = (startAngleDeg + step * i) * Mathf.Deg2Rad;
                Vector3 pos = center + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * radius;
        
                GameObject obj = Instantiate(summonPrefab, pos, Quaternion.identity);
                obj.transform.rotation = Quaternion.identity;       // world rotation = 0 
                obj.transform.localRotation = Quaternion.identity;  // local rotation = 0 (in case prefab had rotation)
            }
        }
        
        public void StartOrbitAttack(int count = 3, float radius = 3f, float angularSpeedDeg = 180f, float duration = 6f)
        {
            if (!orbitPrefab || count <= 0) return;
            
                float step = 360f / count;
                for (int i = 0; i < count; i++)
                {
                    var obj = Instantiate(orbitPrefab, transform.position, Quaternion.identity);
                    var orb = obj.GetComponent<OrbitingCode>();
                    if (!orb) continue;
            
                    orb.target       = this.transform;
                    orb.radius       = radius;
                    orb.angularSpeed = angularSpeedDeg;
                    orb.startAngleDeg= step * i;
                    orb.lifetime     = duration;
                }
        }
        
    }
}

