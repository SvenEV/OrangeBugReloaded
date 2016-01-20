using System;
using System.Linq;
using UnityEngine;

public class SoundDictionary : MonoBehaviour
{
    public static SoundDictionary Instance;

    public NamedSound[] Sounds;

    public AudioClip Default;

    private void Awake()
    {
        Instance = this;
    }

    public AudioClip this[string name]
    {
        get
        {
            var sound = Sounds.FirstOrDefault(o => o.Name == name);
            return (sound == null) ? Default : sound.Clip;
        }
    }
}

[Serializable]
public class NamedSound
{
    public string Name;
    public AudioClip Clip;
}