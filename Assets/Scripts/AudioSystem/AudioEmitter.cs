using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioEmitter : MonoBehaviour
{
    [SerializeField] private List<AudioClipCategory> clipCategories;
    [SerializeField] [Range(0, 1)] private float spatialBlend = 1;
    private GameObject audioSourcePrefab; // Prefab with AudioSource component
    private AudioSource audioSource;
    
    private Dictionary<string, AudioClipCategory> clipCategoryDict;

    void Awake()
    {
        if (audioSourcePrefab == null)
        {
            audioSourcePrefab = Resources.Load<GameObject>("Audio/AudioSource"); 
        }

        // Instantiate the prefab and set it as a child
        GameObject audioSourceObj = Instantiate(audioSourcePrefab, transform);
        audioSource = audioSourceObj.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("The AudioSource component is not found on the prefab.");
            return;
        }

        audioSource.spatialBlend = spatialBlend;
        InitializeClipDictionary();
    }

    void InitializeClipDictionary()
    {
        clipCategoryDict = new Dictionary<string, AudioClipCategory>();
        foreach (var category in clipCategories)
        {
            clipCategoryDict[category.audioName] = category;
        }
    }

    public void PlayClipFromCategory(string categoryName, bool playOneShot = true) 
    {
        if (clipCategoryDict.TryGetValue(categoryName, out var category))
        {
            if (category.clipsWithVolume.Length > 0)
            {
                var clipWithVolume = category.clipsWithVolume[Random.Range(0, category.clipsWithVolume.Length)];
                audioSource.loop = category.loop;
                if (playOneShot)
                {
                    audioSource.PlayOneShot(clipWithVolume.clip, clipWithVolume.volume);
                }
                else //if you need to terminate what is playing already
                {
                    audioSource.volume = clipWithVolume.volume;
                    audioSource.clip = clipWithVolume.clip;
                    audioSource.Play();
                }
            }
        }
        else
        {
            Debug.LogWarning($"No audio clips found for category: {categoryName}");
        }
    }

    public void ChangeLoop(string categoryName, bool isLooping)
    {
        if (clipCategoryDict.TryGetValue(categoryName, out var category))
        {
            category.loop = isLooping;
        }
        else
        {
            Debug.LogWarning($"No audio clips found for category: {categoryName}");
        }
    }
    
    public void StopAudio()
    {
        audioSource.Stop();
    }

    public bool IsPlaying(string categoryName)
    {
        if (clipCategoryDict.TryGetValue(categoryName, out var category))
        {
            return audioSource.isPlaying && audioSource.clip == category.clipsWithVolume[0].clip;
        }
        else
        {
            Debug.LogWarning($"No audio clips found for category: {categoryName}");
            return false;
        }
       
    }
}
