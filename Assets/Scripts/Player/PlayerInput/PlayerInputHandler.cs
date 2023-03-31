using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MovementInputVector { get; private set; }
    public event Action OnJumpEvent;
    public void OnJump(InputAction.CallbackContext ctx) => HandleJump(ctx.performed);
    public void OnMove(InputAction.CallbackContext ctx) => MovementInputVector = ctx.ReadValue<Vector2>();

    private void HandleJump(bool jumpPerformed)
    {
        if (jumpPerformed)
        {
            OnJumpEvent?.Invoke();
        }

    }
}
