using DamageMarker.Views;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



namespace DamageMaker.Views
{
    /// <summary>
    /// DamageFoldersList.xaml 的交互逻辑
    /// </summary>
    public partial class DamageFoldersList : System.Windows.Window
    {
        public DamageFoldersList()
        {
            InitializeComponent();
            if (MainWindow.MainVm != null)
            {
                DataContext = MainWindow.MainVm;
            }
        }


    }
}
