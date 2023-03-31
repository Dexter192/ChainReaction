using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    private PlayerAnimation playerAnimation;
    private PlayerSquish playerSquish;
    private PlayerState playerState;
    private void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerState = GetComponentInParent<PlayerState>();
        playerSquish = GetComponent<PlayerSquish>();
    }

    public void SetupAnimations(Vector2 velocity)
    {
        playerState.UpdatePlayerState(velocity);
        playerAnimation.SetupAnimations(playerState.CurrentState);
        playerSquish.Squish(playerState);
    }
}
