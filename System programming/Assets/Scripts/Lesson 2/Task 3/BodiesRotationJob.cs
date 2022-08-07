using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Lesson_2
{
    public struct BodiesRotationJob : IJobParallelForTransform
    {
        #region Fields

        [ReadOnly] public float RotationFrequency;
        [ReadOnly] public float DeltaTime;

        #endregion

        #region Interface methods

        public void Execute(int index, TransformAccess transform)
        {
            var angle = RotationFrequency * DeltaTime;

            transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
        }

        #endregion
    }
}