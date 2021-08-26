using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementArea
{
    public Dictionary<Tile, int> GenerateMovementArea(Tile origin, int maxDistance)
    {
        Dictionary<Tile, int> possibleTiles = new Dictionary<Tile, int>();

        Queue<Tile> frontier = new Queue<Tile>();
        frontier.Enqueue(origin);

        Dictionary<Tile, int> distance = new Dictionary<Tile, int>();
        distance.Add(origin, 0);

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            //Debug.Log("Visiting " + current.name + "; Lvl: " + level);

            foreach (Tile next in current.edges)
            {
                if (!distance.ContainsKey(next) && next.currentObject == null)
                {
                    frontier.Enqueue(next);
                    distance.Add(next, 1 + distance[current]);
                }
            }
        }

        foreach(Tile t in distance.Keys)
        {
            //Debug.Log(t + " " + distance[t]);
            //t.highlight.GetComponent<MeshRenderer>().material.color = new Color(0.1f * distance[t], 0.2f * distance[t], 0.05f / distance[t]);

            if (distance[t] <= maxDistance)
                possibleTiles.Add(t, distance[t]);
        }

        return possibleTiles;
    }
}
