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
        playerGrounded = GetComponent<PlayerGrounded>();
    }
    public void Squish(PlayerState playerState)
    {
        if (CheckLanding(playerState)) { 
            //reset if its already running
            if (coroutineRunning == true)
            {
                StopCoroutine("SquishCoroutine");
                coroutineRunning = false;
                SquishReset();
            }
            //squish it
            IEnumerator coroutine = SquishCoroutine(0.15f);
            StartCoroutine(coroutine);
        }
    }

    private void OnEnable()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        coroutineRunning = false;
    }
    private bool CheckLanding(PlayerState playerState)
    {
        return playerState.LastState == PlayerState.MovementState.falling && playerGrounded.isGrounded();
    }

    private IEnumerator SquishCoroutine(float time)
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
