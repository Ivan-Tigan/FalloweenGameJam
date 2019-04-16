using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour {

    public static DungeonGeneration Instance;

    public enum TileType
    {
        Blocked, Open, Start, End, Pit, Edge, PitEdge
    }

    [Header("The pit")]
    private Vector2 m_pitPosition;
    [SerializeField]
    private int m_pitSize;
    [HideInInspector]
    public Vector2 m_pitCentre;
    [SerializeField]
    private GameObject m_pitPrefab;

    [Header("Tiles and walls")]
    [SerializeField]
    private GameObject m_tilePrefab;
    [SerializeField]
    private GameObject m_pathPrefab;
    [SerializeField]
    private GameObject m_start;
    [SerializeField]
    private GameObject m_end;
    [SerializeField]
    private GameObject m_wallTopPrefab;
    [SerializeField]
    private GameObject m_wallFacePrefab;
    [SerializeField]
    public int m_tileSize = 1;
    [SerializeField]
    private int m_width;
    [SerializeField]
    private int m_height;
    [SerializeField]
    public int m_pathWidth;

    [Header("Items")]
    [SerializeField]
    private GameObject m_cratePrefab;
    [SerializeField]
    private GameObject m_keyPrefab;
    [SerializeField]
    private GameObject m_goatSkull;

    [Header("Rooms")]
    [SerializeField]
    private int m_minNumberOfRooms;
    [SerializeField]
    private int m_maxNumberOfRooms;
    [SerializeField]
    private int m_minRoomSize; //new value will be found for x and y
    [SerializeField]
    private int m_maxRoomSize;
    private int m_numberOfRooms;
    [SerializeField]
    private GameObject m_roomTilePrefab;

    [Header("Enemy")]
    [SerializeField]
    private GameObject[] m_enemyPrefabs;

    [Header("Player")]
    [SerializeField]
    private GameObject m_playerPrefab;
    [SerializeField]
    private Sprite m_pitQuarter;
    [SerializeField]
    private Sprite m_pitThreeQuarter;
    [SerializeField]
    private Sprite m_pitHalf;

    public List<Room> m_rooms;
    public TileType[,] m_dungeonMap;

    public class Room
    {
        private List<Vector2> m_roomTiles;

        public List<Vector2> RoomTiles
        {
            set
            {
                if (m_roomTiles == null)
                {
                    m_roomTiles = new List<Vector2>();
                }
                m_roomTiles = value;
            }
            get
            {
                if (m_roomTiles == null)
                {
                    m_roomTiles = new List<Vector2>();
                }
                return m_roomTiles;
            }
        }
    }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        GenerateDungeon();
    }

    Room pit = new Room();

    private void GenerateDungeon ()
    {
        m_dungeonMap = new TileType[m_width, m_height];



        SpawnPit();
        GenerateRooms();
        ConnectRooms();
        SetStartAndEndPositions();
        SetEdgeTiles();


        SpawnTiles();
        PitEdges();
        SpawnItems();
    }

    private void SetEdgeTiles ()
    {
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                if (x == m_width - 1 || x == 0 || y == m_height - 1 || y == 0)
                {
                    m_dungeonMap[x, y] = TileType.Edge;
                }
            }
        }
    }

    private void SpawnPit ()
    {
        for (int x = -m_pitSize/2; x < (m_pitSize/2)+1; x++)
        {
            for (int y = -m_pitSize / 2; y < (m_pitSize / 2)+1; y++)
            {
                if (Vector2.Distance(new Vector2(x + (m_width / 2), y + (m_height / 2)), new Vector2(m_width / 2, m_height / 2)) < (m_pitSize)/3)
                {
                    m_dungeonMap[x + (m_width / 2), y + (m_height / 2)] = TileType.Pit;

                    pit.RoomTiles.Add(new Vector2(x + (m_width / 2), y + (m_height / 2)));
                }
            }
        }
        m_pitCentre = new Vector2(m_width / 2, m_height / 2);
    }

    private void SpawnPlayer (Vector2 givenPosition)
    {
        GameObject go = Instantiate(m_playerPrefab);

        go.transform.position = givenPosition;
        go.GetComponent<Player>().stats.health = GameController.health;
        go.GetComponent<Player>().stats.armour = GameController.armour;
        go.GetComponent<Player>().stats.speed = GameController.speed;
        go.GetComponent<Player>().torch.damage = GameController.torchDamage;
        go.GetComponent<Player>().sword.damage = GameController.swordDamage;
    }

    private void SpawnItems ()
    {
        GameObject parent = new GameObject();
        parent.name = "Tiles";
        //int randomRoomIndex = Random.Range(0, m_rooms.Count-1);
        int randomRoomIndex = Random.Range(0, m_rooms.Count);

        SpawnItem(m_rooms[randomRoomIndex], m_keyPrefab, parent.transform, true).name = "key";
        //randomRoomIndex = Random.Range(0, m_rooms.Count - 1);
        randomRoomIndex = Random.Range(0, m_rooms.Count);
        SpawnItem(m_rooms[randomRoomIndex], m_goatSkull, parent.transform, true);

        for (int i = 0; i < 2; i++)
        {

            foreach (Room room in m_rooms)
            {
                if (room.RoomTiles.Count > 0)
                {
                    int spawnChance = Random.Range(0, 100);
                    if (spawnChance < 50)
                    {
                        SpawnItem(room, m_cratePrefab, parent.transform);
                    }
                }
            }
        }



        foreach (Room room in m_rooms)
        {
            if (room.RoomTiles.Count > 0)
            {
                int spawnChance = Random.Range(0, 100);
                if (spawnChance < 30)
                {
                    GameObject enemy = SpawnItem(room, m_enemyPrefabs[Random.Range(0, m_enemyPrefabs.Length)], parent.transform);

                    if (enemy.GetComponent<Enemy>())
                    {
                        enemy.GetComponent<Enemy>().m_gridPosition = enemy.transform.position / m_tileSize;
                    }
                }
            }
        }


    }

    private GameObject SpawnItem (Room room, GameObject givenPrefab, Transform parent, bool forceSpawn = false)
    {
        //Vector2 randomTileInRoom = room.RoomTiles[Random.Range(0, room.RoomTiles.Count - 1)];

        if (room.RoomTiles.Count == 0)
        {
            Weapon weapon = new Weapon();
            weapon.damage = 9999;
            Player.playerInstance.getDamaged(weapon);
        }
        Vector2 randomTileInRoom = room.RoomTiles[Random.Range(0, room.RoomTiles.Count)];
        List<Vector2> exploredTiles = new List<Vector2>();
        int i = 0;
        while (!forceSpawn && (m_dungeonMap[(int)randomTileInRoom.x, (int)randomTileInRoom.y] != TileType.Open 
            || Vector2.Distance(randomTileInRoom, Player.playerInstance.transform.position) < 20f)) //enemy detection range
        {
            i++;
            if (i > room.RoomTiles.Count-1)
            {
                return new GameObject();
            }

            randomTileInRoom = room.RoomTiles[Random.Range(0, room.RoomTiles.Count)];
        }

        GameObject go = Instantiate(givenPrefab);
        go.transform.position = randomTileInRoom * m_tileSize;

        go.transform.parent = parent.transform;

        return go;
    }

    private void PitEdges ()
    {
        List<GameObject> pitEdges = new List<GameObject>();
        foreach (Vector2 tile in pit.RoomTiles)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (m_dungeonMap[(int)(tile.x + x), (int)(tile.y + y)] == TileType.Open && m_dungeonMap[(int)(tile.x), (int)(tile.y)] != TileType.Open)
                    {
                        m_dungeonMap[(int)tile.x, (int)tile.y] = TileType.PitEdge;

                        GameObject a = Instantiate(m_pitPrefab);
                        a.transform.position = tile;

                        pitEdges.Add(a);
                    }
                }
            }
        }

        foreach (GameObject pitEdge in pitEdges)
        {

            //Three quarter
            //right

            if ( (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&




                (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.PitEdge
                || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked) &&






                (m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked))
            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitThreeQuarter;
                pitEdge.name = ("A");
            }

            //left
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x -1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge ||
                m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&



           (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.PitEdge ||
                m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked) &&





                (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked))
            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitThreeQuarter;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x * -1f, pitEdge.transform.localScale.y, pitEdge.transform.localScale.z);
                pitEdge.name = ("B");
            }

            //right down
            else if( (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge ||
                m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&


               (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.PitEdge ||
               m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked) &&



                (m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y + 1)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked))
            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitThreeQuarter;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x, pitEdge.transform.localScale.y * -1f, pitEdge.transform.localScale.z);
                pitEdge.name = ("C");
            }

            //left down
            else if( (m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&



               (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.PitEdge ||
               m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked) &&






                (m_dungeonMap[(int)(pitEdge.transform.position.x+1), (int)(pitEdge.transform.position.y+1)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x+1), (int)(pitEdge.transform.position.y+1)] == TileType.Blocked))
            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitThreeQuarter;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x * -1f, pitEdge.transform.localScale.y * -1f, pitEdge.transform.localScale.z);
            }
            /////////////////////////////////
            //right
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked ) &&





                (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked) &&





                (m_dungeonMap[(int)(pitEdge.transform.position.x-1), (int)(pitEdge.transform.position.y)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitHalf;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x, pitEdge.transform.localScale.y * -1f, pitEdge.transform.localScale.z);
                pitEdge.name = ("D");
            }
            //left
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked) &&



                (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.PitEdge
                || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked) &&





                (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitHalf;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x * -1f, pitEdge.transform.localScale.y * -1f, pitEdge.transform.localScale.z);
                pitEdge.name = ("D");
            }
            //down
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x+1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&



                (m_dungeonMap[(int)(pitEdge.transform.position.x-1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge
                || m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&




                (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y+1)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitHalf;
                pitEdge.transform.eulerAngles -= new Vector3(0f, 0f, 90f);
                pitEdge.name = ("D");
            }
            //up
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&



                  (m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                  || m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&




                  
                  (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Open ||
                  m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitHalf;
                pitEdge.transform.eulerAngles += new Vector3(0f, 0, 90f);
                pitEdge.name = ("!!!!!!!!!!!!!!!!!!!!!!!!!!");
            }

            ///////////////////////////////////////////////////////////
            //quarters
            //right bottom corner
            else if ((m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge
                || m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&



            (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y+1)] == TileType.PitEdge 
            || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked) &&




            (m_dungeonMap[(int)(pitEdge.transform.position.x-1), (int)(pitEdge.transform.position.y + 1)] == TileType.Open ||
            m_dungeonMap[(int)(pitEdge.transform.position.x-1), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitQuarter;
            }

            //left bottom corner
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&



             (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.PitEdge
             || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked) &&




             (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y + 1)] == TileType.Open ||
             m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y + 1)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitQuarter;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x * -1f, pitEdge.transform.localScale.y, pitEdge.transform.localScale.z);
            }


            //right top corner
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) &&




          (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.PitEdge
          || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked) &&




          (m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Open ||
          m_dungeonMap[(int)(pitEdge.transform.position.x - 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitQuarter;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x, pitEdge.transform.localScale.y*-1f, pitEdge.transform.localScale.z);
            }


            //left top corner
            else if ( (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.PitEdge 
                || m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y)] == TileType.Blocked) && 



                    (m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.PitEdge 
                    || m_dungeonMap[(int)(pitEdge.transform.position.x), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked) &&

                    (m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Open ||
                    m_dungeonMap[(int)(pitEdge.transform.position.x + 1), (int)(pitEdge.transform.position.y - 1)] == TileType.Blocked))

            {
                pitEdge.GetComponent<SpriteRenderer>().sprite = m_pitQuarter;
                pitEdge.transform.localScale = new Vector3(pitEdge.transform.localScale.x*-1f, pitEdge.transform.localScale.y*-1f, pitEdge.transform.localScale.z);
            }


        }
    }

    private void SpawnTiles()
    {
        GameObject parent = new GameObject();
        parent.name = "Tiles";
        GameObject go = new GameObject();
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                switch (m_dungeonMap[x, y])
                {
                    case TileType.Blocked:
                        if (IsWithinMapBounds(new Vector2(x,y-1)))
                        {
                            if (m_dungeonMap[x, y - 1] == TileType.Blocked)
                            {
                                go = Instantiate(m_wallTopPrefab);
                            }
                            else
                            {
                                go = Instantiate(m_wallFacePrefab);
                            }
                        }
                        else
                        {
                            go = Instantiate(m_wallTopPrefab);
                        }

                        if (!IsWithinRangeOfFloor(x, y))
                        {
                            go.GetComponent<SpriteRenderer>().color = Color.black;
                        }

                        break;
                    case TileType.Pit:
                        go = Instantiate(m_pitPrefab);
                        go.GetComponent<SpriteRenderer>().sortingOrder = 0;
                        break;
                    case TileType.Open:
                        go = Instantiate(m_tilePrefab);
                        break;
                    case TileType.Start:
                        go = Instantiate(m_start);
                        go.name = "start";
                        break;
                    case TileType.End:
                        go = Instantiate(m_end);
                        go.name = "end";
                        break;
                    case TileType.Edge:

                        if (y == m_height - 1)
                        {
                            go = Instantiate(m_wallFacePrefab);
                        }

                        if (IsWithinMapBounds(new Vector2(x, y - 1)))
                        {
                            go = Instantiate(m_wallTopPrefab);
                        }
                        else
                        {
                            go = Instantiate(m_wallTopPrefab);
                        }

                        if (!IsWithinRangeOfFloor(x, y))
                        {
                            go.GetComponent<SpriteRenderer>().color = Color.black;
                        }
                        break;
                }
                go.transform.position = new Vector2(x, y) * m_tileSize;
                go.transform.parent = parent.transform;
            }

        }
        Destroy(go);
    }

    private bool IsWithinRangeOfFloor (int givenX, int givenY)
    {
        for (int x = -2; x < 3; x++)
        {
            for (int y = -2; y < 3; y++)
            {
                if (IsWithinMapBounds(new Vector2(givenX + x, givenY + y)))
                {
                    if (m_dungeonMap[givenX + x, givenY + y] == TileType.Open || m_dungeonMap[givenX + x, givenY + y] == TileType.Pit)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void ConnectRooms()
    {
        int nextRoomIndex;
        foreach (Room room in m_rooms)
        {
            nextRoomIndex = m_rooms.IndexOf(room) + 1;

            Room nextRoom;
            Vector2 startPos;
            Vector2 targetPos;

            if (nextRoomIndex >= m_rooms.Count)
            {
                startPos = FindClosestTileToRoom(room, pit);
                targetPos = FindClosestTileToRoom(pit, room);

                foreach (Vector2 tile in Pathfinding.Instance.FindPath(startPos, targetPos, true, true))
                {
                    m_dungeonMap[(int)(tile.x), (int)(tile.y)] = TileType.Open; //update dungeon map
                }

                return;
            }

            nextRoom = m_rooms[nextRoomIndex];

            //find closest point to connect each room together
            //so first do current room
            startPos = FindClosestTileToRoom(room, nextRoom);
            targetPos = FindClosestTileToRoom(nextRoom, room);

            foreach (Vector2 tile in Pathfinding.Instance.FindPath(startPos, targetPos, true))
            {
                m_dungeonMap[(int)(tile.x), (int)(tile.y)] = TileType.Open; //update dungeon map
            }
        }
    }

    private Vector2 FindClosestTileToRoom (Room startRoom, Room endRoom)
    {
        Vector2 centre = new Vector2();
        Vector2 closestTile = new Vector2();

        foreach (Vector2 tile in endRoom.RoomTiles)
        {
            centre += tile;
        }

        centre /= endRoom.RoomTiles.Count;

        float closestDistance = float.MaxValue;

        foreach (Vector2 tile in startRoom.RoomTiles)
        {
            if (Vector2.Distance(tile, centre) < closestDistance)
            {
                closestDistance = Vector2.Distance(tile, centre);
                closestTile = tile;
            }
        }

        return closestTile;
    }

    private void GenerateRooms ()
    {
        m_numberOfRooms = Random.Range(m_minNumberOfRooms, m_maxNumberOfRooms);
        m_rooms = new List<Room>();
        Vector2 position = new Vector2();

        for (int i = 0; i < m_numberOfRooms; i++)
        {
            Room room = new Room();

            Vector2 roomCentre = new Vector2(Random.Range(0, m_width), Random.Range(0, m_height));

            Vector2 roomSize = new Vector2(Random.Range(m_minRoomSize, m_maxRoomSize), Random.Range(m_minRoomSize, m_maxRoomSize));

            for (int x = 0; x < roomSize.x; x++)
            {
                for (int y = 0; y < roomSize.y; y++)
                {
                    position = new Vector2(x + (int)roomCentre.x, y + (int)roomCentre.y);
                    if (IsWithinMapBounds(position) && m_dungeonMap[(int)position.x, (int)position.y] != TileType.Edge && m_dungeonMap[(int)position.x, (int)position.y] != TileType.Pit)
                    {
                        m_dungeonMap[(int)position.x, (int)position.y] = TileType.Open;
                        room.RoomTiles.Add(position);
                        //could check here for hole - e.g. set hole tiles to 3 etc
                    }
                    if (IsWithinMapBounds(position) && m_dungeonMap[(int)position.x, (int)position.y] == TileType.Pit)
                    {
                        m_rooms.Remove(room);
                        break;
                    }
                }
            }
            m_rooms.Add(room);
        }
    }

    public bool IsWithinMapBounds (Vector2 givenPosition)
    {
        if (givenPosition.x < 0 || givenPosition.x >= m_dungeonMap.GetLength(0) || givenPosition.y < 0 || givenPosition.y >= m_dungeonMap.GetLength(1))
        {
            return false;
        }
        return true;
    }

    private List<Vector2> SetStartAndEndPositions ()
    {
        float furthestDistance = 0f;

        Room startRoom = m_rooms[0];

        Room endRoom = new Room();

        foreach (Room room in m_rooms)
        {
            if (room != startRoom)
            {
                if (room.RoomTiles != null && room.RoomTiles.Count > 0 &&
                    startRoom.RoomTiles != null && startRoom.RoomTiles.Count > 0)
                {



                    if (Vector2.Distance(room.RoomTiles[0], startRoom.RoomTiles[0]) > furthestDistance)
                    {
                        furthestDistance = Vector2.Distance(room.RoomTiles[0], startRoom.RoomTiles[0]);
                        endRoom = room;
                    }
                }
            }
        }
        Vector2 startPosition = startRoom.RoomTiles[Random.Range(0, startRoom.RoomTiles.Count)];
        int i = 0;

        while (m_dungeonMap[(int)startPosition.x, (int)startPosition.y] != TileType.Open)
        {
            i++;
            if (i > startRoom.RoomTiles.Count - 1)
            {
                break;
            }

            startPosition = startRoom.RoomTiles[Random.Range(0, startRoom.RoomTiles.Count)];
        }

        Vector2 endPosition = endRoom.RoomTiles[Random.Range(0, endRoom.RoomTiles.Count)];


        i = 0;

        while (m_dungeonMap[(int)endPosition.x, (int)endPosition.y] != TileType.Open)
        {
            i++;
            if (i > endRoom.RoomTiles.Count - 1)
            {
                break;
            }

            endPosition = endRoom.RoomTiles[Random.Range(0, endRoom.RoomTiles.Count)];
        }


        List<Vector2> positions = new List<Vector2>();
        positions.Add(startPosition);
        positions.Add(endPosition);
        m_dungeonMap[(int)startPosition.x, (int)startPosition.y] = TileType.Start;
        m_dungeonMap[(int)endPosition.x, (int)endPosition.y] = TileType.End;
        SpawnPlayer(startPosition*m_tileSize);
        return positions;
    }

}