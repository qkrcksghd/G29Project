using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FunctionTest : MonoBehaviour
{
    public int Score = 100; // ����
    public int deadScore = 70; // 70�� ���� Ż��
    public G29CarController controller;
    public float elapsedTime;  // ��� �ð�

    void Start()
    {
        elapsedTime = 0.0f;
    }

    void Update()
    {
        //if (controller.left)
        //{
        //    print("���� �������õ�");
        //}
        //if (controller.right)
        //{
        //    print("���� �������õ�");
        //}
        //if (controller.Night)
        //{
        //    print("����Ʈ");
        //}
        //if (controller.Head)
        //{
        //    print("�����");
        //}
    }
}
