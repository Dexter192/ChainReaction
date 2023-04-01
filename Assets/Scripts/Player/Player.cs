using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private PlayerRenderer playerRenderer;
    private PlayerInputHandler playerInputHandler;
    private PlayerAnimationManager playerAnimationManager;
    private PlayerMovementManager playerMovementManager;

    [SerializeField] private GameObject visuals;

    private void Start()
    {
        playerInputHandler = GetComponent<PlayerInputHandler>();
        playerRenderer = visuals.GetComponent<PlayerRenderer>();
        playerAnimationManager = visuals.GetComponent<PlayerAnimationManager>();
        playerMovementManager = GetComponent<PlayerMovementManager>();
    }

    private void Update()
    {
        playerMovementManager.ManageMovement();
        playerRenderer.RenderPlayer(playerInputHandler.MovementInputVector);
        playerAnimationManager.SetupAnimations(playerMovementManager.PlayerVelocity);
    }
}