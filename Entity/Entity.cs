using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public stats myStats = new stats();
    
    public LayerMask obstacles;

    public class stats
    {
        int maxHP;
        int curHP;
        int weaponDamage;
        float listenRange;
        float visionRange;
        float visionCone;
        float moveSpeed;
        bool isInvincible = false;
        
        public stats()
        { /*default constructor. Not used*/ }

        public stats(int startingHP, int wDam, float lRange, float vRange, float vCone, float mSpd)
        {
            maxHP = startingHP;
            curHP = maxHP;
            weaponDamage = wDam;
            listenRange = lRange;
            visionRange = vRange;
            visionCone = vCone;
            moveSpeed = mSpd;
        }
        public int GetMaxHP()
        {
            return maxHP;
        }
        public void SetMaxHP(int newHP)
        { maxHP = newHP; }
        public int GetCurHP()
        {
            return curHP;
        }
        public void SetHP(int newHP)
        {
            curHP = newHP;
        }
        public float GetSpeed()
        {
            return moveSpeed;
        }
        public int GetweaponDamage()
        {
            return weaponDamage;
        }
        public float GetlistenRange()
        {
            return listenRange;
        }
        public float GetvisionRange()
        {
            return visionRange;
        }
        public float GetvisionCone()
        {
            return visionCone;
        }
        public void ToggleInvincible()
        { isInvincible = !isInvincible; }
        public bool GetInvincible()
        { return isInvincible; }
    }

    //Animator
    public Animator anim;

    //Handles Flinch 
    public AnimationClip myFlinch;
    public string flinchString;
    public bool isStunned;
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        anim = GetComponent<Animator>();

        if (anim == null)
        { Debug.Log(gameObject.name + " does not have an animator attached!"); }

        if (myFlinch == null)
        { Debug.Log(gameObject.name + " does not have its flinch animation assigned!"); }
        else
        { flinchString = myFlinch.name; }

    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    //For Player
    public virtual void Damaged(int d)
    {
       if(myStats.GetInvincible() == true)
        { return; }

        //Debug.Log(gameObject.name + " took " + d + " damage!");
        if (!gameObject.activeSelf) return;
        int tempdam = d;
        int tempHP = myStats.GetCurHP();
        tempHP -= tempdam;
        myStats.SetHP(tempHP);
        //Debug.Log("Entity has " + tempHP + " life left!");

        float flinchChance = .4f * ((float) tempdam / (float) tempHP);
        
        if (tempHP <= 0)
        { Die(); }
        else if (flinchChance > Random.Range(0f, 1f))
        { EnterFlinch(); }
    }

    //For Enemies
    public virtual void Damaged(int d, float sneakMulti)
    {
        if (myStats.GetInvincible() == true)
        { return; }

        //Debug.Log(gameObject.name + " took " + d + " damage!");
        if (!gameObject.activeSelf) return;
        int tempdam = d;
        int tempHP = myStats.GetCurHP();
        tempHP -= tempdam;
        myStats.SetHP(tempHP);
        //Debug.Log("Entity has " + tempHP + " life left!");

        float flinchChance = .4f * ((float)tempdam / (float)tempHP);

        if (tempHP <= 0)
        { Die(); }
        else if (flinchChance > Random.Range(0f, 1f))
        { EnterFlinch(); }
    }

    public virtual void EnterFlinch()
    {
        anim.Play(flinchString);
    }

    //When death animation is created, this should be called from an animation event instead, 
    //and Damaged() should disable behaviors/input and begin the death animation.
    public virtual void Die()
    {
        Debug.Log(gameObject.name + " just died!");
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
    }

    //Checks line of sight between this object and a given other object and returns true/false
    public virtual bool CheckLOS(GameObject other)
    {
        Vector3 toTarget = other.transform.position - transform.position;
        float distance = Vector3.Distance(other.transform.position, transform.position);
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, toTarget, out hit, distance, obstacles))
        { return true; }
        return false;
    }
}
