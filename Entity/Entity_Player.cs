using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Player : Entity
{
    HealthSlider hSlider;

    Collider wCollider;
    Collider bCollider;

    // Start is called before the first frame update
    void Awake()
    {
        myStats = new stats(100, 1, 5, 20, 0, 6);
        hSlider = FindObjectOfType<HealthSlider>();
    }

    public void RegisterWeaponCollider(Collider a, Collider b)
    {
        wCollider = a;
        bCollider = b;
    }

    public override void Damaged(int d)
    {
        base.Damaged(d);
        hSlider.UpdateHealth();
    }

    public override void EnterFlinch()
    {
        base.EnterFlinch();

        //deactivate weapon colliders
        if (wCollider != null)
        { wCollider.enabled = false; }
        if (bCollider != null)
        { bCollider.enabled = false; }
    }

}
