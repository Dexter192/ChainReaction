using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 playerVelocity { get; private set; }
    [SerializeField] private float movementSpeed = 7f;

    public Vector2 CalculateNewPlayerVelocity(Vector2 playerInputMovement, Vector2 currentVelocity)
    {
        playerVelocity = new Vector2(playerInputMovement.x * movementSpeed, currentVelocity.y);
        return playerVelocity;
    }
}
