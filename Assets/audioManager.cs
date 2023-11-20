using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioManager : MonoBehaviour
{
    [Header("----Audio Source-----")]
    [SerializeField] AudioSource playerAudio;


    [Header("----Audio Clip-----")]
    public AudioClip jump;
    public AudioClip doublejump;
    public AudioClip step;
    public AudioClip attack;
    public AudioClip injured;


    void Start()
    {
        playerAudio.enabled = true;
    } 


    public void playAudio(AudioClip clip)
    {
        playerAudio.PlayOneShot(clip);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
