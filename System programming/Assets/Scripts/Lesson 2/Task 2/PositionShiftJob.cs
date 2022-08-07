using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Lesson_2
{
    public struct PositionShiftJob : IJobParallelFor
    {
        #region Fields

        public NativeArray<Vector3> Positions;
        public NativeArray<Vector3> Velocities;
        public NativeArray<Vector3> FinalPositions;
        
        #endregion

        #region Interface methods

        public void Execute(int index)
        {
            FinalPositions[index] = Positions[index] + Velocities[index];
        }

        #endregion
    }
}