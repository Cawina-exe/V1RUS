using System.Collections.Generic;
using UnityEngine;

namespace Pacman.Utils
{
    /// <summary>
    /// Utility class defining core types and constants for the grid environment.
    /// </summary>
    public static class TypesUtils
    {
        // Percepts infered from Environment
        public class Percept : Dictionary<Vector2Int, string> { }

        // Move/Direction Definitions
        public static readonly Dictionary<string, Vector2Int> MOVES = new Dictionary<string, Vector2Int>
        {
            { "UP",    Vector2Int.up },    // (0, 1)
            { "DOWN",  Vector2Int.down },  // (0, -1)
            { "LEFT",  Vector2Int.left },  // (-1, 0)
            { "RIGHT", Vector2Int.right }, // (1, 0)
            { "WAIT",  Vector2Int.zero }   // (0, 0)
        };

        // DIRECTIONS = list(MOVES.keys())
        public static readonly List<string> DIRECTIONS = new List<string>(MOVES.Keys);
    }
}