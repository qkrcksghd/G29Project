using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G29CarController : MonoBehaviour
{
    private WheelCollider[] wheelColliders;
    private GameObject[] wheels;
    private Rigidbody rb;

    [Header("Padel")]
    public float power; //�ӷ�
    private float accPower; //��������
    private int intAccP;
    private int isAcc;

    public float breakPower; //�극��ũ ������
    private float wheelBase; //�� �� ���� ������ �Ÿ�(m����)
    private float rearTrack; //�� �� ��Ƣ ������ �Ÿ�(m����)
    public float turnRadius; //ȸ�� ������(m����)

    [Header("Handle")]
    private float ro; //���� ȸ����
    private float isro;

    public bool isBreak;
    // Start is called before the first frame update
    void Start()
    {
        Init();
        breakPower = 0;
        isBreak = false;
    }

    private void FixedUpdate()
    {
        SteerVehicle(); //��Ŀ�� ����
        WheelPosWithCollider();  //������ ��ġ�� �����̼� ���� �׻� ���� �������
        InputLogitech();
        //�극��ũ ���(�ڵ��� �극��ũ�� 4���� ������ ��� ����Ǿ����)
        for (int i = 0; i < wheels.Length; i++)
        {
            // for���� ���ؼ� ���ݶ��̴� ��ü�� ���� �Է¿� ���� power��ŭ�� ������ �����̰� �Ѵ�.
            wheelColliders[i].motorTorque = isAcc * power;
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
        print(ro);
    }

    void InputLogitech()
    {
        LogitechGSDK.DIJOYSTATE2ENGINES rec;
        rec = LogitechGSDK.LogiGetStateUnity(0);
        ro = rec.lX / 32767f;

        accPower = Mathf.Abs(rec.lY - 32767);
        intAccP = (int)accPower / 10000;
        if (rec.lY != 0)
        {
            for (int i = 0; i < 128; i++)
            {
                if (rec.rgbButtons[i] == 128)
                {
                    if (i == 14) isAcc = 1;
                    else if (i == 15) isAcc = -1;
                }

            }
        }
        if (rec.lRz != 0)
        {
            breakPower = Mathf.Abs(rec.lRz - 32767);
        }
        //Button status :

    }

    //���� mesh�� collider�� ��ġ�� ������
    void WheelPosWithCollider()
    {
        //���� �޾ƿ��� �뵵�� ����
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation); //wheelCollider��  ���� �����ǰ� ���� �����̼� ���� �޾ƿ�
            wheels[i].transform.position = wheelPosition;
            wheels[i].transform.rotation = wheelRotation;
        }
    }

    // ��Ŀ�� ����
    void SteerVehicle()
    {
        //steerAngle = ������ ���� ����

        //���� Rad2Deg * Atan(wheelBase in meter / (turnRadius in meters + (rearTrack in meters / 2to get center)) * steerInput  left
        // Rad2Deg* Atan(wheelBase in meter / (turnRadius in meters - (rearTrack in meters / 2to get center)) *steerInput  right
        //transform.rotation = Quaternion.Euler(0, ro, 0); 
        //���� ���������� ȸ��
        if (ro > 0)
        {
            //Debug.Log("��ȸ��");
            //Rad2Deg -> ������ ���ȿ��� degree(��)�� ��ȯ
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
        }
        //���� �������� ȸ��
        else if (ro < 0)
        {
            //Debug.Log("��ȸ��");
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * ro;
            //Debug.Log(wheelColliders[0].steerAngle);
        }
        else
        {
            //Debug.Log("����");
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }
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
                power = 300;
                break;
            case 6:
                power = 350;
                break;
        }
    }

    //�ʱ�ȭ
    private void Init()
    {
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
            wheelColliders[i].transform.position = wheels[i].transform.position;
        }

        turnRadius = 3;
        wheelBase = wheelColliders[1].transform.position.z - wheelColliders[3].transform.position.z;
        rearTrack = wheelColliders[2].transform.position.x - wheelColliders[3].transform.position.x;
    }
}
