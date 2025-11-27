using UnityEngine;
using Pacman.Utils; // Access TypesUtils if needed

namespace Pacman.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class VirusController : MonoBehaviour
    {
        [Header("Configuration")]
        public float moveSpeed = 5.0f;
        public float snapStrength = 10.0f;

        private Rigidbody _rb;
        private Vector3 _targetVelocity;
        private bool _isMoving = false;
        private string _currentDir = "WAIT";
        private string _nextDir = "WAIT";

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            // Ensure physics constraints are set correctly via code or Inspector
            _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }

        void Update()
        {
            // 1. Read Input (Every Frame) and Buffer the Next Move
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) _nextDir = "UP";
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) _nextDir = "DOWN";
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) _nextDir = "LEFT";
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) _nextDir = "RIGHT";

            // 2. Try to Execute the Buffered Move
            if (_nextDir != "WAIT")
            {
                if (CanMoveInDirection(_nextDir))
                {
                    _currentDir = _nextDir;
                    _nextDir = "WAIT"; // Consume the buffer
                    ApplyMove(_currentDir);
                }
                else
                {
                    // We can't turn yet (e.g., wall to the left), so keep moving straight
                    // But ONLY if straight is also valid. If straight is blocked, we stop.
                    if (CanMoveInDirection(_currentDir))
                        ApplyMove(_currentDir);
                    else
                        ApplyMove("WAIT"); // Stuck
                }
            }
            else
            {
                // No input, keep momentum unless blocked
                if (CanMoveInDirection(_currentDir))
                    ApplyMove(_currentDir);
                else
                    ApplyMove("WAIT");
            }
        }

        void FixedUpdate()
        {
            // Apply Physics (Fixed Interval)

            // Smoothly interpolate velocity for responsive feel
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, _targetVelocity, Time.fixedDeltaTime * 15f);

            // Lane Snapping
            SnapToGridCenter();
        }

        // --- MOVEMENT HELPERS ---

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
            _isMoving = (action != "WAIT" && dir != Vector3.zero);
        }

        private void SnapToGridCenter()
        {
            if (!_isMoving) return;

            Vector3 pos = transform.position;
            float snap = snapStrength * Time.fixedDeltaTime;

            // If moving North/South (Z), snap X to nearest integer
            if (Mathf.Abs(_targetVelocity.z) > 0.1f)
            {
                float targetX = Mathf.Round(pos.x);
                float newX = Mathf.Lerp(pos.x, targetX, snap);
                _rb.MovePosition(new Vector3(newX, pos.y, pos.z));
            }
            // If moving East/West (X), snap Z to nearest integer
            else if (Mathf.Abs(_targetVelocity.x) > 0.1f)
            {
                float targetZ = Mathf.Round(pos.z);
                float newZ = Mathf.Lerp(pos.z, targetZ, snap);
                _rb.MovePosition(new Vector3(pos.x, pos.y, newZ));
            }
        }


        // Helper: Check if a move is valid (Raycast)
        private bool CanMoveInDirection(string dirName)
        {
            if (dirName == "WAIT") return false;

            Vector3 dirVec = Vector3.zero;
            switch (dirName)
            {
                case "UP": dirVec = Vector3.forward; break;
                case "DOWN": dirVec = Vector3.back; break;
                case "LEFT": dirVec = Vector3.left; break;
                case "RIGHT": dirVec = Vector3.right; break;
            }

            // Raycast slightly ahead (0.6f is just over half a tile) to see if wall exists
            // Adjust layer mask as needed (assuming "Wall" layer is defined)
            return !Physics.Raycast(transform.position, dirVec, 0.6f, LayerMask.GetMask("Wall"));
        }

        // --- COLLISION LOGIC ---

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Vaccine"))
            {
                if (Pacman.GameManager.Instance != null)
                    Pacman.GameManager.Instance.HandleDeath();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 2. Hit a Pellet -> Score
            if (other.CompareTag("Pellet"))
            {
                // Notify Manager BEFORE destroying the object
                if (Pacman.GameManager.Instance != null)
                    Pacman.GameManager.Instance.PelletEaten(1);

                Destroy(other.gameObject);
            }
        }
    }
}