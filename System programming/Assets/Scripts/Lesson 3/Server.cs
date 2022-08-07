using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    #region Fields

    private bool _isStarted = false;

    private int _reliableChannel;
    private int _hostID;
    
    private List<ClientData> _currentClients;

    private Regex _nicknameSetRegex;

    #endregion

    #region Unity events

    private void Start()
    {
        _currentClients     = new List<ClientData>();

        _nicknameSetRegex   = new Regex($@"^{NetworkData.SET_NICKNAME_EXPRESSION}");
    }

    private void Update()
    {
        if (!_isStarted) return;

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

            var clientIsConnected = ClientIsConnected(connectionID, out var client);

            switch (recData)
            {
                case NetworkEventType.ConnectEvent:

                    if (!clientIsConnected)
                    {
                        ConnectClient(connectionID);
                    };

                    break;

                case NetworkEventType.DataEvent:

                    if(clientIsConnected)
                    {
                        var message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);

                        if (_nicknameSetRegex.IsMatch(message))
                        {
                            var nickname = message.Replace(NetworkData.SET_NICKNAME_EXPRESSION, "");

                            client.Nickname = nickname;

                            SendMessageToAll($"Player {client.Nickname} has connected.");
                        }
                        else
                        {
                            SendMessageToAll($"{client.Nickname}: {message}");
                        };
                    };

                    break;

                case NetworkEventType.DisconnectEvent:

                    if(clientIsConnected)
                    {
                        DisconnectClient(client.ConnectionID);

                        SendMessageToAll($"Player {client.Nickname} has disconnected.");
                    };

                    break;

                default:
                    break;
            };
        };
    }

    private void OnDestroy()
    {
        Disable();
    }

    #endregion

    #region Methods

    public void Enable()
    {
        if (_isStarted) return;

        NetworkTransport.Init();

        var connectionConfiguration = new ConnectionConfig();

        connectionConfiguration.AddChannel(QosType.Reliable);

        var hostTopolgy = new HostTopology(connectionConfiguration, NetworkData.MAX_CONNECTIONS);

        _hostID = NetworkTransport.AddHost(hostTopolgy, NetworkData.SERVER_PORT);

        _isStarted = true;
    }

    public void Disable()
    {
        if (!_isStarted) return;

        for (int i = 0; i < _currentClients.Count; i++)
        {
            var client = _currentClients[i];

            DisconnectClient(client.ConnectionID);
        };

        _currentClients.Clear();

        NetworkTransport.RemoveHost(_hostID);

        NetworkTransport.Shutdown();

        _isStarted = false;
    }

    private bool ClientIsConnected(int connectionID, out ClientData client)
    {
        client = _currentClients.FirstOrDefault(x => x.ConnectionID == connectionID);

        if (client != default(ClientData))
        {
            return true;
        };

        client = new ClientData();

        return false;
    }

    public void ConnectClient(int connectionID)
    {
        if (ClientIsConnected(connectionID, out var client))
        {
            return;
        };

        client.ConnectionID = connectionID;

        _currentClients.Add(client);
    }
    
    private void DisconnectClient(int connectionID)
    {
        if (ClientIsConnected(connectionID, out var client))
        {
            _currentClients.Remove(client);
        };
    }
    
    private void SendMessageToAll(string message)
    {
        Debug.Log(message);

        for (int i = 0; i < _currentClients.Count; i++)
        {
            SendMessage(message, _currentClients[i].ConnectionID);
        };
    }

    private void SendMessage(string message, int connectionID)
    {
        var buffer = Encoding.Unicode.GetBytes(message);

        NetworkTransport
            .Send(
                _hostID,
                connectionID,
                _reliableChannel,
                buffer,
                message.Length * sizeof(char),
                out var error);

        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.Log((NetworkError)error);
        };
    }

    #endregion
}
