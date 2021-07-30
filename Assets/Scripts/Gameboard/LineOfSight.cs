using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSight
{
    /// <summary>
    /// Generates line of sight from a to b.
    /// </summary>
    /// <param name="map">Map where the LoS is generated</param>
    /// <param name="origin">Observer's point</param>
    /// <param name="destination">Target's point</param>
    /// <returns>List of all tiles that belong to the ray</returns>
    public List<Tile> GenerateLineOfSight(GameObject[,] map, Tile origin, Tile destination)
    {

        List<float[]> points = new List<float[]>();
        int diagonalDistance = DiagonalDistance(origin, destination);

        for (int i = 0; i <= diagonalDistance; i++) // Get the points where the line is
        {
            if (diagonalDistance > 0)
            {
                float p = i / (float)diagonalDistance;
                points.Add(LerpPoints(origin, destination, p));
            }
        }

        List<Tile> los = new List<Tile> { origin }; // Get the tiles from the map

        for (int i = 0; i < points.Count; i++) // 
        {
            int x0 = Mathf.RoundToInt(points[i][0]);
            int z0 = Mathf.RoundToInt(points[i][1]);

            Tile tile0 = map[x0, z0].GetComponent<Tile>();
            Tile tile1 = null;

            try
            {
                int x1 = Mathf.RoundToInt(points[i + 1][0]);
                int z1 = Mathf.RoundToInt(points[i + 1][1]);
                tile1 = map[x1, z1].GetComponent<Tile>();
            }
            catch { }

            if (tile0.canBeAttackedOver && !IsSurrounded(map, tile0, tile1))
            {
                los.Add(tile0);
            }
            else
            {
                break;
            }
        }

        return los;
    }


    /// <summary>
    /// Helper function. Calculates dx, dz
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="destination">Destination tile</param>
    /// <returns>Diagonal distance</returns>
    private int DiagonalDistance(Tile origin, Tile destination)
    {
        int dx = Mathf.Abs(destination.xCoord - origin.xCoord);
        int dz = Mathf.Abs(destination.zCoord - origin.zCoord);
        //Debug.Log("Diagonal distance is " + dx + " " + dz);

        return Mathf.Max(dx, dz);
    }


    /// <summary>
    /// Helper function. Calculates coordinate of the tile.
    /// </summary>
    /// <param name="a">Origin tile</param>
    /// <param name="b">Destination tile</param>
    /// <param name="t">Ratio of the distance</param>
    /// <returns>Coordinate as float[]</returns>
    private float[] LerpPoints(Tile a, Tile b, float t)
    {
        float[] point = new float[2];

        point[0] = Mathf.Lerp((float)a.xCoord + 0.01f, (float)b.xCoord + 0.01f, t); // Adding token 0.01f to make better lines
        point[1] = Mathf.Lerp((float)a.zCoord + 0.01f, (float)b.zCoord + 0.01f, t);

        return point;
    }


    /// <summary>
    /// Check if LoS can make diagonal step. Returns true if there's something blocking it or there's no next tile.
    /// </summary>
    /// <param name="map">Game map where LoS takes place</param>
    /// <param name="firstTile">First tile</param>
    /// <param name="nextTile">Following tile</param>
    /// <returns>If there's something blocking the LoS</returns>
    private bool IsSurrounded(GameObject[,] map, Tile firstTile, Tile nextTile)
    {
        if (nextTile == null) return false;

        int dx = firstTile.xCoord - nextTile.xCoord;
        int dz = firstTile.zCoord - nextTile.zCoord;

        try
        {
            if (AttackOverCheck(map, firstTile, dx, 0 ) || 
                AttackOverCheck(map, firstTile,  0, dz))
                    return false;
        }
        catch
        {
            return false; // Tile was out of index
        }

        return true;
    }


    /// <summary>
    /// Private function just to shorten code.
    /// </summary>
    /// <param name="map">Map where check takes place</param>
    /// <param name="tile">Origin tile</param>
    /// <param name="dx">Difference x</param>
    /// <param name="dz">Difference z</param>
    /// <returns>Can be attack over from certain side</returns>
    private bool AttackOverCheck(GameObject[,] map, Tile tile, int dx, int dz)
    {
        return map[tile.xCoord + dx, tile.zCoord + dz].GetComponent<Tile>().canBeAttackedOver;
    }
}
