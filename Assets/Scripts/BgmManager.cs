using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class BgmManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        SoundManager.Instance.volumeChange += VolumeChange;
        
        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.volume = SoundManager.Instance.BGM_Volume;
        audioSource.Play();
    }

    private void VolumeChange()
    {
        audioSource.volume = SoundManager.Instance.BGM_Volume;
    }

    private void OnDestroy()
    {
        StartCoroutine(FadeVolume(audioSource.volume, 0, 1));
    }

    public IEnumerator FadeVolume(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        audioSource.volume = end;
    }
}
