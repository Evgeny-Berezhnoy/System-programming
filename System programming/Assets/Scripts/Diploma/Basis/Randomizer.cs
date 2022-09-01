using System;

public static class Randomizer
{
    #region Fields

    private static int _seed = DateTime.Now.Millisecond;

    #endregion

    #region Properties

    public static int Seed
    {
        get
        {
            _seed += DateTime.Now.Millisecond;

            if(_seed == int.MaxValue)
            {
                _seed = int.MinValue;
            };

            return _seed;
        }
    }

    #endregion
}