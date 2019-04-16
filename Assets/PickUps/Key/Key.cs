using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : PickUp
{
    public override void onPickup(Stats stats, Weapon weapon)
    {
        stats.addKey();
        base.onPickup(stats, weapon);
    }
}
