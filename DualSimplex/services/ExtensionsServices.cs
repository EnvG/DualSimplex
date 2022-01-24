using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualSimplex.services
{
    public static class ExtensionsServices
    {
        /// <summary>
        /// Заполнить массив нулями, пока его длина не станет равна нужной
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">Массив</param>
        /// <param name="length">Требуемая длина</param>
        public static double[] ZeroExpand (this double[] array, int length)
        {
            if (array.Length >= length)
                return array;

            while (array.Length < length)
            {
                array = array.Append(0).ToArray();
            }

            return array;
        }
        /// <summary>
        /// Дополнить матрицу единичной матрицей
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Dictionary<string, double[]> IdentityExpand(this Dictionary<string, double[]> matrix)
        {
            // Количество столбцов матрицы
            int count = matrix.First().Value.Length;
            for (int i = 0; i < count; i++)
            {
                List<double> column = new List<double>();
                while (column.Count < count)
                {
                    // Заполнение главной диагонали единицами
                    if (column.Count == i)
                    {
                        column.Add(1);
                        continue;
                    }

                    column.Add(0);
                }
                matrix.Add($"x{matrix.Count + 1}", column.ToArray());
            }

            return matrix;
        }
        /// <summary>
        /// Получить индекс первого элемента, равного данному
        /// </summary>
        /// <param name="array"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int GetIndex<T>(this T[] array, T n)
        {
            for (int i = 0;i < array.Length; i++)
            {
                if (array[i].Equals(n))
                    return i;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
