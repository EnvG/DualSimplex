using DualSimplex.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualSimplex
{
    public class SimplexMatrix
    {
        #region Свойства
        /// <summary>
        /// Значения из целевой функции
        /// </summary>
        public double[] C { get; set; }
        /// <summary>
        /// Значения матрицы X
        /// </summary>
        public Dictionary<string, double[]> X { get; set; }
        /// <summary>
        /// Значения свободных членов
        /// </summary>
        public double[] B { get; set; }
        public string[] Basis { get; set; }
        public double[] Deltas
        {
            get
            {
                List<double> result = new List<double>();

                foreach (var x in this.X)
                {
                    double delta = 0.0;
                    int key = Int32.Parse(x.Key.Replace("x", ""));
                    foreach (string basis in this.Basis)
                    {
                        double c = this.C[Int32.Parse(basis.Replace("x", "")) - 1];
                        for (int i = 0; i < this.X[basis].Length; i++)
                        {
                            delta += c * this.X[$"x{key}"][i];
                        }

                    }
                    result.Add(delta - this.C[key - 1]);
                }

                return result.ToArray();
            }
        }
        #endregion
        #region Конструкторы
        public SimplexMatrix(double[] c = null, Dictionary<string, double[]> x = null, double[] b = null)
        {
            int count = x.First().Value.Length;
            List<string> basis = new List<string>();

            // Дополнение матрицы нулями до нужной длины
            c = c.ZeroExpand(count + x.Count);
            // Дополнение матрицы единичной матрицей
            x = x.IdentityExpand();
            // Заполнение базисов
            basis.AddRange(Enumerable.Range(count, x.Count - count + 1).Select(n => $"x{n}").ToArray());

            this.C = c;
            this.X = x;
            this.B = b;
            this.Basis = basis.ToArray();
        }
        #endregion
        #region Методы
        /// <summary>
        /// Получить ключ опорного столбца
        /// </summary>
        /// <returns></returns>
        public string GetSolveColumnKey()
        {
            int index = 0;
            double min = this.Deltas[0];

            // Поиск минимальной дельты
            for (int i = 1; i < this.Deltas.Length; i++)
            {
                if (this.Deltas[i] < min)
                {
                    min = this.Deltas[i];
                    index = i + 1;
                }
            }

            return $"x{index}";
        }
        /// <summary>
        /// Получить ключ опорной строки
        /// </summary>
        /// <returns></returns>
        public string GetSolveRowKey()
        {
            int index = 0;
            // Ключ опорного столбца
            string solveColumnKey = this.GetSolveColumnKey();
            double min = this.B[0] / this.X[solveColumnKey][0];

            for (int i = 0; i < this.B.Length; i++)
            {
                // Отношение свободного члена к соответствующему элементу опорного столбца
                double q = this.B[i] / this.X[solveColumnKey][i];

                if (q < min)
                {
                    min = q;
                    index = i;
                }
            }

            // Возвращение базиса по найденному индексу
            return $"{this.Basis[index]}";
        }
        /// <summary>
        /// Получить опорный элемент
        /// </summary>
        /// <returns></returns>
        public double GetSolveElement()
        {
            string solveColumnKey = this.GetSolveColumnKey();
            string solveRowKey = this.GetSolveRowKey();
            int rowIndex = this.Basis.GetIndex<string>(solveRowKey);

            return this.X[solveColumnKey][rowIndex];
        }
        /// <summary>
        /// Перерасчёт матрицы X
        /// </summary>
        private void RecalculateX(string solveColumnkey, int solveRow, double solveElement)
        {
            Dictionary<string, double[]> matrix = new Dictionary<string, double[]>();
            
            foreach (var x in this.X)
            {
                matrix.Add(x.Key, x.Value);
            }

            for (int i = 0; i < matrix.Count; i++)
            {
                for (int j = 0; j < matrix[$"x{i + 1}"].Length; j++)
                {
                    if (i == solveRow - 1)
                    {
                        matrix[$"x{i + 1}"][solveRow] /= solveElement;
                    }
                    else
                    {
                        matrix[$"x{i + 1}"][j] -= this.X[solveColumnkey][j] / solveElement * this.X[$"x{i + 1}"][this.Basis.GetIndex<string>(solveColumnkey)];
                    }
                }
            }
            this.X = matrix;

        }
        private void RecalculateB(string solveColumnKey, int solveRow)
        {
            this.B[solveRow] /= this.GetSolveElement();
            for (int i = 0; i < this.B.Length; i++)
            {
                if (i != solveRow)
                {
                    this.B[i] -= this.X[solveColumnKey][i] * this.B[solveRow];
                }
            }
        }
        /// <summary>
        /// Переназначить значения базиса
        /// </summary>
        /// <param name="solveColumnKey"></param>
        /// <param name="solveRow"></param>
        private void ResetBasis (string solveColumnKey, int solveRow)
        {
            this.Basis[solveRow] = solveColumnKey;
        }
        /// <summary>
        /// Попытаться решить задачу
        /// </summary>
        /// <returns></returns>
        public bool TrySolve ()
        {
            string solveColumnKey = this.GetSolveColumnKey();
            int solveRow = this.Basis.GetIndex<string>(this.GetSolveRowKey());
            double solveElement = this.GetSolveElement();
            this.RecalculateB(solveColumnKey, solveRow);
            this.ResetBasis(solveColumnKey, solveRow);
            this.RecalculateX(solveColumnKey, solveRow, solveElement);

            return this.CheckSolve();
        }
        /// <summary>
        /// Проверить решённость задачи
        /// </summary>
        /// <returns></returns>
        public bool CheckSolve()
        {
            foreach (decimal delta in this.Deltas)
            {
                if (delta < 0)
                    return false;
            }

            return true;
        }
        #endregion
    }
}
