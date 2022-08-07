using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

using static Delegates;

public class ClientOld : MonoBehaviour
{
    #region Events

    public event OnMessageReceive OnMessageReceive;

    #endregion

    #region Fields

    [SerializeField] private ClientServiceOld _service;
    [SerializeField] private int _port = 0;
    [SerializeField] private string _address = "127.0.0.1";

    private ClientDataOld _data = new ClientDataOld();

    #endregion

    #region Unity events

    private void Update()
    {
        if (!_service.ClientIsRegistered(_data.Port)) return;

        var recBuffer = new byte[_service.RecordingBufferSize];
        
        while (true)
        {
            NetworkEventType recData =
                NetworkTransport
                    .ReceiveFromHost(
                        _data.HostID,
                        out var connectionID,
                        out var channelID,
                        recBuffer,
                        _service.RecordingBufferSize,
                        out var dataSize,
                        out var error);

            if (recData == NetworkEventType.Nothing) break;

            switch (recData)
            {
                case NetworkEventType.ConnectEvent:
                    
                    DisplayMessage($"You have been connected to server.");

                    break;

                case NetworkEventType.DataEvent:
                    
                    var message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    
                    DisplayMessage(message);

                    break;

                case NetworkEventType.DisconnectEvent:
                    
                    DisplayMessage($"You have been disconnected from server.");

                    break;

                default:
                    break;
            };
        };
    }

    private void OnDestroy()
    {
        var handlers =
            OnMessageReceive
                .GetInvocationList()
                .ToList()
                .Cast<OnMessageReceive>()
                .ToList();

        for(int i = 0; i < handlers.Count; i++)
        {
            OnMessageReceive -= handlers[i];
        };
    }

    #endregion

    #region Methods

    public void Connect()
    {
        if (_service.ClientIsConnected(_data, true)) return;

        if(!_service.TryConnectClient(_port, _address, out _data, out var error))
        {
            Debug.LogError((NetworkError) error);

            return;
        };
    }
    public void Disconnect()
    {
        _service.TryDisconnectClient(_data);
    }

    public void SendMessage(string message)
    {
        if (!_service.ClientIsRegistered(_data.Port, true)) return;

        var buffer = Encoding.Unicode.GetBytes(message);

        NetworkTransport
            .Send(
                _data.HostID,
                _data.ConnectionID,
                _service.ReliableChannel,
                buffer,
                message.Length * sizeof(char),
                out var error);

        if ((NetworkError) error != NetworkError.Ok)
        {
            Debug.Log((NetworkError) error);
        };
    }

    private void DisplayMessage(string message)
    {
        OnMessageReceive?.Invoke(message);

        Debug.Log(message);
    }

    #endregion
}