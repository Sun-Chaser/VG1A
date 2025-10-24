using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player
{
    public class GameController : MonoBehaviour
    {
        // ------------------ Existing UI / Health ------------------
        private GameObject[] heartContainers;
        private Image[] heartFills;
        public TMP_Text textXP;
        public TMP_Text textHealth;
        public TMP_Text textSpeed;
        public TMP_Text textTimer;

        public Transform heartsParent;
        public GameObject heartContainerPrefab;

        public static int xp;
        public float timeElapsed;
        public float pointTimer; // Raw use for now

        public int timeLimit = 120;
        
        int SpeedLevel = 1;

        // ------------------ Enemy Spawning (NEW) ------------------
        [Header("Spawning")]
        [Tooltip("All possible spawn points in the scene")]
        public Transform[] spawnPoints;

        [Tooltip("Enemy prefabs; index 0 is 'prefab1' (capped), index 1 is the other type")]
        public GameObject[] enemyPrefabs;

        [Tooltip("Hard cap of total enemies in scene")]
        public int maxEnemies = 20;

        [Tooltip("Minimum enemies to try to keep in scene")]
        public int minEnemies = 15;

        [Tooltip("Max count allowed for enemyPrefabs[0]")]
        public int maxPrefab0 = 5;

        [Tooltip("How often (seconds) to check and (possibly) spawn")]
        public float spawnCheckInterval = 1.5f;

        private float spawnCheckTimer;

        // Track currently alive enemies we spawned
        private readonly List<GameObject> activeEnemies = new List<GameObject>();

        // Small component we tack onto spawned enemies so we can count types
        private class SpawnMeta : MonoBehaviour
        {
            public int prefabIndex;
        }

        private void Start()
        {
            // ------------------ Existing init ------------------
            heartContainers = new GameObject[(int)PlayerHealth.Instance.MaxTotalHealth];
            heartFills = new Image[(int)PlayerHealth.Instance.MaxTotalHealth];
            PlayerHealth.Instance.onHealthChangedCallback += UpdateHeartsHUD;
            InstantiateHeartContainers();
            UpdateHeartsHUD();
            
            xp = 0;
            timeElapsed = 0;
            pointTimer = 0;

            textSpeed.text = "Add Speed XP:" + ((int)(5 * SpeedLevel));
            textHealth.text = "Add Heart XP:" + (((int)PlayerHealth.Instance.Health - 3) * 5);
            textTimer.text = "2:00";

            // ------------------ Spawning init (NEW) ------------------
            spawnCheckTimer = spawnCheckInterval;

            // Optional: seed the scene up to minEnemies on start
            PrimeEnemiesToMinimum();
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            pointTimer += Time.deltaTime;
            if (timeElapsed >= timeLimit)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            // For test use only
            if (pointTimer >= 5)
            {
                pointTimer = 0;
                xp += 5;
            }
            UpdateXPDisplay();
            UpdateTimerDisplay();

            // ------------------ Spawning tick (NEW) ------------------
            spawnCheckTimer -= Time.deltaTime;
            if (spawnCheckTimer <= 0f)
            {
                SpawnTick();
                spawnCheckTimer = spawnCheckInterval;
                // print("Spawning tick");
                // print("Current enemies: " + activeEnemies.Count);
                // print("Current Enemy 0 count: " + CountPrefabIndex(0));
            }
        }

        // ------------------ Existing UI helpers ------------------
        private void UpdateXPDisplay()
        {
            textXP.text = xp.ToString();
        }

        private void UpdateTimerDisplay()
        {
            int timeLeft = timeLimit - (int)timeElapsed;
            String secondLeft = null;
            if (timeLeft % 60 < 10)
            {
                secondLeft = "0" + timeLeft % 60;
            }
            else
            {
                secondLeft = "" + timeLeft % 60;
            }
            textTimer.text = (timeLeft / 60) + ":" + secondLeft;
        }

        public void UpdateHeartsHUD()
        {
            SetHeartContainers();
            SetFilledHearts();
        }

        void SetHeartContainers()
        {
            for (int i = 0; i < heartContainers.Length; i++)
            {
                if (i < PlayerHealth.Instance.MaxHealth)
                {
                    heartContainers[i].SetActive(true);
                }
                else
                {
                    heartContainers[i].SetActive(false);
                }
            }
        }

        void SetFilledHearts()
        {
            for (int i = 0; i < heartFills.Length; i++)
            {
                if (i < PlayerHealth.Instance.Health)
                {
                    heartFills[i].fillAmount = 1;
                }
                else
                {
                    heartFills[i].fillAmount = 0;
                }
            }

            if (PlayerHealth.Instance.Health % 1 != 0)
            {
                int lastPos = Mathf.FloorToInt(PlayerHealth.Instance.Health);
                heartFills[lastPos].fillAmount = PlayerHealth.Instance.Health % 1;
            }
        }

        void InstantiateHeartContainers()
        {
            for (int i = 0; i < PlayerHealth.Instance.MaxTotalHealth; i++)
            {
                GameObject temp = Instantiate(heartContainerPrefab);
                temp.transform.SetParent(heartsParent, false);
                heartContainers[i] = temp;
                heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
            }
        }

        public void Heal()
        {
            int cost = 5;
            if (PlayerHealth.Instance.Health < PlayerHealth.Instance.MaxHealth && xp >= cost)
            {
                PlayerHealth.Instance.Heal(1.0f);
                xp -= cost;
            }
        }

        public void AddHeart()
        {
            int cost = ((int)PlayerHealth.Instance.Health - 3) * 5;
            if (PlayerHealth.Instance.MaxHealth < PlayerHealth.Instance.MaxTotalHealth && xp >= cost)
            {
                PlayerHealth.Instance.AddHealth();
                xp -= cost;

                textHealth.text = "Add Heart XP:" + (((int)PlayerHealth.Instance.Health - 3) * 5);
            }
        }

        public void AddSpeed()
        {
            int cost = (int)(5 * SpeedLevel);

            if (PlayerHealth.Instance.Speed < PlayerHealth.Instance.MaxSpeed && xp >= cost)
            {
                PlayerHealth.Instance.AddSpeed();
                xp -= cost;
                SpeedLevel++;

                textSpeed.text = "Add Speed XP:" + ((int)(5 * SpeedLevel));
            }
        }

        public static void AddXP(int amount)
        {
            xp += amount;
        }

        // =====================================================================
        //                           SPAWN SYSTEM (NEW)
        // =====================================================================

        /// <summary>
        /// Called at start to bring population up to minEnemies (respecting caps).
        /// </summary>
        private void PrimeEnemiesToMinimum()
        {
            CleanEnemyList();
            int current = activeEnemies.Count;
            if (current >= minEnemies) return;

            int toSpawn = Mathf.Min(minEnemies - current, maxEnemies - current);
            SpawnBatch(toSpawn);
        }

        /// <summary>
        /// Runs every spawnCheckInterval seconds: calculates a time/pressure-aware
        /// target population and spawns toward it while respecting caps.
        /// </summary>
        private void SpawnTick()
        {
            CleanEnemyList();

            int currentTotal = activeEnemies.Count;
            int currentPrefab0 = CountPrefabIndex(0);

            // Time left in [0, timeLimit]; clamp to avoid negatives
            float timeLeft = Mathf.Max(0f, timeLimit - timeElapsed);
            float t = Mathf.Clamp01(timeLeft / timeLimit); // 1 at start -> 0 at end

            // Target population ramps from minEnemies (early) to maxEnemies (late)
            int target = Mathf.RoundToInt(Mathf.Lerp(maxEnemies, minEnemies, t));
            target = Mathf.Clamp(target, minEnemies, maxEnemies);

            // If below target, spawn up to the smaller of (target - current) and (max - current).
            int desired = Mathf.Clamp(target - currentTotal, 0, maxEnemies - currentTotal);

            if (desired <= 0) return;

            SpawnBatch(desired);
        }

        /// <summary>
        /// Spawns up to 'count' enemies with constraints:
        /// - total <= maxEnemies
        /// - prefab 0 <= maxPrefab0
        /// </summary>
        private void SpawnBatch(int count)
        {
            // Safety checks
            if (enemyPrefabs == null || enemyPrefabs.Length < 2) return;
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            CleanEnemyList();

            int currentTotal = activeEnemies.Count;
            int remainingRoom = Mathf.Max(0, maxEnemies - currentTotal);
            int toSpawn = Mathf.Min(count, remainingRoom);

            for (int i = 0; i < toSpawn; i++)
            {
                int index = ChoosePrefabIndex();
                if (index < 0) break; // No legal prefab can be spawned right now

                Transform sp = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                GameObject inst = Instantiate(enemyPrefabs[index], sp.position, sp.rotation);
                // Tag with metadata so we can count later
                var meta = inst.AddComponent<SpawnMeta>();
                meta.prefabIndex = index;
                inst.GetComponent<SpriteRenderer>().sortingLayerName = sp.GetComponent<SpriteRenderer>().sortingLayerName;

                activeEnemies.Add(inst);
            }
        }

        /// <summary>
        /// Chooses a prefab index (0 or 1) that satisfies caps.
        /// Prefab 0 has a hard max (maxPrefab0). Prefab 1 is used otherwise.
        /// </summary>
        private int ChoosePrefabIndex()
        {
            CleanEnemyList();

            int total = activeEnemies.Count;
            if (total >= maxEnemies) return -1;

            int prefab0Count = CountPrefabIndex(0);
            bool canSpawn0 = prefab0Count < maxPrefab0;
            bool canSpawn1 = true; // no special cap besides maxEnemies

            if (canSpawn0 && canSpawn1)
            {
                // Mild randomness; you could bias based on situation if desired
                return UnityEngine.Random.value < 0.5f ? 0 : 1;
            }
            else if (canSpawn0) return 0;
            else if (canSpawn1) return 1;
            else return -1;
        }

        /// <summary>
        /// Counts alive enemies for the given prefab index.
        /// </summary>
        private int CountPrefabIndex(int prefabIndex)
        {
            int c = 0;
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                var go = activeEnemies[i];
                if (go == null) continue;
                var meta = go.GetComponent<SpawnMeta>();
                if (meta != null && meta.prefabIndex == prefabIndex) c++;
            }
            return c;
        }

        /// <summary>
        /// Removes destroyed/null entries from our active list.
        /// </summary>
        private void CleanEnemyList()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                    activeEnemies.RemoveAt(i);
            }
        }

        // (Optional) If your enemy scripts can call this OnDeath, we’ll clean faster:
        public void RegisterEnemyDeath(GameObject enemy)
        {
            if (enemy == null) return;
            int idx = activeEnemies.IndexOf(enemy);
            if (idx >= 0) activeEnemies.RemoveAt(idx);
        }
    }
}
