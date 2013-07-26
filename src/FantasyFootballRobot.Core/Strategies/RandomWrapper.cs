using System;

namespace FantasyFootballRobot.Core.Strategies
{
    class RandomWrapper : IRandom
    {
        private static readonly Random Global = new Random();

        [ThreadStatic]
        private static Random _local;

        private Random LocalRandom
        {
            get
            {
                if (_local == null)
                {
                    int seed;
                    lock (Global)
                    {
                        seed = Global.Next();
                    }
                    _local = new Random(seed);
                }
                return _local;
            }
        }

        public int Next(int maxValue)
        {
            return LocalRandom.Next(maxValue);
        }

        public double NextDouble()
        {
            return LocalRandom.NextDouble();
        }
    }
}