using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum FunctionState
{
    Ready,           // 기본 상태 = 0
    StartEngine,    // 시동 걸기 = 1
    ChangeGear,     // 기어 바꾸기 = 2
    ReadyToLights,   //전조등 조작 능력 준비 = 3
    TurnOnHeadlights,   // 전조등 켜기 = 4
    SwitchToHighBeam,   // 상향등으로 바꾸기 = 5
    SwitchToLowBeam,    // 하향등으로 바꾸기 = 6
    TurnOnLeftTurnSignal,   // 좌측방향지시등 켜기 = 7
    TurnOffLeftTurnSignal,  // 좌측방향지시등 끄기 = 8
    TurnOnWipers,   // 와이퍼 켜기 = 9
    TurnOffWipers,  // 와이퍼 끄기 = 10
    StartDriving,    // 주행 시작 = 11
    OutCenterLine,  //중앙선 감점 = 12
    TestSuccess,    //합격입니다 = 13
    TestFail,       //불합격입니다 = 14
    OutCurb,        //연석 감지 = 15
    Emergency       //돌발 = 16
}

//주행 순서 열거형
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
    public int currentScore = 100; // 점수
    private int deadScore = 70; // 70점 이하 탈락
    private int soundIndex;
    private float timeRemaining; // 초기 제한 시간
    private float drivingTotalTime; //주행 총 시간(9분 40초)

    public List<AudioClip> soundClips; // 여러 사운드 클립을 담을 리스트
    private AudioSource audioSource;

    private float accelSpeed;

    private bool isInEmergencyArea;
    private bool[] isTestPass;   // 각 항목 통과 여부를 저장할 bool 배열
    private bool[] isDrivingNum; //드라이브 순서에 맞는지
    public bool isFail;         //시험 실패 여부
    public bool isSuccess;

    private float timeInsideLine; // 콜라이더에 최소로 머무를 시간 (오르막, T 주차) 3초
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
            // 초기 사운드 클립 설정 (리스트가 비어있지 않을 경우 첫 번째 클립 선택)
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
            // 스코어가 70점 이상이어야 진행함
            if (currentScore >= deadScore)
            {
                // 주행 시작 전에만 기능시험 진행
                if (soundIndex < (int)FunctionState.StartDriving)
                    FunctionalPart();
                // 주행 시작 후
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

            // 시험 불합격
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

            else // 이 부분을 추가하여 isFail이 false일 때 재생되도록 합니다.
            {
                // 추가된 부분 시작
                if (soundIndex < (int)FunctionState.StartDriving)
                {
                    if (!audioSource.isPlaying)
                    {
                        UpdateTimerForFunction();
                        CheckTest();
                    }
                }
                // 추가된 부분 끝
            }
        }
    }

    private void FunctionalPart()
    {
        //안내 음성이 끝난 후에 타이머 시작 (기능 시험 부분)
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
                // 콜라이더에 일정 시간 이상 머무른 경우에 수행할 동작
                Debug.Log("콜라이더에 " + timeInsideLine + "초 이상 머물렀습니다!");
                // 여기에 원하는 동작을 추가하면 됩니다.
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

    // 타이머 업데이트 함수(각 항목 제한시간용)
    private void UpdateTimerForFunction()
    {
        if (timeRemaining > 0)
        {
            // 시간이 남았을 경우 타이머 감소
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

    // 시간 종료 시 수행할 동작
    private void TimeEnd()
    {
        if (!isFail)
        {
            CheckScore();   //점수 감점 체크

            soundIndex++;   //다음 테스트 항목으로 넘어감

            audioSource.clip = soundClips[soundIndex];  //사운드 교체
            audioSource.Play(); //오디오 재생


            SetTimeLimit(5.0f);//타이머 초기화
                               //기어 변속은 10초
            if (soundIndex == (int)FunctionState.ChangeGear)
            {
                SetTimeLimit(10.0f);
            }
        }

    }

    // 외부에서 제한 시간을 설정하는 함수
    public void SetTimeLimit(float newTimeLimit)
    {
        timeRemaining = newTimeLimit;
    }

    public void CheckScore()
    {
        //false면 감점
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

    public void CheckTest() // 시험테스트 하는 함수
    {
        if (!isFail)
        {
            if (soundIndex == (int)FunctionState.StartEngine) //1번 시동 On검사
            {
                if (controller.engineStart)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.ChangeGear) //2번 기어 전진 검사
            {
                if (controller.isAcc == 1)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == (int)FunctionState.SwitchToHighBeam) //4번 전조등 On검사
            {
                if (controller.isHeadLight)
                {
                    audioSource.loop = false;
                    isTestPass[soundIndex] = true;
                }
            }
            if (soundIndex == 5) //5번 상향등 On검사
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

        //중앙선 침범 -10
        if (other.gameObject.tag == "CenterLine")
        {
            isTestPass[(int)FunctionState.OutCenterLine] = true;
        }

        //연석 밟음 실격
        if(other.gameObject.tag == "CurbLine")
        {
            isTestPass[(int)FunctionState.OutCurb] = true;
        }

        
        if(other.gameObject.name == "StartCollider")
        {
            Debug.Log("시작");
            isDrivingNum[(int)DrivingState.Start] = true;
        }

        if (other.gameObject.name == "ClimbStartCollider")
        {
            timer = 0f;
            Debug.Log("오르막 시작");
            isDrivingNum[(int)DrivingState.Climb] = true;
        }

        if(other.gameObject.name == "ClimbStopCollider")
        {
            Debug.Log("오르막 감점");
            isInLine = true;
        }

        //오르막길 밀림 지점에 다음
        if (other.gameObject.name == "ClimbMinusCollider")
        {
            Debug.Log("오르막 감점");
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
            //속도 부분
            //accelSpeed = 저장
        }

        if (other.gameObject.name == "AccelCheckCollider")
        {
            //accelSpeed랑 현재 스피드 비교해서 더 커야함
        }

        if (other.gameObject.name == "AccelEndCollider")
        {
            //accelSpeed가 일정 이하여야함
        }

        if (other.gameObject.name == "EndCollider")
        {
            if (!isDrivingNum[(int)DrivingState.Start])
            {
                Debug.Log("후진함....");
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

