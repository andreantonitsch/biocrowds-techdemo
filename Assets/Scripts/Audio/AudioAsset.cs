using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioAsset : ScriptableObject
{
    [SerializeField] private List<AudioClip> _clips;

    public AudioClip Play()
    {
        return _clips[Random.Range(0, _clips.Count)];
    }


}

