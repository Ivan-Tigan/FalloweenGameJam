using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {

        if (Player.playerInstance)
        {
            transform.position = new Vector3(Player.playerInstance.transform.position.x,
                Player.playerInstance.transform.position.y, -10f);
        }
		
	}
}