using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Pacman.Utils;

namespace Pacman.Agents.KBs
{
    /// <summary>
    /// KB for Vaccine Blue and Vaccine Pink (formerly Ghost A).
    /// Logic: Model-Based Reflex Agent using Propositional Logic.
    /// </summary>
    public class KnowledgeBaseA : IKnowledgeBase
    {
        // --- Internal State ---

        // Proposition: Wall_x_y is True if (x,y) is in _walls
        private HashSet<Vector2Int> _walls = new HashSet<Vector2Int>();

        // Proposition: Safe_x_y is True if (x,y) is known safe
        private HashSet<Vector2Int> _safeTiles = new HashSet<Vector2Int>();

        // State Propositions (Only one is True at a time)
        private bool _statePatrolling = true;
        private bool _stateChasing = false;
        private bool _statePursuing = false;
        private bool _stateInvestigating = false;

        // Beliefs (Memory Propositions)
        private Vector2Int? _lastKnownVirus = null;
        private string _lastMove = "WAIT";
        private Vector2Int? _investigationTarget = null;

        // Percept Propositions (Updated every turn)
        private Vector2Int _myPos;
        private bool _seeVirus = false;
        private Vector2Int? _virusPosPercept = null;

        /// <summary>
        /// Updates the Truth Value of propositions based on percepts.
        /// </summary>
        public void Tell(
            Vector2Int myPos,
            Vector2Int? virusPos,
            List<(string id, Vector2Int pos)> otherVaccines,
            Dictionary<Vector2Int, string> percepts
        )
        {
            // Proposition: MyPos_x_y is True
            _myPos = myPos;

            // Map Propositions
            foreach (var kvp in percepts)
            {
                Vector2Int pos = kvp.Key;
                string item = kvp.Value;

                if (item == "WALL")
                {
                    _walls.Add(pos);
                    _safeTiles.Remove(pos);
                }
                else
                {
                    // Proposition: not Wall_x_y => Safe_x_y
                    _safeTiles.Add(pos);
                }
            }

            // Virus Propositions
            _virusPosPercept = virusPos;
            _seeVirus = virusPos.HasValue;

            // --- State Transition Logic (PL Rules) ---
            UpdateStateLogic();
        }

        /// <summary>
        /// Queries the internal model to decide the action.
        /// Returns action string.
        /// </summary>
        public string Ask()
        {
            string action = "WAIT";

            // Query 1: Chasing Logic
            // Proposition: State_Chasing => MoveTowards(LastKnownVirus)
            if (_stateChasing && _lastKnownVirus.HasValue)
            {
                action = SmartMove(_lastKnownVirus.Value);
            }
            // Query 2: Pursuing Logic
            // Proposition: State_Pursuing => MoveTowards(LastKnownVirus)
            else if (_statePursuing && _lastKnownVirus.HasValue)
            {
                action = SmartMove(_lastKnownVirus.Value);
            }
            // Query 3: Investigating Logic
            // Proposition: State_Investigating => MoveTowards(InvestigationTarget)
            else if (_stateInvestigating && _investigationTarget.HasValue)
            {
                action = SmartMove(_investigationTarget.Value);
            }
            // Query 4: Patrolling Logic (Default)
            // Proposition: State_Patrolling => PatrolMove()
            else
            {
                action = GetPatrolMove();
            }

            _lastMove = action;
            return action;
        }

        // --- Internal Logic Helpers ---

        private void UpdateStateLogic()
        {
            // Rule 1: Detection
            // Proposition: SeeVirus => State_Chasing
            if (_seeVirus)
            {
                SetState(chasing: true);
                _lastKnownVirus = _virusPosPercept;
            }

            // Rule 2: Lost Sight (Persistence)
            // Proposition: State_Chasing ^ not SeeVirus => State_Pursuing
            if (_stateChasing && !_seeVirus)
            {
                SetState(pursuing: true);
            }

            // Rule 3: Arrival at Last Known Location
            // Proposition: State_Pursuing ^ (MyPos == LastKnownVirus) => State_Investigating
            if (_statePursuing && _lastKnownVirus.HasValue && _myPos == _lastKnownVirus.Value)
            {
                SetState(investigating: true);

                // Picking target
                // Use PathUtils helper
                var options = PathUtils.GetNeighbors(_myPos)
                                       .Where(n => !_walls.Contains(n))
                                       .ToList();

                if (options.Count > 0)
                {
                    _investigationTarget = options[Random.Range(0, options.Count)];
                }
                else
                {
                    SetState(patrolling: true);
                }
            }

            // Rule 4: Finished Investigating
            // Proposition: State_Investigating ^ (MyPos == InvestigationTarget) => State_Patrolling
            if (_stateInvestigating)
            {
                bool targetReached = _investigationTarget.HasValue && (_myPos == _investigationTarget.Value);
                bool targetBlocked = _investigationTarget.HasValue && _walls.Contains(_investigationTarget.Value);

                if (targetReached || targetBlocked)
                {
                    SetState(patrolling: true);
                }
            }
        }

        private void SetState(bool patrolling = false, bool chasing = false, bool pursuing = false, bool investigating = false)
        {
            _statePatrolling = patrolling;
            _stateChasing = chasing;
            _statePursuing = pursuing;
            _stateInvestigating = investigating;
        }

        /// <summary>
        /// Greedy movement towards target.
        /// Logic: Minimize Distance(MyPos, Target) s.t. not Wall(NextPos)
        /// </summary>
        private string SmartMove(Vector2Int target)
        {
            // Determine desired axes
            int dx = target.x - _myPos.x;
            int dy = target.y - _myPos.y;

            var candidates = new List<string>();

            // Prioritize axis with larger distance
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

            // Add remaining moves as fallback
            foreach (string m in TypesUtils.DIRECTIONS)
            {
                if (m != "WAIT" && !candidates.Contains(m))
                    candidates.Add(m);
            }

            // Check Safety (Proposition: not Wall_next)
            foreach (string move in candidates)
            {
                Vector2Int dir = TypesUtils.MOVES[move];
                Vector2Int nextPos = _myPos + dir;

                if (!_walls.Contains(nextPos))
                {
                    return move;
                }
            }

            return "WAIT";
        }

        /// <summary>
        /// Patrol Logic with Momentum.
        /// </summary>
        private string GetPatrolMove()
        {
            // 1. Identify Valid Moves (Proposition: not Wall_next)
            var validMoves = new List<string>();
            string reverseMove = GetReverse(_lastMove);

            foreach (var kvp in TypesUtils.MOVES)
            {
                string move = kvp.Key;
                Vector2Int dir = kvp.Value;

                if (move == "WAIT") continue;

                Vector2Int nextPos = _myPos + dir;
                if (!_walls.Contains(nextPos))
                {
                    validMoves.Add(move);
                }
            }

            if (validMoves.Count == 0) return "WAIT";

            // 2. Apply Momentum Logic
            // Formal: Valid(LastMove) => Action(LastMove)
            if (validMoves.Contains(_lastMove))
            {
                return _lastMove;
            }

            // 3. Handle Junctions/Corners (Jitter Fix)
            // Logic: If stuck, choose Random from Valid \ {Reverse}
            var nonReverseMoves = validMoves.Where(m => m != reverseMove).ToList();

            if (nonReverseMoves.Count > 0)
            {
                return nonReverseMoves[Random.Range(0, nonReverseMoves.Count)];
            }

            // Dead end: Only reverse is possible
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