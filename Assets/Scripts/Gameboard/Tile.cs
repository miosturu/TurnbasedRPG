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

    public Tile()
    {
        isWalkable = true;
        canBeAttackedOver = true;
    }


    public Tile(bool walkable, bool attackedOver)
    {
        isWalkable = walkable;
        canBeAttackedOver = attackedOver;
    }


    /*public void OnMouseEnter()
    {
        highlight.SetActive(true);
    }


    public void OnMouseExit()
    {
        highlight.SetActive(false);
    }*/


    public void OnMouseUp()
    {
        gameManager.MovePlayer(this.gameObject);
    }


    public void ChangeTileType(TileScriptableObject tileSO)
    {
        isWalkable = tileSO.isWalkable;
        canBeAttackedOver = tileSO.canBeAttackedOver;
        GetComponent<MeshRenderer>().material = tileSO.tileMaterial;
    }
}
