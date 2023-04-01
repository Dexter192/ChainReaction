using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementManager : MonoBehaviour
{

    private PlayerInputHandler playerInputHandler;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private new Rigidbody2D rigidbody;
    public Vector2 PlayerVelocity { get => rigidbody.velocity; }


    [SerializeField] private GameObject playerMovementComponent;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerMovement = playerMovementComponent.GetComponent<PlayerMovement>();
        playerJump = playerMovementComponent.GetComponent<PlayerJump>();
        playerInputHandler.OnJumpEvent += () => {
            rigidbody.velocity = playerJump.Jump(rigidbody.velocity);
        };
    }

    public void ManageMovement()
    {
        rigidbody.velocity = playerMovement.CalculateNewPlayerVelocity(playerInputHandler.MovementInputVector, rigidbody.velocity);
        playerJump.ResetDoubleJump();
    }
}
