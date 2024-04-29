using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AudioClipCategory", menuName = "Audio/Audio Clip Category")]
public class AudioClipCategory: ScriptableObject
{
    public string audioName;
    public ClipWithVolume[] clipsWithVolume;
    public bool loop;
    
    private void OnValidate()
    {
        audioName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
    
    [System.Serializable]
    public class ClipWithVolume
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume = 1.0f;  // Default volume can be set to max
    }
}