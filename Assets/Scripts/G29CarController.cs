using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EffectSound
{
    EngineStart,
    TurnLight,
    Clarksion,
}

public class G29CarController : MonoBehaviour
{
    private WheelCollider[] wheelColliders;
    private GameObject[] wheels;
    private Rigidbody rb;
    private Vector3 m_LastPosition;

    [Header("Camara")]
    public GameObject TPSCamara; // 3인칭 카메라
    public GameObject FPSCamara; // 1인칭 카메라
    public GameObject LEFTCamara; // 좌측사이드미러
    public GameObject RIGHTCamara; // 우측사이드미러
    public float power; //속력

    [Header("Shift")]
    public int isAcc; //전진 후진을 알려주기 위해 추가한듯

    [Header("Handle")]
    public GameObject turnLightLeft; //좌측깜박이
    public GameObject turnLightRight;//우측깜박이
    public GameObject LightNight;//라이트
    public GameObject LightHead;//상향등
    public GameObject LightBreak;//브레이크등
    public float HandleRotate;//핸들범위값
    public GameObject Handle;//핸들오브젝트 불러오기

    [Header("Padel")]
    public float breakPower; //브레이크 범위값
    private float accPower; //엑셀범위
    private int intAccP; //엑셀범위에따른 파워값 조정하는 변수
    public bool engineStart = false; //시동이 걸렸나 안걸렸나 검사하는 변수
    private bool engineStartButtonDown = false;

    [Header("Car")]
    private float wheelBase; //앞 뒤 바퀴 사이의 거리(m단위)
    private float rearTrack; //좌 우 바튀 사이의 거리(m단위)
    public float turnRadius; //회전 반지름(m단위)
    private float ro; //바퀴 회전값
    public float Rpm; //rpm값
    public bool isBreak; //브레이크값
    public bool isLeft = false; //좌측 깜박이
    public bool isRight = false; //우측 깜박이
    public bool isHeadLight = false; //전조등
    public bool isHighBeam = false; //상향등
    public bool isPersonChange = false; //인칭 인칭이 true면 1인칭 false면 3인칭
    public bool isWiper = false; //와이퍼
    private bool isLeftButtonDown = false; // 좌측깜박이 확인하는 변수
    private bool isRightButtonDown = false; // 우측깜박이 확인하는 변수
    private bool isHeadLightButtonDown = false; // 전조등 확인하는 변수
    private bool isHighBeamButtonDown = false; // 상향등 확인하는 변수
    private bool isPersonButtonDown = false; // 인칭 확인하는 변수
    private bool isWiperButtonDown = false; // 좌측깜박이 확인하는 변수
    public GameObject SpeedPoint; //속도계기판 포인터
    public GameObject RpmPoint;//RPM계기판 포인터
    public GameObject leftWiperPoint; //왼쪽 와이퍼 포인터
    public GameObject rightWiperPoint; //우측 와이퍼 포인터
    private float[] wiperRotationValue = new float[2]; //와이퍼의 각 rotation의 값이 다르기 때문에 따로 저장
    private float[] wiperRotationAddValue = new float[2]; //와이퍼를 원상태로 돌리기 위해서 위에 값는 내비두고 새로운 값을 더해서 +, -하기위해서 내비둠
    public bool isWiperRotationUp = false; //와이퍼가 위로 회전하면 true 
    public float m_Speed; //차량의 속도를 나타내주는 변수
    public bool wrong_way = false;

    private AudioSource audioSource;
    public AudioClip[] carEffectSoundList;

    FunctionTest functionTestScript;

    // Start is called before the first frame update
    void Start()
    {
        Init();
        audioSource = GetComponent<AudioSource>();
        functionTestScript = GetComponent<FunctionTest>();
    }

    private void FixedUpdate()
    {
        if(SceneManager.GetActiveScene().name == "FunctionalTestRoom" && functionTestScript.isFail)
        {
            engineStart = false;
        }

        if (!functionTestScript.isFail)
        {
            if (engineStart == true)
            {
                SteerVehicle();
                WheelPosWithCollider();
                for (int i = 0; i < wheels.Length; i++)
                {
                    wheelColliders[i].motorTorque = isAcc * power;
                }
            }
            if (breakPower > 32767)
            {
                isBreak = true;
            }
            else
            {
                isBreak = false;
            }

            foreach (WheelCollider wheel in wheelColliders)
            {
                wheel.brakeTorque = 3 * breakPower;
            }

            m_Speed = GetSpeed();
            Debug.Log(m_Speed);
        }
    }

    void Update()
    {
        if (!functionTestScript.isFail)
        {
            InputLogitech();
            UpdataCar();
            Lights();
        }

        if (wrong_way)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (!engineStart)
        {
            ro = 0;
            isBreak = true;
            breakPower = 1000;
        }
    }

    private void InputLogitech()
    {
        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        rec = LogitechGSDK.LogiGetStateUnity(0);
        ro = rec.lX / 32767f;
        HandleRotate = -(rec.lX / 72.8f);
        accPower = Mathf.Abs(rec.lY - 32767);
        intAccP = (int)accPower / 10000;

        if (rec.rgbButtons[14] == 128) isAcc = 1;
        if (rec.rgbButtons[15] == 128) isAcc = -1;


        //찬홍아 이게 도대체 무슨 코드냐...ㅅㅂ
        if (rec.rgbButtons[4] == 128)
        {
            if (!isRightButtonDown)
            {
                isRightButtonDown = true;

                if (isRight)
                {
                    isRight = false;
                }
                else
                {
                    isRight = true;
                    print("우측 방향 지시등 켜짐");
                }
            }
        }
        else
        {
            isRightButtonDown = false;
        }
        if (rec.rgbButtons[5] == 128)
        {
            if (!isLeftButtonDown)
            {
                isLeftButtonDown = true;

                if (isLeft)
                {
                    isLeft = false;
                    audioSource.Stop();
                }
                else
                {
                    isLeft = true;
                    PlayTurnLightSound();
                    print("좌측 방향 지시등 켜짐");
                }
            }
        }
        else
        {
            isLeftButtonDown = false;
        }
        if (rec.rgbButtons[0] == 128)
        {
            if (!isHighBeamButtonDown)
            {
                isHighBeamButtonDown = true;

                if (isHighBeam)
                {
                    isHighBeam = false;
                }
                else
                {
                    isHighBeam = true;
                    print("상향등 켜짐");
                }
            }
        }
        else
        {
            isHighBeamButtonDown = false;
        }
        if (rec.rgbButtons[2] == 128 || Input.GetKeyDown(KeyCode.P))
        {
            if (!isHeadLightButtonDown)
            {
                isHeadLight = true;

                if (isHeadLight)
                {
                    isHeadLight = false;
                    audioSource.Stop();
                }
                else
                {
                    isHeadLight = true;
                    PlayTurnLightSound();
                    print("라이트 켜짐");
                }
            }
        }
        else
        {
            isHeadLightButtonDown = false;
        }
        if (rec.rgbButtons[11] == 128)
        {
            if (!isWiperButtonDown)
            {
                isWiperButtonDown = true;

                if (isWiper)
                {
                    isWiper = false;
                }
                else
                {
                    isWiper = true;
                    print("와이퍼 켜짐");
                }
            }
        }
        else
        {
            isWiperButtonDown = false;
        }
        if (rec.rgbButtons[7] == 128)
        {
            if (!isPersonButtonDown)
            {
                isPersonButtonDown = true;

                if (isPersonChange)
                {
                    isPersonChange = false;
                    print("1인칭 켜짐");
                }
                else
                {
                    isPersonChange = true;
                    print("3인칭 켜짐");
                }
            }
        }
        else
        {
            isPersonButtonDown = false;
        }

        if (rec.rgbButtons[23] == 128 || Input.GetKeyDown(KeyCode.N))
        {
            if (!engineStartButtonDown)
            {
                engineStartButtonDown = true;

                if (engineStart)
                {
                    engineStart = false;
                    
                }
                else
                {
                    engineStart = true;
                    audioSource.clip = carEffectSoundList[(int)EffectSound.EngineStart];
                    audioSource.loop = false;
                    audioSource.Play();
                    print("엔진 켜짐");
                }
            }
        }
        else
        {
            engineStartButtonDown = false;
        }
        if (rec.lRz != 0)
        {
            breakPower = Mathf.Abs(rec.lRz - 32767);
        }

        
        if(rec.rgbButtons[6] == 128)
        {
            audioSource.clip = carEffectSoundList[(int)EffectSound.Clarksion];
            audioSource.loop = false;
            audioSource.Play();
        }
    }
    private void UpdataCar()
    {
        SpeedPoint.transform.localRotation = Quaternion.Euler(0, 0, 45 + -m_Speed);
        RpmPoint.transform.localRotation = Quaternion.Euler(0, 0, 45 + -Rpm);
        Handle.transform.localRotation = Quaternion.Euler(0, 0, HandleRotate);
        leftWiperPoint.transform.localRotation = Quaternion.Euler(-41.434f, -27.844f, wiperRotationAddValue[0]);
        rightWiperPoint.transform.localRotation = Quaternion.Euler(-49.15f, -0.766f, wiperRotationAddValue[1]);
        if (isWiper) wiperRotation();
        else
        {
            returnWiperRotation();
        }
        TPSCamara.SetActive(!isPersonChange);
        FPSCamara.SetActive(isPersonChange);
        LEFTCamara.SetActive(isPersonChange);
        RIGHTCamara.SetActive(isPersonChange);
    }
    private void wiperRotation()
    {
        if (isWiperRotationUp)
        {
            if (wiperRotationAddValue[0] >= 100 || wiperRotationAddValue[1] >= 100) isWiperRotationUp = false;
            wiperRotationAddValue[0]++;
            wiperRotationAddValue[1]++;
        }
        else
        {
            if (wiperRotationAddValue[0] <= wiperRotationValue[0] && wiperRotationAddValue[1] <= wiperRotationValue[1]) isWiperRotationUp = true;
            wiperRotationAddValue[0]--;
            wiperRotationAddValue[1]--;
        }
    }
    private void returnWiperRotation()
    {
        if(wiperRotationAddValue[0] > wiperRotationValue[0] && wiperRotationAddValue[1] > wiperRotationValue[1])
        {
            wiperRotationAddValue[0]--;
            wiperRotationAddValue[1]--;
        }
    }
    private void WheelPosWithCollider()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation); //wheelCollider의  월드 포지션과 월드 로테이션 값을 받아옴
            wheels[i].transform.position = wheelPosition;
            wheels[i].transform.rotation = wheelRotation;

        }
        float avgRPM = 0;
        for (int i = 0; i < 4; i++)
        {
            avgRPM += wheelColliders[i].rpm;
        }
        avgRPM /= 4;

        // RPM 값을 각도로 변환 (시계 방향이 양수, 반시계 방향이 음수)
        Rpm = avgRPM * 300 / 1000;
    }

    // 애커만 조향
    private void SteerVehicle()
    {
        if (ro > 0)
        {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
        }
        else if (ro < 0)
        {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
        }
        else
        {
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }
        if (accPower == 0) power = 0;
        switch (intAccP)
        {
            case 0:
                power = 0;
                break;
            case 1:
                power = 100;
                break;
            case 2:
                power = 150;
                break;
            case 3:
                power = 200;
                break;
            case 4:
                power = 250;
                break;
            case 5:
                power = 500;
                break;
            case 6:
                power = 600;
                break;
        }
    }
    private void Init()
    {
        power = 0;
        wheelColliders = new WheelCollider[4];
        wheels = new GameObject[4];
        rb = GetComponentInParent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1, 0); //무게중심을 y축 방향으로 낮춤
        wheels[0] = GameObject.FindGameObjectWithTag("FRWheel");
        wheels[1] = GameObject.FindGameObjectWithTag("FLWheel");
        wheels[2] = GameObject.FindGameObjectWithTag("RRWheel");
        wheels[3] = GameObject.FindGameObjectWithTag("RLWheel");
        wheelColliders[0] = GameObject.Find("WheelHubFrontRight").GetComponent<WheelCollider>();
        wheelColliders[1] = GameObject.Find("WheelHubFrontLeft").GetComponent<WheelCollider>();
        wheelColliders[2] = GameObject.Find("WheelHubRearRight").GetComponent<WheelCollider>();
        wheelColliders[3] = GameObject.Find("WheelHubRearLeft").GetComponent<WheelCollider>();

        for (int i = 0; i < 4; i++)
        {
            //wheelColliders[i].transform.position = wheels[i].transform.position;
            wheelColliders[i].transform.position = new Vector3(wheels[i].transform.position.x, wheelColliders[i].transform.position.y, wheels[i].transform.position.z);
            Debug.Log(wheelColliders[i].transform.position);
        }

        turnRadius = 3;
        wheelBase = 2.0f;
        rearTrack = wheelColliders[0].transform.position.x - wheelColliders[2].transform.position.x;

        wiperRotationValue[0] = leftWiperPoint.transform.rotation.eulerAngles.z;
        wiperRotationValue[1] = rightWiperPoint.transform.rotation.eulerAngles.z;
        wiperRotationAddValue[0] = wiperRotationValue[0];
        wiperRotationAddValue[1] = wiperRotationValue[1];
        breakPower = 0;
        isBreak = false;
    }

    private float GetSpeed()
    {
        float speed = (((transform.position - m_LastPosition).magnitude) / Time.deltaTime);
        m_LastPosition = transform.position;
        return speed * 10;
    }
    private void Lights()
    {
        turnLightRight.SetActive(isRight);
        turnLightLeft.SetActive(isLeft);
        LightBreak.SetActive(isBreak);
        LightHead.SetActive(isHighBeam);
        LightNight.SetActive(isHeadLight);
    }

    private void PlayTurnLightSound()
    {
        audioSource.clip = carEffectSoundList[(int)EffectSound.TurnLight];
        audioSource.loop = true;
        audioSource.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "wrong")
        {
            wrong_way = true;
        }
    }
}