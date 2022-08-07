using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

using static Delegates;

public class Client : MonoBehaviour
{
    #region Events

    public event OnMessageReceive OnMessageReceive;

    #endregion

    #region Fields

    [SerializeField] private int _port;
    [SerializeField] private string _address = "127.0.0.1";

    private bool _isConnected = false;

    private int _reliableChannel;
    private int _hostID;
    private int _connectionID;

    private string _nickname;

    #endregion

    #region Properties

    public bool IsConnected => _isConnected;

    public string Nickname
    {
        get => _nickname;
        set => _nickname = value;
    }

    #endregion

    #region Unity events

    private void Update()
    {
        if (!_isConnected) return;

        var recBuffer = new byte[NetworkData.BUFFER_SIZE];

        while (true)
        {
            NetworkEventType recData =
                NetworkTransport
                    .ReceiveFromHost(
                        _hostID,
                        out var connectionID,
                        out var channelID,
                        recBuffer,
                        NetworkData.BUFFER_SIZE,
                        out var dataSize,
                        out var error);

            if (recData == NetworkEventType.Nothing) break;

            switch (recData)
            {
                case NetworkEventType.ConnectEvent:

                    DisplayMessage($"You have been connected to server.");

                    SendMessage($"{NetworkData.SET_NICKNAME_EXPRESSION}{_nickname}"); // sending nickname to server

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
                ?.GetInvocationList()
                .Cast<OnMessageReceive>()
                .ToList();

        for (int i = 0; i < handlers.Count; i++)
        {
            OnMessageReceive -= handlers[i];
        };
    }
    
    #endregion

    #region Methods

    public void Connect()
    {
        if (_isConnected)
        {
            return;
        }
        else if(string.IsNullOrWhiteSpace(_nickname))
        {
            Debug.Log($"Nickname has not been set.");

            return;
        };

        NetworkTransport.Init();

        var connectionConfiguration = new ConnectionConfig();

        _reliableChannel = connectionConfiguration.AddChannel(QosType.Reliable);

        var hostTopolgy = new HostTopology(connectionConfiguration, NetworkData.MAX_CONNECTIONS);

        _hostID = NetworkTransport.AddHost(hostTopolgy, _port);

        _connectionID =
            NetworkTransport
                .Connect(
                    _hostID,
                    _address,
                    NetworkData.SERVER_PORT,
                    0,
                    out var error);
        
        if ((NetworkError) error == NetworkError.Ok)
        {
            _isConnected = true;
        }
        else
        {
            Debug.Log((NetworkError)error);
        };
    }

    public void Disconnect()
    {
        if (!_isConnected) return;
        
        NetworkTransport
            .Disconnect(
                _hostID,
                _connectionID,
                out var error);
        
        _isConnected = false;
    }

    public void SendMessage(string message)
    {
        if (!_isConnected) return;

        var buffer = Encoding.Unicode.GetBytes(message);

        NetworkTransport
            .Send(
                _hostID,
                _connectionID,
                _reliableChannel,
                buffer,
                message.Length * sizeof(char),
                out var error);

        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.Log((NetworkError)error);
        };
    }

    private void DisplayMessage(string message)
    {
        OnMessageReceive?.Invoke(message);

        Debug.Log(message);
    }
    
    #endregion
}
