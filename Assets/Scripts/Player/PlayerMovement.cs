using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Animator anim;

    private bool canDoubleJump;
    private BoxCollider2D coll;
    private Vector3 lastPosition;

    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private AudioSource jumpSound;
    [SerializeField] private LayerMask jumpableGround;

    private bool jumped;
    [SerializeField] private float moveSpeed = 7f;
    private Vector2 movementInput;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private GameObject visuals;


    public enum MovementState { idle, running, jumping, falling };
    private MovementState currentState;


    public void AddVelocity(Vector2 velocity)
    {
        rb.velocity = rb.velocity + velocity;
    }
    public void SetVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }

    public void OnJump(InputAction.CallbackContext ctx) => jumped = ctx.performed;
    public void OnMove(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = visuals.GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        spriteRenderer = visuals.GetComponent<SpriteRenderer>();

        this.currentState = MovementState.idle;

        canDoubleJump = true;
        this.lastPosition = transform.position;
    }
    // Update is called once per frame
    private void Update()
    {
        rb.velocity = new Vector2(movementInput.x * moveSpeed, rb.velocity.y);

        updateAnimationState();
        resetDoubleJump();
        preventPlayerLeavingScreen();
        CheckLanding(currentState);
        // Autoformat with: Alt + Shift + F
        if (jumped && canJump())
        {
            jumpSound.Play();
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumped = false;
        }
    }

    private bool canJump()
    {
        if (isGrounded())
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
    private bool isGrounded()
    {
        // Checks if the box that we create is overlapping with the 2nd box
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size,
                          0f, Vector2.down, .1f, jumpableGround);
    }

    private void preventPlayerLeavingScreen()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        //Debug.Log(screenPos);
        return;
        if (screenPos.x < Screen.width)
        {
            transform.position = new Vector2(screenPos.x, transform.position.y);
        }

        else if (screenPos.x > Screen.width)
        {
            transform.position = new Vector2(Screen.width, transform.position.y);
        }
    }

    private void resetDoubleJump()
    {
        if (!canDoubleJump && isGrounded())
        {
            canDoubleJump = true;
        }
    }
    private void updateAnimationState()
    {
        if (rb.velocity.x > 0f)
        {
            spriteRenderer.flipX = false;
            currentState = MovementState.running;
        }
        else if (rb.velocity.x < 0f)
        {
            spriteRenderer.flipX = true;
            currentState = MovementState.running;
        }
        else
        {
            currentState = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            currentState = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            currentState = MovementState.falling;
        }

        anim.SetInteger("state", (int)currentState);
    }

    public MovementState GetMovementState()
    {
        return currentState;
    }


    #region Squish

    Transform visualChildTransform;
    Vector2 originalScale;
    Vector2 originalPosition;
    MovementState lastState;
    bool coroutineRunning;
    float xSquish = 0.3f;
    float ySquish = -0.2f;
    float yOffset = -0.1f;

    private void OnEnable()
    {
        visualChildTransform = visuals.transform;
        originalScale = visualChildTransform.localScale;
        originalPosition = visualChildTransform.localPosition;
        coroutineRunning = false;
    }
    private void CheckLanding(MovementState state)
    {
        //check if the character has landed if yes, squish!
        if (lastState == MovementState.falling && isGrounded())
        {
            //reset if its already running
            if(coroutineRunning == true)
            {
                StopCoroutine("Squish");
                coroutineRunning = false;
                SquishReset();
            }
            //squish it
            IEnumerator coroutine = Squish(0.15f);
            StartCoroutine(coroutine);
        }
        lastState = state;
    }

    private IEnumerator Squish(float time)
    {
        ApplySquish();
        yield return new WaitForSeconds(time);
        SquishReset();
    }

    void ApplySquish()
    {        
        Vector2 squishScale = new Vector2(originalScale.x + xSquish, originalScale.y + ySquish);
        Vector2 squishPosition = new Vector2(originalPosition.x, originalPosition.y + yOffset);
        visualChildTransform.localScale = squishScale;
        visualChildTransform.localPosition = squishPosition;
        coroutineRunning = true;
    }

    void SquishReset()
    {
        visualChildTransform.localPosition = originalPosition;
        visualChildTransform.localScale = originalScale;
    }
    #endregion
}
