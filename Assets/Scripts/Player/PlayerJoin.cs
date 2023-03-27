using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerJoin : MonoBehaviour
{
    private Playerhandler _playerhandler;
    private CinemachineTargetGroup targetGroup;
    [SerializeField] private Rope rope;
    [SerializeField] public GameObject playerJoinManuallyObserver;

    public void Start()
    {
        _playerhandler = Playerhandler.Instance;
        //Instantiate(playerJoinManuallyObserver);
        
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
        if (playerInput.gameObject.CompareTag("Observer")) { return; }
        // Set the player prefab
        targetGroup = GameObject.Find("CM TargetGroup1").GetComponent<CinemachineTargetGroup>();
        // int playerCount = PlayerInputManager.instance.playerCount;

        // If the player joins by connecting a new device we need to adapt the sprite
        int prefabIndex = _playerhandler.GetPlayerCount() % 4;
        PlayerInputManager.instance.playerPrefab = _playerhandler.GetPlayerPrefabs()[prefabIndex];

        // Add the player to our Linked List Player Manager
        _playerhandler.AddPlayer(playerInput.gameObject);
        
        if (targetGroup.m_Targets.Length > 0 )
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

    public void OnPlayerJoinManually(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) {
            int numberPlayers = _playerhandler.GetPlayerCount();
            if (numberPlayers > Playerhandler.MAX_PLAYERS)
            {
                return;
            }
            PlayerInput playerInput = PlayerInput.Instantiate(Playerhandler.Instance.GetPlayerPrefab(numberPlayers % Playerhandler.MAX_PLAYERS), pairWithDevice: InputSystem.devices[0]);
            // Disable input & active indicator     for players that were spawned manually 
            if (numberPlayers > 0) 
            {
                playerInput.currentActionMap.Disable();
                playerInput.gameObject.GetComponent<ActivePlayerIndicator>().SetInactive();
            }
        }
    }
}
