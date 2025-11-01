using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource audioSource;
    public AudioClip xpUpClip;
    public AudioClip chestOpenClip;
    public AudioClip healClip;
    public AudioClip levelUpClip;
    public AudioClip mainBGM;
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Play BGM
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = mainBGM;
        audioSource.loop = true;
        audioSource.Play();
    }
    
    void Update()
    {
        
    }

    public void PlayXpUpClip()
    {
        audioSource.PlayOneShot(xpUpClip);
    }

    public void PlayChestOpenClip()
    {
        audioSource.PlayOneShot(chestOpenClip);
    }

    public void PlayHealClip()
    {
        audioSource.PlayOneShot(healClip);
    }

    public void PlayLevelUpClip()
    {
        audioSource.PlayOneShot(levelUpClip);
    }
}
