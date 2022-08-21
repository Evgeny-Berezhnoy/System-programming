using UI;
using UnityEngine;
namespace Mechanics
{
    public class CameraOrbit : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Transform _focus;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField, Range(0.01f, 1)] private float _distance = 0.5f;
        [SerializeField, Range(0, 90)] private int _lookAngle;
        [SerializeField, Min(0)] private float _focusRadius = 1;
        [SerializeField, Range(0, 1)] private float _focusCentering = 0.5f;
        [SerializeField, Range(0.1f, 5)] private float _sensitive = 0.5f;
        [SerializeField, Range(1f, 360)] private float _rotationSpeed = 90f;
        [SerializeField, Range(-89, 89)] private float _minVerticalAngle = -30f;
        [SerializeField, Range(-89, 89)] private float _maxVerticalAngle = 60f;
            
        private Vector3 _focusPoint;
        private Vector2 _orbitAngles = new Vector2(45, 0);
        private float _currentDistance;
        private float _desiredDistance;
        private Camera _regularCamera;

        #endregion

        #region Properties

        public int LookAngle => _lookAngle;
        public Vector3 LookPosition { get; private set; }
        private Vector3 _сameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                
                halfExtends.y = _regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
                halfExtends.x = halfExtends.y * _regularCamera.aspect;
                halfExtends.z = 0;

                return halfExtends;
            }
        }

        #endregion

        #region Unity events

        private void OnValidate()
        {
            UpdateMinMaxVerticalAngles();
        }

        #endregion

        #region Static methods

        private static float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Deg2Rad;

            return direction.x < 0 ? 360 - angle : angle;
        }

        #endregion

        #region Methods

        public void Initiate()
        {
            _focusPoint = _focus.position;
            
            _regularCamera = GetComponent<Camera>();

            _desiredDistance = _distance;
            _currentDistance = _distance;

            transform.parent = null;
            transform.localRotation = ConstrainAngles(ref _orbitAngles);
        }

        public void CameraMovement()
        {
            UpdateFocusPoint();
            
            var lookRotation =
                ManualRotation(ref _orbitAngles)
                ? ConstrainAngles(ref _orbitAngles)
                : transform.localRotation;
            
            var lookDirection = lookRotation * Vector3.forward;
            
            LookPosition = _focusPoint + lookDirection;
            
            if (Physics
                    .BoxCast(
                        _focusPoint,
                        _сameraHalfExtends,
                        -lookDirection,
                        out RaycastHit hit,
                        lookRotation,
                        _distance - _regularCamera.nearClipPlane,
                        _obstacleMask))
            {
                _desiredDistance = hit.distance * _regularCamera.nearClipPlane;
            }
            else
            {
                _desiredDistance = _distance;
            };

            _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * 20.0f);
            
            var lookPosition = _focusPoint - lookDirection * _currentDistance;
            
            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        public void SetFov(float fov, float changeSpeed)
        {
            _regularCamera.fieldOfView =
                Mathf
                    .Lerp(
                        _regularCamera.fieldOfView,
                        fov,
                        changeSpeed * Time.deltaTime);
        }

        public void ShowPlayerLabels(PlayerLabel label)
        {
            label.DrawLabel(_regularCamera);
        }

        private void UpdateMinMaxVerticalAngles()
        {
            if (_minVerticalAngle > _maxVerticalAngle)
            {
                _minVerticalAngle = _maxVerticalAngle;
            };
        }

        private void UpdateFocusPoint()
        {
            var targetPoint = _focus.position;
            
            if (_focusRadius > 0)
            {
                var distance    = Vector3.Distance(targetPoint, _focusPoint);
                var t           = 1f;

                if (distance > 0.01
                        && _focusCentering > 0)
                {
                    t = Mathf.Pow(1 - _focusCentering, Time.deltaTime);
                };

                if (distance > _focusRadius)
                {
                    t = Mathf.Min(t, _focusRadius / distance);
                };

                _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
            }
            else
            {
                _focusPoint = targetPoint;
            };
        }
        
        private bool ManualRotation(ref Vector2 orbitAngles)
        {
            var input =
                new Vector2(
                    -Input.GetAxis("Mouse Y"),
                    Input.GetAxis("Mouse X"));

            float e = Mathf.Epsilon;

            if (input.x < -e
                || input.x > e
                || input.y < -e
                || input.y > e)
            {
                orbitAngles += _rotationSpeed * Time.unscaledDeltaTime * input * _sensitive;
                
                return true;
            };

            return false;
        }

        private Quaternion ConstrainAngles(ref Vector2 orbitAngles)
        {
            orbitAngles.x =
                Mathf
                    .Clamp(
                        orbitAngles.x,
                        _minVerticalAngle,
                        _maxVerticalAngle);
            
            if (orbitAngles.y < 0)
            {
                orbitAngles.y += 360;
            }
            else if (orbitAngles.y >= 360)
            {
                orbitAngles.y -= 360;
            };

            return Quaternion.Euler(orbitAngles);
        }
        
        public void EnableCamera()
        {
            _regularCamera.enabled = true;
        }

        #endregion
    }
}
