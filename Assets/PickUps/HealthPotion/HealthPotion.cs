using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : PickUp {

    public int healingAmount;

   

    public override void onPickup(Stats stats, Weapon weapon)
    {
        stats.incHealth(healingAmount);
        base.onPickup(stats, weapon);
    }

}
