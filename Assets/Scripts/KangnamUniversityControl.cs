using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KangnamUniversityControl : MonoBehaviour
{
    public Text wrong_text;
    public G29CarController controller;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (controller.wrong_way)
        {
            wrong_text.text = "경로를 이탈했습니다.";
        }
        
    }
}
