using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CarController : MonoBehaviour
{
    private WheelCollider[] wheelColliders;
    private GameObject[] wheels;
    private Rigidbody rb;
    
    public float power;
    public float breakPower;
    private float wheelBase; //�� �� ���� ������ �Ÿ�(m����)
    private float rearTrack; //�� �� ��Ƣ ������ �Ÿ�(m����)
    public float turnRadius; //ȸ�� ������(m����)

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
        //�극��ũ ���(�ڵ��� �극��ũ�� 4���� ������ ��� ����Ǿ����)
        for (int i = 0; i < wheels.Length; i++)
        {
            // for���� ���ؼ� ���ݶ��̴� ��ü�� Vertical �Է¿� ���� power��ŭ�� ������ �����̰��Ѵ�.
            wheelColliders[i].motorTorque = Input.GetAxis("Vertical") * power;

           

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

        SteerVehicle(); //��Ŀ�� ����
        WheelPosWithCollider();  //������ ��ġ�� �����̼� ���� �׻� ���� �������

    }
    void Update()
    {
        //LogitechGSDK.DIJOYSTATE2ENGINES rec;
        //rec = LogitechGSDK.LogiGetStateUnity(0);
        //ro = rec.lX / 819;
        //transform.rotation = Quaternion.Euler(0, ro, 0);
        //Debug.Log(speed);
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

        //���� ���������� ȸ��
        if (Input.GetAxis("Horizontal") > 0)
        {
            //Rad2Deg -> ������ ���ȿ��� degree(��)�� ��ȯ
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * Input.GetAxis("Horizontal");
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * Input.GetAxis("Horizontal");
            //Debug.Log(wheelColliders[0].steerAngle);
        }
        //���� �������� ȸ��
        else if (Input.GetAxis("Horizontal") < 0)
        {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * Input.GetAxis("Horizontal");
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * Input.GetAxis("Horizontal");
            //Debug.Log(wheelColliders[0].steerAngle);
        }
        else
        {
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }
    }

    //�ʱ�ȭ
    private void Init()
    {
        wheelColliders = new WheelCollider[4];
        wheels = new GameObject[4];
        rb = GetComponentInParent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -1, 0); //�����߽��� y�� �������� ����
        power = 200f;

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
