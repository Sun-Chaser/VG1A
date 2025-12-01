using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance;

        // ---------------- UI ----------------
        [Header("UI")]
        public TMP_Text textXP;
        public TMP_Text textScore;
        public TMP_Text textTimer;     // normal phase timer
        public TMP_Text textBossPrep;  // reused for "prepare" and boss countdown
        public Image imageTimer;

        [Header("Hearts UI")]
        public Transform heartsParent;
        public GameObject heartContainerPrefab;
        
        [Header("Hints")]
        public TMP_Text textHint;              // assign in Inspector
        [SerializeField] float hintInterval = 20f;   // check every 20s
        [SerializeField] float hintDuration = 5f;    // show for 5s
        private int _fallbackHintIndex = 0;          // rotate F / Shift
        private Coroutine _hintLoopCo;

        // ---------------- Player/Score/Timer ----------------
        [Header("Player Spawn")]
        public Transform[] playerSpawnPoints; // assign in Inspector
        
        [Header("Game State")]
        public int xp;
        public int score;
        public float timeElapsed;
        public int timeLimit = 120;       // normal phase duration (seconds)
        public int bossTimeLimit = 60;    // boss phase duration (seconds)

        // Levels
        [Header("Levels")]
        public int HealthLevel = 1;
        public int SpeedLevel = 1;
        public int FireballLevel = 1;
        public int FireballSpeedLevel = 1;
        public int MaxHealthLevel = 7;
        public int MaxSpeedLevel = 10;
        public int MaxFireballLevel = 10;
        public int MaxFireballSpeedLevel = 10;
        
        [Header("Leveling (XP -> Level -> Points)")]
        public int level = 1;
        public int upgradePoints = 0;
        public int xpLinearA = 10;
        public int xpLinearB = 20;
        private int _xpForNext;

        public GameObject[] playerPrefabs;
        public GameObject selectPanelGO;
        public float swapWindowSeconds = 5f;
        private bool canSwap = false;
        private GameObject _currentPlayer;

        // ---------------- Boss ----------------
        [Header("Boss")]
        public Transform bossSpawnPoint;

        // keep the old single boss as fallback (optional)
        public GameObject bossPrefab;

        // New: allow multiple boss types and how many to spawn
        public GameObject[] bossPrefabs;   // if empty/null, we’ll use bossPrefab
        public int bossCount = 3;          // how many bosses to spawn together
        public float bossSpawnRadius = 2.5f; // spread around the spawn point
        
        // Track active bosses
        private readonly List<GameObject> _activeBosses = new();

        // ---------------- Spawning ----------------
        [Header("Enemy Spawning")]
        [Tooltip("All possible spawn points in the scene")]
        public Transform[] spawnPoints;

        [Tooltip("Enemy prefabs; index 0 is the capped type")]
        public GameObject[] enemyPrefabs;

        [Tooltip("Hard cap of total enemies in scene")]
        public int maxEnemies = 20;

        [Tooltip("Minimum enemies to try to keep in scene (normal phase)")]
        public int minEnemies = 15;
        
        [Tooltip("Capped prefab index start")]
        public int cappedGroupStart = 0;
        [Tooltip("Capped prefab index end")]
        public int cappedGroupEndInclusive = 1;
        [Tooltip("Max count allowed for certain enemyPrefabs")]
        public int cappedGroupMax = 20;
        
        [Tooltip("How often (seconds) to check and possibly spawn")]
        public float spawnCheckInterval = 1.5f;

        // ---------------- private state ----------------
        private GameObject[] _heartContainers;
        private UnityEngine.UI.Image[] _heartFills;

        private float _spawnCheckTimer;
        private readonly List<GameObject> _activeEnemies = new();
        private readonly Dictionary<GameObject, int> _prefabIndexByEnemy = new();

        private bool _bossSequenceStarted = false;
        private bool _resultsQueued = false;
        
        // ---- Speed buff stack state ----
        private struct SpeedBuff { public float multiplier; public float endTime; }
        private readonly List<SpeedBuff> _speedBuffs = new();
        private bool  _hasSpeedBaseline = false;
        private float _baseWalkSpeed = 0f;
        private float _baseSpeed     = 0f;

        // ---------------- Unity lifecycle ----------------
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            // Hearts UI build
            _heartContainers = new GameObject[(int)PlayerHealth.instance.MaxTotalHealth];
            _heartFills      = new UnityEngine.UI.Image[(int)PlayerHealth.instance.MaxTotalHealth];
            PlayerHealth.instance.onHealthChangedCallback += UpdateHeartsHUD;
            InstantiateHeartContainers();
            UpdateHeartsHUD();

            // Init state
            xp = 0;
            score = 0;
            timeElapsed = 0;

            if (textTimer) textTimer.text = "2:00";
            if (textBossPrep) textBossPrep.gameObject.SetActive(false);

            ShowSelectPanel(true);
            canSwap = true;
            StartCoroutine(SelectionWindow());

            // Spawner
            _spawnCheckTimer = spawnCheckInterval;
            PrimeEnemiesToMinimum();
            
            // Player Spawner
            SpawnPlayerAtRandomPoint();
            
            // Hint System
            if (textHint) textHint.gameObject.SetActive(false);
            _hintLoopCo = StartCoroutine(HintLoop());
            
            // Level calculation
            _xpForNext = CalcXpForNext(level);
        }

        private void SpawnPlayerAtRandomPoint()
        {
            if (playerSpawnPoints == null || playerSpawnPoints.Length == 0) return;

            // Find player
            var playerGO = PlayerMovement.instance ? PlayerMovement.instance.gameObject
                : GameObject.FindWithTag("Player");
            if (!playerGO) return;

            // Pick a random valid point
            Transform chosen = playerSpawnPoints[Random.Range(0, playerSpawnPoints.Length)];

            // Teleport (2D-friendly)
            var rb2d = playerGO.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                rb2d.velocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
                rb2d.position = (Vector2)chosen.position;
                rb2d.rotation = chosen.eulerAngles.z;
            }
            else
            {
                playerGO.transform.SetPositionAndRotation(chosen.position, chosen.rotation);
            }

            // Ensure desired sorting layer
            var sr = playerGO.GetComponent<SpriteRenderer>();
            if (sr) sr.sortingLayerName = chosen.GetComponent<SpriteRenderer>().sortingLayerName;
        }

        private IEnumerator HintLoop()
        {
            // small initial delay to avoid overlapping with early UI updates (optional)
            yield return new WaitForSeconds(1f);

            while (true)
            {
                // Wait until it's time to check
                yield return new WaitForSeconds(hintInterval);
                
                if (_bossSequenceStarted) continue; // inside the while loop, right after the 20s wait


                // Pick the best hint for the current state
                string msg = ChooseHintMessage();

                // Show it for hintDuration seconds
                if (!string.IsNullOrEmpty(msg) && textHint != null)
                {
                    textHint.text = msg;
                    textHint.gameObject.SetActive(true);
                    yield return new WaitForSeconds(hintDuration);
                    textHint.gameObject.SetActive(false);
                }
            }
        }

        private string ChooseHintMessage()
        {
            // 1) Health not full?
            if (PlayerHealth.instance != null &&
                PlayerHealth.instance.Health < PlayerHealth.instance.MaxHealth)
            {
                return "Remember H is for Heal";
            }

            // 2) Enough XP to upgrade?
            if (upgradePoints > 0)
            {
                return "Remember press M to shop and level up";
            }

            // 3) Rotate fallback hints
            string[] fallbacks = {
                "Remember press F to open chest",
                "Remember hold Shift to speed up"
            };
            string pick = fallbacks[_fallbackHintIndex % fallbacks.Length];
            _fallbackHintIndex++;
            return pick;
        }
        
        private void Update()
        {
            // -------- Normal phase timing --------
            if (!_bossSequenceStarted)
            {
                timeElapsed += Time.deltaTime;
                if (timeElapsed >= timeLimit && !_bossSequenceStarted)
                {
                    _bossSequenceStarted = true;
                    GameObject.FindWithTag("Player").GetComponent<SpriteRenderer>().sortingLayerName = "L1_Chars";
                    StartCoroutine(BossSequence());
                }
            }

            // -------- Dev/test tick to give XP every 5s (optional) --------
            // Remove if not desired
            // (kept as a tiny throttle so you still see progression during tests)
            // ---------------------------------------------------------------
            // none here by default

            // -------- UI updates --------
            UpdateXPDisplay();
            UpdateScoreDisplay();
            if (!_bossSequenceStarted) UpdateTimerDisplay();

            // -------- Spawning (disabled during boss) --------
            if (!_bossSequenceStarted)
            {
                _spawnCheckTimer -= Time.deltaTime;
                if (_spawnCheckTimer <= 0f)
                {
                    SpawnTick();
                    _spawnCheckTimer = spawnCheckInterval;
                }
            }
        }

        // =====================================================================
        // UI helpers
        // =====================================================================
        private void UpdateXPDisplay()
        {
            if (!textXP) return;
            textXP.text = $"{xp}/{_xpForNext}";
        }

        private void UpdateScoreDisplay()
        {
            if (textScore) textScore.text = $"Score: {score}";
        }

        private void UpdateTimerDisplay()
        {
            if (!textTimer) return;
            int timeLeft = Mathf.Max(0, timeLimit - Mathf.FloorToInt(timeElapsed));
            textTimer.text = FormatMMSS(timeLeft);
        }

        public void UpdateHeartsHUD()
        {
            SetHeartContainers();
            SetFilledHearts();
        }

        private void SetHeartContainers()
        {
            for (int i = 0; i < _heartContainers.Length; i++)
            {
                bool on = i < PlayerHealth.instance.MaxHealth;
                _heartContainers[i].SetActive(on);
            }
        }

        private void SetFilledHearts()
        {
            for (int i = 0; i < _heartFills.Length; i++)
            {
                _heartFills[i].fillAmount = (i < PlayerHealth.instance.Health) ? 1f : 0f;
            }

            if (PlayerHealth.instance.Health % 1 != 0)
            {
                int lastPos = Mathf.FloorToInt(PlayerHealth.instance.Health);
                if (lastPos >= 0 && lastPos < _heartFills.Length)
                    _heartFills[lastPos].fillAmount = PlayerHealth.instance.Health % 1f;
            }
        }

        private void InstantiateHeartContainers()
        {
            for (int i = 0; i < PlayerHealth.instance.MaxTotalHealth; i++)
            {
                GameObject temp = Instantiate(heartContainerPrefab, heartsParent, false);
                _heartContainers[i] = temp;
                _heartFills[i] = temp.transform.Find("HeartFill").GetComponent<UnityEngine.UI.Image>();
            }
        }

        // Quick actions
        public void Heal()
        {
            const int cost = 5;
            if (PlayerHealth.instance.Health < PlayerHealth.instance.MaxHealth && xp >= cost)
            {
                SoundManager.instance?.PlayHealClip();
                PlayerHealth.instance.Heal(1.0f);
                xp -= cost;
            }
        }

        public void AddXP(int amount)
        {
            // XP for level up
            xp += amount;
            bool leveled = false;

            // Safe calculation
            while (xp >= _xpForNext)
            {
                xp -= _xpForNext;         // Deduct current point
                level += 1;               // Level up
                upgradePoints += 1;       // Get upgrade point
                _xpForNext = CalcXpForNext(level);
                leveled = true;
            }

            if (leveled)
            {
                SoundManager.instance.PlayLevelUpClip();
                MenuController.instance?.UpdateUpgradePointsDisplay();
            }

            // ADd to score
            score += amount;

            // Update XP/Score UI
            UpdateXPDisplay();
            UpdateScoreDisplay();
        }

        private int CalcXpForNext(int currentLevel)
        {
            // EXP_next = a * Level + b
            return Mathf.Max(1, xpLinearA * currentLevel + xpLinearB);
        }

        /// <summary>Try to consume 1 point to upgrade</summary>
        public bool TrySpendUpgradePoint()
        {
            if (upgradePoints > 0)
            {
                upgradePoints--;
                return true;
            }
            return false;
        }

        // =====================================================================
        // Spawn system (normal phase only)
        // =====================================================================
        private void PrimeEnemiesToMinimum()
        {
            CleanEnemyLists();
            int current = _activeEnemies.Count;
            if (current >= minEnemies) return;

            int toSpawn = Mathf.Min(minEnemies - current, maxEnemies - current);
            SpawnBatch(toSpawn);
        }

        private void SpawnTick()
        {
            CleanEnemyLists();

            int currentTotal = _activeEnemies.Count;

            // Ramp target from maxEnemies (late) to minEnemies (early)
            float timeLeft = Mathf.Max(0f, timeLimit - timeElapsed);
            float t = Mathf.Clamp01(timeLeft / timeLimit); // 1 at start -> 0 at end
            int target = Mathf.RoundToInt(Mathf.Lerp(maxEnemies, minEnemies, t));
            target = Mathf.Clamp(target, minEnemies, maxEnemies);

            int desired = Mathf.Clamp(target - currentTotal, 0, maxEnemies - currentTotal);
            if (desired <= 0) return;

            SpawnBatch(desired);
        }

        private void SpawnBatch(int count)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length < 2) return;
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            CleanEnemyLists();

            int room = Mathf.Max(0, maxEnemies - _activeEnemies.Count);
            int toSpawn = Mathf.Min(count, room);

            for (int i = 0; i < toSpawn; i++)
            {
                int index = ChoosePrefabIndex();
                if (index < 0) break;

                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                
                Vector2 offset2D = Random.insideUnitCircle * 1.5f;
                Vector3 spawnPos = sp.position + new Vector3(offset2D.x, offset2D.y, 0f);
                var inst = Instantiate(enemyPrefabs[index], spawnPos, sp.rotation);

                // adopt spawnpoint's sorting layer if present
                var spSR = sp.GetComponent<SpriteRenderer>();
                var instSR = inst.GetComponent<SpriteRenderer>();
                if (spSR && instSR) instSR.sortingLayerName = spSR.sortingLayerName;

                _activeEnemies.Add(inst);
                _prefabIndexByEnemy[inst] = index;
            }
        }

        private int ChoosePrefabIndex()
        {
            CleanEnemyLists();

            // maxEnemies=> Not spawn
            if (_activeEnemies.Count >= maxEnemies) return -1;

            // Available pool for enemy
            var eligible = BuildEligiblePrefabIndices();
            if (eligible.Count == 0) return -1;
            
            /*
            // Random spawn
            int pick = eligible[Random.Range(0, eligible.Count)];
            return pick;
            */
            
            // How far into the normal phase we are: 0 (start) -> 1 (timeLimit)
            float progress = 0f;
            if (timeLimit > 0f)
                progress = timeElapsed / timeLimit;

            // Use weighted selection based on time progress & prefab index
            return WeightedPickByDifficulty(eligible, progress);
        }
        
        private int WeightedPickByDifficulty(List<int> eligible, float progress)
        {
            if (eligible == null || eligible.Count == 0)
                return -1;
    
            if (eligible.Count == 1)
                return eligible[0];
            
            float[] earlyWeights = { 85f, 5f, 0f, 0f };
            float[] lateWeights = { 20f, 25f, 25f, 30f };
    
            float[] weights = new float[eligible.Count];
            float totalWeight = 0f;
    
            for (int i = 0; i < eligible.Count; i++)
            {
                int prefabIndex = eligible[i];
                int tier = Mathf.Min(prefabIndex, earlyWeights.Length - 1);
                float weight = Mathf.Lerp(earlyWeights[tier], lateWeights[tier], progress);
                weights[i] = weight;
                totalWeight += weight;
                
            }
    
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
    
            float cumulative = 0f;
            for (int i = 0; i < eligible.Count; i++)
            {
                cumulative += weights[i];
                if (randomValue <= cumulative)
                {
                    return eligible[i];
                }
            }
    
            return eligible[eligible.Count - 1];
        }
        

        private int CountPrefabIndex(int prefabIndex)
        {
            int c = 0;
            foreach (var kv in _prefabIndexByEnemy)
            {
                if (kv.Key == null) continue;
                if (kv.Value == prefabIndex) c++;
            }
            return c;
        }

        private void CleanEnemyLists()
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var go = _activeEnemies[i];
                if (go == null)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }
                // keep dictionary in sync
                if (!_prefabIndexByEnemy.ContainsKey(go))
                    _prefabIndexByEnemy[go] = 1; // default if missing
            }

            // remove null keys from dictionary
            var toRemove = new List<GameObject>();
            foreach (var kv in _prefabIndexByEnemy)
                if (kv.Key == null) toRemove.Add(kv.Key);
            foreach (var k in toRemove) _prefabIndexByEnemy.Remove(k);
        }

        public void RegisterEnemyDeath(GameObject enemy)
        {
            if (!enemy) return;
            RegisterEnemyDeath(enemy, enemy.transform.position);
        }
        
        public void RegisterEnemyDeath(GameObject enemy, Vector3 deathPos)
        {
            if (!enemy) return;

            // Remove from tracking
            _activeEnemies.Remove(enemy);
            _prefabIndexByEnemy.Remove(enemy);

            // Try chest drop if the enemy has a dropper
            var dropper = enemy.GetComponent<EnemyLootDropper>();
            if (dropper) dropper.TryDrop(deathPos);
        }
        
        private int CountCappedGroupAlive()
        {
            int cnt = 0;
            foreach (var kv in _prefabIndexByEnemy)
            {
                if (kv.Key == null) continue;
                int idx = kv.Value;
                if (idx >= cappedGroupStart && idx <= cappedGroupEndInclusive)
                    cnt++;
            }
            return cnt;
        }
        
        private List<int> BuildEligiblePrefabIndices()
        {
            var list = new List<int>();
            if (enemyPrefabs == null) return list;

            bool groupFull = CountCappedGroupAlive() >= cappedGroupMax;

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i] == null) continue;
                
                if (groupFull && i >= cappedGroupStart && i <= cappedGroupEndInclusive)
                    continue;
                
                list.Add(i);
            }
            return list;
        }

        
        // Helper: attach to bosses at spawn so we can detect their destruction
        private class BossHandle : MonoBehaviour
        {
            private void OnDestroy()
            {
                // When a boss is destroyed (killed or scene unload), notify controller
                if (GameController.instance != null)
                    GameController.instance.RegisterBossDeath(gameObject);
            }
        }

        // Spawn N bosses around the spawn point in a circle
        private void SpawnBossWave()
        {
            _activeBosses.Clear();

            if (!bossSpawnPoint) return;

            // Use bossPrefabs if provided; otherwise fallback to single bossPrefab
            bool useArray = bossPrefabs != null && bossPrefabs.Length > 0;
            int n = Mathf.Max(1, bossCount);

            for (int i = 0; i < n; i++)
            {
                Vector2 offset = Vector2.zero;
                if (n > 1)
                {
                    float ang = i * (360f / n) * Mathf.Deg2Rad;
                    offset = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * bossSpawnRadius;
                }

                GameObject prefab = useArray
                    ? bossPrefabs[Random.Range(0, bossPrefabs.Length)]
                    : bossPrefab;

                if (!prefab) continue;

                Vector3 pos = bossSpawnPoint.position + (Vector3)offset;
                Quaternion rot = bossSpawnPoint.rotation;

                var boss = Instantiate(prefab, pos, rot);
                boss.AddComponent<BossHandle>();               // so we learn when it dies

                // Optional: put bosses on a specific sorting layer
                var sr = boss.GetComponent<SpriteRenderer>();
                if (sr) sr.sortingLayerName = "L1_chars";

                _activeBosses.Add(boss);
            }
        }

        private void CleanBossList()
        {
            for (int i = _activeBosses.Count - 1; i >= 0; i--)
                if (_activeBosses[i] == null) _activeBosses.RemoveAt(i);
        }

        public void RegisterBossDeath(GameObject bossGO)
        {
            // Called by BossHandle.OnDestroy
            _activeBosses.Remove(bossGO);

            // If all bosses are dead, end immediately
            if (_bossSequenceStarted && !_resultsQueued)
            {
                CleanBossList();
                if (_activeBosses.Count == 0)
                    EndResultsAndSaveHighscore();
            }
        }

        private void EndResultsAndSaveHighscore()
        {
            if (_resultsQueued) return;
            _resultsQueued = true;

            int best = PlayerPrefs.GetInt("HighestScore", 0);
            if (score > best) PlayerPrefs.SetInt("HighestScore", score);

            SceneManager.LoadScene("GameResults");
        }


        // =====================================================================
        // Boss sequence
        // =====================================================================
        private System.Collections.IEnumerator BossSequence()
        {
            // Hide normal timer
            if (textTimer) textTimer.gameObject.SetActive(false);
            if (imageTimer) imageTimer.gameObject.SetActive(false);

            // Teleport player to boss ground and set sorting layer
            var playerGO = PlayerMovement.instance ? PlayerMovement.instance.gameObject : GameObject.FindWithTag("Player");
            if (playerGO && bossSpawnPoint)
            {
                
                var rb2d = playerGO.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    rb2d.velocity = Vector2.zero;
                    rb2d.angularVelocity = 0f;
                    rb2d.position = (Vector2)bossSpawnPoint.position;
                    rb2d.rotation = bossSpawnPoint.eulerAngles.z;
                }
                else
                {
                    playerGO.transform.SetPositionAndRotation(bossSpawnPoint.position, bossSpawnPoint.rotation);
                }

                var sr = playerGO.GetComponent<SpriteRenderer>();
                if (sr) sr.sortingLayerName = "L1_chars";
            }

            // Clear normal spawns during boss
            // (optional) you can also destroy existing enemies here if desired
            foreach (var e in _activeEnemies) if (e) Destroy(e);
            _activeEnemies.Clear(); _prefabIndexByEnemy.Clear();

            // Prepare countdown (10 -> 1)
            const int prepareSeconds = 10;
            if (textBossPrep) textBossPrep.gameObject.SetActive(true);

            for (int t = prepareSeconds; t >= 1; t--)
            {
                if (textBossPrep)
                    textBossPrep.text = $"Be prepared, Boss will be in {t} seconds!!!";
                yield return new WaitForSeconds(1f);
            }

            // Spawn multiple bosses
            SpawnBossWave();

            // Boss battle countdown
            float bossLeft = Mathf.Max(0, bossTimeLimit);
            while (bossLeft > 0f)
            {
                // If all bosses are dead, end right away
                CleanBossList();
                if (_activeBosses.Count == 0)
                {
                    EndResultsAndSaveHighscore();
                    yield break;
                }

                if (textBossPrep)
                    textBossPrep.text = $"Boss Time: {FormatMMSS(bossLeft)}";

                yield return null;
                bossLeft -= Time.deltaTime;
            }

            // Time ran out -> end as well
            EndResultsAndSaveHighscore();

        }

        private IEnumerator SelectionWindow()
        {
            float t = swapWindowSeconds;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                yield return null;
            }
            canSwap = false;
            ShowSelectPanel(false);
        }

        private void ShowSelectPanel(bool on)
        {
            if (selectPanelGO) selectPanelGO.SetActive(on);
        }

        public void PickHero0() { TrySwapTo(0); }
        public void PickHero1() { TrySwapTo(1); }
        public void PickHero2() { TrySwapTo(2); }

        private void TrySwapTo(int index)
        {
            if (!canSwap) return;
            if (playerPrefabs == null || index < 0 || index >= playerPrefabs.Length) return;

            var oldPH = PlayerHealth.instance;

            Transform curT = PlayerMovement.instance ? PlayerMovement.instance.transform
                                                     : GameObject.FindWithTag("Player")?.transform;
            Vector3 pos;
            Quaternion rot;
            if (curT != null)
            {
                pos = curT.position;
                rot = curT.rotation;
                _currentPlayer = curT.gameObject;
            }
            else if (playerSpawnPoints != null && playerSpawnPoints.Length > 0)
            {
                pos = playerSpawnPoints[0].position;
                rot = playerSpawnPoints[0].rotation;
            }
            else
            {
                pos = Vector3.zero;
                rot = Quaternion.identity;
            }

            if (_currentPlayer != null) Destroy(_currentPlayer);

            GameObject prefab = playerPrefabs[index];
            GameObject newPlayer = Instantiate(prefab, pos, rot);
            _currentPlayer = newPlayer;

            var pm = newPlayer.GetComponent<PlayerMovement>();
            if (pm != null && pm.cam == null && Camera.main != null) pm.cam = Camera.main;

            var cf = Camera.main ? Camera.main.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>() : null;
            if (cf != null) cf.target = newPlayer.transform;

            if (oldPH != null) oldPH.onHealthChangedCallback -= UpdateHeartsHUD;
            if (PlayerHealth.instance != null)
            {
                PlayerHealth.instance.onHealthChangedCallback -= UpdateHeartsHUD;
                PlayerHealth.instance.onHealthChangedCallback += UpdateHeartsHUD;
                UpdateHeartsHUD();
            }
        }

        // =====================================================================
        // Utility
        // =====================================================================
        private static string FormatMMSS(float seconds)
        {
            int s = Mathf.Max(0, Mathf.CeilToInt(seconds));
            int m = s / 60;
            int r = s % 60;
            return r < 10 ? $"{m}:0{r}" : $"{m}:{r}";
        }
        
        /// <summary>
        /// Apply a temporary multiplicative speed boost. Stacks with others.
        /// </summary>
        public void ApplyTempSpeed(float multiplier, float duration)
        {
            var pm = PlayerHealth.instance;
            if (pm == null) return;

            // Capture baseline once, when the first buff starts
            if (!_hasSpeedBaseline)
            {
                _baseWalkSpeed = pm.WalkSpeed;
                _baseSpeed     = pm.Speed;
                _hasSpeedBaseline = true;
            }

            var buff = new SpeedBuff
            {
                multiplier = Mathf.Max(0f, multiplier),
                endTime    = Time.time + Mathf.Max(0f, duration)
            };
            _speedBuffs.Add(buff);

            // Recompute immediately with the new buff included
            RecomputeSpeedFromBuffs();

            // Start expiry watcher for this specific buff
            StartCoroutine(ExpireSpeedBuff(buff.endTime));
        }

        /// <summary>
        /// Waits until the given endTime, then removes the expired buff and recomputes.
        /// </summary>
        private IEnumerator ExpireSpeedBuff(float endTime)
        {
            // Wait until expiry time (game-time; use unscaledTime if you want it to tick while paused)
            while (Time.time < endTime) yield return null;

            // Remove all buffs that are expired (in case multiple ended while paused)
            float now = Time.time;
            for (int i = _speedBuffs.Count - 1; i >= 0; i--)
                if (_speedBuffs[i].endTime <= now)
                    _speedBuffs.RemoveAt(i);

            RecomputeSpeedFromBuffs();
        }

        /// <summary>
        /// Multiplies the baseline by all active multipliers. Restores baseline when empty.
        /// </summary>
        private void RecomputeSpeedFromBuffs()
        {
            var pm = PlayerHealth.instance;
            if (pm == null) return;

            if (_speedBuffs.Count == 0)
            {
                // No active buffs: restore baseline and clear snapshot
                if (_hasSpeedBaseline)
                {
                    pm.SetWalkSpeed(_baseWalkSpeed);
                    pm.SetSpeed(_baseSpeed);
                    _hasSpeedBaseline = false;
                }
                return;
            }

            // Product of all multipliers (e.g., 1.5x and 1.2x -> 1.8x)
            float m = 1f;
            for (int i = 0; i < _speedBuffs.Count; i++)
                m *= _speedBuffs[i].multiplier;

            pm.SetWalkSpeed(_baseWalkSpeed * m);
            pm.SetSpeed(_baseSpeed * m);
        }
        
        public void OnPermanentSpeedChanged(float newWalkBase, float newSpeedBase)
        {
            var pm = PlayerHealth.instance;
            if (pm == null) return;

            if (_hasSpeedBaseline)
            {
                _baseWalkSpeed = newWalkBase;
                _baseSpeed     = newSpeedBase;
                RecomputeSpeedFromBuffs(); // keep current buffs effective
            }
            else
            {
                pm.SetWalkSpeed(newWalkBase);
                pm.SetSpeed(newSpeedBase);
            }
        }
    }
}


