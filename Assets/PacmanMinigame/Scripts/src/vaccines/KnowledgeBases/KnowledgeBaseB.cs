using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Pacman.Utils;

namespace Pacman.Agents.KBs
{
  
    public class KnowledgeBaseB : IKnowledgeBase
    {
      
        private HashSet<Vector2Int> _walls = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _safeTiles = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _unknownTiles = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _junctions = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _believedPellets = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _unreachableGoals = new HashSet<Vector2Int>();

        private bool _initialized = false;

        
        private Vector2Int _myPos;
        private bool _virusVisible = false;
        private Vector2Int? _virusLastPos = null;
       
        private List<(Vector2Int pos, int age)> _clues = new List<(Vector2Int, int)>();

       
        private Vector2Int? _goal = null;
        private List<Vector2Int> _currentPath = new List<Vector2Int>();

    
        private Queue<Vector2Int> _visitedJunctions = new Queue<Vector2Int>();
        private const int MAX_VISITED_HISTORY = 12;

        
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

       
            if (virusPos.HasValue && !_virusVisible) _unreachableGoals.Clear();

            if (!_initialized)
            {
                MarkSafe(myPos); 
                
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
                    MarkSafe(pos); 

                    if (item == "PELLET")
                    {
                        _believedPellets.Add(pos);
                    }
                    else if (item == "EMPTY")
                    {
                   
                        if (_believedPellets.Contains(pos))
                        {
                            newCluePos = pos;
                            _believedPellets.Remove(pos);
                        }
                    }
                }
            }

           
            var aliveClues = new List<(Vector2Int, int)>();
            foreach (var clue in _clues)
            {
                
                if (clue.pos == _myPos) continue;
                if (_virusVisible) continue;

                if (clue.age < 25)
                {
                    aliveClues.Add((clue.pos, clue.age + 1));
                }
            }
            _clues = aliveClues;

            
            if (virusPos.HasValue)
            {
                _virusVisible = true;
                _virusLastPos = virusPos;
                _clues.Clear();
                _isLoitering = false;
                _goal = null; 
            }
            else
            {
                _virusVisible = false;
                if (newCluePos.HasValue)
                {
                   
                    _clues.Insert(0, (newCluePos.Value, 0));
                    if (_clues.Count > 2) _clues.RemoveAt(_clues.Count - 1); 
                }
            }
        }

        public string Ask()
        {
        
            var planningMesh = new HashSet<Vector2Int>(_safeTiles);
            planningMesh.UnionWith(_unknownTiles);
            planningMesh.Add(_myPos); 

         
            if (_goal.HasValue && _walls.Contains(_goal.Value))
            {
                _goal = null;
                _currentPath.Clear();
            }

            
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

          
            if (_goal.HasValue && _myPos == _goal.Value)
            {
                if (_junctions.Contains(_myPos))
                {
                    
                    _visitedJunctions.Enqueue(_myPos);
                    while (_visitedJunctions.Count > MAX_VISITED_HISTORY)
                        _visitedJunctions.Dequeue();

                    
                    _isLoitering = true;
                    _loiterTimer = MAX_LOITER_TIME;
                    _loiterAnchor = _myPos;

                    _goal = null;
                    _currentPath.Clear();
                    return "WAIT";
                }
                else
                {
                    
                    _goal = null;
                    _currentPath.Clear();
                }
            }

            int attempts = 0;
        
            while ((_goal == null || _currentPath.Count == 0) && attempts < 10)
            {
                attempts++;

             
                if (_goal == null)
                {
                    _goal = SelectNewGoal(planningMesh);
                }

                if (_goal.HasValue)
                {
                   
                    if (_currentPath == null) _currentPath = new List<Vector2Int>();

                   
                    var newPath = PathUtils.BfsPathfinder(_myPos, _goal, planningMesh);

                    if (newPath != null && newPath.Count > 0)
                    {
                        _currentPath = newPath;
                        if (_currentPath[0] == _myPos) _currentPath.RemoveAt(0);
                    }
                    else
                    {
                     
                        _unreachableGoals.Add(_goal.Value);
                        _goal = null;
                    }
                }
                else
                {
                  
                    var neighbors = PathUtils.GetNeighbors(_myPos)
                                             .Where(n => planningMesh.Contains(n) && !_walls.Contains(n))
                                             .ToList();
                    if (neighbors.Count > 0)
                    {
                        _goal = neighbors[Random.Range(0, neighbors.Count)];
                        _currentPath = new List<Vector2Int> { _goal.Value };
                    }
                    break; 
                }
            }

           
            if (_currentPath != null && _currentPath.Count > 0)
            {
                Vector2Int nextStep = _currentPath[0];

              
                if (_walls.Contains(nextStep))
                {
                    _currentPath.Clear();
                    _goal = null;
                    return "WAIT";
                }

               
                if (nextStep == _myPos)
                {
                    _currentPath.RemoveAt(0);
                    if (_currentPath.Count > 0) nextStep = _currentPath[0];
                    else return "WAIT";
                }

                return PathUtils.GetMoveFromPath(_myPos, new List<Vector2Int> { _myPos, nextStep });
            }

            
            return GetRandomOptimisticMove();
        }


        private void MarkSafe(Vector2Int pos)
        {
            if (_safeTiles.Contains(pos)) return;

            _safeTiles.Add(pos);
            _walls.Remove(pos);
            _unknownTiles.Remove(pos);

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
                
                if (!_walls.Contains(n)) count++;
            }
            if (count >= 3) _junctions.Add(pos);
        }

        private string ExecuteLoiterStep(HashSet<Vector2Int> mesh)
        {
            if (_loiterAnchor == null) return "WAIT";

            if (_myPos == _loiterAnchor.Value)
            {
              
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
           
            return PathUtils.GetMoveFromPath(_myPos, new List<Vector2Int> { _myPos, _loiterAnchor.Value });
        }

        private Vector2Int? SelectNewGoal(HashSet<Vector2Int> mesh)
        {
           
            if (_virusVisible) return _virusLastPos;

           
            if (_clues.Count > 0) return SelectBestClue();

          
            bool IsValid(Vector2Int c) => !_walls.Contains(c) && mesh.Contains(c) && c != _myPos && !_unreachableGoals.Contains(c);

            
            var farJuncs = new HashSet<Vector2Int>();
            foreach (var j in _junctions)
            {
                if (!_visitedJunctions.Contains(j) && IsValid(j))
                {
                    if (Mathf.Abs(j.x - _myPos.x) + Mathf.Abs(j.y - _myPos.y) > 5)
                        farJuncs.Add(j);
                }
            }

            if (farJuncs.Count > 0)
                return PathUtils.FindNearestCoord(_myPos, farJuncs, mesh);

           
            var validUnknowns = new HashSet<Vector2Int>(_unknownTiles.Where(u => IsValid(u)));
            if (validUnknowns.Count > 0)
                return PathUtils.FindNearestCoord(_myPos, validUnknowns, mesh);

           
            var nearbyJuncs = new HashSet<Vector2Int>(_junctions.Where(j => j != _myPos && IsValid(j)));
            if (nearbyJuncs.Count > 0)
                return PathUtils.FindNearestCoord(_myPos, nearbyJuncs, mesh);

            
            var safeList = _safeTiles.Where(s => IsValid(s)).ToList();
            if (safeList.Count > 0) return safeList[Random.Range(0, safeList.Count)];

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