using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player
{
    public class GameControllerOn : MonoBehaviourPun
    {

        [Header("Targets")]
        public PlayerHealthOn target; 

        private GameObject[] heartContainers;
        private Image[] heartFills;
        public TMP_Text textXP;
        public TMP_Text textHealth;
        public TMP_Text textSpeed;
        //public TMP_Text textTimer;

        public Transform heartsParent;
        public GameObject heartContainerPrefab;

        public int xp;
        //public float timeElapsed;
        public float pointTimer; 
        //public int timeLimit = 75;
        int SpeedLevel = 1;

        private void Awake()
        {

            var rootView = GetComponentInParent<PhotonView>();
            if (rootView != null && !rootView.IsMine)
            {
                gameObject.SetActive(false);
                return;
            }

            if (target == null)
                target = GetComponentInParent<PlayerHealthOn>();
        }

        private void Start()
        {
            if (target == null) return;

            target.onHealthChangedCallback += UpdateHeartsHUD;

            heartContainers = new GameObject[(int)target.MaxTotalHealth];
            heartFills = new Image[(int)target.MaxTotalHealth];
            InstantiateHeartContainers();
            UpdateHeartsHUD();

            xp = 0;
            //timeElapsed = 0;
            pointTimer = 0;

            textSpeed.text = "Add Speed XP:" + ((int)(5 * SpeedLevel));
            textHealth.text = "Add Heart XP:" + (((int)target.Health - 3) * 5);
            //textTimer.text = "1:00";
        }

        private void Update()
        {
            if (target == null) return;
            pointTimer += Time.deltaTime;

            //timeElapsed += Time.deltaTime;
            /*
            if (timeElapsed >= timeLimit)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }*/

            // For test use only
            if (pointTimer >= 5)
            {
                pointTimer = 0;
                xp += 5;
            }
            UpdateXPDisplay();
            //UpdateTimerDisplay();
        }

        private void UpdateXPDisplay()
        {
            textXP.text = xp.ToString();
        }

        /*private void UpdateTimerDisplay()
        {
            int timeLeft = timeLimit - (int)timeElapsed;
            string secondLeft = (timeLeft % 60 < 10) ? ("0" + (timeLeft % 60)) : ((timeLeft % 60).ToString());
            textTimer.text = (timeLeft / 60) + ":" + secondLeft;
        }*/

        public void UpdateHeartsHUD()
        {
            if (target == null) return;
            SetHeartContainers();
            SetFilledHearts();
        }

        void SetHeartContainers()
        {
            for (int i = 0; i < heartContainers.Length; i++)
            {
                if (i < target.MaxHealth)
                    heartContainers[i].SetActive(true);
                else
                    heartContainers[i].SetActive(false);
            }
        }

        void SetFilledHearts()
        {
            for (int i = 0; i < heartFills.Length; i++)
            {
                if (i < target.Health)
                    heartFills[i].fillAmount = 1;
                else
                    heartFills[i].fillAmount = 0;
            }

            if (target.Health % 1 != 0)
            {
                int lastPos = Mathf.FloorToInt(target.Health);
                heartFills[lastPos].fillAmount = target.Health % 1;
            }
        }

        void InstantiateHeartContainers()
        {
            for (int i = 0; i < target.MaxTotalHealth; i++)
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
            if (target == null) return;
            if (target.Health < target.MaxHealth && xp >= cost)
            {
                target.Heal(1.0f);
                xp -= cost;
            }
        }

        public void AddHeart()
        {
            if (target == null) return;
            int cost = (((int)target.Health) - 3) * 5;
            if (target.MaxHealth < target.MaxTotalHealth && xp >= cost)
            {
                target.AddHealth();
                xp -= cost;
                textHealth.text = "Add Heart XP:" + ((((int)target.Health) - 3) * 5);
            }
        }

        public void AddSpeed()
        {
            if (target == null) return;

            int cost = (int)(5 * SpeedLevel);

            if (target.Speed < target.MaxSpeed && xp >= cost)
            {
                target.AddSpeed();
                xp -= cost;
                SpeedLevel++;
                textSpeed.text = "Add Speed XP:" + ((int)(5 * SpeedLevel));
            }
        }

        public void AddXP(int amount)
        {
            xp += amount;
        }
    }
}