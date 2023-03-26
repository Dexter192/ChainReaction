using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
    private Animator anim;
    private Playerhandler _playerhandler;
    [SerializeField] private AudioSource deathSound;
    private Rigidbody2D rb;
    private Vector3 spawnPos;

    public void Reset(InputAction.CallbackContext ctx)
    {
        if (ctx.action.triggered) KillPlayers();
    }

    public void Die()
    {
        anim.SetTrigger("death");
        rb.bodyType = RigidbodyType2D.Static;
    }
    private void KillPlayers()
    {
        deathSound.Play();
        LinkedList<GameObject> playerList = _playerhandler.GetPlayerList();
        for (LinkedListNode<GameObject> player = playerList.First; player != null; player = player.Next)
        {
            player.Value.GetComponent<PlayerLife>().Die();
        }
    }

    private void MakePlayerDynamic()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        anim.SetInteger("state", (int)0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            KillPlayers();
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Spawn()
    {
        transform.position = spawnPos;
        Rope rope = transform.GetComponentInChildren<Rope>();
        if (rope != null)
        {
            rope.ResetRopes();
        }
        anim.SetTrigger("spawn");
    }

    private void Start()
    {
        spawnPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        _playerhandler = Playerhandler.Instance;
    }

}