using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public int damage;
    public float attackRate;
    public float range;
    
    [HideInInspector]
    public float nextAttack;
}

[System.Serializable]
public class Torch : Weapon
{

    public float fuel;
    public float maxFuel;
    public float decRateAmount;
    public float decRateTime;
    public float attackFuelCost;

    public float brightness;

    private float nextDec = 0.0f;

    public float burningDamage()
    {
        return 0.1f * damage;
    }

    public float addFuel(float amount)
    {
        fuel += amount;
        return fuel;
    }

    public void Update()
    {
        fuel = Mathf.Clamp(fuel, 0, maxFuel);
        if (Time.time > nextDec) {
            nextDec = Time.time + decRateTime;
            fuel = Mathf.Clamp(fuel-decRateAmount, 0, 100);

        }
        Player.playerInstance.GetComponentsInChildren<Transform>()[5].localScale=new Vector3(brightness*fuel/100f, brightness * fuel / 100f);
    }
}
