using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The whole game is played on 6x9 gameboard.
/// Each map is made out of 3x3 tile regions when randomly generated but one can choose to make their own.
/// This class allows resetting the gameboard.
/// </summary>
public class Gameboard : MonoBehaviour
{
    [Header("Map visual options")]
    public TileScriptableObject[] availableTiles; // Stores the tile prefabs that are used to generate the level.

    [Header("Map generation options")]

    public int mapW = 6, mapH = 9;
    [SerializeField] private GameObject[] tiles; // What tiles are available to be placed // TODO maybe change to just GameObject prefab
    public GameObject[,] map;  // What tiles are on the gameboard
    public Dictionary<Tile, List<Tile>> graph; // Stores all the connections of the gameboard

    private readonly int[,] directions = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 }, { -1, 1 }, { 1, 1 }, { 1, -1 }, { -1, -1 } }; // Used to generate the graph. This is looped to check adjacent tiles.

    private List<float> tileTypeMap; // Used for AI. It requires float array if several elements are wanted. Uses enum's help.

    // The tile regions' tiles
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

    private List<List<TileRegionScriptableObject>> metaList; // List of all the tile lists.

    // Start is called before the first frame update
    void Start()
    {
        metaList = new List<List<TileRegionScriptableObject>> { tilesA, tilesB, tilesC, tilesD, tilesE, tilesF };
        map = new GameObject[mapW, mapH];
        tileTypeMap = new List<float>(); // Used for AI
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
    }


    /// <summary>
    /// Select what kind of level is going to be generated.
    /// </summary>
    /// <param name="mapType">Map type</param>
    public void GenerateLevel(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.Random:
                GenerateRandomLevel();
                break;
            default:
                GenerateBlankLevel();
                break;
        }
    }


    /// <summary>
    /// Generate map from 2D string array. The map will always be of size 6x9. 
    ///     '#' will be inpenetrable wall.
    ///     '_' will be regular tile.
    ///     '@' will be low obstacle.
    /// </summary>
    /// <param name="symbolicMap"></param>
    private void GeneratePremadeLevel(string[,] symbolicMap)
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
        GenerateGraph();
    }


    /// <summary>
    /// Generate level where everything is made out of ground tiles.
    /// </summary>
    private void GenerateBlankLevel()
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
        GenerateGraph();
    }


    /// <summary>
    /// Generate random level from "TileRegionSO"s.
    /// Gameboard is assumed to be 6x9 made out of 3x3 chunks.
    /// The generation begins from bottom left to top right.
    /// This method goes each region one by one and creates new tiles.
    /// This method should be called only once at the begining.
    /// </summary>
    private void GenerateRandomLevel()
    {
        Queue<TileRegionScriptableObject> tileRegions = SelectRandomTileRegions(); // Get the tiles that will be placed on the gameboard

        TileRegionScriptableObject current;

        float offSetX = 0; // Off sets for generating map one region at the time
        float offSetZ = 0;

        while(tileRegions.Count > 0) // Go through every region 
        {
            current = tileRegions.Dequeue();
            // Debug.Log(current.name);
            // Logic of the map generations goes here

            int index = 0; // Used for going over single region at the time

            for (int i = 0; i < current.regionH; i++) // Looping through region
            {
                for (int j = 0; j < current.regionW; j++)
                {
                    GameObject tileObject = null;
                    TileScriptableObject tileSO = null;

                    TileType tileType = current.tiles[index];

                    switch (tileType) // Get the current tile type
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

                    tileTypeMap.Add((float)tileType); // Create tile type array that will be used for AI

                    index++;

                    // Create new tile, change it's type and name it
                    tileObject = Instantiate(tiles[0]);
                    tileObject.GetComponent<Tile>().ChangeTileType(tileSO);
                    tileObject.name = (j + offSetX) + ", " + (i + offSetZ);

                    map[j + (int)offSetX, i + (int)offSetZ] = tileObject; // In the map array, set the newly created tile

                    // Set tile's coordinate
                    Tile tile = tileObject.GetComponent<Tile>();
                    tile.xCoord = (j + (int)offSetX);
                    tile.zCoord = (i + (int)offSetZ);

                    // Place the tile in the game world to it's correct position
                    tileObject.transform.position = new Vector3((j + (int)offSetX), 0.0f, (i + (int)offSetZ));
                    tileObject.transform.parent = transform;
                }
            }

            // Used for looping through the tile regions
            // It asumes that the world is 6x9 thus when horizontal off set is not 0, then new go up one region and start over
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

        // Generates abstract graph for pathfinding
        GenerateGraph();
    }


    /// <summary>
    /// Generate graph for pathfinding.
    /// This works by going through all the tiles and checking if the neighbor is walkable.
    /// </summary>
    private void GenerateGraph()
    {
        graph.Clear(); // If the map is reset, then we have to clear the original graph.

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


    /// <summary>
    /// Select random tiles for each tile region.
    /// Each map is made out of 3x3 tile regions. This was made bacause the coder or desinger then can create pre made areas.
    /// This solution also guarantees walkable path from team 0 to team 1 and vice versa IF the tile regions are made that way.
    /// One should be mindfull of this when creating different tile regions.
    /// </summary>
    /// <returns>Queue if tile regions from A to F</returns>
    private Queue<TileRegionScriptableObject> SelectRandomTileRegions()
    {
        Queue<TileRegionScriptableObject> tileRegions = new Queue<TileRegionScriptableObject>();

        int listIndex = 0;
        foreach (List<TileRegionScriptableObject> list in metaList) // Select random tile for regions.
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

        return tileRegions;
    }


    /// <summary>
    /// Reset the previous map and generate new one randomly. 
    /// No new gameobjects area created, rather pre existing tiles are changed to new ones.
    /// </summary>
    public void ResetGameBoard()
    {
        foreach(GameObject tile in map)
        {
            tile.GetComponent<Tile>().ResetEdges();
        }

        Queue<TileRegionScriptableObject> regions = SelectRandomTileRegions();

        TileRegionScriptableObject current;

        int offSetX = 0; // Off sets for generating map one region at the time
        int offSetZ = 0;

        while (regions.Count > 0)
        {
            current = regions.Dequeue();

            int index = 0; // For looping the tile region

            for (int i = 0; i < current.regionH; i++)
            {
                for (int j = 0; j < current.regionW; j++)
                {
                    TileScriptableObject tileSO = null;
                    TileType tileType = current.tiles[index];

                    switch (tileType) // Get the current tile type
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

                    tileTypeMap.Add((float)tileType); // Create tile type array that will be used for AI
                    map[j + offSetX, i + offSetZ].GetComponent<Tile>().ChangeTileType(tileSO);
                    index++;
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

        GenerateGraph();
    }


    /// <summary>
    /// Return the map as 2D array of tile types. 
    /// This method is planned to be used for AI as a part of a vector of observations.
    /// </summary>
    /// <returns>List of tiles as tile type</returns>
    public List<float> GetTileTypeMap()
    {
        return tileTypeMap;
    }
}
