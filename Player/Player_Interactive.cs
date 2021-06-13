using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Interactive : MonoBehaviour
{
    public Interactive selected;
    int curPrior;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        Interactive tempSel = other.GetComponent<Interactive>();

        if(tempSel == null)
        { return; }

        if (tempSel.GetPriority() > curPrior && tempSel.enabled == true)
        {
            selected = tempSel;
            curPrior = tempSel.GetPriority();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (selected != null && other.gameObject == selected.gameObject)
        {
            ClearSelected();
        }
    }

    public void UseSelected()
    {
        if (selected != null)
        { selected.UseMe(); }
    }

    public void ClearSelected()
    {
        selected = null;
        curPrior = 0;
        Debug.Log("Selected set to null");
    }

    public Interactive getSelected()
    { return selected; }
}
