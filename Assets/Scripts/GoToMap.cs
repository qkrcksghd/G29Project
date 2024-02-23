using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GoToMap : MonoBehaviour
{
    AudioSource bgAudioSource;

    public AudioClip uiAudio;
    private GameObject mainCamera;

    public Image HowToImage;
    public GameObject settingPanel;

    public Slider[] settingSlider;
    public TMP_Text [] settingText;

    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        HowToImage.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        mainCamera = GameObject.Find("Main Camera");
        bgAudioSource = mainCamera.GetComponent<AudioSource>();
        if (HowToImage.gameObject.activeSelf)
        {
            if(Input.GetMouseButtonDown(0))
            {
                HowToImage.gameObject.SetActive(!HowToImage.gameObject.activeSelf);
            }
        }

        if (settingPanel.activeSelf)
        {
            for(int i=0;  i <3; i++)
            {
                settingText[i].text = Mathf.Round(settingSlider[i].value * 100).ToString();
            }
        }
    }
    public void OnStartButtonClicked_School()
    {
        if (!HowToImage.gameObject.activeSelf)
        {
            gameManager.PlayEffectSound(uiAudio);
            SceneManager.LoadScene("School_NotComplete");
        }
    }
    public void OnStartButtonClicked_Fun()
    {
        if (!HowToImage.gameObject.activeSelf)
        {
            gameManager.PlayEffectSound(uiAudio);
            SceneManager.LoadScene("FunctionalTestRoom");
        }
    }
    public void OnHowToPlayButtonClicked()
    {
        gameManager.PlayEffectSound(uiAudio);
        HowToImage.gameObject.SetActive(!HowToImage.gameObject.activeSelf);
    }
    public void OnSettingButtonClicked()
    {
        gameManager.PlayEffectSound(uiAudio);
        settingPanel.gameObject.SetActive(true);
    }
    public void OnSettingBackButtonClicked()
    {
        gameManager.PlayEffectSound(uiAudio);
        settingPanel.gameObject.SetActive(false);
    }
}