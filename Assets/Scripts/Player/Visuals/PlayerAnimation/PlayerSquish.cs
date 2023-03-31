using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerSquish : MonoBehaviour
{
    PlayerGrounded playerGrounded;
    Vector2 originalScale;
    Vector2 originalPosition;
    bool coroutineRunning;
    float xSquish = 0.3f;
    float ySquish = -0.2f;
    float yOffset = -0.1f;

    private void Start()
    {
        playerGrounded = GetComponentInParent<PlayerGrounded>();
    }
    public void Squish(PlayerState playerState)
    {
        CheckLanding(playerState);
    }

    private void OnEnable()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        coroutineRunning = false;
    }
    private void CheckLanding(PlayerState playerState)
    {
        //check if the character has landed if yes, squish!
        if (playerState.LastState == PlayerState.MovementState.falling && playerGrounded.isGrounded())
        {
            //reset if its already running
            if (coroutineRunning == true)
            {
                StopCoroutine("Squish");
                coroutineRunning = false;
                SquishReset();
            }
            //squish it
            IEnumerator coroutine = Squish(0.15f);
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator Squish(float time)
    {
        ApplySquish();
        yield return new WaitForSeconds(time);
        SquishReset();
    }

    void ApplySquish()
    {
        Vector2 squishScale = new(originalScale.x + xSquish, originalScale.y + ySquish);
        Vector2 squishPosition = new(originalPosition.x, originalPosition.y + yOffset);
        transform.localScale = squishScale;
        transform.localPosition = squishPosition;
        coroutineRunning = true;
    }

    void SquishReset()
    {
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;
    }

}
