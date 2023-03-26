using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerJoin : MonoBehaviour
{
    private Playerhandler _playerhandler;
    private CinemachineTargetGroup targetGroup;
    [SerializeField] private Rope rope;

    public void Awake()
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
        // Set the player prefab
        targetGroup = GameObject.Find("CM TargetGroup1").GetComponent<CinemachineTargetGroup>();
        // int playerCount = PlayerInputManager.instance.playerCount;
        PlayerInputManager.instance.playerPrefab = _playerhandler.GetPlayerPrefabs()[targetGroup.m_Targets.Length];

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
        //PlayerInput playerInput = PlayerInput.Instantiate(_playerhandler.GetPlayerPrefabs()[0], pairWithDevice: InputSystem.devices[0]);
        //OnPlayerJoin(playerInput);
    }
}
