using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderControl : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("동작");
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "CenterLine")
        {
            Debug.Log("중앙선 침범 - 10");
        }
    }
    

}
