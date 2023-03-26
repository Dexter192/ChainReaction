using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitcher : MonoBehaviour
{
    private Playerhandler _playerhandler;

    private void Start()
    {
        _playerhandler = Playerhandler.Instance;
    }
    public void SwitchToPreviousPlayer(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameObject nextPlayer = _playerhandler.GetPreviousPlayerCircular(gameObject);
            for (int i = 0; i < _playerhandler.GetPlayerCount(); i++)
            {
                if (nextPlayer.GetComponent<PlayerInput>().currentActionMap.enabled)
                {
                    nextPlayer = _playerhandler.GetPreviousPlayerCircular(nextPlayer);
                    continue;
                }
                else
                {
                    SwitchControls(gameObject, nextPlayer);
                    break;
                }
            }


        }
    }

    public void SwitchToNextPlayer(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameObject nextPlayer = _playerhandler.GetNextPlayerCircular(gameObject);
            for (int i = 0; i < _playerhandler.GetPlayerCount(); i++)
            {
                if (nextPlayer.GetComponent<PlayerInput>().currentActionMap.enabled)
                { 
                    nextPlayer = _playerhandler.GetNextPlayerCircular(nextPlayer);
                    continue;
                }
                else
                {
                    SwitchControls(gameObject, nextPlayer);
                    break;
                }
            }
        }
    }

    private void SwitchControls(GameObject player1, GameObject player2)
    {
        // TODO: Figure out how this works with Controllers, etc. (Maybe I don't need to do this)
        Debug.Log("Switching Controls from " + player1 + " to " + player2);
        PlayerInput player1Input = player1.GetComponent<PlayerInput>();
        InputDevice player1InputDevice = player1Input.devices[0];
        
        PlayerInput player2Input = player2.GetComponent<PlayerInput>();

        player1Input.SwitchCurrentControlScheme(player2Input.devices[0]);
        player2Input.SwitchCurrentControlScheme(player1InputDevice);
    
        player1Input.currentActionMap.Disable();
        player2Input.currentActionMap.Enable();
    }
}
