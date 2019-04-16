using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable<Stats> {

    void onInteract(Stats stats);
}
