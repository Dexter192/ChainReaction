using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    public Vector2 playerVelocity { get => rigidbody.velocity; }
    [SerializeField] private float movementSpeed = 7f;
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void MovePlayer(Vector2 movementVector)
    {
        rigidbody.velocity = new Vector2(movementVector.x * movementSpeed, rigidbody.velocity.y);
    }
}
