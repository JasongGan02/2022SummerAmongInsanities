using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class audioManager : MonoBehaviour
{
    [Header("----Audio Source-----")]
    [SerializeField] AudioSource playerAudio;


    [Header("----Audio Clip-----")]
    public AudioClip[] injured = { injured0, injured1, injured2, injured3 };

    public AudioClip[] step = { step0, step1, step2, step3, step4, step5 };
    private int lastPlayedClipIndex = -1;
    private static AudioClip injured0;
    private static AudioClip injured1;
    private static AudioClip injured2;
    private static AudioClip injured3;

    public AudioClip jump;
    public AudioClip doublejump;

    

    private static AudioClip step0;
    private static AudioClip step1;
    private static AudioClip step2;
    private static AudioClip step3;
    private static AudioClip step4;
    private static AudioClip step5;

    public AudioClip attack;
    public AudioClip death;
    public AudioClip tile_duringbreak;
    public AudioClip tile_endbreak;

    void Start()
    {
        playerAudio.enabled = true;
    } 


    public void playAudio(AudioClip clip)
    {
        playerAudio.clip = clip;
        playerAudio.Play();
    }

    public void StopPlayerAudio()
    {
        playerAudio.Stop();
    }

    public void looponAudio()
    {
        playerAudio.loop = true;
    }
    public void loopoffAudio()
    {
        playerAudio.loop = false;
    }
    // Update is called once per frame

    public AudioClip clip()
    {
        return playerAudio.clip;
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

    public bool IsClipPlaying(AudioClip clip)
    {
        return playerAudio.isPlaying && playerAudio.clip == clip;
    }
    void Update()
    {
        
    }
}
