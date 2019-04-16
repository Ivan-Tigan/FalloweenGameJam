using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour, IDamageable<Weapon> {

    public Stats stats;
    public Weapon weapon;

    public Sprite up;
    public Sprite down;
    public Sprite left;

    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private bool isAlive;

    [SerializeField]
    public float m_detectionRange;

    [SerializeField]
    private Vector2 targetPosition;
    private Vector2 nextPosition;

    public Vector2 m_gridPosition;

    private List<Vector2> m_path;

    private bool m_reachedNextTile;

    private bool m_move;

    private float m_distanceToPlayer;

    // Use this for initialization
    void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        GetComponent<BoxCollider2D>().isTrigger = true;

        isAlive = true;
        m_reachedNextTile = true;
        m_move = false;
        
    }

	void Update ()
    {
        if (stats.health==0)
        {
            die();
        }
        if (isAlive)
        {


            stats.Update();

            handleCombat();

            if (!Player.playerInstance.m_initialised)
            {
                return;
            }
            m_distanceToPlayer = Vector2.Distance(transform.position, Player.playerInstance.transform.position);

            if (m_distanceToPlayer < m_detectionRange)
            {
                MoveToPosition(new Vector2(Mathf.RoundToInt(Player.playerInstance.transform.position.x),
                    Mathf.RoundToInt(Player.playerInstance.transform.position.y)));
            }
            else
            {
                m_move = false;
            }

            if (m_move)
            {
                Movement();
            }
        }
    }

    private void handleCombat()
    {
        if (Vector2.Distance(transform.position, Player.playerInstance.transform.position) < weapon.range)
        {
            if(Time.time>weapon.nextAttack)
            {
                Player.playerInstance.getDamaged(weapon);
                weapon.nextAttack = Time.time + weapon.attackRate;
            }
        }
    }

    public void die()
    {
        isAlive = false;
    }

    private void Movement ()
    {
        if (m_reachedNextTile)
        {
            m_reachedNextTile = false;

            m_gridPosition = new Vector2(Mathf.RoundToInt(transform.position.x / DungeonGeneration.Instance.m_tileSize),
                    Mathf.RoundToInt(transform.position.y / DungeonGeneration.Instance.m_tileSize));

            m_path = Pathfinding.Instance.FindPath(m_gridPosition, targetPosition);

            if (m_path.Count != 1)
            {
                nextPosition = m_path[m_path.Count - 1];

                #region Sprites stuff
                ///////////////////////////////////////////////////////////////////////////////////////
                Dictionary<float, Vector2> dists = new Dictionary<float, Vector2>();
                dists[Vector2.Distance(nextPosition - (Vector2)transform.position, Vector2.up)] = Vector2.up;
                dists[Vector2.Distance(nextPosition - (Vector2)transform.position, Vector2.down)] = Vector2.down;
                dists[Vector2.Distance(nextPosition - (Vector2)transform.position, Vector2.left)] = Vector2.left;
                dists[Vector2.Distance(nextPosition - (Vector2)transform.position, Vector2.right)] = Vector2.right;
                Vector2 dir;
                dists.TryGetValue(dists.Keys.Min(), out dir);
                if (dir == Vector2.up)
                {
                    spriteRenderer.sprite = up;
                    spriteRenderer.flipX = false;
                }
                else if (dir == Vector2.down)
                {
                    spriteRenderer.sprite = down;
                    spriteRenderer.flipX = false;
                }
                else if (dir == Vector2.left)
                {
                    spriteRenderer.sprite = left;
                    spriteRenderer.flipX = false;
                }
                else
                {
                    spriteRenderer.sprite = left;
                    spriteRenderer.flipX = true;
                }

                ////////////////////////////////////
                #endregion
            }
            else
            {
                m_move = false;
            }
        }

        else
        {
            if (Vector2.Distance(transform.position, nextPosition) < 0.05f)
            {
                //DungeonGeneration.Instance.m_dungeonMap[(int)m_gridPosition.x, (int)m_gridPosition.y] = DungeonGeneration.TileType.Open;
                m_reachedNextTile = true;
            }
            else
            {
                Vector2 moveTo = nextPosition - m_gridPosition;
                transform.position += new Vector3(moveTo.x, moveTo.y) * Time.deltaTime * stats.speed;
            }
        }

    }

    public void MoveToPosition (Vector2 givenGridPosition) //pass in a grid vector not real world!
    {
        targetPosition = givenGridPosition;
        m_move = true;
    }

    public void getDamaged(Weapon weapon)
    {
        stats.getDamaged(weapon.damage);
        if (weapon is Torch)
        {
            StartCoroutine(stats.Burn());
        }
    }

}