using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : PickUp
{
    public float speedAmount;
    public override void onPickup(Stats stats, Weapon weapon)
    {
        stats.incSpeed(speedAmount);
        base.onPickup(stats, weapon);
    }
}
