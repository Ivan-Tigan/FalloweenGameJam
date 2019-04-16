using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public static float health=100;
    public static int armour=25;
    public static float speed=5;
    public static int swordDamage = 10;
    public static int torchDamage = 30;


    public static int currentLevel=1;

    public static GameController gameControllerInstance;

    public static bool created = false;

    private void Awake()
    {
        gameControllerInstance = this;
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;


        }
    }
    public static void resetGame()
    {
        health=100;
        armour=25;
        speed=5;
        swordDamage = 10;
        torchDamage = 30;
        currentLevel = 1;
        SceneManager.LoadScene("SampleScene");//change correct scene
    }
    public static void reloadLevel()
    {
        SceneManager.LoadScene("SampleScene");//change correct scene
        currentLevel++;
        
    }

    // Use this for initialization
    void Start () {
   /*     Player.playerInstance.stats.health = health;
        Player.playerInstance.stats.armour = armour;
        Player.playerInstance.stats.speed = speed;
        Player.playerInstance.sword.damage = swordDamage;
        Player.playerInstance.torch.damage = torchDamage;
        */
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
