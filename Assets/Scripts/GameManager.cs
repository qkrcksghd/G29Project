using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    AudioSource bgAudioSource;
    AudioSource efAudioSource;

    public AudioClip []effectClips;

    public Slider[] settingSliders;
    float bgVolume;
    float efVolume;
    float pedalSen;
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }



    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "MainScene")
        {
            bgVolume = settingSliders[0].value;
            efVolume = settingSliders[1].value;
            pedalSen = settingSliders[2].value;
        }

        if (bgAudioSource == null)
        {
            bgAudioSource = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AudioSource>();
        }

        if (efAudioSource == null)
        {
                efAudioSource = GetComponent<AudioSource>();
        }

        bgAudioSource.volume = bgVolume;
        efAudioSource.volume = efVolume;
    }

    public void PlayEffectSound(AudioClip clipName)
    {
        efAudioSource.loop = false;
        efAudioSource.clip = clipName;
        efAudioSource.Play();
    }
    
}
