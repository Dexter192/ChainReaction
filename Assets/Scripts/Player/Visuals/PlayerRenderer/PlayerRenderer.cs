using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public bool IsSpriteFlipped { get => spriteRenderer.flipX; }

    public void RenderPlayer(Vector2 movementInput)
    {
        if (movementInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (movementInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

}
