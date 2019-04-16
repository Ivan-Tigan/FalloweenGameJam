using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fuel : PickUp{

    public int fuelAmount;
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void onPickup(Stats stats, Weapon weapon)
    {
        ((Torch)weapon).addFuel(fuelAmount);
        base.onPickup(stats, weapon);

    }
}
