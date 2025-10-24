// MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlaySingle()
    {
        SceneManager.LoadScene("GameSingle");
    }

    public void PlayOnline()
    {
        SceneManager.LoadScene("GameOnline");
    }
}