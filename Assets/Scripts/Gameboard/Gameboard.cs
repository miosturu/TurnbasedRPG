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
    public GameObject[] tiles; // What tiles are available to be placed // TODO maybe change to just GameObject prefab
    public GameObject[,] map;  // What tiles are on the gameboard
    public Dictionary<Tile, List<Tile>> graph;

    private readonly int[,] directions = new int[,] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 }, { -1, 1 }, { 1, 1 }, { 1, -1 }, { -1, -1 } };

    // Start is called before the first frame update
    void Start()
    {
        map = new GameObject[mapW, mapH];
        graph = new Dictionary<Tile, List<Tile>>();
        //GenerateBlankLevel();

        string[,] tiles = { { "_", "_", "#", "#", "#", "#" },
                            { "_", "_", "_", "_", "_", "_" },
                            { "#", "#", "_", "#", "_", "#" },
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

        //GeneratePremadeLevel(tiles2);
        GenerateBlankLevel();
    }

    /// <summary>
    /// Generate map from 2D string array. The map will always be of size 6x9. 
    ///     '#' will be inpenetrable wall.
    ///     '_' will be regular tile.
    ///     '@' will be low obstacle.
    /// </summary>
    /// <param name="symbolicMap"></param>
    public void GeneratePremadeLevel(string[,] symbolicMap) // TODO
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


    public void GenerateBlankLevel() // TODO somekind of random level generation like in Splunky OR read from text OR TBoI-style
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
}
