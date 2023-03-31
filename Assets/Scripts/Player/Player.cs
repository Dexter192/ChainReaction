using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private PlayerRenderer playerRenderer;
    private PlayerInputHandler playerInputHandler;
    private PlayerAnimationManager playerAnimationManager;

    [SerializeField] private GameObject visuals;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerJump = GetComponent<PlayerJump>();
        playerInputHandler.OnJumpEvent += () => playerJump.Jump();
        playerRenderer = visuals.GetComponent<PlayerRenderer>();
        playerAnimationManager = visuals.GetComponent<PlayerAnimationManager>();
    }

    private void Update()
    {
        playerMovement.MovePlayer(playerInputHandler.MovementInputVector);
        playerRenderer.RenderPlayer(playerInputHandler.MovementInputVector);
        playerAnimationManager.SetupAnimations(playerMovement.playerVelocity);

        playerJump.ResetDoubleJump();
    }
}