using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FantasyFootballRobot.Core.Utils
{
    public class ParallelUtils
    {
        public static void While(Func<bool> condition, ParallelOptions parallelOptions, Action body)
        {
            try
            {
                Parallel.ForEach(IterateUntilFalse(condition), parallelOptions, ignored => body());
            }
            catch (AggregateException ex)
            {
                Debug.WriteLine("Aggregate exception thrown from Parallel while");

                Debug.WriteLine(ex.ToString());

                throw;
            }
        }

        private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition)
        {
            while (condition()) yield return true;
        }
    }
}