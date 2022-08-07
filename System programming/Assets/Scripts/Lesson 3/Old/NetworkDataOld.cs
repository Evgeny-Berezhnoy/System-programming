using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkDataOld : MonoBehaviour
{
    #region Fields

    [SerializeField] private int _maxConnections = 10;
    [SerializeField] private int _recordingBufferSize = 1024;

    private bool _isActive;
    private HostTopology _hostTopolgy;
    private int _reliableChannel;
    private int _serverPort;
    private int _serverHost;
    private List<ClientDataOld> _registeredClients = new List<ClientDataOld>();
    private List<ClientDataOld> _currentClients = new List<ClientDataOld>();

    #endregion

    #region Properties

    public bool IsActive => _isActive;
    public int RecordingBufferSize => _recordingBufferSize;
    public int ReliableChannel => _reliableChannel;
    public int ServerPort
    {
        get => _serverPort;
    }
    public HostTopology HostTopology
    {
        get
        {
            if(_hostTopolgy == null)
            {
                Initialize();
            };

            return _hostTopolgy;
        }
    }
    public List<ClientDataOld> CurrentClients => _currentClients;

    #endregion

    #region Unity events

    private void OnDestroy()
    {
        Shutdown();
    }

    #endregion

    #region Methods

    public void Initialize()
    {
        if (_isActive) return;

        NetworkTransport.Init();

        var connectionConfiguration = new ConnectionConfig();

        connectionConfiguration.AddChannel(QosType.Reliable);

        _hostTopolgy = new HostTopology(connectionConfiguration, _maxConnections);

        _isActive = true;
    }

    public void Shutdown()
    {
        if (!_isActive) return;
        
        for (int i = 0; i < _currentClients.Count; i++)
        {
            var client = _currentClients[i];

            TryDisconnectClient(client);

            RemoveHost(client.HostID);
        };

        _currentClients.Clear();

        _registeredClients.Clear();

        RemoveHost(_serverHost);

        NetworkTransport.Shutdown();

        _isActive = false;
    }
    
    public int AddServerHost(int port)
    {
        _serverPort = port;

        _serverHost = AddHost(port);

        return _serverHost;
    }
    
    public int AddHost(int port)
    {
        return NetworkTransport.AddHost(HostTopology, port);
    }

    public bool ClientIsRegistered(int port)
    {
        var client = _registeredClients.FirstOrDefault(x => x.Port == port);

        return client != default(ClientDataOld) ? true : false;
    }

    public bool ClientIsConnected(ClientDataOld client)
    {
        return _currentClients.Contains(client);
    }

    public bool TryConnectClient(int port, string address, out ClientDataOld client, out byte error)
    {
        client = RegisterClient(port);

        if (ClientIsConnected(client))
        {
            error = (byte)NetworkError.Ok;

            return true;
        };

        var connectionID =
            NetworkTransport
                .Connect(
                    client.HostID,
                    address,
                    _serverPort,
                    0,
                    out error);

        if ((NetworkError)error != NetworkError.Ok)
        {
            return false;
        };

        client.ConnectionID = connectionID;

        _currentClients.Add(client);

        return true;
    }

    public bool TryDisconnectClient(ClientDataOld client)
    {
        if (!ClientIsConnected(client))
        {
            return false;
        };

        NetworkTransport
            .Disconnect(
                client.HostID,
                client.ConnectionID,
                out var error);

        _currentClients.Remove(client);

        return true;
    }

    private bool ClientIsRegistered(int port, out ClientDataOld client)
    {
        client = _registeredClients.FirstOrDefault(x => x.Port == port);

        if(client != default(ClientDataOld))
        {
            return true;
        };

        client = new ClientDataOld();

        return false;
    }

    private ClientDataOld RegisterClient(int port)
    {
        if (ClientIsRegistered(port, out var client))
        {
            return client;
        };

        client          = new ClientDataOld();
        client.Port     = port;
        client.HostID   = AddHost(port);

        _registeredClients.Add(client);

        return client;
    }
    
    private void RemoveHost(int host)
    {
        NetworkTransport.RemoveHost(host);
    }

    #endregion
}