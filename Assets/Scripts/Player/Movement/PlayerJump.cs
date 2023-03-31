using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private AudioSource jumpSound;
    private PlayerGrounded playerGrounded;

    private Rigidbody2D rigidbody;
    private bool canDoubleJump;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();  
        playerGrounded = GetComponentInParent<PlayerGrounded>();
    }

    public void Jump()
    {
        if (CanJump())
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpForce);
            jumpSound.Play();    
        }
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
