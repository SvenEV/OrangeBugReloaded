using UnityEngine;

public class ButtonHelper : MonoBehaviour
{
    private const float _initialDelay = .3f;
    private const float _repeatDelay = .15f;

    private float _pressTime;
    private float _delay;

    public static ButtonHelper Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public bool Get(string buttonName)
    {
        if (Input.GetButtonDown(buttonName))
        {
            _pressTime = Time.time;
            _delay = _initialDelay;
            return true;
        }
        else if (Input.GetButton(buttonName))
        {
            var passed = Time.time - _pressTime;
            
            if (passed > _delay)
            {
                _delay = _repeatDelay;
                _pressTime = Time.time;
                return true;
            }
        }

        return false;
    }
}
