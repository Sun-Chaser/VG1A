using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public GameObject maxLevelPre;

    [Header("Texts")]
    public TMP_Text textHealth;
    public TMP_Text textSpeed;
    public TMP_Text textFireBallNum;
    public TMP_Text textFireBallSpeed;
    public TMP_Text textPoints;

    private readonly Dictionary<GameObject, GameObject> _maxBadgeByButton = new();

    void Awake()
    {
        instance = this;
        HideMenu();
    }

    public void Start()
    {
        // New system: shop costs are points, not XP
        RefreshShopUI();
        UpdateUpgradePointsDisplay();
        CheckAllMaxStates();
        HideMenu();
    }

    // ---------- Menu switching ----------
    public void SwitchMenu(GameObject menu)
    {
        mainMenu.SetActive(false);
        shopMenu.SetActive(false);
        menu.SetActive(true);

        RefreshShopUI();
        CheckAllMaxStates(); // ensure UI state is fresh when switching
        UpdateUpgradePointsDisplay();
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
        UpdateUpgradePointsDisplay(); 
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

    // ---------- Upgrades (spend 1 point each) ----------
    public void AddHeart()
    {
        if (GameController.instance.HealthLevel < GameController.instance.MaxHealthLevel
            && GameController.instance.TrySpendUpgradePoint())
        {
            PlayerHealth.instance.AddHealth();
            GameController.instance.HealthLevel++;

            SoundManager.instance?.PlayLevelUpClip();
            textHealth.text = "Cost: " + GameController.instance.HealthLevel + " point";
            CheckAllMaxStates();
            UpdateUpgradePointsDisplay();
            UpdateButtonsInteractable();
        }
    }

    public void AddSpeed()
    {
        if (GameController.instance.SpeedLevel < GameController.instance.MaxSpeedLevel
            && GameController.instance.TrySpendUpgradePoint())
        {
            PlayerHealth.instance.AddSpeed();
            GameController.instance.SpeedLevel++;

            SoundManager.instance?.PlayLevelUpClip();
            textSpeed.text = "Cost: " + GameController.instance.SpeedLevel + " point";
            CheckAllMaxStates();
            UpdateButtonsInteractable();
        }
    }

    public void AddFireBall()
    {
        if (GameController.instance.FireballLevel < GameController.instance.MaxFireballLevel
            && GameController.instance.TrySpendUpgradePoint())
        {
            PlayerMovement.instance.fireNum += 1;
            GameController.instance.FireballLevel++;

            SoundManager.instance?.PlayLevelUpClip();
            textFireBallNum.text = "Cost: " + GameController.instance.FireballLevel + " point";
            CheckAllMaxStates();
            UpdateUpgradePointsDisplay();
            UpdateButtonsInteractable();
        }
    }

    public void AddFireBallSpeed()
    {
        if (GameController.instance.FireballSpeedLevel < GameController.instance.MaxFireballSpeedLevel
            && GameController.instance.TrySpendUpgradePoint())
        {
            PlayerMovement.instance.fireSpeed += 1.0f;
            GameController.instance.FireballSpeedLevel++;

            SoundManager.instance?.PlayLevelUpClip();
            textFireBallSpeed.text = "Cost:  " + GameController.instance.FireballSpeedLevel +" point";
            CheckAllMaxStates();
            UpdateUpgradePointsDisplay();
            UpdateButtonsInteractable();
        }
    }

    // ---------- Helpers ----------
    private void RefreshShopUI()
    {
        // Default to point-based cost; MAX state will overwrite via CheckAllMaxStates()
        if (textHealth)        textHealth.text        = "Cost: 1 point";
        if (textSpeed)         textSpeed.text         = "Cost: 1 point";
        if (textFireBallNum)   textFireBallNum.text   = "Cost: 1 point";
        if (textFireBallSpeed) textFireBallSpeed.text = "Cost: 1 point";

        UpdateButtonsInteractable();
    }

    // Optional quality-of-life: disable buttons if no points left (requires Button component on those GameObjects)
    private void UpdateButtonsInteractable()
    {
        int points = GameController.instance != null ? GameController.instance.upgradePoints : 0;
        bool hasPoint = points > 0;

        SetButtonInteractable(upHealth,        hasPoint);
        SetButtonInteractable(upSpeed,         hasPoint);
        SetButtonInteractable(upFireball,      hasPoint);
        SetButtonInteractable(upFireballSpeed, hasPoint);
    }

    private void SetButtonInteractable(GameObject go, bool interactable)
    {
        if (!go) return;
        var btn = go.GetComponent<Button>();
        if (btn) btn.interactable = interactable;
    }
    
    public void UpdateUpgradePointsDisplay()
    {
        if (textPoints && GameController.instance)
            textPoints.text = $"Upgrade Points: {GameController.instance.upgradePoints}";
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

            if (_maxBadgeByButton.ContainsKey(upgradeButton))
            {
                upgradeButton.SetActive(false);
                return;
            }

            if (maxLevelPre != null)
            {
                RectTransform btnRect = upgradeButton.GetComponent<RectTransform>();
                RectTransform parent  = upgradeButton.transform.parent as RectTransform;

                var instGO  = Instantiate(maxLevelPre, parent != null ? parent : upgradeButton.transform.parent);
                var instRT  = instGO.GetComponent<RectTransform>();

                if (btnRect != null && instRT != null)
                {
                    instRT.anchorMin        = btnRect.anchorMin;
                    instRT.anchorMax        = btnRect.anchorMax;
                    instRT.pivot            = btnRect.pivot;
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
            if (_maxBadgeByButton.TryGetValue(upgradeButton, out var badge) && badge != null)
                Destroy(badge);

            _maxBadgeByButton.Remove(upgradeButton);
            upgradeButton.SetActive(true);
        }
    }
}
