using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour {

    public static CharacterController characterController;
    public static ThirdPersonController Instance;

    void Awake ()
    {
        characterController = GetComponent("CharacterController") as CharacterController;
        Instance = this;
       // ThirdPersonCamera.UseExistingOrCreateNewMainCamera();
    }

	
	// Update is called once per frame
	void Update ()
    {
        if (Camera.main == null)
            return;
        GetMovementInput();
        HandleActionInput();

        ThirdPersonMotor.Instance.UpdateMotor();

        

    }


    void GetMovementInput()
    {
        var deadZone = 0.1f;

        ThirdPersonMotor.Instance.verticalVelocity = ThirdPersonMotor.Instance.moveVector.y;
        ThirdPersonMotor.Instance.moveVector = Vector3.zero;

        if (Input.GetAxis("Vertical") > deadZone || Input.GetAxis("Vertical") < -deadZone)
        {
            ThirdPersonMotor.Instance.moveVector += new Vector3(0, 0, Input.GetAxis("Vertical"));
         
        }

        if (Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
        {
            ThirdPersonMotor.Instance.moveVector += new Vector3(Input.GetAxis("Horizontal"),0,0);
           
        }

    }

    void HandleActionInput()
    {
        if (Input.GetButton("Fire1"))
        {
            
            Jump();
        }
    }

    void Jump()
    {
        ThirdPersonMotor.Instance.Jump();
    }

}
