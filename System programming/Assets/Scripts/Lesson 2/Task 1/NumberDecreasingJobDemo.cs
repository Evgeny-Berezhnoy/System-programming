using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Lesson_2
{
    public class NumberDecreasingJobDemo : MonoBehaviour
    {
        #region Fields

        [SerializeField] private int[] _numbers;
        [SerializeField] private int[] _jobResult;

        private NativeArray<int> _nativeNumbers;

        private JobHandle _handler;

        #endregion

        #region Unity events

        private void Start()
        {
            StartCoroutine(JobCoroutine());
        }

        private void OnDestroy()
        {
            if (_nativeNumbers != null && _nativeNumbers.IsCreated)
            {
                _nativeNumbers.Dispose();
            };
        }

        #endregion

        #region Methods

        private IEnumerator JobCoroutine()
        {
            _nativeNumbers = new NativeArray<int>(_numbers, Allocator.TempJob);

            var numberDecreasingJob = new NumberDecreasingJob()
            {
                Numbers = _nativeNumbers
            };

            _handler = numberDecreasingJob.Schedule();
            _handler.Complete();

            while (!_handler.IsCompleted)
            {
                yield return null;
            };

            _jobResult = new int[_numbers.Length];

            for(int i = 0; i < _nativeNumbers.Length; i++)
            {
                _jobResult[i] = _nativeNumbers[i];
            };

            _nativeNumbers.Dispose();
        }

        #endregion
    }
}