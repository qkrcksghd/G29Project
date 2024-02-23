using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarBroken : MonoBehaviour
{
    public float percent = 100;
    public Text text_broken;
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
            percent -= 10;
            text_broken.text = "ÆÄ¼Õ·ü : " + percent + "%";
        }

    }
}
