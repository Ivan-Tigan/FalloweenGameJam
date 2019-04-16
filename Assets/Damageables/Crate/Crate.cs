using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Crate : MonoBehaviour, IDamageable<Weapon> {

    public Sprite destroyed;
    public float spawnChance;
    public GameObject[] possibleDrops;

    private bool isDamageable=true;

    // Use this for initialization
    void Start () {
        GetComponent<BoxCollider2D>().isTrigger = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void getDamaged(Weapon weapon)
    {
        if (isDamageable)
        {
            isDamageable = false;
            GetComponent<SpriteRenderer>().sprite = destroyed;
            if (Random.Range(0, 100)<spawnChance)
            {
                Instantiate(possibleDrops[Random.Range(0, possibleDrops.Length)], transform.position, transform.rotation);

            }

            //set current tile to be no longer blocked
            if(DungeonGeneration.Instance!=null)
            DungeonGeneration.Instance.m_dungeonMap[(int)(transform.position.x/ DungeonGeneration.Instance.m_tileSize), 
                (int)(transform.position.y / DungeonGeneration.Instance.m_tileSize)] = DungeonGeneration.TileType.Open;
        }
        
    }
}
