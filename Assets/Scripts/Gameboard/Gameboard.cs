using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gameboard : MonoBehaviour
{
    [Header("Map visual options")]
    public TileScriptableObject[] availableTiles;

    [Header("Map generation options")]

    public int mapW = 6, mapH = 9;
    [SerializeField] private GameObject[] tiles; // What tiles are available to be placed // TODO maybe change to just GameObject prefab
    public GameObject[,] map;  // What tiles are on the gameboard
    public Dictionary<Tile, List<Tile>> graph;

    private readonly int[,] directions = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 }, { -1, 1 }, { 1, 1 }, { 1, -1 }, { -1, -1 } };

    [Header("A")]
    [SerializeField] private List<TileRegionScriptableObject> tilesA = new List<TileRegionScriptableObject>();

    [Header("B")]
    [SerializeField] private List<TileRegionScriptableObject> tilesB = new List<TileRegionScriptableObject>();

    [Header("C")]
    [SerializeField] private List<TileRegionScriptableObject> tilesC = new List<TileRegionScriptableObject>();

    [Header("D")]
    [SerializeField] private List<TileRegionScriptableObject> tilesD = new List<TileRegionScriptableObject>();

    [Header("E")]
    [SerializeField] private List<TileRegionScriptableObject> tilesE = new List<TileRegionScriptableObject>();

    [Header("F")]
    [SerializeField] private List<TileRegionScriptableObject> tilesF = new List<TileRegionScriptableObject>();

    [Header("Universal")]
    [SerializeField] private List<TileRegionScriptableObject> tilesUniversal = new List<TileRegionScriptableObject>();

    private List<List<TileRegionScriptableObject>> metaList;

    // Start is called before the first frame update
    void Start()
    {
        metaList = new List<List<TileRegionScriptableObject>> { tilesA, tilesB, tilesC, tilesD, tilesE, tilesF };
        map = new GameObject[mapW, mapH];
        graph = new Dictionary<Tile, List<Tile>>();

        string[,] tiles = { { "_", "_", "_", "#", "#", "#" },
                            { "_", "_", "_", "_", "_", "_" },
                            { "_", "_", "_", "#", "_", "#" },
                            { "_", "_", "_", "#", "_", "#" },
                            { "#", "#", "#", "#", "_", "#" },
                            { "_", "_", "_", "_", "_", "#" },
                            { "_", "_", "_", "#", "@", "#" },
                            { "_", "_", "_", "_", "_", "_" },
                            { "_", "_", "_", "_", "_", "_" } };

        string[,] tiles2 = { { "_", "_", "#", "_", "_", "_" },
                             { "_", "_", "_", "_", "_", "_" },
                             { "#", "#", "_", "_", "_", "_" },
                             { "_", "_", "_", "_", "_", "_" },
                             { "_", "_", "_", "_", "_", "_" },
                             { "_", "_", "_", "_", "_", "_" },
                             { "_", "_", "_", "_", "_", "_" },
                             { "_", "_", "_", "_", "_", "_" },
                             { "_", "_", "_", "_", "_", "_" } };

        GenerateLevel();
        //GenerateLevel(0);
    }

    /// <summary>
    /// Generate map from 2D string array. The map will always be of size 6x9. 
    ///     '#' will be inpenetrable wall.
    ///     '_' will be regular tile.
    ///     '@' will be low obstacle.
    /// </summary>
    /// <param name="symbolicMap"></param>
    public void GenerateLevel(string[,] symbolicMap)
    {
        float xPos = 0;
        float zPos = 0;

        for (int i = 0; i < mapH; i++)
        {
            for (int j = 0; j < mapW; j++)
            {
                GameObject tileObject = null;
                TileScriptableObject tileSO = null;

                switch (symbolicMap[i, j])
                {
                    case "_":
                        tileSO = availableTiles[0];
                        break;
                    case "#":
                        tileSO = availableTiles[1];
                        break;
                    case "@":
                        tileSO = availableTiles[2];
                        break;
                    default:
                        Debug.Log("Error: unknown tile type");
                        break;
                }

                tileObject = Instantiate(tiles[0]);

                tileObject.GetComponent<Tile>().ChangeTileType(tileSO);

                //GameObject tileObject = Instantiate(tiles[0]);
                tileObject.name = j + ", " + i;

                map[j, i] = tileObject;
                Tile tile = tileObject.GetComponent<Tile>();

                tile.xCoord = (int)xPos;
                tile.zCoord = (int)zPos;

                tileObject.transform.position = new Vector3(xPos, 0.0f, zPos);
                tileObject.transform.parent = transform;

                xPos += 1;
            }
            zPos += 1;
            xPos = 0;
        }

        // Generates abstract graph for pathfinding
        for (int i = 0; i < mapH; i++) // Z
        {
            for (int j = 0; j < mapW; j++) // X
            {
                Tile node = map[j, i].GetComponent<Tile>();
                List<Tile> neighbors = new List<Tile>();
                int nodeX = node.xCoord;
                int nodeZ = node.zCoord;

                for (int k = 0; k < directions.Length; k++)
                {
                    try
                    {
                        Tile potentialNeigbor = map[nodeX + directions[k, 0],
                                                    nodeZ + directions[k, 1]].GetComponent<Tile>();

                        if (potentialNeigbor.isWalkable)
                        {
                            node.edges.Add(potentialNeigbor);
                            neighbors.Add(potentialNeigbor);
                        }
                    }
                    catch { /* No edge in that direction*/ }
                }
                graph.Add(node, neighbors);
            }
        }
    }


    public void GenerateLevel() // TODO somekind of random level generation like in Splunky OR read from text OR TBoI-style
    {
        float xPos = 0;
        float zPos = 0;

        for (int i = 0; i < mapH; i++)
        {
            for (int j = 0; j < mapW; j++)
            {
                GameObject tileObject = Instantiate(tiles[0]);
                tileObject.name = j + ", " + i;

                map[j, i] = tileObject;
                Tile tile = tileObject.GetComponent<Tile>();

                tile.xCoord = (int) xPos;
                tile.zCoord = (int) zPos;

                tileObject.transform.position = new Vector3(xPos, 0.0f, zPos);
                tileObject.transform.parent = transform;

                xPos += 1;
            }
            zPos += 1;
            xPos = 0;
        }

        // Generates abstract graph for pathfinding
        for (int i = 0; i < mapH; i++) // Z
        {
            for (int j = 0; j < mapW; j++) // X
            {
                Tile node = map[j, i].GetComponent<Tile>();
                List<Tile> neighbors = new List<Tile>();
                int nodeX = node.xCoord;
                int nodeZ = node.zCoord;

                for (int k = 0; k < directions.Length; k++)
                {
                    try
                    {
                        Tile potentialNeigbor = map[nodeX + directions[k, 0],
                                                    nodeZ + directions[k, 1]].GetComponent<Tile>();

                        if (potentialNeigbor.isWalkable)
                        {
                            node.edges.Add(potentialNeigbor);
                            neighbors.Add(potentialNeigbor);
                        }
                    }
                    catch { /* No edge in that direction*/ }
                }
                graph.Add(node, neighbors);
            }
        }
    }


    public void GenerateLevel(int num)
    {
        Queue<TileRegionScriptableObject> tileRegions = new Queue<TileRegionScriptableObject>();

        int listIndex = 0;
        foreach(List<TileRegionScriptableObject> list in metaList) // Select random tile regions.
        {
            try
            {
                tileRegions.Enqueue(metaList[listIndex][Random.Range(0, metaList[listIndex].Count)]);
            }
            catch // If there's no available tile regions, then take generic tile
            {
                tileRegions.Enqueue(tilesUniversal[Random.Range(0, tilesUniversal.Count)]);
            }
            listIndex++;
        }

        TileRegionScriptableObject current;

        float offSetX = 0;
        float offSetZ = 0;

        while(tileRegions.Count > 0)
        {
            current = tileRegions.Dequeue();
            Debug.Log(current.name);
            // Logic of the map generations goes here

            int index = 0;

            for (int x = 0; x < current.regionW; x++)
            {
                for (int z = 0; z < current.regionH; z++)
                {
                    GameObject tileObject = null;
                    TileScriptableObject tileSO = null;

                    TileType tileType = current.tiles[index];

                    switch (tileType)
                    {
                        case TileType.Walkable:
                            tileSO = availableTiles[0];
                            break;
                        case TileType.Wall:
                            tileSO = availableTiles[1];
                            break;
                        case TileType.Halfhight:
                            tileSO = availableTiles[2];
                            break;
                        default:
                            Debug.Log("Error: unknown tile type");
                            break;
                    }

                    index++;

                    tileObject = Instantiate(tiles[0]);
                    tileObject.GetComponent<Tile>().ChangeTileType(tileSO);

                    tileObject.name = x + ", " + z;
                    map[x, z] = tileObject;

                    Tile tile = tileObject.GetComponent<Tile>();
                    tile.xCoord = (x + (int)offSetX);
                    tile.zCoord = (z + (int)offSetZ);

                    tileObject.transform.position = new Vector3((x + (int)offSetX), 0.0f, (z + (int)offSetZ));
                    tileObject.transform.parent = transform;
                }
            }

            if (offSetX == 0)
            {
                offSetX += current.regionW;
            }
            else
            {
                offSetZ += current.regionH;
                offSetX = 0;
            }
        }


        for (int x = 0; x < mapW; x++)
        {
            for (int z = 0; z < mapH; z++)
            {
                Tile node = map[x, z].GetComponent<Tile>();
                List<Tile> neighbors = new List<Tile>();
                int nodeX = node.xCoord;
                int nodeZ = node.zCoord;

                for (int k = 0; k < directions.Length; k++)
                {
                    try
                    {
                        Tile potentialNeigbor = map[nodeX + directions[k, 0],
                                                    nodeZ + directions[k, 1]].GetComponent<Tile>();

                        if (potentialNeigbor.isWalkable)
                        {
                            node.edges.Add(potentialNeigbor);
                            neighbors.Add(potentialNeigbor);
                        }
                    }
                    catch { /* No edge in that direction*/ }
                }
                graph.Add(node, neighbors);
            }
        }
    }
}
