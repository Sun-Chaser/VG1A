using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class MenuControllerOn : MonoBehaviourPun, IPunObservable
    {
        public GameObject mainMenu;
        public GameObject shopMenu;

        public GameObject upHealth;
        public GameObject upSpeed;
        public GameObject upFireball;
        public GameObject upFireballSpeed;

        public GameObject maxLevelPre;

        public TMP_Text textHealth;
        public TMP_Text textSpeed;
        public TMP_Text textFireBallNum;
        public TMP_Text textFireBallSpeed;

        public TMP_Text textXP;

        public PlayerHealthOn playerHealthOn;
        public PlayerMovementOn playerMovementOn;

        public int xp = 0;

        public int HealthLevel = 1;
        public int MaxHealthLevel = 10;

        public int SpeedLevel = 1;
        public int MaxSpeedLevel = 10;

        public int FireballLevel = 1;
        public int MaxFireballLevel = 10;

        public int FireballSpeedLevel = 1;
        public int MaxFireballSpeedLevel = 10;

        readonly Dictionary<GameObject, GameObject> _maxBadgeByButton = new();

        void Awake()
        {
            var rootView = GetComponentInParent<PhotonView>();
            if (rootView != null && !rootView.IsMine)
            {
                gameObject.SetActive(false);
                enabled = false;
                return;
            }
            HideMenu();
        }

        void Start()
        {
            if (playerHealthOn == null) playerHealthOn = GetComponentInParent<PlayerHealthOn>();
            if (playerMovementOn == null) playerMovementOn = GetComponentInParent<PlayerMovementOn>();

            if (textHealth) textHealth.text = "Cost: " + HealthLevel * 5;
            if (textSpeed) textSpeed.text = "Cost: " + SpeedLevel * 5;
            if (textFireBallNum) textFireBallNum.text = "Cost: " + FireballLevel * 5;
            if (textFireBallSpeed) textFireBallSpeed.text = "Cost: " + FireballSpeedLevel * 5;

            if (textXP) textXP.text = xp.ToString();

            CheckAllMaxStates();
            HideMenu();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B)) ShowShopMenu();
        }

        public void SwitchMenu(GameObject menu)
        {
            if (mainMenu) mainMenu.SetActive(false);
            if (shopMenu) shopMenu.SetActive(false);
            if (menu) menu.SetActive(true);
            CheckAllMaxStates();
        }

        public void ShowMainMenu()
        {
            SwitchMenu(mainMenu);
            Time.timeScale = 0;
            if (PlayerMovement.instance != null) PlayerMovement.instance.isPaused = true;
        }

        public void ShowShopMenu()
        {
            SwitchMenu(shopMenu);
            Time.timeScale = 0;
            if (PlayerMovement.instance != null) PlayerMovement.instance.isPaused = true;
        }

        public void HideMenu()
        {
            if (mainMenu) mainMenu.SetActive(false);
            if (shopMenu) shopMenu.SetActive(false);
            Time.timeScale = 1;
            if (PlayerMovement.instance != null) PlayerMovement.instance.isPaused = false;
        }

        public void Resume()
        {
            HideMenu();
            Time.timeScale = 1;
        }

        public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        public void Quit() => SceneManager.LoadScene("MainMenu");

        public void AddHeart()
        {
            int cost = HealthLevel * 5;
            if (HealthLevel < MaxHealthLevel && xp >= cost)
            {
                if (playerHealthOn != null) playerHealthOn.AddHealth();
                xp -= cost;
                HealthLevel++;
                if (textHealth) textHealth.text = "Cost: " + HealthLevel * 5;
                if (textXP) textXP.text = xp.ToString();
                CheckAllMaxStates();
            }
        }

        public void AddSpeed()
        {
            int cost = 5 * SpeedLevel;
            if (SpeedLevel < MaxSpeedLevel && xp >= cost)
            {
                if (playerHealthOn != null) playerHealthOn.AddSpeed();
                xp -= cost;
                SpeedLevel++;
                if (textSpeed) textSpeed.text = "Cost: " + 5 * SpeedLevel;
                if (textXP) textXP.text = xp.ToString();
                CheckAllMaxStates();
            }
        }

        public void AddFireBall()
        {
            int cost = 5 * FireballLevel;
            if (FireballLevel < MaxFireballLevel && xp >= cost)
            {
                //if (playerMovementOn != null) playerMovementOn.fireNum += 1;
                xp -= cost;
                FireballLevel++;
                if (textFireBallNum) textFireBallNum.text = "Cost: " + 5 * FireballLevel;
                if (textXP) textXP.text = xp.ToString();
                CheckAllMaxStates();
            }
        }

        public void AddFireBallSpeed()
        {
            int cost = 5 * FireballSpeedLevel;
            if (FireballSpeedLevel < MaxFireballSpeedLevel && xp >= cost)
            {
                if (playerMovementOn != null) playerMovementOn.fireSpeed += 1.0f;
                xp -= cost;
                FireballSpeedLevel++;
                if (textFireBallSpeed) textFireBallSpeed.text = "Cost: " + 5 * FireballSpeedLevel;
                if (textXP) textXP.text = xp.ToString();
                CheckAllMaxStates();
            }
        }

        void CheckAllMaxStates()
        {
            CheckAndReplace(HealthLevel >= MaxHealthLevel, upHealth, textHealth);
            CheckAndReplace(SpeedLevel >= MaxSpeedLevel, upSpeed, textSpeed);
            CheckAndReplace(FireballLevel >= MaxFireballLevel, upFireball, textFireBallNum);
            CheckAndReplace(FireballSpeedLevel >= MaxFireballSpeedLevel, upFireballSpeed, textFireBallSpeed);
        }

        void CheckAndReplace(bool isMax, GameObject upgradeButton, TMP_Text label)
        {
            if (upgradeButton == null) return;

            if (isMax)
            {
                if (label != null) label.text = "MAX LEVEL";

                if (_maxBadgeByButton.ContainsKey(upgradeButton))
                {
                    upgradeButton.SetActive(false);
                    return;
                }

                if (maxLevelPre != null)
                {
                    RectTransform btnRect = upgradeButton.GetComponent<RectTransform>();
                    RectTransform parent = upgradeButton.transform.parent as RectTransform;

                    var instGO = Instantiate(maxLevelPre, parent != null ? parent : upgradeButton.transform.parent);
                    var instRT = instGO.GetComponent<RectTransform>();

                    if (btnRect != null && instRT != null)
                    {
                        instRT.anchorMin = btnRect.anchorMin;
                        instRT.anchorMax = btnRect.anchorMax;
                        instRT.pivot = btnRect.pivot;
                        instRT.anchoredPosition = btnRect.anchoredPosition;
                        instRT.sizeDelta = btnRect.sizeDelta;
                        instRT.localScale = btnRect.localScale;
                    }

                    _maxBadgeByButton[upgradeButton] = instGO;
                }

                upgradeButton.SetActive(false);
            }
            else
            {
                if (_maxBadgeByButton.TryGetValue(upgradeButton, out var badge) && badge != null)
                    Destroy(badge);

                _maxBadgeByButton.Remove(upgradeButton);
                upgradeButton.SetActive(true);
            }
        }

        public void AddXP(int amount)
        {
            xp += amount;
            if (textXP) textXP.text = xp.ToString();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(xp);
                stream.SendNext(HealthLevel);
                stream.SendNext(MaxHealthLevel);
                stream.SendNext(SpeedLevel);
                stream.SendNext(MaxSpeedLevel);
                stream.SendNext(FireballLevel);
                stream.SendNext(MaxFireballLevel);
                stream.SendNext(FireballSpeedLevel);
                stream.SendNext(MaxFireballSpeedLevel);
            }
            else
            {
                xp = (int)stream.ReceiveNext();
                HealthLevel = (int)stream.ReceiveNext();
                MaxHealthLevel = (int)stream.ReceiveNext();
                SpeedLevel = (int)stream.ReceiveNext();
                MaxSpeedLevel = (int)stream.ReceiveNext();
                FireballLevel = (int)stream.ReceiveNext();
                MaxFireballLevel = (int)stream.ReceiveNext();
                FireballSpeedLevel = (int)stream.ReceiveNext();
                MaxFireballSpeedLevel = (int)stream.ReceiveNext();

                if (textXP) textXP.text = xp.ToString();
                if (textHealth) textHealth.text = HealthLevel >= MaxHealthLevel ? "MAX LEVEL" : "Cost: " + HealthLevel * 5;
                if (textSpeed) textSpeed.text = SpeedLevel >= MaxSpeedLevel ? "MAX LEVEL" : "Cost: " + 5 * SpeedLevel;
                if (textFireBallNum) textFireBallNum.text = FireballLevel >= MaxFireballLevel ? "MAX LEVEL" : "Cost: " + 5 * FireballLevel;
                if (textFireBallSpeed) textFireBallSpeed.text = FireballSpeedLevel >= MaxFireballSpeedLevel ? "MAX LEVEL" : "Cost: " + 5 * FireballSpeedLevel;
                CheckAllMaxStates();
            }
        }
    }
}
