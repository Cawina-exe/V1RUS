using UnityEngine;
using Pacman.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;

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

        private float _rayDistance = 0.6f;
        public LayerMask wallLayer;
        
        [Header("VFXs")]
        public VisualEffect pelletEatenFX;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            
            _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }

        void Update()
        {
        
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) _nextDir = "UP";
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) _nextDir = "DOWN";
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) _nextDir = "LEFT";
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) _nextDir = "RIGHT";

            if (_nextDir != "WAIT")
            {
                if (CanMoveInDirection(_nextDir))
                {
                    _currentDir = _nextDir;
                    _nextDir = "WAIT";
                    ApplyMove(_currentDir);
                }
                else
                {
                    if (CanMoveInDirection(_currentDir)) ApplyMove(_currentDir);
                    else ApplyMove("WAIT");
                }
            }
            else
            {
                if (CanMoveInDirection(_currentDir)) ApplyMove(_currentDir);
                else ApplyMove("WAIT");
            }
        }

        void FixedUpdate()
        {
           
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, _targetVelocity, Time.fixedDeltaTime * 15f);
            SnapToGridCenter();
        }

        private void ApplyMove(string action)
        {
            Vector3 dir = Vector3.zero;
            switch (action)
            {
                case "UP": dir = Vector3.forward; break;
                case "DOWN": dir = Vector3.back; break;
                case "LEFT": dir = Vector3.left; break;
                case "RIGHT": dir = Vector3.right; break;
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

            if (Mathf.Abs(_targetVelocity.z) > 0.1f)
            {
                float targetX = Mathf.Round(pos.x);
                float newX = Mathf.Lerp(pos.x, targetX, snap);
                _rb.MovePosition(new Vector3(newX, pos.y, pos.z));
            }
            else if (Mathf.Abs(_targetVelocity.x) > 0.1f)
            {
                float targetZ = Mathf.Round(pos.z);
                float newZ = Mathf.Lerp(pos.z, targetZ, snap);
                _rb.MovePosition(new Vector3(pos.x, pos.y, newZ));
            }
        }

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
            return !Physics.Raycast(transform.position, dirVec, _rayDistance, wallLayer);
        }

      

       
        private void OnCollisionEnter(Collision collision)
        {
           
            if (collision.gameObject.CompareTag("Vaccine"))
            {
                Debug.Log("HIT VACCINE!"); 
                if (Pacman.PacmanMiniGame.Instance != null)
                {
                    Pacman.PacmanMiniGame.Instance.HandleDeath();
                }
            }
        }

        
        private void OnTriggerEnter(Collider other)
        {
           
            if (other.CompareTag("Pellet"))
            {
                if (Pacman.PacmanMiniGame.Instance != null)
                {
                    Pacman.PacmanMiniGame.Instance.PelletEaten(1);
                    pelletEatenFX.Play();
                }
                Destroy(other.gameObject);
            }
        }

        //private IEnumerator PelletFX()
        //{
        //    yield return new WaitForSeconds(0.4f);
        //    pelletEatenFX.SetActive(false);
        //}
    }
}