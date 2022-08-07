using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerOld : MonoBehaviour
{
    #region Fields

    [SerializeField] private NetworkDataOld _network;
    [SerializeField] private int _port = 5805;

    private bool _isStarted;

    private int _hostID;

    #endregion

    #region Unity events

    private void Update()
    {
        if (!_isStarted) return;
        
        var recBuffer = new byte[_network.RecordingBufferSize];

        while (true)
        {
            NetworkEventType recData =
                NetworkTransport
                    .ReceiveFromHost(
                        _hostID,
                        out var connectionID,
                        out var channelID,
                        recBuffer,
                        _network.RecordingBufferSize,
                        out var dataSize,
                        out var error);

            if (recData == NetworkEventType.Nothing) break;

            switch (recData)
            {
                case NetworkEventType.ConnectEvent:
                    
                    SendMessageToAll($"Player {connectionID} has connected.");
                    
                    break;

                case NetworkEventType.DataEvent:
                    
                    var message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    
                    SendMessageToAll($"Player {connectionID}: {message}");
                    
                    break;

                case NetworkEventType.DisconnectEvent:
                    
                    SendMessageToAll($"Player {connectionID} has disconnected.");
                    
                    break;

                default:
                    break;
            };
        };
    }

    #endregion

    #region Methods

    public void Enable()
    {
        if (_isStarted) return;

        _network.Initialize();

        _hostID = _network.AddServerHost(_port);
        
        _isStarted  = true;
    }

    public void Disable()
    {
        if (!_isStarted) return;

        _network.Shutdown();

        _isStarted = false;
    }

    private void SendMessageToAll(string message)
    {
        Debug.Log(message);

        for (int i = 0; i < _network.CurrentClients.Count; i++)
        {
            SendMessage(message, _network.CurrentClients[i].ConnectionID);
        };
    }

    private void SendMessage(string message, int connectionID)
    {
        var buffer = Encoding.Unicode.GetBytes(message);
        
        NetworkTransport
            .Send(
                _hostID,
                connectionID,
                _network.ReliableChannel,
                buffer,
                message.Length * sizeof(char),
                out var error);

        if ((NetworkError) error != NetworkError.Ok)
        {
            Debug.Log((NetworkError) error);
        };
    }

    #endregion
}