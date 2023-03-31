using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrounded : MonoBehaviour
{

    private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask jumpableGround;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
    public bool isGrounded()
    {
        // Checks if the box that we create is overlapping with the 2nd box
        return Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size,
                          0f, Vector2.down, .1f, jumpableGround);
    }

}
