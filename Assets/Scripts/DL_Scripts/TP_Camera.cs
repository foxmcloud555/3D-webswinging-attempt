using UnityEngine;
using System.Collections;

public class TP_Camera : MonoBehaviour
{
    public static TP_Camera Instance;

    public Transform TargetLookAt;
    public float distance = 10f;
    public float distanceMin = 5f;
    public float distanceMax = 20f;
    public float DistanceSmooth;
    public float distanceResumeSmooth =1f;
    public float x_MouseSensitivity = 5f;
    public float y_MouseSensitivity = 5f;
    public float wheel_MouseSensitivity = 5f;

    public float x_Smooth = 0.05f;
    public float y_Smooth = 0.1f;
    public float z_Smooth = 0.05f;




    public float y_MinLimit = -40f;
    public float y_MaxLimit = 80;

    public float OcclusionDistanceStep = 0.5f;
    public int MaxOcclusionChecks = 10;


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

    private float preOccludeDistance = 0f;
    private float distanceSmooth;



    // Use this for initialization
    void Awake()
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
    void LateUpdate()
    {
        //if (TargetLookAt = null)
        //{
        //    print("Targetlookat = null");
        //    return;
        //}

        HandlePlayerInput();

        var count = 0;
        do
        {
            CalculateDesiredPosistion();
            count++;
        } while (CheckIfOccluded(count));
        

        

        UpdatePosistion();
    }

    public void HandlePlayerInput()
    {

        var deadZone = 0.9f;

        if (Input.GetAxis("RightH") < -deadZone || Input.GetAxis("RightH") > deadZone)
        {

            mouseX += Input.GetAxis("RightH") * x_MouseSensitivity;

        }

        if (Input.GetAxis("RightV") < -deadZone || Input.GetAxis("RightV") > deadZone)
        {
            mouseY += Input.GetAxis("RightV") * y_MouseSensitivity;
        }

        mouseY = Helper.ClampAngle(mouseY, y_MinLimit, y_MaxLimit);

        if (Input.GetAxis("RightV") < -deadZone || Input.GetAxis("RightV") > deadZone)
        {
            desiredDistance = Mathf.Clamp(distance + Input.GetAxis("RightV") * wheel_MouseSensitivity, distanceMin, distanceMax);
            preOccludeDistance = desiredDistance;
            distanceSmooth = DistanceSmooth;
        }

    }

    void CalculateDesiredPosistion()
    {

        ResetDesiredDistance();

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
      
    }


    bool CheckIfOccluded(int count)
    {
        var isOccluded = false;

        Vector3 vectorBetween = Vector3.Normalize(  Camera.main.transform.position - TargetLookAt.position);
        Vector3 pointBetween = TargetLookAt.position + 3 * vectorBetween;


        // var nearestDistance = CheckCameraPoints(TargetLookAt.position, desiredPosition);
        var nearestDistance = CheckCameraPoints(pointBetween, desiredPosition);


        if (nearestDistance != -1)
        {
            if (count < MaxOcclusionChecks)
            {
                isOccluded = true;
                distance -= OcclusionDistanceStep;

                if (distance < 0.25f)
                    distance = 0.25f;
            }
            else
            {
                distance = nearestDistance + Camera.main.nearClipPlane;
            }
            desiredDistance = distance;
            distanceSmooth = distanceResumeSmooth;
     
        }
        return isOccluded;
    }


    float CheckCameraPoints(Vector3 from, Vector3 to)
    {
        var nearDistance = -1f;
        RaycastHit hitInfo;
        Helper.ClipPlanePoints clipPlanePoints = Helper.ClipPlaneAtNear(to);

        // draw lines in editor to visualise
        Debug.DrawLine(from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, Color.red) ;
        Debug.DrawLine(from, clipPlanePoints.UpperLeft);
        Debug.DrawLine(from, clipPlanePoints.UpperRight);
        Debug.DrawLine(from, clipPlanePoints.LowerLeft);
        Debug.DrawLine(from, clipPlanePoints.LowerRight);

        Debug.DrawLine(clipPlanePoints.UpperLeft, clipPlanePoints.UpperRight);
        Debug.DrawLine(clipPlanePoints.UpperRight, clipPlanePoints.LowerRight);
        Debug.DrawLine(clipPlanePoints.LowerLeft, clipPlanePoints.LowerRight);
        Debug.DrawLine(clipPlanePoints.LowerLeft, clipPlanePoints.UpperLeft);

        if (Physics.Linecast(from, clipPlanePoints.UpperLeft, out hitInfo) && hitInfo.collider.tag != "Player")
            nearDistance = hitInfo.distance;

        if (Physics.Linecast(from, clipPlanePoints.LowerLeft, out hitInfo) && hitInfo.collider.tag != "Player")
            if (hitInfo.distance < nearDistance || nearDistance == -1)
                nearDistance = hitInfo.distance;


        if (Physics.Linecast(from, clipPlanePoints.LowerRight, out hitInfo) && hitInfo.collider.tag != "Player")
            if (hitInfo.distance < nearDistance || nearDistance == -1)
                nearDistance = hitInfo.distance;


        if (Physics.Linecast(from, clipPlanePoints.UpperLeft, out hitInfo) && hitInfo.collider.tag != "Player")
            if (hitInfo.distance < nearDistance || nearDistance == -1)
                nearDistance = hitInfo.distance;


        if (Physics.Linecast(from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, out hitInfo) && hitInfo.collider.tag != "Player")
            if (hitInfo.distance < nearDistance || nearDistance == -1)
                nearDistance = hitInfo.distance;

        return nearDistance;
    }

    void ResetDesiredDistance()
    {
        if (desiredDistance < preOccludeDistance)
        {
            var pos = CalculatePosition(mouseY, mouseX, preOccludeDistance);

            var nearestDistance = CheckCameraPoints(TargetLookAt.position, pos);

            if (nearestDistance == -1 || nearestDistance > preOccludeDistance)
            {
                desiredDistance = preOccludeDistance;
            }
        }
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
        preOccludeDistance = distance;
    }

    //public static void UseExistingOrCreateNewMainCamera()
    //{

    //    GameObject tempCamera;
    //    GameObject targetLookat;
    //    ThirdPersonCamera myCamera;

    //    if (Camera.main == null)
    //    {

    //        tempCamera = Camera.main.gameObject;
    //    }
    //    else
    //    {
    //        tempCamera = new GameObject("Main Camera");
    //        tempCamera.AddComponent<Camera>();
    //        tempCamera.tag = "MainCamera";
    //    }

    //    tempCamera.AddComponent<ThirdPersonCamera>();
    //    myCamera = tempCamera.GetComponent<ThirdPersonCamera>() as ThirdPersonCamera;

    //    targetLookat = GameObject.Find("targetLookat") as GameObject;

    //    if (targetLookat == null)
    //    {

    //        targetLookat = new GameObject("targetLookat");
    //        targetLookat.transform.position = Vector3.zero;
    //    }

    //    myCamera.TargetLookAt = targetLookat.transform;

    //}
}
