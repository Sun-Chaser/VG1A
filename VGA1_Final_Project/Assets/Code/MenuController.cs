using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject shopMenu;

    [Header("Upgrade Buttons")]
    public GameObject upHealth;
    public GameObject upSpeed;
    public GameObject upFireball;
    public GameObject upFireballSpeed;

    [Header("Max Badge Prefab")]
    public GameObject maxLevelPre; // prefab shown when maxed

    [Header("Texts")]
    public TMP_Text textHealth;
    public TMP_Text textSpeed;
    public TMP_Text textFireBallNum;
    public TMP_Text textFireBallSpeed;

    // track spawned "MAX LEVEL" badges so we can avoid duplicates / clean up
    private readonly Dictionary<GameObject, GameObject> _maxBadgeByButton = new();

    void Awake()
    {
        instance = this;
        HideMenu();
    }

    public void Start()
    {
        // initial costs
        textHealth.text        = "Cost: " + GameController.instance.HealthLevel        * 5;
        textSpeed.text         = "Cost: " + GameController.instance.SpeedLevel         * 5;
        textFireBallNum.text   = "Cost: " + GameController.instance.FireballLevel      * 5;
        textFireBallSpeed.text = "Cost: " + GameController.instance.FireballSpeedLevel * 5;

        CheckAllMaxStates();
        HideMenu();
    }

    // ---------- Menu switching ----------
    public void SwitchMenu(GameObject menu)
    {
        mainMenu.SetActive(false);
        shopMenu.SetActive(false);
        menu.SetActive(true);
        CheckAllMaxStates(); // ensure UI state is fresh when switching
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
        mainMenu.SetActive(false);
        shopMenu.SetActive(false);
        Time.timeScale = 1;
        if (PlayerMovement.instance != null) PlayerMovement.instance.isPaused = false;
    }

    public void Resume()
    {
        HideMenu();
        Time.timeScale = 1;
    }
    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void Quit()    => SceneManager.LoadScene("MainMenu");

    // ---------- Upgrades ----------
    public void AddHeart()
    {
        int cost = GameController.instance.HealthLevel * 5;
        if (GameController.instance.HealthLevel < GameController.instance.MaxHealthLevel
            && GameController.instance.xp >= cost)
        {
            PlayerHealth.instance.AddHealth();
            GameController.instance.xp -= cost;
            GameController.instance.HealthLevel++;

            SoundManager.instance.PlayLevelUpClip();
            textHealth.text = "Cost: " + GameController.instance.HealthLevel * 5;
            CheckAllMaxStates();
        }
    }

    public void AddSpeed()
    {
        int cost = 5 * GameController.instance.SpeedLevel;
        if (GameController.instance.SpeedLevel < GameController.instance.MaxSpeedLevel
            && GameController.instance.xp >= cost)
        {
            PlayerHealth.instance.AddSpeed();
            GameController.instance.xp -= cost;
            GameController.instance.SpeedLevel++;
            
            SoundManager.instance.PlayLevelUpClip();
            textSpeed.text = "Cost: " + 5 * GameController.instance.SpeedLevel;
            CheckAllMaxStates();
        }
    }

    public void AddFireBall()
    {
        int cost = 5 * GameController.instance.FireballLevel;
        if (GameController.instance.FireballLevel < GameController.instance.MaxFireballLevel
            && GameController.instance.xp >= cost)
        {
            PlayerMovement.instance.fireNum += 1;
            GameController.instance.xp -= cost;
            GameController.instance.FireballLevel++;

            SoundManager.instance.PlayLevelUpClip();
            textFireBallNum.text = "Cost: " + 5 * GameController.instance.FireballLevel;
            CheckAllMaxStates();
        }
    }

    public void AddFireBallSpeed()
    {
        int cost = 5 * GameController.instance.FireballSpeedLevel;
        if (GameController.instance.FireballSpeedLevel < GameController.instance.MaxFireballSpeedLevel
            && GameController.instance.xp >= cost)
        {
            PlayerMovement.instance.fireSpeed += 1.0f;
            GameController.instance.xp -= cost;
            GameController.instance.FireballSpeedLevel++;

            SoundManager.instance.PlayLevelUpClip();
            textFireBallSpeed.text = "Cost: " + 5 * GameController.instance.FireballSpeedLevel;
            CheckAllMaxStates();
        }
    }

    // ---------- Max-level checks & swapping ----------
    private void CheckAllMaxStates()
    {
        var g = GameController.instance;

        CheckAndReplace(
            isMax: g.HealthLevel        >= g.MaxHealthLevel,
            upgradeButton: upHealth,
            label: textHealth
        );

        CheckAndReplace(
            isMax: g.SpeedLevel         >= g.MaxSpeedLevel,
            upgradeButton: upSpeed,
            label: textSpeed
        );

        CheckAndReplace(
            isMax: g.FireballLevel      >= g.MaxFireballLevel,
            upgradeButton: upFireball,
            label: textFireBallNum
        );

        CheckAndReplace(
            isMax: g.FireballSpeedLevel >= g.MaxFireballSpeedLevel,
            upgradeButton: upFireballSpeed,
            label: textFireBallSpeed
        );
    }

    private void CheckAndReplace(bool isMax, GameObject upgradeButton, TMP_Text label)
    {
        if (upgradeButton == null) return;

        if (isMax)
        {
            if (label != null) label.text = "MAX LEVEL";

            // already swapped? then just ensure button hidden
            if (_maxBadgeByButton.ContainsKey(upgradeButton))
            {
                upgradeButton.SetActive(false);
                return;
            }

            // instantiate max badge at the same rect as the button
            if (maxLevelPre != null)
            {
                RectTransform btnRect = upgradeButton.GetComponent<RectTransform>();
                RectTransform parent  = upgradeButton.transform.parent as RectTransform;

                var instGO  = Instantiate(maxLevelPre, parent != null ? parent : upgradeButton.transform.parent);
                var instRT  = instGO.GetComponent<RectTransform>();

                if (btnRect != null && instRT != null)
                {
                    instRT.anchorMin = btnRect.anchorMin;
                    instRT.anchorMax = btnRect.anchorMax;
                    instRT.pivot     = btnRect.pivot;
                    instRT.anchoredPosition = btnRect.anchoredPosition;
                    instRT.sizeDelta        = btnRect.sizeDelta;
                    instRT.localScale       = btnRect.localScale;
                }

                _maxBadgeByButton[upgradeButton] = instGO;
            }

            upgradeButton.SetActive(false);
        }
        else
        {
            // not max: restore button (if we had swapped earlier)
            if (_maxBadgeByButton.TryGetValue(upgradeButton, out var badge) && badge != null)
            {
                Destroy(badge);
            }
            _maxBadgeByButton.Remove(upgradeButton);
            upgradeButton.SetActive(true);
        }
    }
}
