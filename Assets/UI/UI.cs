using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {

    public Image healthBar;
    public Image fuelBar;
    public Image key;
    public Image goat;
    public Text level;
    public Text sacrifice;

	// Use this for initialization
	void Start () {
        healthBar = gameObject.GetComponentsInChildren<Image>()[1];
        fuelBar = gameObject.GetComponentsInChildren<Image>()[3];
        key = gameObject.GetComponentsInChildren<Image>()[4];
        goat = gameObject.GetComponentsInChildren<Image>()[5];
        level = gameObject.GetComponentsInChildren<Text>()[1];
        sacrifice = gameObject.GetComponentsInChildren<Text>()[2];
    }

    // Update is called once per frame
    void Update () {
        healthBar.fillAmount = (float)Player.playerInstance.stats.health / (float)Player.playerInstance.stats.minMaxV.maxHealth;
        fuelBar.fillAmount = Player.playerInstance.torch.fuel / Player.playerInstance.torch.maxFuel;
        key.enabled = Player.playerInstance.stats.hasKey;
        goat.enabled = Player.playerInstance.stats.hasGoatSkull;
        level.text = GameController.currentLevel.ToString();
        sacrifice.enabled = Player.playerInstance.stats.canThrowSkull;
    }
}
