using UnityEngine;
using UnityEngine.Networking;
using Network;

namespace Mechanics
{
    public class PlanetOrbit : NetworkMovableObject
    {
        #region Constants

        private const float _circleRadians = Mathf.PI * 2;

        #endregion

        #region Fields

        [SerializeField] private Transform _aroundPoint;
        [SerializeField] private float _smoothTime      = 0.3f;
        [SerializeField] private float _circleInSecond  = 1f;
        [SerializeField] private float _offsetSin       = 1f;
        [SerializeField] private float _offsetCos       = 1f;
        [SerializeField] private float _rotationSpeed;
        
        private float _distance;
        private float _currentAngle;
        private float _currentRotationAngle;
        private Vector3 _currentPositionSmoothVelocity;
        
        #endregion

        #region Base roperties

        protected override float _speed => _smoothTime;

        #endregion

        #region Unity events

        [ClientCallback]
        private void Start()
        {
            if (isServer)
            {
                _distance = (transform.position - _aroundPoint.position).magnitude;
            };

            Initiate(UpdatePhase.FixedUpdate);
        }

        #endregion

        #region Base methods

        protected override void HasAuthorityMovement()
        {
            if (!isServer) return;
            
            var aroundPointPosition = _aroundPoint.position;

            aroundPointPosition.x += Mathf.Sin(_currentAngle) * _distance * _offsetSin;
            aroundPointPosition.z += Mathf.Cos(_currentAngle) * _distance * _offsetCos;
            
            _currentRotationAngle += _rotationSpeed * Time.deltaTime;
            _currentRotationAngle = Mathf.Clamp(_currentRotationAngle, 0, 361);

            if (_currentRotationAngle >= 360)
            {
                _currentRotationAngle = 0;
            }

            transform.position = aroundPointPosition;
            transform.rotation = Quaternion.AngleAxis(_currentRotationAngle, transform.up);

            _currentAngle += _circleRadians * _circleInSecond * Time.deltaTime;
            
            SendToServer();
        }

        protected override void SendToServer()
        {
            _serverPosition = transform.position;
            _serverEuler    = transform.eulerAngles;
        }

        protected override void FromServerUpdate()
        {
            if (!isClient) return;
            
            transform.position =
                Vector3
                    .SmoothDamp(
                        transform.position,
                        _serverPosition,
                        ref _currentPositionSmoothVelocity,
                        _speed);
            
            transform.rotation = Quaternion.Euler(_serverEuler);
        }

        #endregion
    }
}
