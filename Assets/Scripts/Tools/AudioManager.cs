using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource source;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameSet.instance.audioManager = this;
    }

    public void PlayAudio(AudioClip clip) {
        if (GameSet.instance.userData.setData.audio)
        {
            source.PlayOneShot(clip);
        }
        
        if (clip == GameSet.instance.matter.BtnAudio || clip==GameSet.instance.matter.CloseBtnAudio) {
            GameSet.instance.Shake(AllShake.btn);
        }
    }
}
