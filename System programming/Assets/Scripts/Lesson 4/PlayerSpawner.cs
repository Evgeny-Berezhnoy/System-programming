using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpawner : NetworkBehaviour
{
    #region Fields

    [SerializeField] private GameObject _playerPrefab;
    
    private GameObject _playerCharacter;

    #endregion

    #region Unity events

    private void Start()
    {
        SpawnCharacter();
    }

    #endregion

    #region Methods

    public void SpawnCharacter()
    {
        if (!isServer) return;

        _playerCharacter = Instantiate(_playerPrefab, transform);

        NetworkServer
            .SpawnWithClientAuthority(
                _playerCharacter,
                connectionToClient);
    }

    #endregion
}
