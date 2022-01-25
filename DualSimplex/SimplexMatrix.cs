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
        /// Шапка таблицы
        /// </summary>
        public List<string> Headers { get; set; }
        /// <summary>
        /// Таблица
        /// </summary>
        private List<string[]> Matrix { get; set; }
        /// <summary>
        /// Вектор базисов
        /// </summary>
        public List<string> Basis { get; set; }
        /// <summary>
        /// Матрица X
        /// </summary>
        public List<double[]> X
        {
            get
            {
                List<double[]> matrixX = new List<double[]>();

                // Получаем матрицу X (от конца первого столбца (базисы) до конца предпоследнего (свободные члены))
                for (int i = 0; i < this.Matrix.Count - 1; i++)
                {
                    double[] x = this.Matrix[i].Select(m => Double.Parse(m)).ToArray();
                    matrixX.Add(x);
                }

                return matrixX;
            }
        }
        /// <summary>
        /// Вектор свободных членов
        /// </summary>
        public double[] B
        {
            get
            {
                return this.Matrix.Last().Select(m => Double.Parse(m)).ToArray();
            }
        }
        /// <summary>
        /// Коэффициенты при x в целевой функции
        /// </summary>
        public double[] C { get; set; }
        public double[] Deltas
        {
            get
            {
                List<double> deltas = new List<double>();
                for(int i = 0; i < this.X.Count; i++)
                {
                    double delta = 0;
                    for (int j = 0; j < this.X[i].Length; j++)
                    {
                        delta += this.X[i][j] * this.GetC(j);
                    }
                    deltas.Add(delta - this.C[i]);
                }

                return deltas.ToArray();
            }
        }
        public bool IsSolved
        {
            get
            {
                foreach (double delta in this.Deltas)
                {
                    if (delta < 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion
        #region Индексаторы
        public double this [int row, int column]
        {
            get => this.X[column][row];
            set => this.Matrix[column][row] = value.ToString();
        }
        #endregion
        #region Конструктор
        public SimplexMatrix(double[] C, List<double[]> X, double[] B, params string[] Headers)
        {
            this.Matrix = new List<string[]>();
            this.Headers = Headers.ToList();
            this.Basis = new List<string>();
            this.C = C;
            this.C = this.C.ZeroExpand(X.Count + X[0].Length);

            foreach (int i in Enumerable.Range(X.Count + 1, X[0].Length))
            {
                this.Basis.Add($"x{i}");
            }

            X = X.IdentityExpand();
            

            for (int i = 0; i < X.Count; i++)
            {
                this.Matrix.Add(X[i].Select(x => x.ToString()).ToArray());
            }

            this.Matrix.Add(B.Select(b => b.ToString()).ToArray());
        }
        #endregion
        #region Методы
        /// <summary>
        /// Получить значение коэффициента по индексу в матрице, которые соответствует нужному базису
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private double GetC(int i)
        {
            int index = Int32.Parse(this.Basis[i].Substring(1)) - 1;

            return this.C[index];
        }
        /// <summary>
        /// Получить индекс разрешающего столбца
        /// </summary>
        /// <returns></returns>
        private int GetSolveColumnIndex()
        {
            int index = 0;
            double min = this.Deltas[index];

            for (int i = 1; i < this.Deltas.Length; i++)
            {
                if (this.Deltas[i] < min)
                {
                    min = this.Deltas[i];
                    index = i;
                }
            }

            return index;
        }
        /// <summary>
        /// Получить индекс разрешающей строки
        /// </summary>
        /// <returns></returns>
        private int GetSolveRowIndex ()
        {
            int index = 0;
            int solveColumnIndex = this.GetSolveColumnIndex();
            double min = this.B[0] / this[0, solveColumnIndex];

            for (int i = 1; i < this.B.Length; i++)
            {
                double q = this.B[i] / this[i, solveColumnIndex];
                if (q < min)
                {
                    min = q;
                    index = i;
                }
            }

            return index;
        }
        /// <summary>
        /// Получить разрешающий элемент
        /// </summary>
        /// <returns></returns>
        private double GetSolveElement()
        {
            int solveRowIndex = this.GetSolveRowIndex();
            int solveColumnIndex = this.GetSolveColumnIndex();

            return this[solveRowIndex, solveColumnIndex];
        }
        private void RecalculateB(double solveElement)
        {
            for (int i = 0; i < this.Matrix.Last().Length; i++)
            {
                this.Matrix.Last()[i] = $"{Double.Parse(this.Matrix.Last()[i]) / solveElement}";
            }
        }
        public bool TrySolve()
        {
            int solveRowIndex = this.GetSolveRowIndex();
            int solveColumnIndex = this.GetSolveColumnIndex();
            // Копия матрицы X
            List<double[]> x = this.X.Select(n => (double[])n.Clone()).ToList();
            
            // Деление разразрешающей строки на разразрешающий элемент
            for (int i = 0; i < this.X.Count; i++)
            {
                this[solveRowIndex, i] /= x[solveColumnIndex][solveRowIndex];
            }

            // Вычисление матрицы X
            for (int i = 0; i < this.X.Count; i++)
            {
                for (int j = 0; j < this.X[i].Length; j++)
                {
                    if (j != solveRowIndex)
                    {
                        this[j, i] -= x[solveColumnIndex][j] * this[solveRowIndex, i];
                    }
                }
            }

            this.RecalculateB(x[solveColumnIndex][solveRowIndex]);

            return this.IsSolved;
        }
        #endregion
    }
}