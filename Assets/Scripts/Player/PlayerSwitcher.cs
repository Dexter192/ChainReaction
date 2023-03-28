using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

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
            LinkedList<GameObject> playerList = _playerhandler.GetPlayerList();
            LinkedListNode<GameObject> currentPlayer = playerList.Find(gameObject);
            LinkedListNode<GameObject> previousPlayer = currentPlayer;
            
            for (int i = 0; i < Playerhandler.MAX_PLAYERS; i++)
            {
                previousPlayer = previousPlayer.Previous;
                if (previousPlayer == null)
                {
                    previousPlayer = playerList.Last;
                }
                // Only switch to players that don't have a controller assigned to
                if (previousPlayer.Value.GetComponent<PlayerInput>().user.pairedDevices.Count == 0)
                {
                    SwitchControls(gameObject, previousPlayer.Value);
                    return;
                }
            }
        }
    }

    public void SwitchToNextPlayer(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            LinkedList<GameObject> playerList = _playerhandler.GetPlayerList();
            LinkedListNode<GameObject> currentPlayer = playerList.Find(gameObject);
            LinkedListNode<GameObject> nextPlayer = currentPlayer;

            for (int i = 0; i < Playerhandler.MAX_PLAYERS; i++)
            {
                nextPlayer = nextPlayer.Next;
                if (nextPlayer == null)
                {
                    nextPlayer = playerList.First;
                }
                // Only switch to players that don't have a controller assigned to
                if (nextPlayer.Value.GetComponent<PlayerInput>().user.pairedDevices.Count == 0)
                {
                    SwitchControls(gameObject, nextPlayer.Value);
                    return;
                }
            }
        }
    }

    private void SwitchControls(GameObject player1, GameObject player2)
    {
        // TODO: Figure out how this works with Controllers, etc. (Maybe I don't need to do this)
        Debug.Log("Switching Controls from " + player1 + " to " + player2);
        PlayerInput player1Input = player1.GetComponent<PlayerInput>();
        
        PlayerInput player2Input = player2.GetComponent<PlayerInput>();

        //player1Input.SwitchCurrentControlScheme(player2Input.devices[0]);
        //player2Input.SwitchCurrentControlScheme(player1InputDevice);
        InputDevice player1InputDevice = player1Input.user.pairedDevices[0];
        
        // Change device pairing
        player1Input.user.UnpairDevices();
        InputUser.PerformPairingWithDevice(player1InputDevice, player2Input.user, InputUserPairingOptions.UnpairCurrentDevicesFromUser);

        // En-/Disable Actionmap
        player1Input.DeactivateInput();
        player2Input.ActivateInput();
        //player1Input.currentActionMap.Disable();
        //player2Input.currentActionMap.Enable();

        // Swap active player indicator
        ActivePlayerIndicator player1Indicator = player1.GetComponent<ActivePlayerIndicator>();
        ActivePlayerIndicator player2Indicator = player2.GetComponent<ActivePlayerIndicator>();
        player1Indicator.SetInactive();
        player2Indicator.SetActive();

        // Set playerindicator colour
        Color player1Colour = player1Indicator.GetColor();
        player1Indicator.SetColor(player2Indicator.GetColor());
        player2Indicator.SetColor(player1Colour);
    }
}
