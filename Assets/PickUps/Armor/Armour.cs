using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armour : PickUp
{
    public int armourAmount;
    public override void onPickup(Stats stats, Weapon weapon)
    {
        stats.incArmour(armourAmount);
        base.onPickup(stats, weapon);
    }
}
