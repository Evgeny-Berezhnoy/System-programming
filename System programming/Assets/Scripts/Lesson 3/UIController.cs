using UnityEngine;
using TMPro;

using Button = UnityEngine.UI.Button;

public class UIController : MonoBehaviour
{
    #region Fields

    [Header("Components")]
    [SerializeField] private Server _server;
    [SerializeField] private Client _client;
    [SerializeField] private Chat _chat;

    [Header("Buttons")]
    [SerializeField] private Button _buttonStartServer;
    [SerializeField] private Button _buttonShutDownServer;
    [SerializeField] private Button _buttonConnectClient;
    [SerializeField] private Button _buttonDisconnectClient;
    [SerializeField] private Button _buttonSendMessage;
    
    [Header("Texts")]
    [SerializeField] private TMP_InputField _nicknameField;
    [SerializeField] private TMP_InputField _inputField;

    #endregion

    #region Unity events

    private void Start()
    {
        _buttonStartServer.onClick.AddListener(() => StartServer());
        _buttonShutDownServer.onClick.AddListener(() => ShutDownServer());
        _buttonConnectClient.onClick.AddListener(() => Connect());
        _buttonDisconnectClient.onClick.AddListener(() => Disconnect());
        _buttonSendMessage.onClick.AddListener(() => SendMessage());

        _nicknameField.onValueChanged.AddListener(value => OnNicknameChange(value));

        _client.OnMessageReceive += ReceiveMessage;
    }

    private void OnDestroy()
    {
        _buttonStartServer.onClick.RemoveAllListeners();
        _buttonShutDownServer.onClick.RemoveAllListeners();
        _buttonConnectClient.onClick.RemoveAllListeners();
        _buttonDisconnectClient.onClick.RemoveAllListeners();
        _buttonSendMessage.onClick.RemoveAllListeners();

        _nicknameField.onValueChanged.RemoveAllListeners();
    }

    #endregion

    #region Methods

    private void StartServer()
    {
        _server.Enable();
    }

    private void ShutDownServer()
    {
        _server.Disable();
    }

    private void Connect()
    {
        _client.Connect();
    }

    private void Disconnect()
    {
        _client.Disconnect();
    }

    private void SendMessage()
    {
        _client.SendMessage(_inputField.text);
        
        _inputField.text = "";
    }

    private void ReceiveMessage(object message)
    {
        _chat.ReceiveMessage(message);
    }

    private void OnNicknameChange(string nickname)
    {
        if (_client.IsConnected)
        {
            _nicknameField.SetTextWithoutNotify(_client.Nickname);
        }
        else
        {
            _client.Nickname = nickname;
        };
    }

    #endregion
}