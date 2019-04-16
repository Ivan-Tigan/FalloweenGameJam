using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PickUp : MonoBehaviour, IPickupable<Stats,Weapon> {
    

    // Use this for initialization
    public void Start () {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void onPickup(Stats stats, Weapon weapon)
    {
        Destroy(this.gameObject);
    }

}
