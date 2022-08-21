using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Main;
using Mechanics;
using Network;
using UI;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class ShipController : NetworkMovableObject
    {
        #region Fields

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private PlayerLabel _playerLabel;
        [SerializeField] private CameraOrbit _cameraOrbit;
        [SerializeField] private LayerMask _overlapMask;

        [SyncVar] private string _playerName;
        
        private NetworkManager _network;
        private Collider[] _colliderBuffer;
        private float _shipSpeed;

        #endregion

        #region Properties

        protected override float _speed => _shipSpeed;

        #endregion

        #region Unity events

        private void Start()
        {
            gameObject.name = _playerName;
        }

        public override void OnStartAuthority()
        {
            CmdStartAuthorityOnServer(PlayerPrefs.GetString("Player name"));

            gameObject.name = _playerName;

            _cameraOrbit.Initiate();
            _cameraOrbit.EnableCamera();

            base.OnStartAuthority();
        }

        private void FixedUpdate()
        {
            if (!isServer) return;

            var bufferSize =
                Physics
                    .OverlapBoxNonAlloc(
                        transform.position,
                        _collider.size / 2,
                        _colliderBuffer,
                        transform.rotation,
                        _overlapMask);

            if (bufferSize == 0) return;

            var otherColliders =
                _colliderBuffer
                    .Where(x => x != null && x.gameObject != gameObject)
                    .ToArray();

            if (otherColliders.Length == 0) return;

            gameObject.SetActive(false);

            RpcSetActive(false);

            var spawnPoint = _network.GetStartPosition();

            _serverPosition = spawnPoint.position;
            _serverEuler    = spawnPoint.eulerAngles;

            RpcSetPositionAndRotation();

            gameObject.SetActive(true);

            RpcSetActive(true);
        }

        [ClientCallback]
        private void LateUpdate()
        {
            if (!hasAuthority) return;

            _cameraOrbit?.CameraMovement();
        }
        
        private void OnGUI()
        {
            _cameraOrbit?.ShowPlayerLabels(_playerLabel);
        }

        #endregion

        #region Base methods

        protected override void HasAuthorityMovement()
        {
            var spaceShipSettings = SettingsContainer.Instance?.SpaceShipSettings;
            
            if (spaceShipSettings == null) return;

            var isFaster = Input.GetKey(KeyCode.LeftShift);

            var speed = spaceShipSettings.ShipSpeed;

            var faster = isFaster ? spaceShipSettings.Faster : 1;

            _shipSpeed =
                Mathf.Lerp(
                    _shipSpeed,
                    speed * faster,
                    spaceShipSettings.Acceleration);
            
            var currentFov =
                isFaster
                    ? spaceShipSettings.FasterFov
                    : spaceShipSettings.NormalFov;
            
            _cameraOrbit
                .SetFov(
                    currentFov,
                    spaceShipSettings.ChangeFovSpeed);

            var velocity = _cameraOrbit.transform.TransformDirection(Vector3.forward) * _shipSpeed;

            _rigidbody.velocity = velocity * Time.deltaTime;

            if (!Input.GetKey(KeyCode.C))
            {
                var angle = Quaternion.AngleAxis(_cameraOrbit.LookAngle, -transform.right);

                var targetRotation = Quaternion.LookRotation(angle * velocity);
                
                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        Time.deltaTime * speed);
            };
        }

        protected override void FromServerUpdate() { }

        protected override void SendToServer() { }

        #endregion

        #region Server methods

        [Command]
        private void CmdStartAuthorityOnServer(string playerName)
        {
            _playerName     = playerName;
            gameObject.name = playerName;

            _colliderBuffer = new Collider[2];

            _network = FindObjectOfType<NetworkManager>();
        }

        #endregion

        #region Server to client methods

        [ClientRpc]
        private void RpcSetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        [ClientRpc]
        private void RpcSetPositionAndRotation()
        {
            transform.position = _serverPosition;
            transform.rotation = Quaternion.Euler(_serverEuler);
        }

        #endregion
    }
}