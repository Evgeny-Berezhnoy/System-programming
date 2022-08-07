using System;
using UnityEngine;

namespace Lesson_2
{
    [Serializable]
    public struct PositionShiftData
    {
        #region Fields

        [SerializeField] public Vector3 Position;
        [SerializeField] public Vector3 Velocity;

        #endregion
    }
}