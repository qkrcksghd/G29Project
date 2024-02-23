using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float LimitTime;
    public Text text_Timer;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        int minutes = Mathf.FloorToInt(LimitTime / 60);
        int seconds = Mathf.FloorToInt(LimitTime % 60);
        LimitTime += Time.deltaTime;

        text_Timer.text = "½Ã°£ : " + Mathf.Round(minutes) + ":" + Mathf.Round(seconds);
    }
}
