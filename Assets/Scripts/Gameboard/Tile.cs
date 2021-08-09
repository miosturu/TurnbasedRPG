using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
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
            gameManager.MovePlayer(this.gameObject);
        else
            gameManager.selectedAction.Action(this);
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
}
