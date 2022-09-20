using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The gameboard is made out of these tiles. The tiles are square tiles with at maximum of 8 neighbors.
/// Each tile has fallowing information:
///     1) What the game manager is.
///     2) What is the tile's coordinate.
///     3) What are the tile's neighbors or 'edges'.
///     4) What is the current token on the tile.
///     5) Is the tile walkable.
///     6) Can the tile be attacked over.
/// Tiles also have a highlight game object, that can be turned on and off as wanted.
/// </summary>
public class Tile : MonoBehaviour
{
    private GameManager gameManager;

    public GameObject highlight;
    public int xCoord, zCoord;
    public List<Tile> edges;
    public GameObject currentObject;
    public bool isWalkable;
    public bool canBeAttackedOver;

    private void Start()
    {
        //gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager = GetComponentInParent<GameManager>();
    }


    /// <summary>
    /// Tile constructor for blank tile.
    /// </summary>
    public Tile()
    {
        isWalkable = true;
        canBeAttackedOver = true;
    }


    /// <summary>
    /// Tile constructor.
    /// </summary>
    /// <param name="walkable">Players can move here</param>
    /// <param name="attackedOver">Tile blocks line of sight</param>
    public Tile(bool walkable, bool attackedOver)
    {
        isWalkable = walkable;
        canBeAttackedOver = attackedOver;
    }


    /// <summary>
    /// When the tile was clicked then either move the player or do selected action
    /// </summary>
    public void OnMouseUp()
    {
        if (gameManager.selectedAction == null)
        {
            gameManager.MovePlayer(this.gameObject);
        }
        else
        {
            gameManager.DoSelectedAction(gameManager.currentPlayer.GetGameObject().GetComponentInParent<Tile>(), this);
        }
    }


    /// <summary>
    /// Change tile's type. Used in map creation
    /// </summary>
    /// <param name="tileSO">Tile Scriptable object</param>
    public void ChangeTileType(TileScriptableObject tileSO)
    {
        isWalkable = tileSO.isWalkable;
        canBeAttackedOver = tileSO.canBeAttackedOver;
        GetComponent<MeshRenderer>().material = tileSO.tileMaterial;
    }


    /// <summary>
    /// Return this tile's gameboard;
    /// </summary>
    /// <returns>Tile's gameboard</returns>
    public GameObject[,] GetGameboardOfTile()
    {
        return GetComponentInParent<Gameboard>().map;
    }


    /// <summary>
    /// Create new list of edges.
    /// </summary>
    public void ResetEdges()
    {
        edges = new List<Tile>();
    }
}
