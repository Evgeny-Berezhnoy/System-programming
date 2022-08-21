using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Main;

namespace UI
{
    public class LoginController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private NetworkManagerHUD _networkHUD;
        [SerializeField] private SolarSystemNetworkManager _networkManager;
        [SerializeField] private InputField _playerName;
        [SerializeField] private Button _loginButton;

        #endregion

        #region Unity events

        private void Start()
        {
            _loginButton.onClick.AddListener(() => Login());
        }

        private void OnDestroy()
        {
            _loginButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Methods

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(_playerName.text)) return;

            _networkHUD.enabled = true;

            PlayerPrefs.SetString("Player name", _playerName.text);

            gameObject.SetActive(false);
        }

        #endregion
    }
}
