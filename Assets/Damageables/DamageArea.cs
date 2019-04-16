using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DamageArea : MonoBehaviour {

    public Weapon weapon;
    public BoxCollider2D trigger;
	// Use this for initialization
	void Start () {
        trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MonoBehaviour[] list = collision.gameObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour mb in list)
        {
            if (mb is IDamageable<Weapon>)
            {
                IDamageable<Weapon> damageable = (IDamageable<Weapon>)mb;
                damageable.getDamaged(weapon);
            }
        }
    }

}
