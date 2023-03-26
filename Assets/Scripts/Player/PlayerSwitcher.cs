using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitcher : MonoBehaviour
{
    private Playerhandler _playerhandler;

    private void Start()
    {
        _playerhandler = Playerhandler.Instance;
    }
    public void SwitchToNextPlayer()
    {
        GameObject nextPlayer = _playerhandler.GetNextPlayerCircular(gameObject);
        // TODO: Assign controls to next player
    }
    public void JoinNewPlayer(InputAction.CallbackContext ctx)
    {
        
    }
}
