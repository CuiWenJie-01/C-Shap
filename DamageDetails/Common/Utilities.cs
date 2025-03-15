using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMaker.Common
{
    public static class Utilities
    {
        public static bool AreArraysEqual<T>(T[][] array1, T[][] array2) where T : IEquatable<T>
        {
            if (array1 == null && array2 == null)
            {
                return true;
            }
            else if (array1.GetLength(0) == 0 && array2.GetLength(0) == 0)
            {
                return true;
            }
            else if ((array1.GetLength(0) != array2.GetLength(0) || array1[0].Length != array2[0].Length))
            {
                return false;
            }

            return array1.Rank == 2 && array2.Rank == 2 &&
                   Enumerable.Range(0, array1.GetLength(0))
                             .All(i => Enumerable.Range(0, array1.GetLength(1))
                                               .All(j => array1[i][j].Equals(array2[i][j])));
        }
    }
}
