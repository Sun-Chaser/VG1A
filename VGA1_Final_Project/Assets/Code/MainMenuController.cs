// MainMenuController.cs

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip bgm;
    
    public GameObject instruction;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = bgm;
        audioSource.loop = true;
        audioSource.Play();
        instruction.SetActive(false);
    }

    public void PlaySingle()
    {
        SceneManager.LoadScene("GameSingle");
    }

    public void PlayOnline()
    {
        SceneManager.LoadScene("Loading");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Instruction()
    {
        instruction.SetActive(true);
    }

    public void Back()
    {
        instruction.SetActive(false);
    }
}