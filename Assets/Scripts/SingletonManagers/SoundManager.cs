using Firebase.Database;
using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Action volumeChange;

    public float BGM_Volume = 1f;
    public float Effect_Volume = 1f;

    public void ChangeBGM_Volume(float volume)
    {
        BGM_Volume = volume;
        volumeChange.Invoke();
    }

    public void ChangeEffect_Volume(float volume)
    {
        Effect_Volume = volume;
        volumeChange.Invoke();
    }
}
