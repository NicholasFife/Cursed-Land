using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Inventory : MonoBehaviour
{
    PlayerAnimPassthrough animPass;

    Transform lHand;
    Transform rHand;

    public Weapon[] weapons = new Weapon[3];

    int weaponSlot;
    Weapon equippedWeapon;

    bool weaponsFull = false;

    void Awake()
    {
        animPass = GetComponent<PlayerAnimPassthrough>();
    }

    // Start is called before the first frame update
    void Start()
    {
        lHand = GetComponentInChildren<LHand>().transform;
        rHand = GetComponentInChildren<RHand>().transform;

        //Counts how many weapons are equipped to left hand
        int rightHandLength = rHand.childCount;
        //Debug.Log("rightHandLength = " + rightHandLength);

        int leftHandLength = lHand.childCount;
        //Debug.Log("leftHandLength = " + leftHandLength);

        //Debug.Log("Weapons length is " + weapons.Length);

        //Runs to order equipped weapons in the array
        for (int a = 0; a < 3; a++)
        {
            //Right hand Weapons
            if (a < rightHandLength)
            {
                //Set Array Slot
                weapons[a] = GetComponentInChildren<RHand>().transform.GetChild(a).GetComponent<Weapon>();
                //Turn off Object until Equipped
                weapons[a].gameObject.SetActive(false);
            }
            //Left hand weapons
            else if (a < rightHandLength + leftHandLength)
            {
                //Set Array Slot
                weapons[a] = GetComponentInChildren<LHand>().GetComponentInChildren<Weapon>();
                //Turn off Object until Equipped
                weapons[a].gameObject.SetActive(false);
            }
            else
            { Debug.Log("Weapon Slot " + a + " = null"); }
            
        }
        if (weapons[0] != null)
        { StartWeapon(); }
    }

    void StartWeapon()
    {
        weapons[weaponSlot].gameObject.SetActive(true);

        equippedWeapon = weapons[weaponSlot].SetWeapon();
        animPass.RegisterWeapon();
    }

    public void SwitchEquippedWeapon(int i)
    {
        weapons[weaponSlot].gameObject.SetActive(false);

        if (weapons[i] != null)
        {
            weaponSlot = i;
        }

        weapons[weaponSlot].gameObject.SetActive(true);

        equippedWeapon = weapons[weaponSlot].SetWeapon();
        animPass.RegisterWeapon();
    }
    
    public void PickUpWeapon(Weapon newWeapon, bool lHanded, Vector3 rot)
    {
        for (int i = 0; i < 3 ; i++)
        {
            if (weapons[i] == null)
            {
                weapons[i] = newWeapon;
                Debug.Log("successfully attached weapon in slot " + i);

                //Figure out if it needs attached to the left or right hand!
                if(lHanded == true)
                { newWeapon.transform.parent = lHand; }
                else
                { newWeapon.transform.parent = rHand.GetChild(0); }

                newWeapon.transform.localPosition = new Vector3(0, 0, 0);
                newWeapon.transform.localRotation = Quaternion.Euler(rot);

                SwitchEquippedWeapon(i);

                //Attach the new weapon to appropriate hand. 

                //deactivate the interactive script once equipped!
                
                return;
            }
        }

        Debug.LogError("Too many weapons already equipped. ReplaceWeapon() should have been called instead!");
    }
    
    public Weapon GetEquippedWeapon()
    { return equippedWeapon; }

    public Weapon[] GetEquippedWeapons()
    {
        return weapons;
    }

    public void SetEquippedWeapons(Weapon[] w)
    {
        weapons = w;
    }
}
