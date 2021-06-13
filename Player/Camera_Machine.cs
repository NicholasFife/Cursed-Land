using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera_Machine : MonoBehaviour
{
    Transform camT;
    Transform playerT;

    Mouse mouse;

    Player_Input pInput;

    //Handles setting player translucency
    public MeshRenderer[] mr;
    bool isTranslucent;
    bool isInvisible;
    //public Material normalColor;
    //public Material ghostColor;

    //Used for third person follow
    public LayerMask obLayer; //set to obstacle & any other layers that shouldn't be allowed to block the player!
    float camDistance;
    Vector3 camDirection;
    Vector3 focusPos;
    Vector3 smoothedFocusPos; //position value to be applied to camera
    
    //Distance is constrained between these values
    float minDistance = 0f;
    float actDistance = 8;

    //Used for third person rotate
    bool willRotate = true;
    float playerX;
    float playerY;
    float rotY = 0; //holds the target rotation value for y
    Quaternion smoothedRotation; //rotation value to be applied to camera parent

    //Used as a baseline for all lerps
    float smoothSpd = 10;

    //Initialize State Machine
    public enum CamState
    {
        Third,
        Focus,
        //Lock,
        //Animated,
        //Hiding,
    }

    public CamState curState;

    Dictionary<CamState, Action> csm = new Dictionary<CamState, Action>();

    //Start is called before the first frame update
    void Awake()
    {
        //Assign States to Actions
        csm.Add(CamState.Third, new Action(ThirdCntl));
        csm.Add(CamState.Focus, new Action(FocusCntl));
        //Set Starting State
        curState = CamState.Third;

        //Assign References
        pInput = FindObjectOfType<Player_Input>();
        camT = transform.GetChild(0).GetChild(0);
        playerT = transform.parent.GetChild(0);
        
        //Create distance reference point
        camDistance = Vector3.Distance(transform.position, camT.position);
        camDirection = camT.localPosition.normalized;
        //Create rotation reference point
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;

        mouse = Mouse.current;
    }

    // Update is called once per frame
    void Update()
    {
        
        //Keep the camera focus object (this object) in the appropriate position relative to the player
        FocusFollow();

        //Run the appropriate camera behavior based on state
        //Creating the position and rotation value to be applied
        csm[curState].Invoke();
    }



    //Move Camera Focus object to follow player position smoothly
    void FocusFollow()
    {
        //Find Desired position
        focusPos = playerT.position;
        //Calculate Smoothed Position of the focus object
        smoothedFocusPos = Vector3.Lerp(transform.position, focusPos, smoothSpd * Time.deltaTime);
        //Apply smoothed position to focus object
        transform.position = smoothedFocusPos;
    }

    //Switch to third person state
    public void EnterThird()
    {
        //enable mesh renderers
        for (int i = 0; i <= mr.Length - 1; i++)
        { mr[i].enabled = true; }

        isInvisible = false;
        actDistance = 8;
        smoothSpd = 10;
        curState = CamState.Third;
        pInput.SetState(Player_Input.InputState.Third);
    }
    //Called from Update while in 3rd person
    void ThirdCntl()
    {
        ThirdRotate();
        CamDistance();
    }

    //Calculates the position camera will be told to move to.
    //DOES NOT MOVE CAMERA
    void CamDistance()
    {
        //Calculate the camera's preferred position
        Vector3 desiredCamPos = transform.TransformPoint(camDirection * actDistance);

        Debug.DrawLine(playerT.position, desiredCamPos);

        RaycastHit hit;
        //test and calculate how close the camera has to be to avoid obstacles
        if (Physics.Linecast(playerT.position, desiredCamPos, out hit, obLayer))
        {
            //Set the distance to slightly closer than the object that was detected
            camDistance = Mathf.Clamp((hit.distance * .8f), minDistance, actDistance);
        }
        else //No obstacle detected. Set the chosen distance to maximum.
        { camDistance = actDistance; }


        if (camDistance <= 2 && !isTranslucent)
        {
            for (int i = 0; i <= mr.Length - 1; i++)
            { mr[i].enabled = false; }
            isTranslucent = true;
            //Debug.Log("isTranslucent = true");
        }
        else if (camDistance > 2 && isTranslucent)
        {
            for (int i = 0; i <= mr.Length - 1; i++)
            { mr[i].enabled = true; }
            isTranslucent = false;
            //Debug.Log("isTranslucent = false");
        }

        //Move camera
        camT.localPosition = (camDirection * camDistance);
    }
    //Calculates the rotation camera will be told to rotate to.
    //DOES NOT rotate CAMERA
    void ThirdRotate()
    {
        playerY = playerT.eulerAngles.y;
        rotY = playerY;
    }

    public void EnterFocus()
    {
        //Debug.Log("EnterFirst()");
        actDistance = 4;
        smoothSpd = 15;
        curState = CamState.Focus;
        
    }

    void FocusCntl()
    {
        CamDistance();
        pFocusRotate();
    }
    
    /*Obsolete because it's designed for 1st person
    void FocusDistace()
    {
        //Calculate the camera's preferred position
        Vector3 desiredCamPos = transform.TransformPoint(camDirection * 0);

        //Move camera
        camT.localPosition = Vector3.Lerp(camT.localPosition, camDirection * 0, Time.deltaTime * smoothSpd);

        camDistance = Vector3.Distance(transform.position, camT.position);

        //Check if renderer can be turned off
        if (camDistance <= .5 && !isInvisible)
        {
            Debug.Log("Turning mr off");
            mr.enabled = false;
            isInvisible = true;
        }
    }*/

    void pFocusRotate()
    {
        Debug.Log("Player rotation updated by camera machine");
        //check the y rotation of the camera
        Quaternion newrot = Quaternion.Euler(playerT.eulerAngles.x, camT.eulerAngles.y, playerT.eulerAngles.z);
        
        //set the player y rotation to the camera's
        playerT.rotation = newrot;
    }

    //Tells cam if it should be rotating or not.
    public void RotateCam(bool b)
    {
        willRotate = b;
    }
    
}
