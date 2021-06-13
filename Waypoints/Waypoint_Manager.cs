using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint_Manager : MonoBehaviour
{
    [SerializeField]
    Waypoint_Single[] Ways;
    public LayerMask obstacles;
    // Start is called before the first frame update
    void Awake()
    {
        Ways = GetComponentsInChildren<Waypoint_Single>();
        //Reset rotation just in case
        transform.rotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i <= Ways.Length - 1; i++)
        {
            //These need to reset to default each time a new waypoint starts assigning variables
            float northDistance = 1000;
            float westDistance = 1000;
            float eastDistance = 1000;
            float southDistance = 1000;

            for (int b = 0; b <= Ways.Length - 1; b++)
            {
                //Check Distance & Find DotProduct
                float distance = Vector3.Distance(Ways[i].transform.position, Ways[b].transform.position);


                Vector3 targetDir = Ways[b].transform.position - Ways[i].transform.position;
                targetDir = targetDir.normalized;
                float dotProduct = Vector3.Dot(Ways[i].transform.forward, targetDir);
                
                //Debug.Log("Waypoint " + i + " checked against waypoint " + b + " dotproduct is " + dotProduct + ". Distance was " + distance);
                //Check North
                if(Ways[i] == Ways[b])
                {
                    //Debug.Log("Cannot check waypoint against self");
                }

                else if (dotProduct > .5)
                {
                    //Debug.Log("Waypoint identified as North");
                    //see if the new waypoint is closer
                    if(distance < northDistance)
                    {
                       // Debug.Log("Waypoint Identified as closer than currently selected waypoint by " + (distance - northDistance));
                        if(CheckLOS(Ways[i].transform, Ways[b].transform))
                        {
                            //Debug.Log("Waypoint " + Ways[b] + " was identified as being closer to " + Ways[i] + " than previously selected waypoint");

                            Ways[i].SetNorth(Ways[b]);
                            northDistance = distance;
                        }
                    }
                }
                //Check West && East
                else if (dotProduct < .5f && dotProduct > -.5f)
                {
                    //West
                    if (Ways[i].transform.position.x > Ways[b].transform.position.x)
                    {
                        if (distance < westDistance)
                        {
                            //Debug.Log("Waypoint identified as West");
                            //see if the new waypoint is closer
                            if (distance < westDistance)
                            {
                                //Debug.Log("Waypoint Identified as closer than currently selected waypoint by " + (distance - westDistance));
                                if (CheckLOS(Ways[i].transform, Ways[b].transform))
                                {
                                    //Debug.Log("Waypoint " + Ways[b] + " was identified as being closer to " + Ways[i] + " than previously selected waypoint");

                                    Ways[i].SetWest(Ways[b]);
                                    westDistance = distance;
                                }
                            }
                        }
                    }
                    //East
                    else if (Ways[i].transform.position.x < Ways[b].transform.position.x)
                    {
                        if (distance < eastDistance)
                        {
                            //Debug.Log("Waypoint identified as East");
                            //see if the new waypoint is closer
                            if (distance < eastDistance)
                            {
                                //Debug.Log("Waypoint Identified as closer than currently selected waypoint by " + (distance - eastDistance));
                                if (CheckLOS(Ways[i].transform, Ways[b].transform))
                                {
                                    //Debug.Log("Waypoint " + Ways[b] + " was identified as being closer to " + Ways[i] + " than previously selected waypoint");

                                    Ways[i].SetEast(Ways[b]);
                                    eastDistance = distance;
                                }
                            }
                        }
                    }
                }
                //Check South
                else //Object must be south by elimination
                {
                    if (distance < southDistance)
                    {
                        //Debug.Log("Waypoint identified as South");
                        //see if the new waypoint is closer
                        if (distance < southDistance)
                        {
                            //Debug.Log("Waypoint Identified as closer than currently selected waypoint by " + (distance - southDistance));
                            if (CheckLOS(Ways[i].transform, Ways[b].transform))
                            {
                                //Debug.Log("Waypoint " + Ways[b] + " was identified as being closer to " + Ways[i] + " than previously selected waypoint");

                                Ways[i].SetSouth(Ways[b]);
                                southDistance = distance;
                            }
                        }
                    }
                }
            }
        }
    }

    public virtual bool CheckLOS(Transform one, Transform two)
    {
        Vector3 toTarget = two.position - one.position;
        float distance = Vector3.Distance(one.position, two.position);
        RaycastHit hit;
        if (!Physics.Raycast(one.position, toTarget, out hit, distance, obstacles))
        { return true; }
        return false;
    }

    public Waypoint_Single AssignStartPOS(Transform enemyT)
    {
        float savedDistance = 1000;
        Waypoint_Single savedWaypoint;
        savedWaypoint = Ways[0];
        for (int i = 0; i <= Ways.Length -1; i++)
        {
            float thisDistance = Vector3.Distance(Ways[i].transform.position, enemyT.position);
            if (thisDistance < savedDistance)
            {
                savedWaypoint = Ways[i];
                savedDistance = thisDistance;
            }
        }

        Debug.Log("Enemy at coordinates: " + enemyT.position + " is being assigned waypoint named: " + savedWaypoint.name + "at coordinates: " + savedWaypoint.transform.position + " as their reference to find a new destination");
        return savedWaypoint;
    }
}
