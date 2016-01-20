using UnityEngine;
using System.Linq;
using System;

public class SpriteDictionary : MonoBehaviour
{
    public static SpriteDictionary Instance;

    public NamedSprite[] Sprites;

    public Sprite Default;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite this[string name]
    {
        get
        {
            var sprite = Sprites.FirstOrDefault(o => o.Name == name);
            return (sprite == null) ? Default : sprite.Sprite;
        }
    }
}

[Serializable]
public class NamedSprite
{
    public string Name;
    public Sprite Sprite;
}