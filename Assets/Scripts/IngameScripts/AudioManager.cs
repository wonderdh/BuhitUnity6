using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioSource BAS;    //Button Audio Source
    [SerializeField]
    AudioSource SC_AS;  // Success/CLear Audio Source

    /**0 = Button, 1 = Success, 2 = Clear**/
    [SerializeField]
    AudioClip[] audioClip;

    enum CLIP
    {
        BUTTON = 0,
        SUCCESS,
        CLEAR,
    }

    public void PlayButton()
    {
        BAS.PlayOneShot(audioClip[(int)CLIP.BUTTON]);
    }

    public void PlaySuccess()
    {
        SC_AS.PlayOneShot(audioClip[(int)CLIP.SUCCESS]);
    }

    public void PlayClear()
    {
        SC_AS.PlayOneShot(audioClip[(int)CLIP.CLEAR]);
    }
}
