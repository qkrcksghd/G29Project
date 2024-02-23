using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum FunctionState
{
    Ready,           // �⺻ ���� = 0
    StartEngine,    // �õ� �ɱ� = 1
    ChangeGear,     // ��� �ٲٱ� = 2
    ReadyToLights,   //������ ���� �ɷ� �غ� = 3
    TurnOnHeadlights,   // ������ �ѱ� = 4
    SwitchToHighBeam,   // ��������� �ٲٱ� = 5
    SwitchToLowBeam,    // ��������� �ٲٱ� = 6
    TurnOnLeftTurnSignal,   // �����������õ� �ѱ� = 7
    TurnOffLeftTurnSignal,  // �����������õ� ���� = 8
    TurnOnWipers,   // ������ �ѱ� = 9
    TurnOffWipers,  // ������ ���� = 10
    StartDriving,    // ���� ���� = 11
    OutCenterLine,  //�߾Ӽ� ���� = 12
    TestSuccess,    //�հ��Դϴ� = 13
    TestFail,       //���հ��Դϴ� = 14
    OutCurb,        //���� ���� = 15
    Emergency       //���� = 16
}

//���� ���� ������
public enum DrivingState
{
    Start,
    Climb,
    TParking,
    Accel,
    End,
    Emeregency
}


public class FunctionTest : MonoBehaviour
{
    public int currentScore = 100; // ����
    private int deadScore = 70; // 70�� ���� Ż��
    private int soundIndex;
    private float timeRemaining; // �ʱ� ���� �ð�
    private float drivingTotalTime; //���� �� �ð�(9�� 40��)

    public List<AudioClip> soundClips; // ���� ���� Ŭ���� ���� ����Ʈ
    private AudioSource audioSource;

    private float accelSpeed;

    private bool isInEmergencyArea;
    private bool[] isTestPass;   // �� �׸� ��� ���θ� ������ bool �迭
    private bool[] isDrivingNum; //����̺� ������ �´���
    public bool isFail;         //���� ���� ����
    public bool isSuccess;

    private float timeInsideLine; // �ݶ��̴��� �ּҷ� �ӹ��� �ð� (������, T ����) 3��
    private bool isInLine = false;
    private float timer = 0f;

    private bool isOnceTime;

    private G29CarController controller;

    void Start()
    {
        isTestPass = new bool[16];
        isDrivingNum = new bool[10];
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<G29CarController>();
        if (soundClips.Count > 0)
        {
            // �ʱ� ���� Ŭ�� ���� (����Ʈ�� ������� ���� ��� ù ��° Ŭ�� ����)
            audioSource.clip = soundClips[0];
        }
        soundIndex = (int)FunctionState.Ready;
        timeInsideLine = 3f;
        timeRemaining = 10.0f;
        drivingTotalTime = 580.0f;
        audioSource.clip = soundClips[soundIndex];
        audioSource.Play();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "FunctionalTestRoom")
        {
            // ���ھ 70�� �̻��̾�� ������
            if (currentScore >= deadScore)
            {
                // ���� ���� ������ ��ɽ��� ����
                if (soundIndex < (int)FunctionState.StartDriving)
                    FunctionalPart();
                // ���� ���� ��
                else
                {
                    DrivingPart();
                }
            }

            if (soundIndex < (int)FunctionState.StartDriving)
            {
                if (isDrivingNum[(int)DrivingState.Start])
                    isFail = true;
            }

            // ���� ���հ�
            if (isFail)
            {
                audioSource.clip = soundClips[(int)FunctionState.TestFail];

                if (!isOnceTime)
                {
                    isOnceTime = true;
                    audioSource.Play();
                    Invoke("RestartScene", 3f);
                }
            }


            if (isSuccess)
            {
                if (!isOnceTime)
                {
                    isOnceTime = true;
                    isDrivingNum[(int)DrivingState.End] = true;
                    audioSource.clip = soundClips[(int)FunctionState.TestSuccess];
                    audioSource.Play();
                    Invoke("MoveMainScene", 3f);
                }
            }

            else // �� �κ��� �߰��Ͽ� isFail�� false�� �� ����ǵ��� �մϴ�.
            {
                // �߰��� �κ� ����
                if (soundIndex < (int)FunctionState.StartDriving)
                {
                    if (!audioSource.isPlaying)
                    {
                        UpdateTimerForFunction();
                        CheckTest();
                    }
                }
                // �߰��� �κ� ��
            }
        }
    }

    private void FunctionalPart()
    {
        //�ȳ� ������ ���� �Ŀ� Ÿ�̸� ���� (��� ���� �κ�)
        if (soundIndex < (int)FunctionState.StartDriving)
        {
            if (!audioSource.isPlaying)
            {
                UpdateTimerForFunction();
                CheckTest();
            }
        }

        if (isInLine)
        {
            timer += Time.deltaTime;

            if (timer >= timeInsideLine)
            {
                // �ݶ��̴��� ���� �ð� �̻� �ӹ��� ��쿡 ������ ����
                Debug.Log("�ݶ��̴��� " + timeInsideLine + "�� �̻� �ӹ������ϴ�!");
                // ���⿡ ���ϴ� ������ �߰��ϸ� �˴ϴ�.
            }
        }
    }

    private void DrivingPart()
    {

        MinusOrFailCase();
    }

    private void MinusOrFailCase()
    {
        if (!isFail)
        {
            if (isTestPass[(int)FunctionState.OutCenterLine])
            {
                isTestPass[(int)FunctionState.OutCenterLine] = false;
                audioSource.clip = soundClips[(int)FunctionState.TestFail];
                audioSource.loop = false;
                audioSource.Play();
            }
            if (isTestPass[(int)FunctionState.OutCurb])
            {
                isTestPass[(int)FunctionState.OutCenterLine] = false;
                audioSource.clip = soundClips[(int)FunctionState.TestFail];
                audioSource.loop = false;
                audioSource.Play();
            }
        }
    }

    // Ÿ�̸� ������Ʈ �Լ�(�� �׸� ���ѽð���)
    private void UpdateTimerForFunction()
    {
        if (timeRemaining > 0)
        {
            // �ð��� ������ ��� Ÿ�̸� ����
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                TimeEnd();
            }
        }
    }

    private void UpdateTimerForDriving()
    {
        drivingTotalTime -= Time.deltaTime;
    }

    // �ð� ���� �� ������ ����
    private void TimeEnd()
    {
        if (!isFail)
        {
            CheckScore();   //���� ���� üũ

            soundIndex++;   //���� �׽�Ʈ �׸����� �Ѿ

            audioSource.clip = soundClips[soundIndex];  //���� ��ü
            audioSource.Play(); //����� ���


            SetTimeLimit(5.0f);//Ÿ�̸� �ʱ�ȭ
                               //��� ������ 10��
            if (soundIndex == (int)FunctionState.ChangeGear)
            {
                SetTimeLimit(10.0f);
            }
        }

    }

    // �ܺο��� ���� �ð��� �����ϴ� �Լ�
    public void SetTimeLimit(float newTimeLimit)
    {
        timeRemaining = newTimeLimit;
    }

    public void CheckScore()
    {
        //false�� ����
        if (!isTestPass[soundIndex])
        {
            if (soundIndex != 0 && soundIndex != 3 && soundIndex != 4 && soundIndex != 11) 
            {
                currentScore -= 5;
            }
            if (soundIndex == 11)
            {
                print(currentScore);
            }
        }
    }

    public void CheckTest() // �����׽�Ʈ �ϴ� �Լ�
    {
        if (!isFail)
        {
            if (soundIndex == (int)FunctionState.StartEngine) //1�� �õ� On�˻�
            {
                if (controller.engineStart)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.ChangeGear) //2�� ��� ���� �˻�
            {
                if (controller.isAcc == 1)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.SwitchToHighBeam) //4�� ������ On�˻�
            {
                if (controller.isHeadLight)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == 5) //5�� ����� On�˻�
            {
                if (controller.isHighBeam)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == 6) //
            {
                if (!controller.isHeadLight && !controller.isHighBeam)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.TurnOnLeftTurnSignal)
            {
                if (controller.isLeft)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.TurnOffLeftTurnSignal)
            {
                if (!controller.isLeft)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.TurnOnWipers)
            {
                if (controller.isWiper == true)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.TurnOffWipers)
            {
                if (controller.isWiper == false)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        //�߾Ӽ� ħ�� -10
        if (other.gameObject.tag == "CenterLine")
        {
            isTestPass[(int)FunctionState.OutCenterLine] = true;
        }

        //���� ���� �ǰ�
        if(other.gameObject.tag == "CurbLine")
        {
            isTestPass[(int)FunctionState.OutCurb] = true;
        }

        
        if(other.gameObject.name == "StartCollider")
        {
            Debug.Log("����");
            isDrivingNum[(int)DrivingState.Start] = true;
        }

        if (other.gameObject.name == "ClimbStartCollider")
        {
            timer = 0f;
            Debug.Log("������ ����");
            isDrivingNum[(int)DrivingState.Climb] = true;
        }

        if(other.gameObject.name == "ClimbStopCollider")
        {
            Debug.Log("������ ����");
            isInLine = true;
        }

        //�������� �и� ������ ����
        if (other.gameObject.name == "ClimbMinusCollider")
        {
            Debug.Log("������ ����");
            if (isInLine)
            {
                currentScore -= 10;
            }
        }

        if(other.gameObject.name == "ParkingStartCollider")
        {
            timer = 0f;
            isDrivingNum[(int)DrivingState.TParking] = true;
        }

        if (other.gameObject.name == "ParkingLineCollider")
        {
            isInLine = true;
        }

        if (other.gameObject.name == "ParkingStopCollider")
        {
            currentScore -= 10;
        }

        if(other.gameObject.name == "AccelStartCollider")
        {
            isDrivingNum[(int)DrivingState.Accel] = true;
            //�ӵ� �κ�
            //accelSpeed = ����
        }

        if (other.gameObject.name == "AccelCheckCollider")
        {
            //accelSpeed�� ���� ���ǵ� ���ؼ� �� Ŀ����
        }

        if (other.gameObject.name == "AccelEndCollider")
        {
            //accelSpeed�� ���� ���Ͽ�����
        }

        if (other.gameObject.name == "EndCollider")
        {
            if (!isDrivingNum[(int)DrivingState.Start])
            {
                Debug.Log("������....");
                isFail = true;
                
            }
            else
            {
                isSuccess = true;
            }
            if(isSuccess)
            {
                isDrivingNum[(int)DrivingState.End] = true;
                audioSource.clip = soundClips[(int)FunctionState.TestSuccess];
                audioSource.Play();
            }
        }

        if(other.gameObject.name == "EmergencyArea")
        {
            isInEmergencyArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.name == "EmergencyArea")
        {
            isInEmergencyArea = false;
        }
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void MoveMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}

