using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    public static Pathfinding Instance;

    private int[,] m_visitedTiles;
    private List<TileInformation> m_openTiles;
    private bool m_unableToReachTile;

    [SerializeField]
    private TileInformation m_startTile;
    [SerializeField]
    private TileInformation m_endTile;

    private float m_cheapestTileCost;

    private TileInformation m_currentTile;

    [System.Serializable]
    private class TileInformation
    {
        public List<TileInformation> FindTilePath(List<TileInformation> givenPath)
        {
            if (m_previousTile == null)
            {
                //print("Found start tile at " + m_tilePosition);
                return givenPath;
            }
            else
            {
                givenPath.Add(this);
                m_previousTile.FindTilePath(givenPath);
                return givenPath;
            }
        }

        public Vector2 m_tilePosition;
        public float m_pathCost; //running cost of all tiles 
        public float m_tileDistance; //heuristic, straight line distance to end vector

        public TileInformation m_previousTile;
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

    //put in coroutine!
    public List<Vector2> FindPath(Vector2 givenStartVector, Vector2 givenEndVector, bool cutPath = false, bool brutalCut = false)
    {
        m_unableToReachTile = false;

        m_visitedTiles = new int[DungeonGeneration.Instance.m_dungeonMap.GetLength(0), DungeonGeneration.Instance.m_dungeonMap.GetLength(1)];

        m_openTiles = new List<TileInformation>();

        m_startTile = new TileInformation();
        m_endTile = new TileInformation();

        m_startTile.m_tilePosition = givenStartVector;
        m_endTile.m_tilePosition = givenEndVector;

        m_currentTile = m_startTile;

        m_visitedTiles[(int)m_currentTile.m_tilePosition.x, (int)m_currentTile.m_tilePosition.y] = 1;

        while (m_currentTile.m_tilePosition != m_endTile.m_tilePosition)
        {
            m_cheapestTileCost = float.MaxValue;

            foreach (TileInformation tileInformation in FindAdjacentTiles(m_currentTile, m_endTile, DungeonGeneration.Instance.m_dungeonMap, cutPath, brutalCut))
            {
                bool tileAlreadyExists = false;

                for (int i = 0; i < m_openTiles.Count; i++)
                {
                    //if the tile already exists
                    if (m_openTiles[i].m_tilePosition == tileInformation.m_tilePosition)
                    {
                        m_openTiles[i].m_pathCost = tileInformation.m_pathCost;
                        tileAlreadyExists = true;
                    }
                }

                if (!tileAlreadyExists)
                {
                    m_openTiles.Insert(0, tileInformation); //put at front of list, as recently looked at adjacent tiles are likely to be most efficient to be looked at next
                }
            }

            foreach (TileInformation tileInformation in m_openTiles)
            {
                if ((tileInformation.m_pathCost + (int)tileInformation.m_tileDistance) < m_cheapestTileCost)
                {
                    m_cheapestTileCost = tileInformation.m_pathCost + (int)tileInformation.m_tileDistance;
                    m_currentTile = tileInformation;
                }
            }
            m_visitedTiles[(int)m_currentTile.m_tilePosition.x, (int)m_currentTile.m_tilePosition.y] = 1;
            m_openTiles.Remove(m_currentTile);

            if (m_openTiles.Count == 0) //explored all tiles, means we've tried to get to a blocked area
            {
                m_unableToReachTile = true;
                break;
            }
        }

        List<Vector2> m_calculatedPath = new List<Vector2>();

        if (m_unableToReachTile)
        {
            print("can't reach tile");
        }

        else
        {
            foreach (TileInformation tileDetails in m_currentTile.FindTilePath(new List<TileInformation>()))
            {
                m_calculatedPath.Add(tileDetails.m_tilePosition);
            }
        }

        if (cutPath)
        {
            return WidenPath(m_calculatedPath);
        }
        else
        {
            return (m_calculatedPath);
        }
    }

    private List<Vector2> WidenPath (List<Vector2> givenPath)
    {
        List<Vector2> widenedPath = new List<Vector2>(givenPath);

        Vector2 tile;

        foreach (Vector2 currentTile in givenPath) {

            for (int x = 0; x < DungeonGeneration.Instance.m_pathWidth; x++)
            {
                for (int y = 0; y < DungeonGeneration.Instance.m_pathWidth; y++)
                {
                    tile = new Vector2(currentTile.x+x, currentTile.y+y);

                    if (!widenedPath.Contains(tile) && DungeonGeneration.Instance.IsWithinMapBounds(tile/DungeonGeneration.Instance.m_tileSize))
                    {
                        widenedPath.Add(tile);
                    }
                }
            }
        }

        return widenedPath;
    }

    private List<TileInformation> FindAdjacentTiles(TileInformation givenCurrentTile, TileInformation givenTargetTile, DungeonGeneration.TileType[,] givenTileArray, bool cutPath, bool brutalCut)
    {
        List<TileInformation> m_adjacentTiles = new List<TileInformation>();
        TileInformation m_adjacentTile;

        for (int x = -1; x < 2; x++) //check a 3x3 grid around the givenCurrentTile
        {
            for (int y = -1; y < 2; y++)
            {
                //ignore corners
                if (cutPath && (x == -1 && y == 1 || x == -1 && y == -1 || x == 1 && y == 1 || x == 1 && y == -1))
                {
                    //so so hacky
                }
                else
                { 
                m_adjacentTile = new TileInformation();

                m_adjacentTile.m_tilePosition.x = (int)(x + givenCurrentTile.m_tilePosition.x);
                m_adjacentTile.m_tilePosition.y = (int)(y + givenCurrentTile.m_tilePosition.y);

                    if (DungeonGeneration.Instance.IsWithinMapBounds(m_adjacentTile.m_tilePosition) && m_visitedTiles[(int)m_adjacentTile.m_tilePosition.x, (int)m_adjacentTile.m_tilePosition.y] == 0) //if tile is not already visited
                    {
                        DungeonGeneration.TileType tileType = new DungeonGeneration.TileType();
           
                        tileType = givenTileArray[(int)m_adjacentTile.m_tilePosition.x, (int)m_adjacentTile.m_tilePosition.y];
                        
                        if (cutPath)
                        {
                            if (tileType != DungeonGeneration.TileType.Pit && tileType != DungeonGeneration.TileType.Edge)
                            {
                                tileType = DungeonGeneration.TileType.Open; //if we are cutting new paths, allow all movement as long as it's not a pit or edge
                            }
                            if (brutalCut)
                            {
                                tileType = DungeonGeneration.TileType.Open; //if we are cutting new paths, allow all movement as long as it's not a pit or edge
                            }
                        }

                        if (tileType == DungeonGeneration.TileType.Open && m_adjacentTile.m_tilePosition != givenCurrentTile.m_tilePosition)
                        {
                            m_adjacentTile.m_tileDistance = Vector2.Distance(m_adjacentTile.m_tilePosition, givenTargetTile.m_tilePosition) * DungeonGeneration.Instance.m_tileSize;

                            m_adjacentTile.m_previousTile = givenCurrentTile;

                            m_adjacentTile.m_pathCost += givenCurrentTile.m_pathCost;
                            m_adjacentTiles.Add(m_adjacentTile);
                        }
                    }
                }

            }
        }
        return m_adjacentTiles;
    }

}