using System.Collections.Generic;
using System.Linq;

namespace Utils.Math
{
    public static class Statistics
    {
        /// <summary>
        ///   Gets the total number of possible combination of the specified maximum length.
        /// </summary>
        /// <param name="n"> The number of elements. </param>
        /// <param name="length"> The length of the combination. </param>
        /// <returns> </returns>
        public static int GetTotalCombinationCount(int n, int length)
        {
            int result = 0;
            for (int k = 1; k <= length; k++)
                result += GetCombinationCount(n, k);
            return result;
        }

        /// <summary>
        ///   Gets the number of possible combination of the specified length.
        /// </summary>
        /// <param name="n"> The number of elements. </param>
        /// <param name="length"> The length of the combination. </param>
        /// <returns> </returns>
        public static int GetCombinationCount(int n, int length)
        {
            return BinomialCoefficient(n, length);
        }

        /// <summary>
        ///   Gets the number of possible permitations.
        /// </summary>
        /// <param name="n"> The number of elements. </param>
        /// <returns> </returns>
        public static int GetPermutationCount(int n)
        {
            return Factorial(n);
        }

        /// <summary>
        ///   Gets the binomial coefficient n over k.
        /// </summary>
        /// <param name="n"> The n. </param>
        /// <param name="k"> The k. </param>
        /// <returns> </returns>
        public static int BinomialCoefficient(int n, int k)
        {
            if (k > n) return 0;
            int result = 1;
            for (int i = 0; i < k; i++)
            {
                result *= (n - i);
                result /= (i + 1);
            }
            return result;
        }

        /// <summary>
        ///   Gets n!
        /// </summary>
        /// <param name="n"> The n. </param>
        /// <returns> </returns>
        public static int Factorial(int n)
        {
            if (n < 2) return 1;
            int result = 1;
            for (int i = 2; i <= n; i++) result *= i;
            return result;
        }

        /// <summary>
        ///   Gets all possible combinations combinations with the specified length.
        /// </summary>
        /// <typeparam name="T"> The type of the result. </typeparam>
        /// <param name="elements"> The elements. </param>
        /// <param name="length"> The length. </param>
        /// <returns> </returns>
        public static List<List<T>> GetCombinations<T>(List<T> elements, int length)
        {
            return GetCombinations(0, elements, System.Math.Min(length, elements.Count));
        }

        /// <summary>
        ///   Gets all possible combinations with the specified length.
        /// </summary>
        /// <typeparam name="T"> The type of the result type. </typeparam>
        /// <param name="pos"> The current pos in the elements list. </param>
        /// <param name="elements"> The elements. </param>
        /// <param name="length"> The length. </param>
        /// <returns> </returns>
        private static List<List<T>> GetCombinations<T>(int pos, List<T> elements, int length)
        {
            var result = new List<List<T>>();
            if (length > 0)
            {
                for (int i = pos; i <= elements.Count - length; i++)
                {
                    foreach (List<T> lTmpResults in GetCombinations(i + 1, elements, length - 1))
                    {
                        lTmpResults.Insert(0, elements[i]);
                        result.Add(lTmpResults);
                    }
                }
            }
            else
            {
                result.Add(new List<T>());
            }
            return result;
        }

        /// <summary>
        ///   Gets all possible permutations.
        /// </summary>
        /// <typeparam name="T"> The type of the result. </typeparam>
        /// <param name="elements"> The elements. </param>
        /// <returns> </returns>
        public static List<List<T>> GetPermutations<T>(List<T> elements)
        {
            var result = new List<List<T>>();
            for (int i = 0; i < elements.Count; i++)
            {
                var remaining = elements.Where((t, j) => i != j).ToList();
                if (remaining.Count > 0)
                {
                    var tmp = GetPermutations(remaining);
                    foreach (var tmpResults in tmp)
                    {
                        tmpResults.Insert(0, elements[i]);
                        result.Add(tmpResults);
                    }
                }
                else
                {
                    var results = new List<T> {elements[i]};
                    result.Add(results);
                }
            }
            return result;
        }
    }
}