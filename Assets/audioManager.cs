using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class audioManager : MonoBehaviour
{
    [Header("----Audio Source-----")]
    [SerializeField] AudioSource playerAudio;
    [SerializeField] AudioSource weaponAudio;
    [SerializeField] AudioSource towerAudio;
    [SerializeField] AudioSource playerReactAudio;
    [SerializeField] AudioSource enemyAudio;
    [SerializeField] AudioSource BGM;


    [Header("----Audio Clip-----")]
    public AudioClip[] injured = { injured0, injured1};
    private int lastPlayedClipIndex = -1;
    private static AudioClip injured0;
    private static AudioClip injured1;

    public AudioClip[] step = { step0, step1, step2, step3, step4, step5 };
    private static AudioClip step0;
    private static AudioClip step1;
    private static AudioClip step2;
    private static AudioClip step3;
    private static AudioClip step4;
    private static AudioClip step5;

    public AudioClip jump;
    public AudioClip doublejump;
    public AudioClip attack;
    public AudioClip bow;
    public AudioClip shoot;

    public AudioClip death;
    public AudioClip tile_duringbreak;
    public AudioClip tile_endbreak;

    public AudioClip catapult_shootRock;


    public AudioClip DayTime;
    public AudioClip NightTime;



    void Start()
    {
        playerAudio.enabled = true;
    }


    public void playBGM(AudioClip clip)
    {
        BGM.clip = clip;
        BGM.Play();
    }

    public void playAudio(AudioClip clip)
    {
        playerAudio.clip = clip;
        playerAudio.Play();
    }

    public void playWeaponAudio(AudioClip clip) 
    {
        weaponAudio.clip = clip;
        weaponAudio.Play();
    }

    public void playTowerAudio(AudioClip clip)
    {
        towerAudio.clip = clip;
        towerAudio.Play();
    }

    public void playReactAudio(AudioClip clip)
    {
        playerReactAudio.clip = clip;
        playerReactAudio.Play();
    }

    public void StopWeaponAudio()
    {
        weaponAudio.Stop();
    }

    public void looponWeaponAudio()
    {
        weaponAudio.loop = true;
    }
    public void loopoffWeaponAudio()
    {
        weaponAudio.loop = false;
    }
    // Update is called once per frame

    public AudioClip weaponclip()
    {
        return weaponAudio.clip;
    }


    public void PlayFootstep()
    {
        if (step.Length > 0)
        {
            int index;
            do
            {
                index = Random.Range(0, step.Length);
            } while (index == lastPlayedClipIndex && step.Length > 1); // Ensure a different clip is selected if there are multiple clips

            lastPlayedClipIndex = index; // Remember the last played clip index
            playerAudio.clip = step[index];
            playerAudio.Play();
        }
    }

   

    public bool IsWeaponPlaying(AudioClip clip)
    {
        return weaponAudio.isPlaying && weaponAudio.clip == clip;
    }
    void Update()
    {
        
    }
}
