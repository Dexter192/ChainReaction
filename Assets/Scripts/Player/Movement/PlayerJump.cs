using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private AudioSource jumpSound;
    private PlayerGrounded playerGrounded;

    private bool canDoubleJump;

    void Start()
    {
        playerGrounded = GetComponent<PlayerGrounded>();
    }

    public Vector2 Jump(Vector2 currentVelocity)
    {
        if (CanJump())
        {
            jumpSound.Play();
            currentVelocity = new Vector2(currentVelocity.x, jumpForce);
        }
        return currentVelocity;
    }

    private bool CanJump()
    {
        if (playerGrounded.isGrounded())
        {
            canDoubleJump = true;
            return true;
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            return true;
        }
        return false;
    }

    public void ResetDoubleJump()
    {
        if (!canDoubleJump && playerGrounded.isGrounded())
        {
            canDoubleJump = true;
        }
    }


}
