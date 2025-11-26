using System.Collections.Generic;
using UnityEngine;

namespace Pacman.Utils
{
    /// <summary>
    /// Pathfinding utilities.
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// Gets the 4 adjacent neighbors of a coordinate.
        /// </summary>
        public static List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            return new List<Vector2Int>
            {
                new Vector2Int(pos.x + 1, pos.y), // Right
                new Vector2Int(pos.x - 1, pos.y), // Left
                new Vector2Int(pos.x, pos.y + 1), // Up
                new Vector2Int(pos.x, pos.y - 1)  // Down
            };
        }

        /// <summary>
        /// Finds the shortest path from start to goal using BFS.
        /// Only searches tiles within 'safeTiles'.
        /// Returns a list of coordinates (including start and end), or null if no path.
        /// </summary>
        public static List<Vector2Int> BfsPathfinder(
            Vector2Int startPos,
            Vector2Int? goalPos,
            HashSet<Vector2Int> safeTiles)
        {
            if (goalPos == null) 
                return null;

            Vector2Int goal = goalPos.Value; // Unwrap nullable

            if (startPos == goal)
                return new List<Vector2Int> { startPos };

            // Check if goal or start are even reachable/safe
            if (!safeTiles.Contains(goal) || !safeTiles.Contains(startPos))
                return null;

            // Queue stores: (CurrentPosition, PathSoFar)
            var queue = new Queue<(Vector2Int, List<Vector2Int>)>();
            queue.Enqueue((startPos, new List<Vector2Int> { startPos }));

            var visited = new HashSet<Vector2Int> { startPos };

            while (queue.Count > 0)
            {
                var (currentPos, path) = queue.Dequeue();

                foreach (Vector2Int neighbor in GetNeighbors(currentPos))
                {
                    if (neighbor == goal)
                    {
                        // Path found: append neighbor and return
                        var finalPath = new List<Vector2Int>(path) { neighbor };
                        return finalPath;
                    }

                    if (safeTiles.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        // Create new path extending the current one
                        var newPath = new List<Vector2Int>(path) { neighbor };
                        queue.Enqueue((neighbor, newPath));
                    }
                }
            }

            return null; // No path found
        }

        /// <summary>
        /// Determines the move ('UP', 'DOWN', etc.) from the current position
        /// to the next step in the path.
        /// </summary>
        public static string GetMoveFromPath(Vector2Int currentPos, List<Vector2Int> path)
        {
            if (path == null || path.Count < 2)
                return "WAIT";

            // path[0] is currentPos, path[1] is the next step
            Vector2Int nextPos = path[1];
            Vector2Int diff = nextPos - currentPos;

            // Compare diff against our defined MOVES
            foreach (var kvp in TypesUtils.MOVES)
            {
                if (kvp.Value == diff)
                    return kvp.Key;
            }

            return "WAIT";
        }

        /// <summary>
        /// Finds the coordinate in 'targetCoords' that is closest to 'startPos'
        /// by path distance, using only 'safeTiles'.
        /// </summary>
        public static Vector2Int? FindNearestCoord(
            Vector2Int startPos,
            HashSet<Vector2Int> targetCoords,
            HashSet<Vector2Int> safeTiles)
        {
            if (targetCoords.Contains(startPos))
                return startPos;

            if (targetCoords.Count == 0 || safeTiles.Count == 0)
                return null;

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(startPos);

            var visited = new HashSet<Vector2Int> { startPos };

            while (queue.Count > 0)
            {
                Vector2Int currentPos = queue.Dequeue();

                foreach (Vector2Int neighbor in GetNeighbors(currentPos))
                {
                    if (targetCoords.Contains(neighbor))
                        return neighbor; // Found the closest target

                    if (safeTiles.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return null; // No reachable target
        }
    }
}