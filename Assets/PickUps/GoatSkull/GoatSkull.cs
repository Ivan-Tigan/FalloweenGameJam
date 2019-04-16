using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoatSkull : PickUp
{
    public override void onPickup(Stats stats, Weapon weapon)
    {
        stats.addGoatSkull();
        base.onPickup(stats, weapon);
    }
}
