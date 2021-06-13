using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint_Single : MonoBehaviour
{
    public Waypoint_Single[] neighbors= new Waypoint_Single[4];
    bool singleReturn;
    Transform playerT;
    
    int totalNeighbors = 4;
    // Start is called before the first frame update

    void Awake()
    {
        playerT = FindObjectOfType<Entity_Player>().transform;
    }

    void Start()
    {

        for (int i = 0; i <= neighbors.Length - 1; i++)
        {
            if(neighbors[i] == null)
            {
                totalNeighbors--;
            }
        }
        if (totalNeighbors == 1)
        { singleReturn = true; }
    }

    void Update()
    {
        DebugLines();
    }
    
    public Waypoint_Single GetWaypoint(Waypoint_Single lastWaypoint)
    {
        if (singleReturn)//If true, GetWaypoint will return before completing the rest of the method
        {
            for (int i = 0; i <= neighbors.Length-1; i++)
            {
                if(neighbors[i] != null)
                { return neighbors[i]; }
            }
            //Just in case my code isn't working as expected, this should catch it
            Debug.LogError("GetWaypoint singleReturn Error: lastWaypoint repeating to recover");
            return lastWaypoint;
        }

        //Generate a number within appropriate range, excluding 1 for the last waypoint which should not repeat
        int randdir = Random.Range(0, totalNeighbors-1);
        for (int i = 0; i <= neighbors.Length -1; i++)
        {
            //check if it is valid and needs accounted for in the count
            if(neighbors[i] != null && neighbors[i] != lastWaypoint)
            {
                if (randdir == 0)
                {
                    //Debug.Log("GetWaypoint has worked correctly, returning the waypoint in array slot: " + i);
                    return neighbors[i];
                }
                else randdir--;
                
                //Used to recover in case something goes wrong - *shouldn't* ever run...
                if(randdir < 0)
                {
                    Debug.LogError("Waypoint_Single.GetWaypoint counted past the desired waypoint: lastWaypoint repeating to recover");
                    return lastWaypoint;
                }
            }
        }
        Debug.LogError("GetWaypoint reached end of method without returning an appropriate waypoint: lastWaypoint repeating to recover");
        return lastWaypoint;
    }

    //Called by Enemy when exiting
    public Waypoint_Single FindWaypointToPlayer()
    {
        Vector3 targetDir = playerT.position - transform.position;
        targetDir = targetDir.normalized;
        float dotProduct = Vector3.Dot(transform.forward, targetDir);

        return CardinalToPlayer(dotProduct);
    }

    //Returns a reference to whichever point is in the correct direction
    Waypoint_Single CardinalToPlayer(float dot)
    {
        if (dot > .5f) //Player is North
        {
            Debug.Log("Player was north of " + gameObject.name);
           
            return neighbors[0];

        }
        else if (dot < .5f && dot > -.5f)//Player is East or West
        {
            if (transform.position.x > playerT.position.x) //Player is west
            {
                Debug.Log("Player was west of " + gameObject.name);


                    return neighbors[1];
                
            }
            else //Player is east
            {
                Debug.Log("Player was east of " + gameObject.name);

                    return neighbors[2];

            }
        }
        else //player is south
        {
            Debug.Log("Player was south of " + gameObject.name);

                return neighbors[3];
            
        }
    }

    public Waypoint_Single OnCardinalToPlayerFail(Transform enemy)
    {
        Vector3 targetDir = enemy.position - transform.position;
        targetDir = targetDir.normalized;
        float dotProduct = Vector3.Dot(transform.forward, targetDir);
        dotProduct = -dotProduct;


        if (dotProduct > .5f)
        {
            Debug.Log("Player was north of " + gameObject.name);

            if (neighbors[0] != null)
            {
                return neighbors[0];
            }

        }
        else if (dotProduct < .5f && dotProduct > -.5f)
        {
            if (transform.position.x > (enemy.position.x * -1)) 
            {
                Debug.Log("Player was west of " + gameObject.name);

                if (neighbors[1] != null)
                {
                    return neighbors[1];
                }

            }
            else
            {
                Debug.Log("Player was east of " + gameObject.name);
                if (neighbors[2] != null)
                {
                    return neighbors[2];
                }

            }
        }
        else //player is south
        {
            Debug.Log("Player was south of " + gameObject.name);
            if (neighbors[3] != null)
            {
                return neighbors[3];
            }
        }
        return GetComponent<Waypoint_Single>();
    }

    //Used to set neighbor references
    public void SetNorth(Waypoint_Single n)
    {
        neighbors[0] = n;
    }
    public void SetWest(Waypoint_Single n)
    {
        neighbors[1] = n;
    }
    public void SetEast(Waypoint_Single n)
    {
        neighbors[2] = n;
    }
    public void SetSouth(Waypoint_Single n)
    {
        neighbors[3] = n;
    }

    //Activate to draw lines between point connections
    void DebugLines()
    {
        if (neighbors[0] != null)
        { Debug.DrawLine(transform.position, neighbors[0].transform.position); }
        if (neighbors[1] != null)
        { Debug.DrawLine(transform.position, neighbors[1].transform.position); }
        if (neighbors[2] != null)
        { Debug.DrawLine(transform.position, neighbors[2].transform.position); }
        if (neighbors[3] != null)
        { Debug.DrawLine(transform.position, neighbors[3].transform.position); }
    }
}