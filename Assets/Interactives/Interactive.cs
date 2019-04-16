using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Interactive : MonoBehaviour, IInteractable<Stats> {

    public Sprite preInteract;
    public Sprite postInteract;

    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Start () {
        GetComponent<BoxCollider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = preInteract;
	}

    public virtual void onInteract(Stats stats)
    {
        spriteRenderer.sprite = postInteract;
    }
}
