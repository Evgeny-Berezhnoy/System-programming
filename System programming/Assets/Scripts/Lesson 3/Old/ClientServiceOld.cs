using UnityEngine;
using UnityEngine.Networking;

public class ClientServiceOld : MonoBehaviour
{
    #region Fields

    [SerializeField] private NetworkDataOld _network;

    #endregion

    #region Properties

    public bool IsActive => _network.IsActive;
    public int RecordingBufferSize => _network.RecordingBufferSize;
    public int ReliableChannel => _network.ReliableChannel;

    #endregion

    #region Methods
    
    public bool ClientIsRegistered(int port, bool aloud = false)
    {
        if (!_network.IsActive)
        {
            if (aloud)
            {
                Debug.LogError("Network is not active now");
            };

            return false;
        };

        return _network.ClientIsRegistered(port);
    }

    public bool ClientIsConnected(ClientDataOld client, bool aloud = false)
    {
        if (!_network.IsActive)
        {
            if (aloud)
            {
                Debug.LogError("Network is not active now");
            };

            return false;
        };

        return _network.ClientIsConnected(client);
    }

    public bool TryConnectClient(int port, string address, out ClientDataOld client, out byte error)
    {
        if (!_network.IsActive)
        {
            Debug.LogError("Network is not active now");

            client = new ClientDataOld();

            error = (byte)NetworkError.DNSFailure;

            return false;
        };
        
        return _network.TryConnectClient(port, address, out client, out error);
    }

    public bool TryDisconnectClient(ClientDataOld client)
    {
        if (!_network.IsActive)
        {
            Debug.LogError("Network is not active now");

            return false;
        };
        
        return _network.TryDisconnectClient(client);
    }

    #endregion
}