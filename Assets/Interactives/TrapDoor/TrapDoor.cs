using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapDoor : Interactive
{

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Player.playerInstance.stats.useKey();
            //onInteract(stats);
        }
    }

    public override void onInteract(Stats stats)
    {
        if (stats.hasKey)
        {
            base.onInteract(stats);
            stats.useKey();
        }
    }
}
