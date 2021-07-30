using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadthFirstSearch
{

    /// <summary>
    /// Pathfinding with Breath first search. At the moment, there's not early exit or maximum distance from origin.
    /// </summary>
    /// <param name="origin">Origin tile</param>
    /// <param name="destination">Target tile</param>
    public List<Tile> GeneratePath(Tile origin, Tile destination) //Early exit?
    {
        Queue<Tile> frontier = new Queue<Tile>();
        frontier.Enqueue(origin);

        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>(); // 'from-to'
        cameFrom.Add(origin, null); // Can't go from origin

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            //Debug.Log("Visiting " + current.name);

            if (current == destination) break;

            foreach (Tile next in current.edges)
            {
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom.Add(next, current);
                }
            }
        }

        Tile cur = destination;
        List<Tile> path = new List<Tile>();

        while (cur != origin)
        {
            path.Add(cur);
            cur = cameFrom[cur];
        }
        path.Add(origin);
        path.Reverse();

        foreach (Tile n in cameFrom.Keys)
        {
            //Debug.Log(n + "<-->" + cameFrom[n]);
        }

        return path;
    }
}
