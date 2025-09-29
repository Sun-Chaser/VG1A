/*
 *  Author: ariel oliveira [o.arielg@gmail.com]
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Player
{
    public class GameController : MonoBehaviour
    {
        private GameObject[] heartContainers;
        private Image[] heartFills;
        public TMP_Text textXP;
        public TMP_Text textHealth;
        public TMP_Text textSpeed;

        public Transform heartsParent;
        public GameObject heartContainerPrefab;

        public int xp;
        public float timeElapsed;

        private void Start()
        {
            // Should I use lists? Maybe :)
            heartContainers = new GameObject[(int)PlayerHealth.Instance.MaxTotalHealth];
            heartFills = new Image[(int)PlayerHealth.Instance.MaxTotalHealth];
            PlayerHealth.Instance.onHealthChangedCallback += UpdateHeartsHUD;
            InstantiateHeartContainers();
            UpdateHeartsHUD();

            xp = 0;
            timeElapsed = 0;
            
            textSpeed.text = "Add Speed XP:" + (((int)PlayerHealth.Instance.Speed - 2.0) * 5);
            textHealth.text = "Add Heart XP:" + (((int)PlayerHealth.Instance.Health - 3) * 5);
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            
            // For test use only
            if (timeElapsed >= 5)
            {
                timeElapsed = 0;
                xp += 10;
            }
            UpdateXPDisplay();
        }

        private void UpdateXPDisplay()
        {
            textXP.text = xp.ToString();
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
            int cost = ((int) PlayerHealth.Instance.Health - 3) * 5;
            if (PlayerHealth.Instance.MaxHealth < PlayerHealth.Instance.MaxTotalHealth && xp >= cost)
            {
                PlayerHealth.Instance.AddHealth();
                xp -= cost;

                textHealth.text = "Add Heart XP:" + (((int)PlayerHealth.Instance.Health - 3) * 5);
            }
        }

        public void AddSpeed()
        {
            int cost = (int)(PlayerHealth.Instance.Speed - 2.0) * 5;
            if (PlayerHealth.Instance.Speed < PlayerHealth.Instance.MaxSpeed && xp >= cost)
            {
                PlayerHealth.Instance.AddSpeed();
                xp -= cost;
                
                textSpeed.text = "Add Speed XP:" + (((int)PlayerHealth.Instance.Speed - 2.0) * 5);
            }
        }
    }

}
