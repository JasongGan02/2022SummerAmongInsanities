using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // Editor-only API
#endif

[CreateAssetMenu(fileName = "AudioClipCategory", menuName = "Audio/Audio Clip Category")]
public class AudioClipCategory: ScriptableObject
{
    public string audioName;
    public ClipWithVolume[] clipsWithVolume;
    public bool loop;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Only run this code in the Editor
        audioName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
    }
#endif
    
    [System.Serializable]
    public class ClipWithVolume
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume = 1.0f;  // Default volume can be set to max
    }
}