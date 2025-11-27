using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pacman.Agents;
using Pacman.Agents.KBs;
using Pacman.Utils;

namespace Pacman.Agents
{
    public class VaccineController : MonoBehaviour
    {
        [Header("Configuration")]
        public VaccineType vaccineType;
        public float moveSpeed = 5.0f;
        public float decisionInterval = 0.5f; // 2 decisions per second
        public LayerMask wallLayer; // Assign "Wall" layer here
        public LayerMask virusLayer; // Assign "Virus" layer here (for vision)

        [Header("Debug Info")]
        [SerializeField] private string currentAction = "WAIT";
        [SerializeField] private Vector2Int gridPos;

        // The Logic Core
        private Vaccine _logicAgent;
        private Rigidbody _rb;
        private Vector3 _targetVelocity;
        private bool _isMoving = false;

        // Enums to select strategy in Inspector
        public enum VaccineType
        {
            Orange, // KB_A (Reactive)
            Pink,   // KB_A (Reactive)
            Blue,   // KB_B (Optimistic)
            Red     // KB_C (Overlord)
        }

        void Start()
        {
            _rb = GetComponent<Rigidbody>();

            // 1. Initialize the correct Brain based on Inspector selection
            IKnowledgeBase kb = null;
            switch (vaccineType)
            {
                case VaccineType.Orange:
                case VaccineType.Pink:
                    kb = new KnowledgeBaseA();
                    break;
                case VaccineType.Blue:
                    kb = new KnowledgeBaseB();
                    break;
                case VaccineType.Red:
                    kb = new KnowledgeBaseC();
                    break;
            }

            _logicAgent = new Vaccine(kb);

            // 2. Start the Brain Loop (0.5s heartbeat)
            StartCoroutine(DecisionLoop());
        }

        void FixedUpdate()
        {
            // 3. Apply Continuous Physics (Run every physics tick)

            // Apply velocity smoothly
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, _targetVelocity, Time.fixedDeltaTime * 10f);

            // Lane Snapping: If moving North/South, snap X to center. If East/West, snap Z.
            SnapToGridCenter();
        }

        IEnumerator DecisionLoop()
        {
            // Wait a tiny bit for physics to settle
            yield return new WaitForSeconds(0.1f);

            while (true)
            {
                // --- STEP 1: SENSE (Body -> Brain) ---
                Vector2Int myPos = GetGridPosition();
                gridPos = myPos; // For debug inspector

                // Perform Raycasts to see Walls, Virus, and other Vaccines
                var (virusPos, percepts) = PerformSensors(myPos);

                // (For now, we pass an empty list for 'otherVaccines' unless you implement a manager to track them)
                var otherVaccines = new List<(string, Vector2Int)>();

                // --- STEP 2: THINK (Brain) ---
                // This runs your C# Logic classes (KB_A, KB_B, etc.)
                string action = _logicAgent.GetNextMove(myPos, virusPos, otherVaccines, percepts);
                currentAction = action;

                // --- STEP 3: ACT (Brain -> Body) ---
                ApplyMove(action);

                // Wait for next turn
                yield return new WaitForSeconds(decisionInterval);
            }
        }

        // --- HELPER: SENSORS ---

        private (Vector2Int? virusPos, Dictionary<Vector2Int, string> percepts) PerformSensors(Vector2Int center)
        {
            var percepts = new Dictionary<Vector2Int, string>();
            Vector2Int? visibleVirus = null;

            // Check 4 directions
            foreach (var kvp in TypesUtils.MOVES)
            {
                string dirName = kvp.Key;
                if (dirName == "WAIT") continue;

                Vector2Int dirVec = kvp.Value; // (0,1), (1,0), etc.

                // Convert Logic Direction (2D) to World Direction (3D)
                // Logic Y (Up) = World Z (Forward)
                Vector3 worldDir = new Vector3(dirVec.x, 0, dirVec.y);

                // Cast rays up to 4 tiles away (Vision range)
                for (int i = 1; i <= 4; i++)
                {
                    Vector2Int targetTile = center + (dirVec * i);
                    Vector3 rayOrigin = transform.position + new Vector3(0, 0.5f, 0); // Lift up slightly

                    // Check for Wall
                    if (Physics.Raycast(rayOrigin, worldDir, i, wallLayer))
                    {
                        percepts[targetTile] = "WALL";
                        break; // Vision blocked
                    }

                    // Check for Virus
                    if (Physics.Raycast(rayOrigin, worldDir, out RaycastHit hit, i, virusLayer))
                    {
                        // Double check distance to ensure it's actually on *that* tile
                        if (Vector3.Distance(transform.position, hit.transform.position) < i + 0.5f)
                        {
                            visibleVirus = targetTile;
                        }
                    }

                    // If empty, mark as safe/empty logic handled by KB default
                    percepts.TryAdd(targetTile, "EMPTY");
                }
            }

            return (visibleVirus, percepts);
        }

        // --- HELPER: MOVEMENT & PHYSICS ---

        private void ApplyMove(string action)
        {
            Vector3 dir = Vector3.zero;

            switch (action)
            {
                case "UP": dir = Vector3.forward; break; // Z+
                case "DOWN": dir = Vector3.back; break; // Z-
                case "LEFT": dir = Vector3.left; break; // X-
                case "RIGHT": dir = Vector3.right; break; // X+
                case "WAIT": dir = Vector3.zero; break;
            }

            _targetVelocity = dir * moveSpeed;

            // If we are moving, we enable snapping logic in FixedUpdate
            _isMoving = (action != "WAIT");
        }

        private void SnapToGridCenter()
        {
            if (!_isMoving) return;

            Vector3 pos = transform.position;
            float snapSpeed = 10f * Time.fixedDeltaTime;

            // If moving along Z (Up/Down), align X to nearest integer
            if (Mathf.Abs(_targetVelocity.z) > 0.1f)
            {
                float targetX = Mathf.Round(pos.x);
                float newX = Mathf.Lerp(pos.x, targetX, snapSpeed);
                _rb.MovePosition(new Vector3(newX, pos.y, pos.z));
            }
            // If moving along X (Left/Right), align Z to nearest integer
            else if (Mathf.Abs(_targetVelocity.x) > 0.1f)
            {
                float targetZ = Mathf.Round(pos.z);
                float newZ = Mathf.Lerp(pos.z, targetZ, snapSpeed);
                _rb.MovePosition(new Vector3(pos.x, pos.y, newZ));
            }
        }

        private Vector2Int GetGridPosition()
        {
            return new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.z)
            );
        }
    }

}