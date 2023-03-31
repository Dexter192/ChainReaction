using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationInvoke : MonoBehaviour
{
    PlayerLife playerLife;
    void Awake()
    {
        playerLife = gameObject.transform.parent.GetComponent<PlayerLife>();
    }

    private void Spawn()
    {
        playerLife.Spawn();
    }

    private void MakePlayerDynamic()
    {
        playerLife.MakePlayerDynamic();
    }
}
