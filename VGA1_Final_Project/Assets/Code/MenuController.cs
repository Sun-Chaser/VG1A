using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    
    public GameObject mainMenu;

    void Awake()
    {
        instance = this;
        Hide();
    }

    public void Show()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(true);
        Time.timeScale = 0;
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.isPaused = true;
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.isPaused = false;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
