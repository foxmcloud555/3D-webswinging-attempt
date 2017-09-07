using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
  //  public Transform player;

    public static ThirdPersonCamera Instance;

    public Transform TargetLookAt;
    public float distance = 10f;
    public float distanceMin = 5f;
    public float distanceMax = 20f;
    public float distanceSmooth;
    public float x_MouseSensitivity = 5f;
    public float y_MouseSensitivity = 5f;
    public float wheel_MouseSensitivity = 5f;

    public float x_Smooth = 0.05f;
    public float y_Smooth = 0.1f;
    public float z_Smooth = 0.05f;




    public float y_MinLimit = -40f;
    public float y_MaxLimit = 80;


    private float velX = 0f;
    private float velY = 0f;
    private float velZ = 0f;

    private Vector3 position = Vector3.zero;

    private float mouseX = 0f;
    private float mouseY = 0f;
    private float velDistance = 0f;
    private float startDistance = 0f;
    private Vector3 desiredPosition = Vector3.zero;
    private float desiredDistance = 0f;



    // Use this for initialization
    void Awake ()
    {
        Instance = this;
	}

    void Start()
    {
     
        distance = Mathf.Clamp(distance, distanceMin, distanceMax);
        startDistance = distance;
        Reset();
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        //if (TargetLookAt = null)
        //{
        //    print("Targetlookat = null");
        //    return;
        //}

        HandlePlayerInput();

        CalculateDesiredPosistion();

        UpdatePosistion();
    }

    public void HandlePlayerInput()
    {
        
        var deadZone = 0.5f;

        if (Input.GetAxis("RightH") < -deadZone || Input.GetAxis("RightH") > deadZone)
        {
           
            mouseX += Input.GetAxis("RightH") * x_MouseSensitivity;
            
        }

        if( Input.GetAxis("RightV") < -deadZone || Input.GetAxis("RightV") > deadZone)
        {
            mouseY += Input.GetAxis("RightV") * y_MouseSensitivity;
        }

        mouseY = Helper.ClampAngle(mouseY, y_MinLimit, y_MaxLimit);

        if ( Input.GetAxis("Triggers") < -deadZone || Input.GetAxis("Triggers") > deadZone)
        {
            desiredDistance = Mathf.Clamp(distance - Input.GetAxis("Triggers") * wheel_MouseSensitivity, distanceMin, distanceMax);
        }

    }

    void CalculateDesiredPosistion()
    {
        //evaluate distance
        distance = Mathf.SmoothDamp(distance, desiredDistance, ref velDistance, distanceSmooth);
        //calculate desired posistion

        desiredPosition = CalculatePosition(mouseX, mouseY, distance);
        
    }


    Vector3 CalculatePosition(float rotationY, float rotationX, float distance)
    {
     
        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        
         return TargetLookAt.position + rotation * direction;
        //return new Vector3(0, 5, -5);
    }

    void UpdatePosistion()
    {
        var posX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, x_Smooth);
        var posY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, y_Smooth);
        var posZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, z_Smooth);
        position = new Vector3(posX, posY, posZ);

        transform.position = position;

        transform.LookAt(TargetLookAt);

    
    }

    void Reset()
    {
        mouseX = 0f;
        mouseY = 10;
        distance = startDistance;
        desiredDistance = distance;
    }

    public static void UseExistingOrCreateNewMainCamera()
    {
      
        GameObject tempCamera;
        GameObject targetLookat;
        ThirdPersonCamera myCamera;
         
        if (Camera.main == null)
        {
         
            tempCamera = Camera.main.gameObject;
        }
        else
        {
            tempCamera = new GameObject("Main Camera");
            tempCamera.AddComponent<Camera>();
            tempCamera.tag = "MainCamera";
        }

        tempCamera.AddComponent<ThirdPersonCamera>();
        myCamera = tempCamera.GetComponent<ThirdPersonCamera>() as ThirdPersonCamera;

        targetLookat = GameObject.Find("targetLookat") as GameObject;

        if (targetLookat == null)
        {
       
            targetLookat = new GameObject("targetLookat");
            targetLookat.transform.position = Vector3.zero;
        }

        myCamera.TargetLookAt = targetLookat.transform;
        
    }
}
