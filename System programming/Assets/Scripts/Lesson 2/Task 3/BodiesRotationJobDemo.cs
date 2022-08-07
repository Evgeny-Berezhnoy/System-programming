using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Lesson_2
{
    public class BodiesRotationJobDemo : MonoBehaviour, IDisposable
    {
        #region Fields

        [SerializeField] private Transform[] _bodies;
        [SerializeField] private float _rotationFrequency;

        private TransformAccessArray _bodiesNativeArray;

        #endregion

        #region Unity events

        private void Start()
        {
            _bodiesNativeArray = new TransformAccessArray(_bodies);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Update()
        {
            var job = new BodiesRotationJob()
            {
                RotationFrequency   = _rotationFrequency * 360,
                DeltaTime           = Time.deltaTime
            };

            job
                .Schedule(_bodiesNativeArray)
                .Complete();
        }

        #endregion

        #region Interface methods

        public void Dispose()
        {
            if (_bodiesNativeArray.isCreated)
            {
                _bodiesNativeArray.Dispose();
            };
        }

        #endregion
    }
}