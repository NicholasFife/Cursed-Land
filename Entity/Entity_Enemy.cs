using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class Entity_Enemy : Entity
{
    //List possible states here
    public enum EnemyStates
    {
        Idle,
        Patrol,
        Pursue,
        Search,
        Attack,
        Flinch,

        NUM_STATES
    }

    //Holds current state
    public EnemyStates curState;
    //definte dictionary entries - Link an EnemyState to a method
    public Dictionary<EnemyStates, Action> esm = new Dictionary<EnemyStates, Action>();

    //The player
    public GameObject pBody;
    //Holds targeted object - Set to null when untargeting
    public GameObject target;

    //Navigation & Movement
    private Waypoint_Manager waypointMGR;
    NavMeshAgent agent;
    public Waypoint_Single patrolPoint; //temporarily public until spawning and auto-begin set up
    private Waypoint_Single lastPoint;

    //Idle State
    float IdleMax = 1f;
    float IdleCount;

    //Pursuit State
    public float followDistance = 5;
    bool isLerping = false;
    Transform rotator;
    Vector3 lerpPOS;

    //Attack State
    float attackDistance = 1;
    public float pickedAttack = 0;
    float atkCoolMax = 3;
    float atkCoolCount;
    bool atkReady = false;
    public E_Weapon[] myWeaponScript;
    public Collider[] myWeaponCol = new Collider[2];
    
    //Search State
    public Transform searchTarget;
    public float searchTime;
    public float searchTimeMax = 3;

    //Alertness Manager
    public AlertnessMeter am;

    //Audio
    AudioSource aSource;
    public AudioClip damagedSound;
    public AudioClip searchSound;
    public AudioClip pursuitSound;
    public AudioClip attackSound;
    public AudioClip deadSound;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        
        waypointMGR = FindObjectOfType<Waypoint_Manager>();
        agent = GetComponent<NavMeshAgent>();
        rotator = transform.parent.GetComponentInChildren<RotateMe>().transform;
        aSource = GetComponent<AudioSource>();
        am = GetComponentInChildren<AlertnessMeter>();

        //Debug.Log("Initializing " + gameObject.name + "...");
        //Declare states from Enum here to associate with a function
        esm.Add(EnemyStates.Idle, new Action(StateIdle));
        esm.Add(EnemyStates.Patrol, new Action(StatePatrol));
        esm.Add(EnemyStates.Search, new Action(StateSearch));
        esm.Add(EnemyStates.Pursue, new Action(StatePursue));
        esm.Add(EnemyStates.Attack, new Action(StateAttack));
        esm.Add(EnemyStates.Flinch, new Action(StateFlinch));

        EnterIdle();

        //Declare stats (HP, Weapon Damage, Lisen Range, Vision Range, Vision Cone (Dot product value))
        myStats = new stats(70, 5, 10, 25, .5f, 6);

        //Assign References to other objects
        pBody = FindObjectOfType<Entity_Player>().gameObject;//Reference to player game object

        //Set beginning navigation info
        //Sets the last point it was at to whatever is the closest waypoint
        lastPoint = waypointMGR.AssignStartPOS(transform);
        //Asks the closest waypoint for a neighbor to move to
        patrolPoint = lastPoint.GetWaypoint(lastPoint);



        myWeaponScript = GetComponentsInChildren<E_Weapon>();
        //Debug.Log("Found " + myWeaponScript.Length + " weapon components on enemy " + gameObject.name);
        for (int i = 0; i < myWeaponScript.Length; i++)
        {
            //Debug.Log("Assigning collider from " + myWeaponScript[i].name);
            myWeaponCol[i] = myWeaponScript[i].gameObject.GetComponent<Collider>();
            //Debug.Log("Assigned myWeaponCol " + i);
        }

        StartAttackCool();
    }

    // Update is called once per frame
    public override void Update()
    {
        esm[curState].Invoke();

        if (target != null)
        { LerpLookPos(); }

        if (!atkReady)
        { CoolAttack(); }

        base.Update();
    }

    public virtual void EnterIdle()
    {
        curState = EnemyStates.Idle;
        //Debug.Log("Entering Idle...");
        IdleCount = IdleMax;
        anim.SetBool("isWalking", false);
    }

    public virtual void StateIdle()
    {
        Look(pBody);
        if (IdleCount <= 0)
        { EnterPatrol(); }
        else
        { IdleCount -= Time.deltaTime; }
    }

    public virtual void EnterPatrol()
    {
        anim.SetBool("isWalking", true);
        curState = EnemyStates.Patrol;
        //Debug.Log("Entering Patrol...");
        //Debug.Log("Agent: " + agent.name);
        Debug.Log("Patrol Point: " + patrolPoint.name);
        agent.SetDestination(patrolPoint.transform.position);
    }
    public virtual void StatePatrol()
    {
        Look(pBody);
    }
    
    public virtual void EnterSearch(Transform pos)
    {
        aSource.PlayOneShot(searchSound);
        curState = EnemyStates.Search;
        //Debug.Log("Entering Search...");
        agent.SetDestination(pos.position);
        searchTime = searchTimeMax;
    }
    public virtual void StateSearch()
    {
        Look(pBody);
        if (searchTime <= 0)
        {
            lastPoint = waypointMGR.AssignStartPOS(transform);
            Debug.Log("StateSearch set lastPoint to " + lastPoint.name + " upon exiting StateSearch");
            patrolPoint = lastPoint.FindWaypointToPlayer();

            if(patrolPoint == null)
            {
                patrolPoint = lastPoint.OnCardinalToPlayerFail(gameObject.transform);
            }
            Debug.Log("StateSearch set patrol point to " + patrolPoint.name + " upon exiting StateSearch");
            EnterIdle();
            target = null;
        }
        else
        { searchTime -= Time.deltaTime; }
    }

    public virtual void EnterPursue(GameObject newTarget)
    {
        aSource.PlayOneShot(pursuitSound);
        //Debug.Log("Entering Pursuit...");
        curState = EnemyStates.Pursue;
        target = newTarget;
    }

    public virtual void StatePursue()
    {
        if (CheckDistance(target) > 5)
        {
            //Debug.Log(CheckDistance(target) + " is > than " + followDistance);
            agent.SetDestination(target.transform.position);
            anim.SetBool("isWalking", true);

            if (isLerping)
            {
                isLerping = false;
            }
        }
        else
        {
            //Debug.Log("Target distance reached.");
            agent.SetDestination(transform.position);
            anim.SetBool("isWalking", false);
            if (!isLerping)
            {
                isLerping = true;
                lerpPOS = target.transform.position;
            }

            //Lerp toward the player's position
            RotateMe();
        }
        if (CheckLOS(target) == false)
        {
            EnterSearch(target.transform);
        }
        if (atkReady)
        {

            EnterAttack();
        }
    }

    public virtual void EnterAttack()
    {
        Debug.Log("Entering Attack...");
        aSource.PlayOneShot(attackSound);
        PickAttack();
        curState = EnemyStates.Attack;
    }

    public virtual void StateAttack()
    {
        Debug.Log("StateAttack running");
        if (CheckDistance(target) > attackDistance)
        {
            //Debug.Log(CheckDistance(target) + " is > than " + attackDistance);
            agent.SetDestination(target.transform.position);
            anim.SetBool("isWalking", true);
        }
        else
        {
            //Debug.Log("Target distance reached.");

            agent.SetDestination(transform.position);
            anim.SetBool("isWalking", false);
            //about a 90 degree front facing cone
            if (CheckFacing() > .5f)
            {
                
                isLerping = true;
                atkReady = false;
                anim.SetBool("isAttacking", true);
            }
            else
            {
                if (!isLerping)
                {
                    isLerping = true;
                    lerpPOS = target.transform.position;
                }

                //Lerp toward the player's position
                RotateMe();
            }
            if (CheckLOS(target) == false)
            {
                EnterSearch(target.transform);
            }
        }
        
        //Exit conditions
        if (CheckLOS(target) == false)
        {
            EnterSearch(target.transform);
            anim.SetBool("isAttacking", false);
        }

    }

    float CheckFacing()
    {
        Vector3 targetDir = transform.position - target.transform.position;
        targetDir = targetDir.normalized;
        float tempDot = Vector3.Dot(target.transform.forward, targetDir);
        //Debug.Log("Dot Product is..." + tempDot);
        return tempDot;
    }

    public virtual void Look(GameObject tempTarget)
    {
        float tempDistance = CheckDistance(tempTarget);

        //Check if target is within vision range
        if (tempDistance < myStats.GetvisionRange() && CheckDot(tempTarget) > myStats.GetvisionCone() && CheckLOS(tempTarget))
        {
            am.IncreaseAwareness(tempDistance);
            //Debug.Log("Target is detected");
        }
        else
        {
            am.LowerAwareness();
        }
    }

    public virtual void Listen(Transform source, float volume)
    {
        if(target == null && (curState != EnemyStates.Pursue || curState != EnemyStates.Attack))
        {
            //Debug.Log("Noise detected at " + source.position + " with a volume of " + volume);
            EnterSearch(source);
            //Use search state to move to source
        }
    }

    float CheckDistance(GameObject tempTarget)
    { 
        float tempDistance = Vector3.Distance(transform.position, pBody.transform.position);
        //Debug.Log("Distance to player is..." + tempDistance);
        return tempDistance;
    }

    float CheckDot(GameObject tempTarget)
    {
        Vector3 targetDir = tempTarget.transform.position - transform.position;
        targetDir = targetDir.normalized;
        float tempDot = Vector3.Dot(transform.forward, targetDir);
        //Debug.Log("Dot Product is..." + tempDot);
        return tempDot;
    }

    void OnTriggerEnter(Collider other)
    {
        if(curState == EnemyStates.Patrol && other.gameObject == patrolPoint.gameObject)
        {
            //Debug.Log("Waypoint Reached");
            EnterIdle();
            Waypoint_Single newWaypoint = patrolPoint.GetWaypoint(lastPoint);
            lastPoint = patrolPoint;
            patrolPoint = newWaypoint;
            //Debug.Log("New Waypoint is " + patrolPoint.name);
        }
    }

    //Runs on Update - does not rotate the enemy just calculates reference values
    public virtual void LerpLookPos()
    {
        //Start by making sure the rotator's position matches the enemy's
        rotator.position = transform.position;
        
        //Create a reference to the player's position with Y coordinates changed to match this gameObject;
        Vector3 horizontalPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        
        //calculate smoothed position to look at
        lerpPOS = Vector3.Lerp(lerpPOS, horizontalPos, Time.deltaTime * 3);

        //Turn the reference object to face the player's position to create a reference to the "ideal" rotation
        rotator.LookAt(lerpPOS);
    }

    //Only runs in pursuit when not moving - does rotate the enemy
    public virtual void RotateMe()
    {
        transform.rotation = rotator.rotation;
    }

    public virtual void StartAttackCool()
    {
        //Debug.Log("Beginning enemy attack cooldown.");
        atkReady = false;
        atkCoolCount = UnityEngine.Random.Range(1, atkCoolMax);
    }
    public virtual void CoolAttack()
    {
        if (atkCoolCount > 0)
        { atkCoolCount -= Time.deltaTime; }
        else
        { atkReady = true; }
    }


    public virtual void PickAttack()
    {
        this.pickedAttack = UnityEngine.Random.Range(0f, 1f);
        //Debug.Log("Enemy: newAttack value is " + pickedAttack);
    }

    public virtual void StartSwing()
    {
        for (int i = 0; i < myWeaponCol.Length; i++)
        {
            myWeaponCol[i].enabled = true;
        }
    }

    public virtual void EndSwing()
    {
        for (int i = 0; i < myWeaponCol.Length; i++)
        {
            myWeaponCol[i].enabled = false;
        }
    }
    public override void Damaged(int d, float sneakMulti)
    {
        aSource.PlayOneShot(damagedSound);
        if (curState != EnemyStates.Pursue && curState != EnemyStates.Attack)
        {
            float newDamage = d * sneakMulti;
            d = Mathf.CeilToInt(newDamage);
            Debug.Log("Sneak Attacked!");
        }

        base.Damaged(d);

        if (curState != EnemyStates.Flinch)
        {
            EnterPursue(pBody);
        }
    }
    public override void EnterFlinch()
    {
        base.EnterFlinch();

        curState = EnemyStates.Flinch;
        agent.SetDestination(transform.position);
        
        for (int i = 0; i < myWeaponCol.Length; i++)
        {
            myWeaponCol[i].enabled = false;
        }
    }

    public virtual void StateFlinch()
    {

    }

    public virtual void EndFlinch()
    {
        EnterPursue(pBody);
    }
}
