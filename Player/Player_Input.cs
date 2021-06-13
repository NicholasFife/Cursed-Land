/*
Made with work from:
Brackeys. (2020, May 24). THIRD PERSON MOVEMENT in Unity [Video File]. Retrieved from https://www.youtube.com/watch?v=4HpC--2iowE

Brackeys. (2019, October 27). FIRST PERSON MOVEMENT in Unity - FPS Controller [Video File]. Retrieved from https://youtu.be/_QajrabyTJc
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class Player_Input : MonoBehaviour
{
    Gamepad gamepad;
    Keyboard keyboard;
    Mouse mouse;
    Animator anim;

    NoiseMaker pNoise;
    Entity_Player pEnt;
    Player_LockOn lockCntl;
    StaminaSlider staCntl;
    Menu_Debug debugMenu;
    Player_Inventory pInv;
    Player_Interactive pInter;
    
    //Initialize State Machine
    //This must match the CamState enum on Camera_Machine as they'll be synced
    public enum InputState
    {
        Third,
        FocusRanged,
        FocusMelee,
        //Lock,
        //Animated,
        //Hiding,
    }

    InputState curState;

    Dictionary<InputState, Action> ism = new Dictionary<InputState, Action>();

    //PlayerStatus
    private bool nonAtkMove = true;

    //Movement
    CharacterController playerCC;
    public float moveSpd;
    Transform camPos;
    Vector3 momentumDir;
    float momentumStr;
    float runSpeed = 1.5f;
    float sneakSpeed = .5f;
    float runCost = 1;
    
    //Gravity & Jumping
    float gravity = -30f;
    Vector3 moveY;
    public Transform groundCheck;
    float groundDistance = .2f;
    public LayerMask walkableMask;
    [SerializeField]
    bool onGround;
    bool featherFall;
    public float jumpHeight = 1f;
   
    Camera_Machine camMac;
    float turnsmoothVelocity; //Used to hold current value
    float turnSmoothTime = 0.1f; //smooth damp angle time
    
    void Awake()
    {
        //Assign States to Actions
        ism.Add(InputState.Third, new Action(ThirdInput));
        ism.Add(InputState.FocusRanged, new Action(FocusRangedInput));
        ism.Add(InputState.FocusMelee, new Action(FocusMeleeInput));

        //Set Starting State
        curState = InputState.Third;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Assign variables
        playerCC = GetComponent<CharacterController>();
        camPos = transform.parent.GetChild(1);
        camMac = camPos.GetComponent<Camera_Machine>();
        anim = GetComponentInChildren<Animator>();
        pNoise = GetComponent<NoiseMaker>();
        pEnt = playerCC.GetComponent<Entity_Player>();
        lockCntl = GetComponent<Player_LockOn>();
        staCntl = FindObjectOfType<StaminaSlider>();
        pInv = GetComponent<Player_Inventory>();
        pInter = GetComponentInChildren<Player_Interactive>();
        Debug.Log("pInter was found? " + pInter.name);

        //Debug.Log("Player_LockOn found");
        moveSpd = pEnt.myStats.GetSpeed();

        debugMenu = FindObjectOfType<Menu_Debug>();



        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //Find Keyboard and gamepad
        if (Gamepad.current != null)
        {
            gamepad = Gamepad.current;
            //Debug.Log("Gamepad : " + Gamepad.current.name);
        }
        keyboard = Keyboard.current;
        mouse = Mouse.current;
    }

    // Update is called once per frame
    void Update()
    {
        if(keyboard.backquoteKey.wasPressedThisFrame)
        {
            debugMenu.toggleDebug();
        }

        ism[curState].Invoke();
    }

    public void SetState(InputState newState)
    { curState = newState; }

    void ThirdInput()
    {

        StandardMovement360();
        
        //StandardMovement360(); //WASD Movement with player rotation
        VerticalMovement(); //Jumping & Gravity

        if (onGround)
        { PlayerThirdHands(); }

        CamInput(); //Mouse Movement
    }
    void FocusRangedInput()
    {
        FocusMovement();

        PlayerHandsFocusRanged();
        
        VerticalMovement(); //Jumping & Gravity

        CamInput(); //Mouse Movement
    }

    void FocusMeleeInput()
    {
        FocusMovement();

        PlayerHandsFocusMelee();
        
        VerticalMovement(); //Jumping & Gravity

        CamInput(); //Mouse Movement
    }

    void PlayerThirdHands()
    {
        ListenThirdAttack();

        ListenWeaponChange();

        ListenInteract();
    }

    void PlayerHandsFocusRanged()
    {
        ListenFocusRanged();

        ListenWeaponChange();
    }

    void PlayerHandsFocusMelee()
    {
        ListenFocusMelee();

        ListenWeaponChange();
    }

    void ListenFocusMelee()
    {
        if (mouse.rightButton.wasReleasedThisFrame)
        {
            camMac.EnterThird();
            anim.SetBool("isBlocking", false);
        }
        else if (mouse.leftButton.wasPressedThisFrame)
        {
            //Charge Attack?
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            //Activate Attack?
        }
    }

    void ListenFocusRanged()
    {
        if (mouse.rightButton.wasReleasedThisFrame || !staCntl.HasStamina())
        {
            camMac.EnterThird();
            pInv.GetEquippedWeapon().CrosshairOff();
            pInv.GetEquippedWeapon().ResetCrosshair();
            anim.SetBool("isAiming", false);
            //Release isAiming anim bool to false
        }
        else if (mouse.leftButton.isPressed)
        {
            pInv.GetEquippedWeapon().Charging();
            anim.SetBool("isShoot", true);
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            pInv.GetEquippedWeapon().ResetCrosshair();
            anim.SetBool("isShoot", false);
            pInv.GetEquippedWeapon().SetCanCharge(false);
        }
    }
    
    void ListenThirdAttack()
    {
        if (mouse.leftButton.wasPressedThisFrame && staCntl.HasStamina())
        {
            PrimaryButton();
        }
        
        if (mouse.rightButton.wasPressedThisFrame && staCntl.HasStamina())
        {
            SecondaryButton();
        }
        
        /* POWER ATTACK NOT CURRENTLY IN USE - INTEND ON CHANGING THE WAY IT IS TRIGGERED AND MAKING IT A SINGLE ATTACK
         * if (mouse.rightButton.wasPressedThisFrame)
        {
            PowerAttack();
        }*/
    }

    void ListenInteract()
    {
        if (keyboard.eKey.wasPressedThisFrame)
        {
            Debug.Log("was a player_interactive script found? " + pInter.name);
            pInter.UseSelected();
        }
    }

    void ListenWeaponChange()
    {
        if (keyboard.digit1Key.wasPressedThisFrame)
        { pInv.SwitchEquippedWeapon(0); }
        if (keyboard.digit2Key.wasPressedThisFrame)
        { pInv.SwitchEquippedWeapon(1); }
        if (keyboard.digit3Key.wasPressedThisFrame)
        { pInv.SwitchEquippedWeapon(2); }
    }
    
    void FocusMovement()
    {
        Vector3 momentumNew;

        float moveX = 0;
        float moveZ = 0;
        float str = 0;

        //Takes player input and stores it
        if (gamepad != null)
        {
            moveX = gamepad.leftStick.x.ReadValue();
            moveZ = gamepad.leftStick.y.ReadValue();
            str = gamepad.leftStick.EvaluateMagnitude();
        }

        //Keyboard Input
        if (keyboard.wKey.isPressed)
        { str = 1; moveZ = 1; }
        else if (keyboard.sKey.isPressed)
        { str = 1; moveZ = -1; }
        if (keyboard.dKey.isPressed)
        { str = 1; moveX = 1; }
        else if (keyboard.aKey.isPressed)
        { str = 1; moveX = -1; }

        //Movement speed is lowered in Focus mode
        str = str * .5f;
        
        anim.SetFloat("MoveZ", str);
        
        Vector3 moveMe = new Vector3(moveX, 0, moveZ);

        //Debug.Log("moveX = " + moveX);
        //Debug.Log("moveZ = " + moveZ);
        //Debug.Log("moveMe = " + moveMe);

        if (moveMe.magnitude >= .1f && camMac.curState == Camera_Machine.CamState.Third)
        {
            //Controls player rotation
            float targetRotation = Mathf.Atan2(moveMe.x, moveMe.z) * Mathf.Rad2Deg + camPos.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnsmoothVelocity, turnSmoothTime);

            //Controls player movement
            Vector3 moveDir = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
            //Debug.Log("momentumNew = " + moveDir + " * " + Time.deltaTime + " * " + str + " * " + moveSpd);
            
            momentumNew = moveDir * Time.deltaTime * str * moveSpd;
            //Debug.Log("movement is " + momentumNew);
            playerCC.Move(momentumNew);
        }
        else if (camMac.curState == Camera_Machine.CamState.Focus)
        {
            float targetRotation = Mathf.Atan2(moveMe.x, moveMe.z) * Mathf.Rad2Deg + transform.eulerAngles.y;
            Vector3 firstRotation = camPos.rotation.eulerAngles;
            firstRotation.x = 0;
            firstRotation.z = 0;

            //Controls player movement
            Vector3 moveDir = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
            momentumNew = moveDir * Time.deltaTime * str * moveSpd;
            //Debug.Log("movement is " + momentumNew);
            playerCC.Move(momentumNew);
        }
        if (str > 1f)
        { pNoise.MakeNoise(1, 10); }
        else if (str == 1f)
        { pNoise.MakeNoise(1, 5); }

    }
    
    void StandardMovement360()
    {
        Vector3 momentumNew;

        float moveX = 0;
        float moveZ = 0;
        float str = 0;
        
        //Takes player input and stores it
        if (gamepad != null)
        {
            moveX = gamepad.leftStick.x.ReadValue();
            moveZ = gamepad.leftStick.y.ReadValue();
            str = gamepad.leftStick.EvaluateMagnitude();
        }
        
        //Keyboard Input
        if (keyboard.wKey.isPressed)
        { str = 1; moveZ = 1; }
        else if (keyboard.sKey.isPressed)
        { str = 1; moveZ = -1; }
        if (keyboard.dKey.isPressed)
        { str = 1; moveX = 1; }
        else if (keyboard.aKey.isPressed)
        { str = 1; moveX = -1; }

        if (nonAtkMove)
        {
            //Dash Modification
            if (keyboard.leftShiftKey.isPressed && staCntl.HasStamina())
            {

                staCntl.MinusStaminaOT(runCost);
                str = str * runSpeed;
            }
            if (keyboard.leftCtrlKey.isPressed && onGround)
            {
                str = str * sneakSpeed;
                anim.SetBool("isCrouched", true);
            }
            else if (keyboard.leftCtrlKey.wasReleasedThisFrame)
            {
                anim.SetBool("isCrouched", false);
            }
        }
        else
        { str = str * .5f; }

        anim.SetFloat("MoveZ", str);
        //Debug.Log("MoveZ set on " + anim.name + " value is " + str);

        Vector3 moveMe = new Vector3(moveX, 0, moveZ);

        //Debug.Log("moveX = " + moveX);
        //Debug.Log("moveZ = " + moveZ);
        //Debug.Log("moveMe = " + moveMe);

        if (moveMe.magnitude >= .1f && camMac.curState == Camera_Machine.CamState.Third)
        {
            //Controls player rotation
            float targetRotation = Mathf.Atan2(moveMe.x, moveMe.z) * Mathf.Rad2Deg + camPos.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnsmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            //Controls player movement
            Vector3 moveDir = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
            //Debug.Log("momentumNew = " + moveDir + " * " + Time.deltaTime + " * " + str + " * " + moveSpd);



            momentumNew = moveDir * Time.deltaTime * str * moveSpd;
            //Debug.Log("movement is " + momentumNew);
            playerCC.Move(momentumNew );
        }
        else if (camMac.curState == Camera_Machine.CamState.Focus)
        {
            float targetRotation = Mathf.Atan2(moveMe.x, moveMe.z) * Mathf.Rad2Deg + transform.eulerAngles.y;
            Vector3 firstRotation = camPos.rotation.eulerAngles;
            firstRotation.x = 0;
            firstRotation.z = 0;
            transform.rotation = Quaternion.Euler(firstRotation);
            
            //Controls player movement
            Vector3 moveDir = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
            momentumNew = moveDir * Time.deltaTime * str * moveSpd;
            //Debug.Log("movement is " + momentumNew);
            playerCC.Move(momentumNew);
        }
        if(str > .5f)
        { pNoise.MakeNoise(1, 10); }
        
    }

    void VerticalMovement()
    {
        //Debug.Log("VerticalMovement() is running");
        //Drags player that tiny bit further down to touch the ground while stabilizing their grounded downward velocity.
        if (onGround && moveY.y < 0) 
        { moveY.y = -2f; }

        //Determine if the player is touching the ground
        if (Physics.CheckSphere(groundCheck.position, groundDistance, walkableMask, QueryTriggerInteraction.Ignore))
        {
            onGround = true;
        }
        else { onGround = false; }

        if(onGround == false && moveY.y < 0)
        {
            anim.SetBool("isJumping", false);
        }


        //Activate Jump
        if(onGround && keyboard.spaceKey.wasPressedThisFrame && nonAtkMove && staCntl.curStamina > 0)
        { Jump(); }

        //Add gravity to calculation
        moveY.y += gravity * Time.deltaTime;
        //Debug.Log("MoveY.y = " + moveY.y);
        //Apply vertical movement
        playerCC.Move(moveY * Time.deltaTime);
    }
    
    public void Jump()
    {
        staCntl.MinusStaminaOnce(2);
        moveY.y = Mathf.Sqrt(jumpHeight * -1.5f * gravity);
        onGround = false;
        anim.SetBool("isCrouched", false);
        anim.SetBool("isJumping", true);
    }


    void CamInput()
    {
        Vector3 rot = camPos.localRotation.eulerAngles;
        float rotX = rot.x;
        float rotY = rot.y;
        float lookX = 0;
        float lookY = 0;
        
        if (gamepad != null && gamepad.rightStick.IsActuated())
        {
            lookX = gamepad.rightStick.y.ReadValue() * -1;
            lookY = gamepad.rightStick.x.ReadValue();
        }

        Vector2Control mouseMove = Pointer.current.delta;

        lookX = mouseMove.y.ReadValue() * -1;
        lookY = mouseMove.x.ReadValue();
        
        rotX += lookX * Time.deltaTime * 5;
        rotY += lookY * Time.deltaTime * 5;

        rotX = Mathf.Clamp(rotX, 0, 90);
        
        Quaternion newRotation = Quaternion.Euler(rotX, rotY, 0);
        //Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, newRotation, camSmoothSpeed * Time.deltaTime * speedModifier);
        //Quaternion finalSmoothed = Quaternion.Euler(smoothedRotation.x, smoothedRotation.y, 0);
        //Debug.Log("New manually set camera rotation is: " + finalSmoothed);
        camPos.rotation = newRotation;


        //Convert camera rotation to vector3 from Quaternion
        Vector3 targetRotation = new Vector3(0, camPos.eulerAngles.y, 0);
        Quaternion playerRotation = Quaternion.Euler(targetRotation);

        //Quaternion playerRotation = Quaternion.Euler(0, camPos.localRotation.y, 0);
        //Debug.Log("Setting playerRotation to " + playerRotation);





        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //THIS IS PROBABLY STILL NECESSARY FOR THE FOCUS CAMERA MODE
        //transform.rotation = playerRotation;
    }

    void PrimaryButton()
    {
        //Debug.Log("Checking lockCntl" + lockCntl.name);
        //Debug.Log("Checking equippedWeapon " + equippedWeapon.name);
        //Find the closest target as long as it's within weapon range.
        Transform softTarget = lockCntl.GetSoftLock(pInv.GetEquippedWeapon());
        if (softTarget != null)
        { Debug.Log("Soft Lock found: " + softTarget); }
        else
        { Debug.Log("Target Too Far Away for soft lock"); }

        Vector3 targetPos = new Vector3(softTarget.position.x, transform.position.y, softTarget.position.z);

        transform.LookAt(targetPos);

        pInv.GetEquippedWeapon().PrimaryButton(softTarget);
    }

    void SecondaryButton()
    {
        pInv.GetEquippedWeapon().SecondaryButton();
    }
    
    float CheckFacing(GameObject target)
    {
            Vector3 targetDir = transform.position - target.transform.position;
            targetDir = targetDir.normalized;
            float tempDot = Vector3.Dot(target.transform.forward, targetDir);
            //Debug.Log("Dot Product is..." + tempDot);
            return tempDot;
    }

    public void SetAttackMove(bool b)
    {
        nonAtkMove = b;
    }
}
