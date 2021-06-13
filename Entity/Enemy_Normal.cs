using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Normal : Entity_Enemy
{
    public override void Start()
    {
        followDistance = 2;
        base.Start();
    }

    //returns a value between 1 (inclusive) and 4 (exclusive)
    public override void PickAttack()
    {
        //base function picks a value between 0 and 1 
        base.PickAttack();

        if (pickedAttack <= .7)
        { Swat(); }
        else if (pickedAttack > .7)
        { Lunge(); }
        //defaults to Swat and prints an error if anything goes wrong
        else
        {
            Debug.LogError("Error with PickAttack() defaulted to Swat");
            Swat();
        }
    }

    private void Swat()
    {
        for (int i = 0; i < myWeaponScript.Length; i++)
        {
            myWeaponScript[i].SetMultiplier(1f);
        }
        anim.SetBool("isSwat", true);
    }

    private void Lunge()
    {
        for (int i = 0; i < myWeaponScript.Length; i++)
        {
            myWeaponScript[i].SetMultiplier(1.5f);
        }
        anim.SetBool("isLunge", true);
    }

    public void EndAttack()
    {
        //Debug.Log("Ending Attack");
        anim.SetBool("isLunge", false);
        anim.SetBool("isSwat", false);
        anim.SetBool("isAttacking", false);
        StartAttackCool();
        EnterPursue(target);
    }
}
