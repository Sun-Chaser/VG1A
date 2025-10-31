// MainMenuController.cs

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip bgm;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = bgm;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlaySingle()
    {
        SceneManager.LoadScene("GameSingle");
    }

    public void PlayOnline()
    {
        SceneManager.LoadScene("GameOnline");
    }
}