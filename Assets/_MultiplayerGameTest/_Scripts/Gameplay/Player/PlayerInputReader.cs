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
        private readonly InputAction _spawnItem;
        private readonly InputAction _interact;
        private readonly InputAction _throwItem;
        private readonly InputAction _deleteItem;

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
            _spawnItem = new InputAction("SpawnItem", InputActionType.Button, "<Keyboard>/q");
            _interact = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
            _throwItem = new InputAction("Throw", InputActionType.Button, "<Mouse>/leftButton");
            _deleteItem = new InputAction("Delete", InputActionType.Button, "<Keyboard>/x");

            _move.Enable();
            _look.Enable();
            _jump.Enable();
            _toggleCursor.Enable();
            _spawnItem.Enable();
            _interact.Enable();
            _throwItem.Enable();
            _deleteItem.Enable();
        }

        public Vector2 Move => _move.ReadValue<Vector2>();
        public Vector2 Look => _look.ReadValue<Vector2>();
        public bool JumpPressed => _jump.WasPressedThisFrame();
        public bool ToggleCursorPressed => _toggleCursor.WasPressedThisFrame();
        public bool SpawnItemPressed => _spawnItem.WasPressedThisFrame();
        public bool InteractPressed => _interact.WasPressedThisFrame();
        public bool ThrowPressed => _throwItem.WasPressedThisFrame();
        public bool DeletePressed => _deleteItem.WasPressedThisFrame();

        public void Dispose()
        {
            _move.Dispose();
            _look.Dispose();
            _jump.Dispose();
            _toggleCursor.Dispose();
            _spawnItem.Dispose();
            _interact.Dispose();
            _throwItem.Dispose();
            _deleteItem.Dispose();            
        }  
    }
}