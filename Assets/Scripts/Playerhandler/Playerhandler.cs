using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Playerhandler : MonoBehaviour
{
    public static LinkedList<GameObject> playerList = new();
    public static int MAX_PLAYERS { get; } = 3;
    [SerializeField] private GameObject[] playerPrefabs;


    private static Playerhandler instance;
    public static Playerhandler Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(Playerhandler)) as Playerhandler;

            return instance;
        }
        set
        {
            instance = value;
        }
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

    public void AddPlayer(GameObject player)
    {
        playerList.AddLast(player);
    }

    public void SwitchPlayer()
    {

    }

    public LinkedList<GameObject> GetPlayerList()
    {
        return playerList;
    }

    public GameObject[] GetPlayerPrefabs()
    {
        return playerPrefabs;
    }

    public GameObject GetPlayerPrefab(int idx)
    {
        return playerPrefabs[idx];
    }

    public GameObject GetPreviousPlayerCircular(GameObject player)
    {
        GameObject previousPlayer = GetPreviousPlayer(player);
        if (previousPlayer == null)
        {
            return playerList.Last.Value;
        }
        return previousPlayer;
    }

    public GameObject GetPreviousPlayer(GameObject player)
    {
        LinkedListNode<GameObject> currentPlayer = playerList.Find(player);
        if (currentPlayer.Previous == null)
        {
            return null;
        }
        else
        {
            return currentPlayer.Previous.Value;
        }
    }

    // Returns the next player. If the player passed in the argument is the last player in the list, return the first player
    public GameObject GetNextPlayerCircular(GameObject player)
    {
        GameObject nextPlayer = GetNextPlayer(player);
        if (nextPlayer == null)
        {
            return playerList.First.Value;
        }
        return nextPlayer;
    }
    public GameObject GetNextPlayer(GameObject player)
    {
        LinkedListNode<GameObject> currentPlayer = playerList.Find(player);
        if (currentPlayer.Next == null)
        {
            return null;
        }
        else
        {
            return currentPlayer.Next.Value;
        }
    }

    public int GetPlayerCount()
    {
        return playerList.Count;
    }

}