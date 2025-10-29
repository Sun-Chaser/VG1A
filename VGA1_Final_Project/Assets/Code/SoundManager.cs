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
    
    void Awake()
    {
        instance = this;
    }

    void start()
    {
        audioSource = GetComponent<AudioSource>();
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
