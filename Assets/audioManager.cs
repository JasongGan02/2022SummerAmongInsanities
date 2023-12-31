using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioManager : MonoBehaviour
{
    [Header("----Audio Source-----")]
    [SerializeField] AudioSource playerAudio;


    [Header("----Audio Clip-----")]
    public AudioClip[] injured = { injured0, injured1, injured2, injured3 };

    private static AudioClip injured0;
    private static AudioClip injured1;
    private static AudioClip injured2;
    private static AudioClip injured3;

    public AudioClip jump;
    public AudioClip doublejump;
    public AudioClip step;
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
    void Update()
    {
        
    }
}
