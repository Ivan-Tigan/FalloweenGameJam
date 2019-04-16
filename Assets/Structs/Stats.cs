using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MinMaxValues
{
    public int maxHealth;
    public int minArmour, maxArmour;
    public float minSpeed, maxSpeed;
}


[System.Serializable]
public class Stats
{
    public MinMaxValues minMaxV;

    public float health;
    public int armour;
    public float speed;

    public bool hasKey;
    public bool hasGoatSkull;
    public bool canThrowSkull;
    

    public void Update()
    {
        health = Mathf.Clamp(health, 0, minMaxV.maxHealth);
        armour = Mathf.Clamp(armour, minMaxV.minArmour, minMaxV.maxArmour);
        speed = Mathf.Clamp(speed, minMaxV.minSpeed, minMaxV.maxSpeed);
    }

    public IEnumerator Burn()
    {
        for (int i = 0; i < 3; i++)
        {
            health -= Player.playerInstance.torch.burningDamage();
            yield return new WaitForSeconds(0.5f);
        }
        
        yield return null;
    }

    public void pitBuff()
    {
        health = (health + Random.Range(-0.4f, 2f) * health);
        armour = (int)(armour + Random.Range(-0.4f, 2f) * armour);
        speed = speed + Random.Range(-0.4f, 2f) * speed;
        Player.playerInstance.torch.damage = (int)(Player.playerInstance.sword.damage + Random.Range(-0.4f, 2f) * Player.playerInstance.sword.damage);
        Player.playerInstance.torch.damage = (int)(Player.playerInstance.torch.damage + Random.Range(-0.4f, 2f) * Player.playerInstance.torch.damage);
    }

    public void addGoatSkull()
    {
        hasGoatSkull = true;
    }

    public void addKey()
    {
        hasKey = true;
    }

    public void UseSkull()
    {
        hasGoatSkull = false;
    }



    public void useKey()
    {
        hasKey = false;
        GameController.health = health;
        GameController.armour = armour;
        GameController.speed = speed;
        GameController.swordDamage=Player.playerInstance.sword.damage;
        GameController.torchDamage = Player.playerInstance.torch.damage;
        GameController.reloadLevel();
    }

    public void getDamaged(int damage)
    {
        health = health - (int)((float)armour / 100 * (float)damage);
    }

    public float incHealth(int amount)
    {
        health += amount;
        return health;
    }

    public int incArmour(int amount)
    {
        armour += amount;
        return armour;
    }

    public float incSpeed(float amount)
    {
        speed += amount;
        return speed;
    }
}
