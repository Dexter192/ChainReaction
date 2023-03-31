using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerState : MonoBehaviour
{
    public enum MovementState { idle, running, jumping, falling };
    public MovementState CurrentState { get; private set; }
    public MovementState LastState { get; private set; }

    private void Start()
    {
        CurrentState = MovementState.idle;
    }

    public void UpdatePlayerState(Vector2 velocity)
    {
        LastState = CurrentState;
        if (Mathf.Abs(velocity.x) > 0f)
        {
            CurrentState = MovementState.running;
        }
        else
        {
            CurrentState = MovementState.idle;
        }

        if (velocity.y > .1f)
        {
            CurrentState = MovementState.jumping;
        }
        else if (velocity.y < -.1f)
        {
            CurrentState = MovementState.falling;
        }

    }
}
