using System.Collections.Generic;
using UnityEngine;

public static class PathUtils
{
    // Returns a list of steps including start and end
    public static List<Vector2Int> BFS(Vector2Int start, Vector2Int? goal, HashSet<Vector2Int> walkableTiles)
    {
        if (goal == null || !walkableTiles.Contains(goal.Value) || !walkableTiles.Contains(start))
            return null;

        var queue = new Queue<(Vector2Int pos, List<Vector2Int> path)>();
        queue.Enqueue((start, new List<Vector2Int> { start }));

        var visited = new HashSet<Vector2Int> { start };

        while (queue.Count > 0)
        {
            var (current, path) = queue.Dequeue();

            if (current == goal.Value)
                return path;

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (walkableTiles.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    var newPath = new List<Vector2Int>(path) { neighbor };
                    queue.Enqueue((neighbor, newPath));
                }
            }
        }
        return null;
    }

    public static List<Vector2Int> GetNeighbors(Vector2Int p)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(p.x + 1, p.y), new Vector2Int(p.x - 1, p.y),
            new Vector2Int(p.x, p.y + 1), new Vector2Int(p.x, p.y - 1)
        };
    }

    // Helper to calculate direction string from path
    public static string GetMoveFromPath(Vector2Int current, List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return "WAIT";
        Vector2Int next = path[1]; // path[0] is current
        Vector2Int delta = next - current;

        if (delta == Vector2Int.up) return "UP";
        if (delta == Vector2Int.down) return "DOWN";
        if (delta == Vector2Int.left) return "LEFT";
        if (delta == Vector2Int.right) return "RIGHT";
        return "WAIT";
    }
}