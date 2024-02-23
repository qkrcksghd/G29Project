using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip uiAudio;
    private GameObject mainCamera;
    public GameObject mainMenuCanvas;
    public GameObject selectionCanvas;
   

    private void Update()
    {
        mainCamera = GameObject.Find("Main Camera");
        audioSource = mainCamera.GetComponent<AudioSource>();
    }
    void Start()
    {
        mainMenuCanvas.SetActive(true);
        selectionCanvas.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        audioSource.PlayOneShot(uiAudio, 1f);
        mainMenuCanvas.SetActive(false);

        selectionCanvas.SetActive(true);
    }
}
