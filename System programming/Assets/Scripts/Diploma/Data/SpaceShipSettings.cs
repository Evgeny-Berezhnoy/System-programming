using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Space ship settings", menuName = "Settings/Space Ship Settings")]
    public class SpaceShipSettings : ScriptableObject
    {
        #region Fields

        [SerializeField, Range(1f, 5f)] private int _faster;
        [SerializeField, Range(0.1f, 5f)] private float _changeFovSpeed = 0.5f;
        [SerializeField, Range(1f, 2000f)] private float _shipSpeed;
        [SerializeField, Range(0.01f, 0.1f)] private float _acceleration;
        [SerializeField, Range(0.01f, 179f)] private float _normalFov = 60;
        [SerializeField, Range(0.01f, 179f)] private float _fasterFov = 30;

        #endregion

        #region Properties
        
        public float Faster => _faster;
        public float ChangeFovSpeed => _changeFovSpeed;
        public float ShipSpeed => _shipSpeed;
        public float Acceleration => _acceleration;
        public float NormalFov => _normalFov;
        public float FasterFov => _fasterFov;

        #endregion
    }
}