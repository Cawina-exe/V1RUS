using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Pacman.Utils;

namespace Pacman.Agents.KBs
{
    /// <summary>
    /// KB for Vaccine Orange (formerly Ghost B).
    /// Logic: Optimistic Model-Based Agent using PL to route through the fog of war.
    /// </summary>
    public class KnowledgeBaseB : IKnowledgeBase
    {
        // --- World Model ---
        private HashSet<Vector2Int> _walls = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _safeTiles = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _unknownTiles = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _junctions = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _believedPellets = new HashSet<Vector2Int>();

        private bool _initialized = false;

        // --- Beliefs ---
        private Vector2Int _myPos;
        private bool _virusVisible = false;
        private Vector2Int? _virusLastPos = null;
        // Clues: List of (Position, Age)
        private List<(Vector2Int pos, int age)> _clues = new List<(Vector2Int, int)>();

        // --- Memory & Plan ---
        private Vector2Int? _goal = null;
        private List<Vector2Int> _currentPath = new List<Vector2Int>();

        // --- Junction Memory ---
        // FIFO Queue for last N visited junctions
        private Queue<Vector2Int> _visitedJunctions = new Queue<Vector2Int>();
        private const int MAX_VISITED_HISTORY = 12;

        // --- Camping Logic ---
        private bool _isLoitering = false;
        private int _loiterTimer = 0;
        private Vector2Int? _loiterAnchor = null;
        private const int MAX_LOITER_TIME = 4;

        public void Tell(
            Vector2Int myPos,
            Vector2Int? virusPos,
            List<(string id, Vector2Int pos)> otherVaccines,
            Dictionary<Vector2Int, string> percepts
        )
        {
            _myPos = myPos;

            if (!_initialized)
            {
                MarkSafe(myPos); // Current tile is safe
                // Pre-seed unknown frontier
                foreach (var n in PathUtils.GetNeighbors(myPos))
                {
                    if (!_walls.Contains(n)) _unknownTiles.Add(n);
                }
                _initialized = true;
            }
            else
            {
                MarkSafe(myPos);
            }

            Vector2Int? newCluePos = null;

            foreach (var kvp in percepts)
            {
                Vector2Int pos = kvp.Key;
                string item = kvp.Value;

                if (item == "WALL")
                {
                    _walls.Add(pos);
                    _safeTiles.Remove(pos);
                    _unknownTiles.Remove(pos);
                    _junctions.Remove(pos);
                }
                else
                {
                    MarkSafe(pos); // Visible tiles are safe

                    if (item == "PELLET")
                    {
                        _believedPellets.Add(pos);
                    }
                    else if (item == "EMPTY")
                    {
                        // If we thought there was a pellet here, but now we see empty,
                        // that means Pacman ate it recently -> Clue!
                        if (_believedPellets.Contains(pos))
                        {
                            newCluePos = pos;
                            _believedPellets.Remove(pos);
                        }
                    }
                }
            }

            // 2. Update Clues (Aging)
            var aliveClues = new List<(Vector2Int, int)>();
            foreach (var clue in _clues)
            {
                // Ignore if we are standing on it or if we see the virus
                if (clue.pos == _myPos) continue;
                if (_virusVisible) continue;

                if (clue.age < 25)
                {
                    aliveClues.Add((clue.pos, clue.age + 1));
                }
            }
            _clues = aliveClues;

            // 3. Update Virus Interaction
            if (virusPos.HasValue)
            {
                _virusVisible = true;
                _virusLastPos = virusPos;
                _clues.Clear();
                _isLoitering = false;
                _goal = null; // Reset goal to chase immediately
            }
            else
            {
                _virusVisible = false;
                if (newCluePos.HasValue)
                {
                    // Insert new clue at start (index 0)
                    _clues.Insert(0, (newCluePos.Value, 0));
                    if (_clues.Count > 2) _clues.RemoveAt(_clues.Count - 1); // Keep max 2
                }
            }
        }

        public string Ask()
        {
            // "Unknown" is "Walkable until proven otherwise"
            var planningMesh = new HashSet<Vector2Int>(_safeTiles);
            planningMesh.UnionWith(_unknownTiles);
            planningMesh.Add(_myPos); // Ensure start is valid

            // Defensive: if goal became a wall, drop it
            if (_goal.HasValue && _walls.Contains(_goal.Value))
            {
                _goal = null;
                _currentPath.Clear();
            }

            // 1. LOITERING (Camping at junction)
            if (_isLoitering)
            {
                _loiterTimer--;
                if (_loiterTimer <= 0 || _virusVisible)
                {
                    _isLoitering = false;
                    _goal = null;
                    _currentPath.Clear();
                }
                else
                {
                    return ExecuteLoiterStep(planningMesh);
                }
            }

            // 2. ARRIVAL & CAMPING
            if (_goal.HasValue && _myPos == _goal.Value)
            {
                if (_junctions.Contains(_myPos))
                {
                    // Update Queue
                    // We just Enqueue. The limit logic handles the overflow.
                    _visitedJunctions.Enqueue(_myPos);
                    while (_visitedJunctions.Count > MAX_VISITED_HISTORY)
                        _visitedJunctions.Dequeue();

                    // Start Loitering
                    _isLoitering = true;
                    _loiterTimer = MAX_LOITER_TIME;
                    _loiterAnchor = _myPos;

                    _goal = null;
                    _currentPath.Clear();
                    return "WAIT";
                }
                else
                {
                    // Reached non-junction goal (e.g. clue) -> just continue
                    _goal = null;
                    _currentPath.Clear();
                }
            }

            // 3. GOAL SELECTION
            if (_goal == null)
            {
                _goal = SelectNewGoal(planningMesh);
                _currentPath.Clear();
            }

            // 4. PATHFINDING
            if (_goal.HasValue)
            {
                bool needsPath = (_currentPath.Count == 0);

                // If we have a path, check if it's still valid (continuity)
                if (_currentPath.Count > 0)
                {
                    Vector2Int nextStep = _currentPath[0];
                    // If next step is not neighbor and not self, path is broken
                    if (!PathUtils.GetNeighbors(_myPos).Contains(nextStep) && nextStep != _myPos)
                        needsPath = true;
                }

                if (needsPath)
                {
                    // Pass OPTIMISTIC mesh to BFS
                    _currentPath = PathUtils.BfsPathfinder(_myPos, _goal, planningMesh);

                    // BFS returns [start, next, ..., goal]. Remove start.
                    if (_currentPath != null && _currentPath.Count > 0 && _currentPath[0] == _myPos)
                    {
                        _currentPath.RemoveAt(0);
                    }

                    if (_currentPath == null) _goal = null; // Unreachable
                }
            }

            // 5. EXECUTION
            if (_currentPath != null && _currentPath.Count > 0)
            {
                Vector2Int nextStep = _currentPath[0];

                // Double check wall
                if (_walls.Contains(nextStep))
                {
                    _currentPath.Clear();
                    return "WAIT";
                }

                // Optimization: Pop step if we are on it
                if (nextStep == _myPos)
                {
                    _currentPath.RemoveAt(0);
                    if (_currentPath.Count > 0) nextStep = _currentPath[0];
                    else return "WAIT";
                }

                // Construct a mini-path for the helper
                return PathUtils.GetMoveFromPath(_myPos, new List<Vector2Int> { _myPos, nextStep });
            }

            // 6. FALLBACK
            return GetRandomOptimisticMove();
        }

        // --- Helpers ---

        private void MarkSafe(Vector2Int pos)
        {
            if (_safeTiles.Contains(pos)) return;

            _safeTiles.Add(pos);
            _walls.Remove(pos);
            _unknownTiles.Remove(pos);

            // Add neighbors to unknown if fresh
            foreach (var n in PathUtils.GetNeighbors(pos))
            {
                if (!_safeTiles.Contains(n) && !_walls.Contains(n))
                    _unknownTiles.Add(n);
            }

            InferJunction(pos);
            foreach (var n in PathUtils.GetNeighbors(pos))
            {
                if (_safeTiles.Contains(n)) InferJunction(n);
            }
        }

        private void InferJunction(Vector2Int pos)
        {
            if (_walls.Contains(pos)) return;
            int count = 0;
            foreach (var n in PathUtils.GetNeighbors(pos))
            {
                // Optimistic: if not a wall, it's a path
                if (!_walls.Contains(n)) count++;
            }
            if (count >= 3) _junctions.Add(pos);
        }

        private string ExecuteLoiterStep(HashSet<Vector2Int> mesh)
        {
            if (_loiterAnchor == null) return "WAIT";

            if (_myPos == _loiterAnchor.Value)
            {
                // Step out to a random valid neighbor
                var neighbors = PathUtils.GetNeighbors(_loiterAnchor.Value)
                                         .Where(n => mesh.Contains(n))
                                         .ToList();
                if (neighbors.Count > 0)
                {
                    Vector2Int stepOut = neighbors[Random.Range(0, neighbors.Count)];
                    return PathUtils.GetMoveFromPath(_myPos, new List<Vector2Int> { _myPos, stepOut });
                }
                return "WAIT";
            }
            // Step back to anchor
            return PathUtils.GetMoveFromPath(_myPos, new List<Vector2Int> { _myPos, _loiterAnchor.Value });
        }

        private Vector2Int? SelectNewGoal(HashSet<Vector2Int> mesh)
        {
            // 1. CHASE
            if (_virusVisible) return _virusLastPos;

            // 2. CLUES
            if (_clues.Count > 0) return SelectBestClue();

            // Helper for finding nearest
            bool IsValid(Vector2Int c) => !_walls.Contains(c) && mesh.Contains(c) && c != _myPos;

            // 3. FAR JUNCTIONS (Avoid visited)
            var farJuncs = new HashSet<Vector2Int>();
            foreach (var j in _junctions)
            {
                if (!_visitedJunctions.Contains(j) && IsValid(j))
                {
                    // Simple Manhattan distance > 5
                    if (Mathf.Abs(j.x - _myPos.x) + Mathf.Abs(j.y - _myPos.y) > 5)
                        farJuncs.Add(j);
                }
            }

            if (farJuncs.Count > 0)
                return PathUtils.FindNearestCoord(_myPos, farJuncs, mesh);

            // 4. UNKNOWN FRONTIER
            var validUnknowns = new HashSet<Vector2Int>(_unknownTiles.Where(u => IsValid(u)));
            if (validUnknowns.Count > 0)
                return PathUtils.FindNearestCoord(_myPos, validUnknowns, mesh);

            // 5. ANY JUNCTION
            var nearbyJuncs = new HashSet<Vector2Int>(_junctions.Where(j => j != _myPos && IsValid(j)));
            if (nearbyJuncs.Count > 0)
                return PathUtils.FindNearestCoord(_myPos, nearbyJuncs, mesh);

            return null;
        }

        private Vector2Int? SelectBestClue()
        {
            Vector2Int? best = null;
            float minScore = 9999f;

            foreach (var (cPos, cAge) in _clues)
            {
                float dist = Mathf.Abs(cPos.x - _myPos.x) + Mathf.Abs(cPos.y - _myPos.y);
                float score = dist + (cAge * 2);
                if (score < minScore)
                {
                    minScore = score;
                    best = cPos;
                }
            }
            return best;
        }

        private string GetRandomOptimisticMove()
        {
            var possible = new List<string>();
            foreach (var kvp in TypesUtils.MOVES)
            {
                string move = kvp.Key;
                if (move == "WAIT") continue;

                Vector2Int next = _myPos + kvp.Value;
                // Valid if not a known wall, and is either safe or unknown
                if (!_walls.Contains(next) && (_safeTiles.Contains(next) || _unknownTiles.Contains(next)))
                {
                    possible.Add(move);
                }
            }

            if (possible.Count > 0) return possible[Random.Range(0, possible.Count)];
            return "WAIT";
        }
    }
}