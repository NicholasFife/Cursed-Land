using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Notes:
//Once weapon switching is set up, wCollider needs to be updated when a weapon is being changed. 

public class PlayerAnimPassthrough : MonoBehaviour
{
    Animator anim;
    Player_Input pInput;
    Collider wCollider; //Holds the capsule collider used for detecting weapon damage events
    Collider bCollider; //Holds the box collider used for detecting weapon block events
    Weapon equippedWeapon;
    Player_Sounds ps;
    Entity_Player pEnt;

    StaminaSlider staBar;

    void Awake()
    {
        anim = GetComponent<Animator>();
        pInput = GetComponent<Player_Input>();
        ps = GetComponent<Player_Sounds>();
        staBar = FindObjectOfType<StaminaSlider>();
        pEnt = GetComponent<Entity_Player>();
    }

    public void RegisterWeapon()
    {
        equippedWeapon = GetComponentInChildren<Weapon>();
        //Find the attack collider
        wCollider = equippedWeapon.GetComponent<CapsuleCollider>();
        
        //Register the blocking collider
        bCollider = equippedWeapon.GetComponent<BoxCollider>();

        Debug.Log("looking for bCollider " + wCollider.name);

        //register both colliders with Player_Entity so it can be turned off at Flinch.
        pEnt.RegisterWeaponCollider(wCollider, bCollider);

        wCollider.enabled = false;
        bCollider.enabled = false;
    }

    //Called at the start of the swing
    public void StartQuickAttack()
    {
        //stamina bar stops recharging
        staBar.StopRecharge();
        //Reset values so a new attack can be queued
        anim.SetBool("isQuickAttack", false);
        anim.SetBool("isPowerAttack", false);
        anim.SetBool("isCrouched", false);

        //Stop Player Movement
        pInput.SetAttackMove(false);

        //Turn on weapon Collider
        wCollider.enabled = true;

        //Play swish
        ps.PlayShortSwish();

        //Subtract stamina cost
        staBar.MinusStaminaOnce(equippedWeapon.thisWeapon.GetStaCost());
    }

    public void StartPowerAttack()
    {
        //Stamina bar stops recharging
        staBar.StopRecharge(); 
        //Reset values so a new attack can be queued
        anim.SetBool("isQuickAttack", false);
        anim.SetBool("isPowerAttack", false);
        anim.SetBool("isCrouched", false);

        //Stop Player Movement
        pInput.SetAttackMove(false);

        //Turn on weapon Collider
        wCollider.enabled = true;

        //Play swish
        ps.PlayLongSwish();

        //Subtract stamina cost
        staBar.MinusStaminaOnce(equippedWeapon.thisWeapon.GetStaCost());
    }

    //Called at the end of the swing
    public void EndAttack()
    {
        //Turn off weapon Collider
        wCollider.enabled = false;
        equippedWeapon.ClearDamageArray();
    }

    //Called when player leaves the attack state
    public void EndCombo()
    {
        //Stamina bar starts recharging again
        staBar.StartRecharge();
        Debug.Log("EndCombo() was called");
        //Reset values so a new attack can be queued
        anim.SetBool("isQuickAttack", false);
        anim.SetBool("isPowerAttack", false);

        //Reenable player movement when exiting the attack state
        pInput.SetAttackMove(true);

        //Weapon collider should already be set back to disabled
    }

    public void DrawArrow()
    {
        equippedWeapon.DrawArrow();
    }

    public void CanCharge()
    { equippedWeapon.SetCanCharge(true); }
    public void FireRanged()
    {
        equippedWeapon.Shoot();
    }
    public void RemoveArrow()
    {
        equippedWeapon.RemoveArrow();
    }
    
    public void StartBlock()
    {
        bCollider.enabled = true;
        equippedWeapon.SetIsBlocking(true);
    }
    public void EndBlock()
    {
        bCollider.enabled = false;
        equippedWeapon.SetIsBlocking(false);
    }
}
