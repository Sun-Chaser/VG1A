using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource audioSource;
    public AudioClip levelUpClip;
    public AudioClip chestOpenClip;
    public AudioClip healClip;
    
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

    public void PlayLevelUpClip()
    {
        audioSource.PlayOneShot(levelUpClip);
    }

    public void PlayChestOpenClip()
    {
        audioSource.PlayOneShot(chestOpenClip);
    }

    public void PlayHealClip()
    {
        audioSource.PlayOneShot(healClip);
    }
}
