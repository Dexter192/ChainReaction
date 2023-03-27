using UnityEngine;
using UnityEngine.InputSystem;


public class JoinPlayerManually : MonoBehaviour
{
    public void OnPlayerJoinManually(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            int numberPlayers = Playerhandler.Instance.GetPlayerCount();
            if (numberPlayers >= Playerhandler.MAX_PLAYERS)
            {
                return;
            }
            int prefabIndex = numberPlayers % Playerhandler.MAX_PLAYERS;
            // We need to instantiate with an invalid device ID. This allows us to assign new devices to existing players
            PlayerInput playerInput = PlayerInput.Instantiate(Playerhandler.Instance.GetPlayerPrefab(prefabIndex), pairWithDevice: new InputDevice());
            // Disable input & active indicator for players that were spawned manually 
            playerInput.currentActionMap.Disable();
            playerInput.gameObject.GetComponent<ActivePlayerIndicator>().SetInactive();
            playerInput.user.UnpairDevices();
        }
    }
}
