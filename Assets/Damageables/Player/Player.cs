using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour, IDamageable<Weapon> {

    public AudioClip m_death;
    public AudioClip m_pit;

    public AudioSource sacrificeAudio;
    public Stats stats;
    public Weapon sword;
    public Torch torch;
    public DirectionOffsets damageAreaOffsets;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    public bool m_initialised;

    [HideInInspector]
    public bool canAttack;
    private bool isAttacking;
    private DamageArea dmgArea;

    public static Player playerInstance;

    private RaycastHit2D[] m_hits;

    private void Awake()
    {
        playerInstance = this;
        
    }

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        sacrificeAudio = GetComponent<AudioSource>();

        canAttack = true;
        isAttacking = false;
        dmgArea = GetComponentInChildren<DamageArea>();
        GetComponentsInChildren<SpriteRenderer>()[1].enabled = false;
        GetComponentsInChildren<SpriteRenderer>()[2].enabled = false;

        m_initialised = true;

    }


    IEnumerator swishSword()
    {
        GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        yield return new WaitForSeconds(0.3f);
        GetComponentsInChildren<SpriteRenderer>()[1].enabled = false;
        yield return null;
    }

    IEnumerator swishTorch()
    {
        GetComponentsInChildren<SpriteRenderer>()[2].enabled = true;
        yield return new WaitForSeconds(0.3f);
        GetComponentsInChildren<SpriteRenderer>()[2].enabled = false;
        yield return null;
    }

    // Update is called once per frame
    void Update () {
        stats.Update();
        torch.Update();

        

        if (stats.health == 0)
        {
            die();
        }

        if(stats.canThrowSkull && Input.GetButtonDown("Sacrifice"))
        {
            stats.canThrowSkull = false;
            stats.hasGoatSkull = false;
            stats.pitBuff();
            sacrificeAudio.clip = m_pit;
            sacrificeAudio.Play();
        }

        
        if (Input.GetButtonDown("Attack1") && Time.time>sword.nextAttack)
        {
            sword.nextAttack = Time.time + sword.attackRate;
            dmgArea.weapon = sword;
            dmgArea.trigger.enabled = true;
            StartCoroutine(swishSword());
        }
        else if(Input.GetButtonDown("Attack2") && Time.time>torch.nextAttack)
        {
            torch.nextAttack = Time.time + torch.attackRate;
            torch.fuel -= torch.attackFuelCost;
            dmgArea.weapon = torch;
            dmgArea.trigger.enabled = true;
            StartCoroutine(swishTorch());
        }
        else
        {
            if(dmgArea.trigger!=null)
            dmgArea.trigger.enabled = false;
        }

       

	}

    private void FixedUpdate()
    {
        //moveVec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector2 moveVec = new Vector2(Mathf.Lerp(0, h * stats.speed, 0.8f),
                                                Mathf.Lerp(0, v * stats.speed, 0.8f));
        
             anim.SetFloat("Horizontal", Mathf.Abs(h));
             anim.SetFloat("Vertical", v);
         
        //restrict x movement
        m_hits = Physics2D.RaycastAll(transform.position, new Vector2(moveVec.x, 0f), 0.5f);

        foreach (RaycastHit2D hit in m_hits)
        {
            if (hit.transform.gameObject.tag == "Obstacle")
            {
                //if wall is left of player, and input is driving player left
                if (hit.transform.position.x < transform.position.x && moveVec.x < 0)
                {
                    moveVec = new Vector2(0f, moveVec.y);
                }

                if (hit.transform.position.x > transform.position.x && moveVec.x > 0)
                {
                    moveVec = new Vector2(0f, moveVec.y);
                }
            }
        }

        m_hits = Physics2D.RaycastAll(transform.position, new Vector2(0f, moveVec.y), 0.5f);


        foreach (RaycastHit2D hit in m_hits)
        {
            if (hit.transform.gameObject.tag == "Obstacle")
            {
                if (hit.transform.position.y < transform.position.y && moveVec.y < 0)
                {
                    moveVec = new Vector2(moveVec.x, 0f);
                }


                if (hit.transform.position.y > transform.position.y && moveVec.y > 0)
                {
                    moveVec = new Vector2(moveVec.x, 0f);
                }

            }

        }
        

        transform.position += new Vector3(moveVec.x, moveVec.y, 0f) * Time.deltaTime * stats.speed * 0.1f;

        bool flipSprite = (spriteRenderer.flipX ? (moveVec.x > 0.0f) : (moveVec.x < 0.0f));
        if (flipSprite)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        if (moveVec != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveVec.y, moveVec.x) * Mathf.Rad2Deg-90;
            transform.GetChild(0).rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        //animations


    }

    public void getDamaged(Weapon weapon)
    {
        stats.getDamaged(weapon.damage);
        sacrificeAudio.clip = m_death;
        sacrificeAudio.Play();
    }

    public void die()
    {
        GameController.resetGame();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        
            MonoBehaviour[] list = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mb in list)
            {
                if (mb is IPickupable<Stats, Weapon>)
                {
                    IPickupable<Stats, Weapon> pickupable = (IPickupable<Stats, Weapon>)mb;
                    pickupable.onPickup(stats, torch);
                } else if(mb is IInteractable<Stats>)
                {
                    IInteractable<Stats> interactable = (IInteractable<Stats>)mb;
                    interactable.onInteract(stats);
                }
            }
        

    }
}
