using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinTest : MonoBehaviour
{
    public void OnPlayerJoinManually(InputAction.CallbackContext ctx)
    {
        Debug.Log("Joining player manually");
        
        //PlayerInput playerInput = PlayerInput.Instantiate(_playerhandler.GetPlayerPrefabs()[0], pairWithDevice: InputSystem.devices[0]);
        //OnPlayerJoin(playerInput);
    }
}
