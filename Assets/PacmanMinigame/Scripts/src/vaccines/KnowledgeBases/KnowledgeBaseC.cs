using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Pacman.Utils;
using Pacman.Utils.FOL; // Using your FOL engine and Predicates

namespace Pacman.Agents.KBs
{
    /// <summary>
    /// KB for Vaccine Red (formerly Ghost C).
    /// Logic: Model-Based Utility Agent using First-Order Logic (FOL).
    /// </summary>
    public class KnowledgeBaseC : IKnowledgeBase
    {
        // FOL Knowledge Base: Set of ground predicates
        private HashSet<Predicate> _facts = new HashSet<Predicate>();

        // Internal state
        private Vector2Int _myPos;
        private Vector2Int? _goal = null;
        private List<Vector2Int> _currentPath = new List<Vector2Int>();

        // Tracking for vector calculation
        private Vector2Int? _lastVirusPos = null;
        private Vector2Int _lastVirusVector = Vector2Int.zero;

        private int _currentTime = 0;

        // Common Variables for Queries (Reused)
        private readonly Variable X = new Variable("X");
        private readonly Variable Y = new Variable("Y");
        private readonly Variable T = new Variable("T");
        private readonly Variable ID = new Variable("ID");
        private readonly Variable DX = new Variable("DX");
        private readonly Variable DY = new Variable("DY");

        public void Tell(
            Vector2Int myPos,
            Vector2Int? virusPos,
            List<(string id, Vector2Int pos)> otherVaccines,
            Dictionary<Vector2Int, string> percepts
        )
        {
            _currentTime++;
            _myPos = myPos;

            // 1. Manage History (Keep last 10 steps)
            RetractOldHistory(keepLastN: 10);

            // 2. Assert current position in history
            AssertFact(new VisitedAtTime(new Constant(myPos.x.ToString()), new Constant(myPos.y.ToString()), new Constant(_currentTime.ToString())));

            // Clean up old unreachable goals
            RetractOldUnreachableGoals(ageThreshold: 7);

            // FOL Rule 0: Store last position to prevent backtracking
            Retract(new LastPos(new Constant("C"), X, Y));
            AssertFact(new LastPos(new Constant("C"), new Constant(myPos.x.ToString()), new Constant(myPos.y.ToString())));

            // FOL Rule 1 & 2: Map Learning
            LearnMapTopology(myPos, percepts);

            // Retract dynamic beliefs before re-asserting
            RetractDynamicBeliefs();

            // FOL Rule 3: Assert current positions
            AssertFact(new VaccinePos(new Constant("C"), new Constant(myPos.x.ToString()), new Constant(myPos.y.ToString())));

            foreach (var (id, pos) in otherVaccines)
            {
                AssertFact(new VaccinePos(new Constant(id), new Constant(pos.x.ToString()), new Constant(pos.y.ToString())));
            }

            // FOL: Assert VirusPos if visible
            if (virusPos.HasValue)
            {
                AssertFact(new VirusPos(new Constant(virusPos.Value.x.ToString()), new Constant(virusPos.Value.y.ToString())));

                // Compute VirusVector
                if (_lastVirusPos.HasValue)
                {
                    Vector2Int diff = virusPos.Value - _lastVirusPos.Value;
                    if (diff != Vector2Int.zero)
                    {
                        _lastVirusVector = diff;
                    }
                }
                _lastVirusPos = virusPos;

                // Assert: VirusVector(dx, dy)
                AssertFact(new VirusVector(new Constant(_lastVirusVector.x.ToString()), new Constant(_lastVirusVector.y.ToString())));
            }
        }

        public string Ask()
        {
            Vector2Int? newGoal = null;

            // === PRIORITY 0: CONTINUE ESCAPE ===
            // FOL Query: EscapeState(gx, gy, t)
            Vector2Int? escapeTarget = QueryActiveEscape(duration: 20);

            if (escapeTarget.HasValue)
            {
                if (escapeTarget.Value == _myPos)
                {
                    // Reached safety
                    RetractEscapeState();
                }
                else
                {
                    newGoal = escapeTarget;
                }
            }

            // === PRIORITY 0.5: DETECT NEW OSCILLATION ===
            if (newGoal == null && InferOscillation())
            {
                Vector2Int? farGoal = FindFarthestSafeTile();
                if (farGoal.HasValue)
                {
                    AssertFact(new EscapeState(
                        new Constant(farGoal.Value.x.ToString()),
                        new Constant(farGoal.Value.y.ToString()),
                        new Constant(_currentTime.ToString())
                    ));
                    newGoal = farGoal;
                }
                else
                {
                    return GetSafeFallbackMove();
                }
            }

            // === PRIORITY 1: REPULSION FROM NEARBY VACCINES ===
            if (newGoal == null)
            {
                var nearbyVaccines = QueryNearbyVaccines(repulsionDistance: 5);
                if (nearbyVaccines.Count > 0)
                {
                    newGoal = InferRepulsionGoal(nearbyVaccines);
                }
            }

            // === PRIORITY 2: CLOSE CHASE (distance <= 2) ===
            if (newGoal == null)
            {
                var virusInfo = QueryVirusState();
                if (virusInfo.HasValue)
                {
                    (Vector2Int pPos, Vector2Int vVec) = virusInfo.Value;
                    int dist = ManhattanDist(_myPos, pPos);

                    if (dist <= 4)
                    {
                        if (dist <= 2)
                        {
                            if (!IsUnreachableGoal(pPos)) newGoal = pPos;
                        }
                    }
                }
            }

            // === PRIORITY 3: INTERCEPT (3 <= distance <= 4) ===
            if (newGoal == null)
            {
                var virusInfo = QueryVirusState();
                if (virusInfo.HasValue)
                {
                    (Vector2Int pPos, Vector2Int vVec) = virusInfo.Value;
                    int dist = ManhattanDist(_myPos, pPos);

                    if (dist >= 3 && dist <= 4)
                    {
                        // Predict 2 steps ahead
                        Vector2Int target = pPos + (vVec * 2);
                        Vector2Int? interceptGoal = FindNearestSafeToTarget(target);

                        if (interceptGoal.HasValue && !IsUnreachableGoal(interceptGoal.Value))
                        {
                            newGoal = interceptGoal;
                        }
                    }
                }
            }

            // === PRIORITY 4: VECTOR-ALIGNED EXPLORATION ===
            if (newGoal == null)
            {
                Vector2Int? vec = QueryVector();
                if (vec.HasValue && vec.Value != Vector2Int.zero)
                {
                    Vector2Int? alignedGoal = FindVectorAlignedFrontier(vec.Value);
                    if (alignedGoal.HasValue && !IsUnreachableGoal(alignedGoal.Value))
                    {
                        newGoal = alignedGoal;
                    }
                }
            }

            // === PRIORITY 5: FRONTIER EXPLORATION ===
            if (newGoal == null)
            {
                Vector2Int? frontierGoal = FindNearestFrontierGoal();
                if (frontierGoal.HasValue && !IsUnreachableGoal(frontierGoal.Value))
                {
                    newGoal = frontierGoal;
                }
            }

            // === PRIORITY 6: SPREAD ===
            if (newGoal == null)
            {
                Vector2Int? spreadGoal = FindSpreadGoal();
                if (spreadGoal.HasValue && !IsUnreachableGoal(spreadGoal.Value))
                {
                    newGoal = spreadGoal;
                }
            }

            // === PATHFINDING & EXECUTION ===
            if (newGoal.HasValue)
            {
                bool needReplan = (_goal != newGoal) || IsPathObstructed();

                if (needReplan)
                {
                    _goal = newGoal;
                    HashSet<Vector2Int> safeCoords = QueryAllSafeCoords();
                    safeCoords.Add(_goal.Value); // Optimistically assume goal is safe to step on

                    _currentPath = PathUtils.BfsPathfinder(_myPos, _goal, safeCoords);

                    if (_currentPath == null)
                    {
                        // Mark original goal as unreachable
                        AssertFact(new UnreachableGoal(
                            new Constant(_goal.Value.x.ToString()),
                            new Constant(_goal.Value.y.ToString()),
                            new Constant(_currentTime.ToString())
                        ));

                        // --- FALLBACK LOGIC ---
                        Vector2Int? fallback = FindNearestFrontierGoal();
                        if (!fallback.HasValue) fallback = FindSpreadGoal();

                        if (fallback.HasValue)
                        {
                            _goal = fallback;
                            // Try pathfinding to fallback
                            _currentPath = PathUtils.BfsPathfinder(_myPos, _goal, safeCoords);
                            if (_currentPath == null) _currentPath = new List<Vector2Int>();
                        }
                        else
                        {
                            _currentPath = new List<Vector2Int>();
                        }
                    }
                    // Python also checks for "path length 1 but not my_pos" which means start==goal but invalid
                    else if (_currentPath.Count == 1 && _currentPath[0] != _myPos)
                    {
                        AssertFact(new UnreachableGoal(
                           new Constant(_goal.Value.x.ToString()),
                           new Constant(_goal.Value.y.ToString()),
                           new Constant(_currentTime.ToString())
                       ));
                    }
                }
            }

            // Execute Next Move
            if (_currentPath != null && _currentPath.Count > 0)
            {
                if (!_currentPath.Contains(_myPos))
                {
                    _currentPath.Clear();
                    _goal = null;
                    return GetSafeFallbackMove();
                }

                int idx = _currentPath.IndexOf(_myPos);
                if (idx + 1 < _currentPath.Count)
                {
                    Vector2Int nextPos = _currentPath[idx + 1];

                    bool isSafe = IsPositionSafe(nextPos);
                    bool isTargetGoal = (nextPos == _goal);

                    if (isSafe || isTargetGoal)
                    {
                        if (ManhattanDist(_myPos, nextPos) == 1)
                        {
                            return PathUtils.GetMoveFromPath(_myPos, new List<Vector2Int> { _myPos, nextPos });
                        }
                    }
                }
            }

            return GetSafeFallbackMove();
        }

        // --- Internal Logic Helpers ---

        private void LearnMapTopology(Vector2Int myPos, Dictionary<Vector2Int, string> percepts)
        {
            Constant cx = new Constant(myPos.x.ToString());
            Constant cy = new Constant(myPos.y.ToString());

            // Current pos is safe
            if (!QueryExists(new LearnedSafe(cx, cy))) AssertFact(new LearnedSafe(cx, cy));

            foreach (var kvp in percepts)
            {
                Constant px = new Constant(kvp.Key.x.ToString());
                Constant py = new Constant(kvp.Key.y.ToString());

                if (kvp.Value == "WALL")
                {
                    if (!QueryExists(new LearnedWall(px, py)))
                    {
                        AssertFact(new LearnedWall(px, py));
                        Retract(new LearnedSafe(px, py));
                    }
                }
                else
                {
                    if (!QueryExists(new LearnedSafe(px, py))) AssertFact(new LearnedSafe(px, py));
                }
            }
        }

        private List<Vector2Int> QueryNearbyVaccines(int repulsionDistance)
        {
            var nearby = new List<Vector2Int>();
            var results = GetUnifications(new VaccinePos(ID, X, Y));

            foreach (var sub in results)
            {
                string id = sub[ID].Value;
                int gx = int.Parse(sub[X].Value);
                int gy = int.Parse(sub[Y].Value);
                Vector2Int gPos = new Vector2Int(gx, gy);

                if (id != "C")
                {
                    if (ManhattanDist(_myPos, gPos) < repulsionDistance)
                    {
                        nearby.Add(gPos);
                    }
                }
            }
            return nearby;
        }

        private (Vector2Int, Vector2Int)? QueryVirusState()
        {
            var posResults = GetUnifications(new VirusPos(X, Y));
            if (posResults.Count == 0) return null;

            int px = int.Parse(posResults[0][X].Value);
            int py = int.Parse(posResults[0][Y].Value);

            int vx = 0, vy = 0;
            var vecResults = GetUnifications(new VirusVector(DX, DY));
            if (vecResults.Count > 0)
            {
                vx = int.Parse(vecResults[0][DX].Value);
                vy = int.Parse(vecResults[0][DY].Value);
            }

            return (new Vector2Int(px, py), new Vector2Int(vx, vy));
        }

        private Vector2Int? QueryVector()
        {
            var vecResults = GetUnifications(new VirusVector(DX, DY));
            if (vecResults.Count > 0)
            {
                return new Vector2Int(int.Parse(vecResults[0][DX].Value), int.Parse(vecResults[0][DY].Value));
            }
            return null;
        }

        private HashSet<Vector2Int> QueryAllSafeCoords()
        {
            var safe = new HashSet<Vector2Int>();
            var results = GetUnifications(new LearnedSafe(X, Y));
            foreach (var sub in results)
            {
                safe.Add(new Vector2Int(int.Parse(sub[X].Value), int.Parse(sub[Y].Value)));
            }
            return safe;
        }

        private bool IsUnreachableGoal(Vector2Int goal)
        {
            var results = GetUnifications(new UnreachableGoal(new Constant(goal.x.ToString()), new Constant(goal.y.ToString()), T));
            return results.Count > 0;
        }

        private bool IsPositionSafe(Vector2Int pos)
        {
            return QueryExists(new LearnedSafe(new Constant(pos.x.ToString()), new Constant(pos.y.ToString())));
        }

        private bool IsPathObstructed()
        {
            if (_currentPath == null || _currentPath.Count == 0) return true;

            if (_currentPath.Count == 1)
                return _currentPath[0] != _myPos;

            foreach (var tile in _currentPath)
            {
                if (!IsPositionSafe(tile)) return true;
            }
            return false;
        }

        private Vector2Int InferRepulsionGoal(List<Vector2Int> threats)
        {
            // Simplified vector logic
            float avgX = 0, avgY = 0;
            foreach (var t in threats) { avgX += t.x; avgY += t.y; }
            avgX /= threats.Count;
            avgY /= threats.Count;

            // Vector AWAY from threats
            float escX = _myPos.x - avgX;
            float escY = _myPos.y - avgY;

            Vector2 escapeDir = new Vector2(escX, escY).normalized;

            Vector2Int bestGoal = _myPos;
            float bestScore = float.MinValue;

            var safeTiles = QueryAllSafeCoords();

            foreach (var tile in safeTiles)
            {
                int dist = ManhattanDist(_myPos, tile);
                if (dist > 6 || dist == 0) continue;

                // Alignment (Dot productish)
                float dx = tile.x - _myPos.x;
                float dy = tile.y - _myPos.y;
                float alignment = (dx * escapeDir.x) + (dy * escapeDir.y);

                // Min dist to any threat
                float minThreatDist = float.MaxValue;
                foreach (var t in threats)
                {
                    float d = ManhattanDist(tile, t);
                    if (d < minThreatDist) minThreatDist = d;
                }

                float score = alignment * 3 + minThreatDist;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestGoal = tile;
                }
            }
            return bestGoal;
        }

        private Vector2Int? FindNearestFrontierGoal()
        {
            var frontier = ComputeFrontier();
            var validFrontier = new HashSet<Vector2Int>(frontier.Where(f => !IsUnreachableGoal(f)));

            if (validFrontier.Count == 0) return null;

            var safeCoords = QueryAllSafeCoords();
            Vector2Int? nearest = PathUtils.FindNearestCoord(_myPos, validFrontier, safeCoords);

            if (!nearest.HasValue) return null;

            // If adjacent, return it
            if (ManhattanDist(_myPos, nearest.Value) == 1) return nearest;

            // Otherwise find adjacent safe tile
            foreach (var n in PathUtils.GetNeighbors(nearest.Value))
            {
                if (safeCoords.Contains(n)) return n;
            }
            return null;
        }

        private Vector2Int? FindVectorAlignedFrontier(Vector2Int vec)
        {
            var frontier = ComputeFrontier();
            Vector2Int? best = null;
            int minDst = int.MaxValue;

            foreach (var f in frontier)
            {
                int dx = f.x - _myPos.x;
                int dy = f.y - _myPos.y;

                int dot = (dx * vec.x) + (dy * vec.y);
                if (dot > 0)
                {
                    int d = Mathf.Abs(dx) + Mathf.Abs(dy);
                    if (d < minDst)
                    {
                        minDst = d;
                        best = f;
                    }
                }
            }

            if (!best.HasValue) return null;

            // Return safe neighbor if needed
            var safeCoords = QueryAllSafeCoords();
            if (ManhattanDist(_myPos, best.Value) == 1) return best;

            foreach (var n in PathUtils.GetNeighbors(best.Value))
            {
                if (safeCoords.Contains(n)) return n;
            }
            return null;
        }

        private Vector2Int? FindSpreadGoal()
        {
            var safe = QueryAllSafeCoords();
            foreach (var t in safe)
            {
                if (ManhattanDist(_myPos, t) > 10) return t;
            }
            return null;
        }

        private HashSet<Vector2Int> ComputeFrontier()
        {
            var safe = QueryAllSafeCoords();
            var frontier = new HashSet<Vector2Int>();

            foreach (var s in safe)
            {
                foreach (var n in PathUtils.GetNeighbors(s))
                {
                    // Frontier = Neighbor is NOT safe AND NOT known wall
                    bool isSafe = QueryExists(new LearnedSafe(new Constant(n.x.ToString()), new Constant(n.y.ToString())));
                    bool isWall = QueryExists(new LearnedWall(new Constant(n.x.ToString()), new Constant(n.y.ToString())));

                    if (!isSafe && !isWall)
                    {
                        frontier.Add(n);
                    }
                }
            }
            return frontier;
        }

        // --- Core FOL Methods ---

        private void AssertFact(Predicate fact)
        {
            if (fact.IsGround()) _facts.Add(fact);
        }

        private void Retract(Predicate queryTemplate)
        {
            var toRemove = new List<Predicate>();
            foreach (var f in _facts)
            {
                if (FOLUtils.Unify(queryTemplate, f) != null) toRemove.Add(f);
            }
            foreach (var r in toRemove) _facts.Remove(r);
        }

        private void RetractOldHistory(int keepLastN)
        {
            var results = GetUnifications(new VisitedAtTime(X, Y, T));
            var toRetract = new List<Predicate>();

            foreach (var sub in results)
            {
                int tVal = int.Parse(sub[T].Value);
                if (_currentTime - tVal > keepLastN)
                {
                    toRetract.Add(new VisitedAtTime(sub[X], sub[Y], sub[T]));
                }
            }
            foreach (var r in toRetract) _facts.Remove(r);
        }

        private void RetractOldUnreachableGoals(int ageThreshold)
        {
            var results = GetUnifications(new UnreachableGoal(X, Y, T));
            var toRetract = new List<Predicate>();

            foreach (var sub in results)
            {
                int tVal = int.Parse(sub[T].Value);
                if (_currentTime - tVal > ageThreshold)
                {
                    toRetract.Add(new UnreachableGoal(sub[X], sub[Y], sub[T]));
                }
            }
            foreach (var r in toRetract) _facts.Remove(r);
        }

        private void RetractDynamicBeliefs()
        {
            Retract(new VaccinePos(ID, X, Y));
            Retract(new VirusPos(X, Y));
            Retract(new VirusVector(DX, DY));
        }

        private List<Dictionary<Variable, Constant>> GetUnifications(Predicate query)
        {
            var list = new List<Dictionary<Variable, Constant>>();
            foreach (var f in _facts)
            {
                if (f.Name == query.Name) // Quick check
                {
                    var sub = FOLUtils.Unify(query, f);
                    if (sub != null) list.Add(sub);
                }
            }
            return list;
        }

        private bool QueryExists(Predicate query)
        {
            return GetUnifications(query).Count > 0;
        }

        // --- Misc Helpers ---

        private Vector2Int? QueryActiveEscape(int duration)
        {
            var results = GetUnifications(new EscapeState(X, Y, T));
            if (results.Count > 0)
            {
                var sub = results[0];
                int startT = int.Parse(sub[T].Value);

                if (_currentTime - startT > duration)
                {
                    Retract(new EscapeState(X, Y, T));
                    return null;
                }
                return new Vector2Int(int.Parse(sub[X].Value), int.Parse(sub[Y].Value));
            }
            return null;
        }

        private void RetractEscapeState()
        {
            Retract(new EscapeState(X, Y, T));
        }

        private bool InferOscillation()
        {
            // Check if we were here 2 and 4 ticks ago
            var t2 = new Constant((_currentTime - 2).ToString());
            var t4 = new Constant((_currentTime - 4).ToString());
            var cx = new Constant(_myPos.x.ToString());
            var cy = new Constant(_myPos.y.ToString());

            bool here2 = QueryExists(new VisitedAtTime(cx, cy, t2));
            bool here4 = QueryExists(new VisitedAtTime(cx, cy, t4));

            return here2 && here4;
        }

        private Vector2Int? FindFarthestSafeTile()
        {
            var safe = QueryAllSafeCoords();
            if (safe.Count == 0) return null;

            Vector2Int? best = null;
            int maxDist = -1;

            foreach (var t in safe)
            {
                int d = ManhattanDist(_myPos, t);
                if (d > maxDist)
                {
                    maxDist = d;
                    best = t;
                }
            }
            return best;
        }

        private Vector2Int? FindNearestSafeToTarget(Vector2Int target)
        {
            var safe = QueryAllSafeCoords();
            return PathUtils.FindNearestCoord(target, safe, safe);
        }

        private string GetSafeFallbackMove()
        {
            var safeMoves = new List<string>();
            foreach (var kvp in TypesUtils.MOVES)
            {
                if (kvp.Key == "WAIT") continue;
                Vector2Int next = _myPos + kvp.Value;
                if (IsPositionSafe(next)) safeMoves.Add(kvp.Key);
            }

            if (safeMoves.Count > 0) return safeMoves[Random.Range(0, safeMoves.Count)];
            return "WAIT";
        }

        private int ManhattanDist(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}