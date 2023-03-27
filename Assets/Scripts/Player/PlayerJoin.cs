using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerJoin : MonoBehaviour
{
    private Playerhandler _playerhandler;
    private CinemachineTargetGroup targetGroup;
    [SerializeField] private Rope rope;
    [SerializeField] public GameObject playerJoinManuallyObserver;

    public void Start()
    {
        _playerhandler = Playerhandler.Instance;
        
        //PlayerInput.Instantiate(_playerhandler.GetPlayerPrefabs()[0], pairWithDevice: InputSystem.devices[0]);
        //PlayerInput.Instantiate(_playerhandler.GetPlayerPrefabs()[1], pairWithDevice: InputSystem.devices[0]);
        //PlayerInput.Instantiate(_playerhandler.GetPlayerPrefabs()[2], pairWithDevice: InputSystem.devices[0]);
        //PlayerInput.Instantiate(_playerhandler.GetPlayerPrefabs()[3], pairWithDevice: Gamepad.all[0]);
    }

    private void AddPlayerToCinemachine(PlayerInput playerInput, CinemachineTargetGroup targetGroup)
    {
        Cinemachine.CinemachineTargetGroup.Target newPlayerTarget;
        newPlayerTarget.target = playerInput.transform;
        newPlayerTarget.weight = 1;
        newPlayerTarget.radius = 1;

        Array.Resize(ref targetGroup.m_Targets, targetGroup.m_Targets.Length + 1);
        targetGroup.m_Targets[targetGroup.m_Targets.Length - 1] = newPlayerTarget;
    }

    //TODO: Find the respective player and remove him from the target group
    private void OnPlayerExit(PlayerInput playerInput)
    {
    }

    public void OnPlayerJoin(PlayerInput playerInput)
    {
        // If a player without a paired device exists, assign the device to the empty player
        GameObject unassignedPlayer = GetFirstUnassignedPlayer();
        /*TODO
         * if (unassignedPlayer != null)
        {
            ActivatePlayer(unassignedPlayer, playerInput.user.pairedDevices[0]);
             PlayerInput.Destroy(playerInput);
            return;
        }*/
        // Set the player prefab
        // If the player joins by connecting a new device we need to adapt the sprite
        int prefabIndex = _playerhandler.GetPlayerCount() % Playerhandler.MAX_PLAYERS;
        PlayerInputManager.instance.playerPrefab = _playerhandler.GetPlayerPrefabs()[prefabIndex];

        // Add the player to our Linked List Player Manager
        _playerhandler.AddPlayer(playerInput.gameObject);
        

        targetGroup = GameObject.Find("CM TargetGroup1").GetComponent<CinemachineTargetGroup>();
        if (targetGroup.m_Targets.Length > 0)
        {
            Instantiate(rope.gameObject, playerInput.gameObject.transform);
        }

        Debug.Log("Number of players: " + targetGroup.m_Targets.Length);

        Debug.Log("Player Index: " + playerInput.playerIndex);
        Debug.Log("Player GameObject: " + playerInput.gameObject);

        // Add the new player to the camera controller
        AddPlayerToCinemachine(playerInput, targetGroup);
        Debug.Log("");
    }

    // Returns the first unassigned player (or null if none)
    private GameObject GetFirstUnassignedPlayer()
    {
        LinkedList<GameObject> playerList = _playerhandler.GetPlayerList();
        for (LinkedListNode<GameObject> player = playerList.First; player != null; player = player.Next)
        {
            if (player.Value.GetComponent<PlayerInput>().user.pairedDevices.Count == 0)
                return player.Value;
        }
        return null;
    }

    private void ActivatePlayer(GameObject player, InputDevice inputDevice)
    {        
        PlayerInput playerInput = player.GetComponent<PlayerInput>();

        // Assign device with player
        InputUser.PerformPairingWithDevice(inputDevice, playerInput.user, InputUserPairingOptions.UnpairCurrentDevicesFromUser);

        // Enable Actionmap
        playerInput.currentActionMap.Enable();

        // Enable active indicator
        playerInput.GetComponent<ActivePlayerIndicator>().SetActive();

    }
}
