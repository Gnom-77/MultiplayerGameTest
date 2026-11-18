using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _MultiplayerGameTest._Scripts.Gameplay.Player
{
    public class PlayerInputReader : IDisposable
    {
        private readonly InputAction _move;
        private readonly InputAction _look;
        private readonly InputAction _jump;
        private readonly InputAction _toggleCursor;

        public PlayerInputReader()
        {
            _move = new InputAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            _jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            _toggleCursor = new InputAction("ToggleCursor", InputActionType.Button, "<Keyboard>/escape");

            _move.Enable();
            _look.Enable();
            _jump.Enable();
            _toggleCursor.Enable();
        }

        public Vector2 Move => _move.ReadValue<Vector2>();
        public Vector2 Look => _look.ReadValue<Vector2>();
        public bool JumpPressed => _jump.WasPressedThisFrame();
        public bool ToggleCursorPressed => _toggleCursor.WasPressedThisFrame();

        public void Dispose()
        {
            _move.Dispose();
            _look.Dispose();
            _jump.Dispose();
            _toggleCursor.Dispose();
        }  
    }
}