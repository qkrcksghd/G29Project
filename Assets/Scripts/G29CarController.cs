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
    public GameObject TPSCamara; // 3��Ī ī�޶�
    public GameObject FPSCamara; // 1��Ī ī�޶�
    public GameObject LEFTCamara; // �������̵�̷�
    public GameObject RIGHTCamara; // �������̵�̷�
    public float power; //�ӷ�

    [Header("Shift")]
    public int isAcc; //���� ������ �˷��ֱ� ���� �߰��ѵ�

    [Header("Handle")]
    public GameObject turnLightLeft; //����������
    public GameObject turnLightRight;//����������
    public GameObject LightNight;//����Ʈ
    public GameObject LightHead;//�����
    public GameObject LightBreak;//�극��ũ��
    public float HandleRotate;//�ڵ������
    public GameObject Handle;//�ڵ������Ʈ �ҷ�����

    [Header("Padel")]
    public float breakPower; //�극��ũ ������
    private float accPower; //��������
    private int intAccP; //�������������� �Ŀ��� �����ϴ� ����
    public bool engineStart = false; //�õ��� �ɷȳ� �Ȱɷȳ� �˻��ϴ� ����
    private bool engineStartButtonDown = false;

    [Header("Car")]
    private float wheelBase; //�� �� ���� ������ �Ÿ�(m����)
    private float rearTrack; //�� �� ��Ƣ ������ �Ÿ�(m����)
    public float turnRadius; //ȸ�� ������(m����)
    private float ro; //���� ȸ����
    public float Rpm; //rpm��
    public bool isBreak; //�극��ũ��
    public bool isLeft = false; //���� ������
    public bool isRight = false; //���� ������
    public bool isHeadLight = false; //������
    public bool isHighBeam = false; //�����
    public bool isPersonChange = false; //��Ī ��Ī�� true�� 1��Ī false�� 3��Ī
    public bool isWiper = false; //������
    private bool isLeftButtonDown = false; // ���������� Ȯ���ϴ� ����
    private bool isRightButtonDown = false; // ���������� Ȯ���ϴ� ����
    private bool isHeadLightButtonDown = false; // ������ Ȯ���ϴ� ����
    private bool isHighBeamButtonDown = false; // ����� Ȯ���ϴ� ����
    private bool isPersonButtonDown = false; // ��Ī Ȯ���ϴ� ����
    private bool isWiperButtonDown = false; // ���������� Ȯ���ϴ� ����
    public GameObject SpeedPoint; //�ӵ������ ������
    public GameObject RpmPoint;//RPM����� ������
    public GameObject leftWiperPoint; //���� ������ ������
    public GameObject rightWiperPoint; //���� ������ ������
    private float[] wiperRotationValue = new float[2]; //�������� �� rotation�� ���� �ٸ��� ������ ���� ����
    private float[] wiperRotationAddValue = new float[2]; //�����۸� �����·� ������ ���ؼ� ���� ���� ����ΰ� ���ο� ���� ���ؼ� +, -�ϱ����ؼ� �����
    public bool isWiperRotationUp = false; //�����۰� ���� ȸ���ϸ� true 
    public float m_Speed; //������ �ӵ��� ��Ÿ���ִ� ����
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


        //��ȫ�� �̰� ����ü ���� �ڵ��...����
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
                    print("���� ���� ���õ� ����");
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
                    print("���� ���� ���õ� ����");
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
                    print("����� ����");
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
                    print("����Ʈ ����");
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
                    print("������ ����");
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
                    print("1��Ī ����");
                }
                else
                {
                    isPersonChange = true;
                    print("3��Ī ����");
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
                    print("���� ����");
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
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation); //wheelCollider��  ���� �����ǰ� ���� �����̼� ���� �޾ƿ�
            wheels[i].transform.position = wheelPosition;
            wheels[i].transform.rotation = wheelRotation;

        }
        float avgRPM = 0;
        for (int i = 0; i < 4; i++)
        {
            avgRPM += wheelColliders[i].rpm;
        }
        avgRPM /= 4;

        // RPM ���� ������ ��ȯ (�ð� ������ ���, �ݽð� ������ ����)
        Rpm = avgRPM * 300 / 1000;
    }

    // ��Ŀ�� ����
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
        rb.centerOfMass = new Vector3(0, -1, 0); //�����߽��� y�� �������� ����
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