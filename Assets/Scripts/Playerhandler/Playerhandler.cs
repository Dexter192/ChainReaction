using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerhandler : MonoBehaviour
{
    public static LinkedList<GameObject> playerList = new LinkedList<GameObject>();
    private static Playerhandler _instance;
    [SerializeField] private GameObject[] playerPrefabs;

    public static Playerhandler Instance { get; private set; }

    public void AddPlayer(GameObject player)
    {
        playerList.AddLast(player);
    }

    public LinkedList<GameObject> GetPlayerList()
    {
        return playerList;
    }

    public GameObject[] GetPlayerPrefabs()
    {
        return playerPrefabs;
    }

    public GameObject GetPreviousPlayer(GameObject Player)
    {
        for (LinkedListNode<GameObject> p = playerList.First; p != null; p = p.Next)
        {
            if (p.Value.GetInstanceID() == Player.GetInstanceID())
            {
                LinkedListNode<GameObject> PreviousPlayer = p.Previous;
                // Return null if we are the first player
                if (PreviousPlayer == null)
                {
                    return null;
                }
                else
                {
                    return PreviousPlayer.Value;
                }
            }
        }

        return null;
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

    }

}