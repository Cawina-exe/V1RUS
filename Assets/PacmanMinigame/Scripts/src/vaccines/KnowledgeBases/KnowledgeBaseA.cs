using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Pacman.Utils;

namespace Pacman.Agents.KBs
{
  
    public class KnowledgeBaseA : IKnowledgeBase
    {
       
        private HashSet<Vector2Int> _walls = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> _safeTiles = new HashSet<Vector2Int>();

        private bool _statePatrolling = true;
        private bool _stateChasing = false;
        private bool _statePursuing = false;
        private bool _stateInvestigating = false;

       
        private Vector2Int? _lastKnownVirus = null;
        private string _lastMove = "WAIT";
        private Vector2Int? _investigationTarget = null;

        
        private Vector2Int _myPos;
        private bool _seeVirus = false;
        private Vector2Int? _virusPosPercept = null;

        public void Tell(
            Vector2Int myPos,
            Vector2Int? virusPos,
            List<(string id, Vector2Int pos)> otherVaccines,
            Dictionary<Vector2Int, string> percepts
        )
        {
            _myPos = myPos;

            foreach (var kvp in percepts)
            {
                if (kvp.Value == "WALL")
                {
                    _walls.Add(kvp.Key);
                    _safeTiles.Remove(kvp.Key);
                }
                else
                {
                    _safeTiles.Add(kvp.Key);
                }
            }

            _virusPosPercept = virusPos;
            _seeVirus = virusPos.HasValue;

            UpdateStateLogic();
        }

        public string Ask()
        {
            string action = "WAIT";

            if (_stateChasing && _lastKnownVirus.HasValue)
                action = SmartMove(_lastKnownVirus.Value);
            else if (_statePursuing && _lastKnownVirus.HasValue)
                action = SmartMove(_lastKnownVirus.Value);
            else if (_stateInvestigating && _investigationTarget.HasValue)
                action = SmartMove(_investigationTarget.Value);
            else
                action = GetPatrolMove();

            _lastMove = action;
            return action;
        }

        private void UpdateStateLogic()
        {
            if (_seeVirus)
            {
                SetState(chasing: true);
                _lastKnownVirus = _virusPosPercept;
            }

            if (_stateChasing && !_seeVirus) SetState(pursuing: true);

            if (_statePursuing && _lastKnownVirus.HasValue && _myPos == _lastKnownVirus.Value)
            {
                SetState(investigating: true);
                var options = PathUtils.GetNeighbors(_myPos).Where(n => !_walls.Contains(n)).ToList();
                if (options.Count > 0)
                    _investigationTarget = options[Random.Range(0, options.Count)];
                else
                    SetState(patrolling: true);
            }

            if (_stateInvestigating)
            {
                bool reached = _investigationTarget.HasValue && (_myPos == _investigationTarget.Value);
                bool blocked = _investigationTarget.HasValue && _walls.Contains(_investigationTarget.Value);
                if (reached || blocked) SetState(patrolling: true);
            }
        }

        private void SetState(bool patrolling = false, bool chasing = false, bool pursuing = false, bool investigating = false)
        {
            _statePatrolling = patrolling;
            _stateChasing = chasing;
            _statePursuing = pursuing;
            _stateInvestigating = investigating;
        }

        private string SmartMove(Vector2Int target)
        {
            int dx = target.x - _myPos.x;
            int dy = target.y - _myPos.y;

            var candidates = new List<string>();

        
            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                candidates.Add(dx > 0 ? "RIGHT" : "LEFT");
                if (dy != 0) candidates.Add(dy > 0 ? "UP" : "DOWN");
            }
            else
            {
                candidates.Add(dy > 0 ? "UP" : "DOWN");
                if (dx != 0) candidates.Add(dx > 0 ? "RIGHT" : "LEFT");
            }

           
            foreach (string m in TypesUtils.DIRECTIONS)
            {
                if (m != "WAIT" && !candidates.Contains(m)) candidates.Add(m);
            }

         
            string reverse = GetReverse(_lastMove);
            if (candidates.Contains(reverse))
            {
                candidates.Remove(reverse);
                candidates.Add(reverse);
            }

           
            foreach (var move in candidates)
            {
                Vector2Int nextPos = _myPos + TypesUtils.MOVES[move];
                if (!_walls.Contains(nextPos)) return move;
            }

            return "WAIT";
        }

        private string GetPatrolMove()
        {
            var validMoves = new List<string>();
            string reverseMove = GetReverse(_lastMove);

            foreach (var kvp in TypesUtils.MOVES)
            {
                if (kvp.Key == "WAIT") continue;
                Vector2Int nextPos = _myPos + kvp.Value;
                if (!_walls.Contains(nextPos)) validMoves.Add(kvp.Key);
            }

            if (validMoves.Count == 0) return "WAIT";

       
            if (validMoves.Contains(_lastMove)) return _lastMove;

           
            var nonReverseMoves = validMoves.Where(m => m != reverseMove).ToList();
            if (nonReverseMoves.Count > 0)
                return nonReverseMoves[Random.Range(0, nonReverseMoves.Count)];

           
            return validMoves[0];
        }

        private string GetReverse(string move)
        {
            switch (move)
            {
                case "UP": return "DOWN";
                case "DOWN": return "UP";
                case "LEFT": return "RIGHT";
                case "RIGHT": return "LEFT";
                default: return "WAIT";
            }
        }
    }
}