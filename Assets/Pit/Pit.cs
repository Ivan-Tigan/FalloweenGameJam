using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class Pit : MonoBehaviour {

    

	// Use this for initialization
	void Start () {
        GetComponent<CircleCollider2D>().isTrigger = true;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnTriggerExit2D(Collider2D collision)
    {
        MonoBehaviour[] list = collision.gameObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour mb in list)
        {
            if (mb is Player)
            {
                Player player = (Player)mb;
                player.stats.canThrowSkull = false;

            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        MonoBehaviour[] list = collision.gameObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour mb in list)
        {
            if (mb is Player)
            {
                Player player = (Player)mb;
                if (player.stats.hasGoatSkull)
                {
                    player.stats.canThrowSkull = true;
                    
                }
                
            }
        }
    }

}
