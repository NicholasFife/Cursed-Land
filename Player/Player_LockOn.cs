using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is set up to return an appropriate lock-on target 
//when either of the two public methods are called.
public class Player_LockOn : MonoBehaviour
{
    Entity_Enemy[] Enemies;

    public LayerMask obLayer;
    [SerializeField]
    private GameObject targeted;
    private int arrayLoc;

    public float savedDistance;
    public float testDistance;

    //Needs replaced with weapon stats when ready
    [SerializeField]
    float weaponDistance = 5f;
    [SerializeField]
    float stealthDistance = 5f;

    bool SecondPass;

    void Awake()
    {
        Enemies = FindObjectsOfType<Entity_Enemy>();
    }

    public Transform GetSoftLock(Weapon equipped)
    {
        savedDistance = stealthDistance;
        for (int i = 0; i<= Enemies.Length-1; i++)
        {
            RaycastHit hit;
            testDistance = Vector3.Distance(transform.position, Enemies[i].transform.position);

            if(testDistance < savedDistance && !Physics.Linecast(transform.position,Enemies[i].transform.position, out hit, obLayer) && Enemies[i].GetComponentInChildren<Renderer>().isVisible)
            {
                targeted = Enemies[i].gameObject;
                savedDistance = testDistance;
                arrayLoc = i;
            }
        }

        if (targeted == null)
        {
            targeted = gameObject;
            Debug.Log("No Target Found, setting self as placeholder target");
        }
        return targeted.transform;
    }
}