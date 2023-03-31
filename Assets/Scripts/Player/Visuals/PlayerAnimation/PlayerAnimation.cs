using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;

public class PlayerAnimation : MonoBehaviour
{

    private Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void SetupAnimations(PlayerState.MovementState currentState)
    {
        anim.SetInteger("state", (int)currentState);
    }
}
