using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DualSimplex.viewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        #endregion
        #region Поля
        private SimplexMatrix matrix;
        #endregion
        #region Свойства
        public SimplexMatrix Matrix
        {
            get => this.matrix;
            set
            {
                this.matrix = value;
                this.OnPropertyChanged();
            }
        }
        #endregion
        public MainViewModel()
        {
            double[] C = new double[] { 30, 40 };
            Dictionary<string, double[]> X = new Dictionary<string, double[]>()
            {
                { "x1", new double[] { 12, 4, 3, 7 } },
                { "x2", new double[] { 4, 4, 12, 7 } },
            };
            double[] B = new double[] { 300, 120, 252, 7 };

            this.Matrix = new SimplexMatrix(C, X, B);
        }
    }
}
