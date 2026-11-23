using _MultiplayerGameTest._Scripts.Core.Pause;
using Unity.Netcode;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;
        [SerializeField] private PauseState pauseState;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Look")]
        [SerializeField] private float lookSensitivity = 0.1f;
        [SerializeField] private float maxPitch = 80f;

        private PlayerInputReader _input;
        private float _pitch;
        private float _verticalVelocity;

        public PlayerInputReader InputReader => _input;

        public bool InputActive => _input != null && !pauseState.IsPaused;

        public override void OnNetworkSpawn()
        {
            var isLocal = IsOwner;

            controller.enabled = isLocal;
            playerCamera.enabled = isLocal;
            audioListener.enabled = isLocal;

            if (!isLocal)
            {
                enabled = false;
                return;
            }

            _input = new PlayerInputReader();
            pauseState.SetPaused(false);
            pauseState.Changed += ApplyCursor;
            ApplyCursor();
        }

        public override void OnNetworkDespawn()
        {
            if (_input == null)
                return;

            pauseState.Changed -= ApplyCursor;
            _input.Dispose();
            _input = null;

            pauseState.SetPaused(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (_input == null)
                return;

            if (_input.ToggleCursorPressed)
                pauseState.SetPaused(!pauseState.IsPaused);

            var active = !pauseState.IsPaused;
            var move = active ? _input.Move : Vector2.zero;
            var look = active ? _input.Look : Vector2.zero;
            var jump = active && _input.JumpPressed;

            ApplyLook(look);
            ApplyMovement(move, jump);
        }

        private void ApplyLook(Vector2 look)
        {
            transform.Rotate(0f, look.x * lookSensitivity, 0f);
            _pitch = Mathf.Clamp(_pitch - look.y * lookSensitivity, -maxPitch, maxPitch);
            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void ApplyMovement(Vector2 move, bool jump)
        {
            var direction = transform.right * move.x + transform.forward * move.y;
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            if (controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (jump && controller.isGrounded)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            _verticalVelocity += gravity * Time.deltaTime;

            var velocity = direction * moveSpeed + Vector3.up * _verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void ApplyCursor()
        {
            var locked = !pauseState.IsPaused;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}