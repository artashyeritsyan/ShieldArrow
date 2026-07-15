using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private int startSourcesCount = 5;
    private List<AudioSource> sources;

    private void Awake()
    {
        sources = new List<AudioSource>();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < startSourcesCount; i++)
        {
            CreateSource();
        }
    }

    private AudioSource CreateSource()
    {
        GameObject obj = new GameObject("AudioSource_" + sources.Count);
        obj.transform.SetParent(transform);
        obj.transform.position = Vector3.zero;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.maxDistance = 400;
        sources.Add(source);

        return source;
    }
    // spitalBlend can be in range (0-1);
    public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f, float spitalBlend = 0f)
    {
        //if (volume > 1f) volume = 1f;

        if (pitch > 3f) pitch = 3;
        else if (pitch < -3) pitch = -3;

        if (spitalBlend > 1) spitalBlend = 1;
        else if (spitalBlend < 0) spitalBlend = 0;

        AudioSource freeSource = null;

        for (int i = 0; i < Instance.sources.Count; i++)
        {
            if (!Instance.sources[i].isPlaying)
            {
                freeSource = Instance.sources[i];
                break;
            }
        }

        if (freeSource == null)
        {
            freeSource = Instance.CreateSource();
        }

        freeSource.transform.position = Vector3.zero;
        freeSource.clip = clip;
        freeSource.volume = volume;
        freeSource.pitch = pitch;
        freeSource.spatialBlend = spitalBlend;
        
        freeSource.Play();  
    }
}