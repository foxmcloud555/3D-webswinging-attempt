using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMotor : MonoBehaviour {

    public static ThirdPersonMotor Instance;

    public float moveSpeed = 10.0f;
    public float jumpSpeed = 6f;
    public float gravity = 21f;
    public float terminalVelocity = 20f;

    public Vector3 moveVector { get; set; }
    public float verticalVelocity { get; set; }

    // Use this for initialization
    void Awake () {

        Instance = this;
	}
	
	// Update is called once per frame
	void Update () {


        SnapAlignCharacterToCamera();
        ProcessMotion();


    }

    public void UpdateMotor()
    {
        
        ProcessMotion();
        SnapAlignCharacterToCamera();
    }

    void ProcessMotion()
    {
        // transform movevector to world space
        moveVector = transform.TransformDirection(moveVector);
       
        // normalise movevector if magnitude > 1
        if (moveVector.magnitude > 1)
            moveVector = Vector3.Normalize(moveVector);

        // multiply movevector by movespeed 
        moveVector *= moveSpeed;

        // multiply movevector by deltatime
        moveVector = new Vector3(moveVector.x, verticalVelocity, moveVector.z);
       // moveVector *= Time.deltaTime;

        ApplyGravity();

        // move the character in world space
        ThirdPersonController.characterController.Move(moveVector * Time.deltaTime);
     //   print(moveVector);
    }

    void ApplyGravity()
    {
        if (moveVector.y > -terminalVelocity)
        {
            moveVector = new Vector3(moveVector.x, moveVector.y - gravity * Time.deltaTime, moveVector.z);
        }

        if (ThirdPersonController.characterController.isGrounded && moveVector.y < -1)
        {

            moveVector = new Vector3(moveVector.x, -1, moveVector.z);
        }

         print(moveVector);

    }

    public void Jump()
    {
        if (ThirdPersonController.characterController.isGrounded)
        {
            verticalVelocity = jumpSpeed;
        }
    }

    void SnapAlignCharacterToCamera()
    {
        if (moveVector.x != 0 || moveVector.z != 0)
        {
          //  print("moveVector.x or z does not = 0");
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
                                                  Camera.main.transform.eulerAngles.y,
                                                  transform.eulerAngles.z);
        }
    }
}
