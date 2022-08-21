using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpawner : NetworkBehaviour
{
    #region Fields

    [SerializeField] protected GameObject _playerPrefab;
    
    protected GameObject _playerCharacter;

    #endregion

    #region Unity events

    private void Start()
    {
        SpawnCharacter();
    }

    #endregion

    #region Methods

    public virtual void SpawnCharacter()
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
