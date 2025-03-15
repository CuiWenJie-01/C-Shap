using DamageMarker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DamageMaker.Views
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class RailwayInfoInputWindow : HandyControl.Controls.Window
    {
        public RailwayInfoInputWindow()
        {
            InitializeComponent();
            DataContext = MainWindow.MainVm;
            Capturing.RailWayInfoInputed += OnRailwayInfoInputed;

        }

        private void OnRailwayInfoInputed(object sender, EventArgs e)
        {
            this.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Capturing.RailWayInfoInputed -= OnRailwayInfoInputed;
        }
    }
}
