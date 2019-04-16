using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickupable <Stats, Weapon> {

    void onPickup(Stats stats, Weapon weapon);
}
