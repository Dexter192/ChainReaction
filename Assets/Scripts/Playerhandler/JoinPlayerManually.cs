using UnityEngine;
using UnityEngine.InputSystem;


public class JoinPlayerManually : MonoBehaviour
{
    public void OnPlayerJoinManually(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            int numberPlayers = Playerhandler.Instance.GetPlayerCount();
            if (numberPlayers > Playerhandler.MAX_PLAYERS)
            {
                return;
            }
            int prefabIndex = numberPlayers % Playerhandler.MAX_PLAYERS;
            PlayerInput playerInput = PlayerInput.Instantiate(Playerhandler.Instance.GetPlayerPrefab(prefabIndex));
            // Disable input & active indicator     for players that were spawned manually 
            if (numberPlayers > 0)
            {
                playerInput.currentActionMap.Disable();
                playerInput.gameObject.GetComponent<ActivePlayerIndicator>().SetInactive();
                playerInput.user.UnpairDevices();
            }
        }
    }
}
