using Unity.Collections;
using Unity.Jobs;

namespace Lesson_2
{
    public struct NumberDecreasingJob : IJob
    {
        #region Fields

        public NativeArray<int> Numbers;

        #endregion

        #region Interface methods

        public void Execute()
        {
            for(int i = 0; i < Numbers.Length; i++)
            {
                if(Numbers[i] > 10)
                {
                    Numbers[i] = 0;
                };
            };
        }

        #endregion
    }
}