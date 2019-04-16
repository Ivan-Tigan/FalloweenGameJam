using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable<Weapon> {

    void getDamaged(Weapon weapon);
}
