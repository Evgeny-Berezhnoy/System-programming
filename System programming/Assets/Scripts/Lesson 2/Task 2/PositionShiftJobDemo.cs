using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Lesson_2
{
    public class PositionShiftJobDemo : MonoBehaviour, IDisposable
    {
        #region Fields

        [SerializeField] private PositionShiftData[] _positionShiftDatas;
        [SerializeField] private Vector3[] _jobResults;

        private JobHandle _handler;

        private NativeArray<Vector3> _positions;
        private NativeArray<Vector3> _velocities;
        private NativeArray<Vector3> _finalPositions;

        #endregion
        
        #region Unity events

        private void Start()
        {
            _positions      = new NativeArray<Vector3>(_positionShiftDatas.Length, Allocator.Persistent);
            _velocities     = new NativeArray<Vector3>(_positionShiftDatas.Length, Allocator.Persistent);
            _finalPositions = new NativeArray<Vector3>(_positionShiftDatas.Length, Allocator.Persistent);

            for (int i = 0; i < _positionShiftDatas.Length; i++)
            {
                _positions[i]   = _positionShiftDatas[i].Position;
                _velocities[i]  = _positionShiftDatas[i].Velocity;
            };

            var job = new PositionShiftJob()
            {
                Positions       = _positions,
                Velocities      = _velocities,
                FinalPositions  = _finalPositions
            };

            _handler = job.Schedule(_positions.Length, 1);
            _handler.Complete();

            StartCoroutine(AwaitForJobDone());
        }

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region Interface methods

        public void Dispose()
        {
            if (_positions.IsCreated)
            {
                _positions.Dispose();
            };

            if (_velocities.IsCreated)
            {
                _velocities.Dispose();
            };

            if (_finalPositions.IsCreated)
            {
                _finalPositions.Dispose();
            };
        }
        
        #endregion

        #region Methods

        private IEnumerator AwaitForJobDone()
        {
            while (!_handler.IsCompleted)
            {
                yield return null;
            };

            _jobResults = new Vector3[_finalPositions.Length];

            for(int i = 0; i < _finalPositions.Length; i++)
            {
                _jobResults[i] = _finalPositions[i];
            };

            Dispose();
        }

        #endregion
    }
}