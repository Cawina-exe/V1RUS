using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pacman.Agents;
using Pacman.Agents.KBs;
using Pacman.Utils;

public class VaccineController : MonoBehaviour
{
    [Header("Configuration")]
    public VaccineType vaccineType;
    public float moveSpeed = 5.0f;
    public float decisionInterval = 0.5f;

    [Tooltip("Set this to match your world scale!")]
    public float tileSize = 1.0f; // SET THIS IN INSPECTOR

    public LayerMask wallLayer;
    public LayerMask virusLayer;

    [Header("Debug Info")]
    [SerializeField] private string currentAction = "WAIT";
    [SerializeField] private Vector2Int gridPos;

    private Vaccine _logicAgent;
    private Rigidbody _rb;
    private Vector3 _targetVelocity;
    private bool _isMoving = false;

    public enum VaccineType { Orange, Pink, Blue, Red }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        IKnowledgeBase kb = null;
        switch (vaccineType)
        {
            case VaccineType.Orange:
            case VaccineType.Pink: kb = new KnowledgeBaseA(); break;
            case VaccineType.Blue: kb = new KnowledgeBaseB(); break;
            case VaccineType.Red: kb = new KnowledgeBaseC(); break;
        }

        _logicAgent = new Vaccine(kb);
        StartCoroutine(DecisionLoop());
    }

    void FixedUpdate()
    {
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, _targetVelocity, Time.fixedDeltaTime * 10f);
        SnapToGridCenter();
    }

    IEnumerator DecisionLoop()
    {
        yield return new WaitForSeconds(0.1f);

        while (true)
        {
            Vector2Int myPos = GetGridPosition();
            gridPos = myPos;

            var (virusPos, percepts) = PerformSensors(myPos);
            var otherVaccines = new List<(string, Vector2Int)>();

            string action = _logicAgent.GetNextMove(myPos, virusPos, otherVaccines, percepts);
            currentAction = action;

            ApplyMove(action);
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    private (Vector2Int? virusPos, Dictionary<Vector2Int, string> percepts) PerformSensors(Vector2Int center)
    {
        var percepts = new Dictionary<Vector2Int, string>();
        Vector2Int? visibleVirus = null;

        foreach (var kvp in TypesUtils.MOVES)
        {
            string dirName = kvp.Key;
            if (dirName == "WAIT") continue;

            Vector2Int dirVec = kvp.Value;
            Vector3 worldDir = new Vector3(dirVec.  x, 0, dirVec.y);

            for (int i = 1; i <= 4; i++)
            {
                Vector2Int targetTile = center + (dirVec * i);

                // ADAPTATION: Ray origin offset scales with Tile Size
                // New: Lift up AND push forward slightly so we don't hit ourselves
                Vector3 rayOrigin = transform.position + new Vector3(0, 0.5f, 0) + (worldDir * 0.1f);

                // ADAPTATION: Ray length scales with Tile Size (i * tileSize)
                bool hitWall = Physics.Raycast(rayOrigin, worldDir, i * tileSize, wallLayer);

                Debug.DrawRay(rayOrigin, worldDir * (i * tileSize), Color.red, 0.1f);

                if (hitWall)
                {
                    percepts[targetTile] = "WALL";
                    break;
                }

                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, worldDir, out hit, i * tileSize, virusLayer))
                {
                    Debug.DrawLine(rayOrigin, hit.point, Color.green, 0.5f);

                    // Distance check scaled by Tile Size
                    if (Vector3.Distance(transform.position, hit.transform.position) < (i + 0.5f) * tileSize)
                    {
                        visibleVirus = targetTile;
                    }
                }
                percepts.TryAdd(targetTile, "EMPTY");
            }
        }
        return (visibleVirus, percepts);
    }

    private void ApplyMove(string action)
    {
        Vector3 dir = Vector3.zero;
        Vector3 sideDir = Vector3.zero; // Direction to the "side" relative to movement

        switch (action)
        {
            case "UP":
                dir = Vector3.forward;
                sideDir = Vector3.right;
                break;
            case "DOWN":
                dir = Vector3.back;
                sideDir = Vector3.right;
                break;
            case "LEFT":
                dir = Vector3.left;
                sideDir = Vector3.forward;
                break;
            case "RIGHT":
                dir = Vector3.right;
                sideDir = Vector3.forward;
                break;
            case "WAIT":
                dir = Vector3.zero;
                break;
        }

        // SAFETY CHECK: Prevent pushing into a wall (With Whiskers!)
        if (dir != Vector3.zero)
        {
            // 1. Setup Origins
            float shoulderOffset = 0.35f * tileSize; // Almost the width of the collider
            Vector3 centerOrigin = transform.position + new Vector3(0, 0.5f, 0);
            Vector3 rightWhisker = centerOrigin + (sideDir * shoulderOffset);
            Vector3 leftWhisker = centerOrigin - (sideDir * shoulderOffset);

            float checkDist = 0.55f * tileSize;

            // 2. Fire 3 Rays (Center, Left Shoulder, Right Shoulder)
            bool hitCenter = Physics.Raycast(centerOrigin, dir, checkDist, wallLayer);
            bool hitRight = Physics.Raycast(rightWhisker, dir, checkDist, wallLayer);
            bool hitLeft = Physics.Raycast(leftWhisker, dir, checkDist, wallLayer);

            // Debug drawing to see them in Scene View
            Debug.DrawRay(centerOrigin, dir * checkDist, Color.yellow);
            Debug.DrawRay(rightWhisker, dir * checkDist, Color.yellow);
            Debug.DrawRay(leftWhisker, dir * checkDist, Color.yellow);

            // 3. If ANY ray hits a wall, STOP.
            if (hitCenter || hitRight || hitLeft)
            {
                _targetVelocity = Vector3.zero;
                _isMoving = false;

                // Optional: Snap to grid immediately to fix alignment if we hit a corner
                // _rb.MovePosition(new Vector3(Mathf.Round(transform.position.x), transform.position.y, Mathf.Round(transform.position.z)));

                return;
            }
        }

        _targetVelocity = dir * moveSpeed;
        _isMoving = (action != "WAIT");
    }

    private void SnapToGridCenter()
    {
        if (!_isMoving) return;

        Vector3 pos = transform.position;
        // Reduce snap speed slightly to prevent jitter
        float snapSpeed = 5f * Time.fixedDeltaTime;

        // Calculate target
        Vector3 targetPos = pos;
        bool snapNeeded = false;

        if (Mathf.Abs(_targetVelocity.z) > 0.1f) // Moving Z
        {
            float gridX = Mathf.Round(pos.x / tileSize);
            float targetX = gridX * tileSize;
            // Only snap if we are significantly off-center (> 0.05)
            if (Mathf.Abs(pos.x - targetX) > 0.05f)
            {
                targetPos = new Vector3(Mathf.Lerp(pos.x, targetX, snapSpeed), pos.y, pos.z);
                snapNeeded = true;
            }
        }
        else if (Mathf.Abs(_targetVelocity.x) > 0.1f) // Moving X
        {
            float gridZ = Mathf.Round(pos.z / tileSize);
            float targetZ = gridZ * tileSize;
            // Only snap if we are significantly off-center (> 0.05)
            if (Mathf.Abs(pos.z - targetZ) > 0.05f)
            {
                targetPos = new Vector3(pos.x, pos.y, Mathf.Lerp(pos.z, targetZ, snapSpeed));
                snapNeeded = true;
            }
        }

        // Apply Snap ONLY if no wall blocks the slide
        if (snapNeeded)
        {
            Vector3 snapDir = (targetPos - pos).normalized;
            Vector3 rayOrigin = transform.position + new Vector3(0, 0.5f, 0);

            // Check lateral wall collision
            if (!Physics.Raycast(rayOrigin, snapDir, 0.6f * tileSize, wallLayer))
            {
                _rb.MovePosition(targetPos);
            }
        }
    }
    private Vector2Int GetGridPosition()
    {
        // ADAPTATION: Convert Big World Coords -> Small Grid Coords
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / tileSize),
            Mathf.RoundToInt(transform.position.z / tileSize)
        );
    }
}