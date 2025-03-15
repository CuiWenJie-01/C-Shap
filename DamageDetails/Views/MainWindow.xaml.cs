using DamageMaker.Common;
using DamageMaker.Models;
using DamageMaker.Views;
using DamageMarker.ViewModels;
using HandyControl.Controls;
using HandyControl.Interactivity;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace DamageMarker.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {

        public static MainWindowViewModel MainVm;
        public MainWindow()
        {
            InitializeComponent();
            NonClientAreaContent = new NonClient();
            MainVm = new MainWindowViewModel();
            DataContext = MainVm;
            MainWindowViewModel.ScreenShotPopuped += OnScreenShotPopuped;
            PlaybackWindow.ScreenshotFinished += OnScreenshotFinished;
            Scal.ScaleX = 1 / DamageMaker.Common.Monitor.ScaleX;
            Scal.ScaleY = 1 / DamageMaker.Common.Monitor.ScaleY;
        }
        private void OnScreenShotPopuped(object sender, EventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void OnScreenshotFinished(object sender, EventArgs e)
        {
            WindowState = WindowState.Maximized;
        }
        protected override void OnClosing(CancelEventArgs e)
        {

            base.OnClosing(e);
            if (MainVm.Cap != null)
            {
                MainVm.Cap.Close();
            }
        }



        private void bthClick(object sender, RoutedEventArgs e)
        {
            Geometry? EyeCloseGeometry = System.Windows.Application.Current.FindResource("EyeCloseGeometry") as Geometry;
            Geometry? EyeOpenGeometry = System.Windows.Application.Current.FindResource("EyeOpenGeometry") as Geometry;
            var bth = sender as ToggleButton;

            if (bth != null)
            {
                if (bth.IsChecked ?? false)
                {
                    HandyControl.Controls.IconElement.SetGeometry(bth, EyeCloseGeometry);
                }
                else
                {
                    HandyControl.Controls.IconElement.SetGeometry(bth, EyeOpenGeometry);
                }
            }
        }

        private void ThumbnailList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ThumbnailList.ScrollIntoView((sender as ListBox).SelectedItem);
        }

        private void TextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!MainWindowViewModel.isBeiJIngOnly)
            {
                return;
            }

            var tb = (sender as TextBlock);
            var t = tb?.Text ?? " ";
            var tb2 = VisualTreeHelper.GetParent(tb);

            if (t.Contains("核伤1"))
            {
                RuleInfo.Text = "核伤1：在轨头区域找冒尖的，纵坐标最长（3个点以上）";
            }
            else if (t.Contains("核伤2"))
            {
                RuleInfo.Text = "核伤2：在一次波区域（2个点以上）";
            }
            else if (t.Contains("核伤3"))
            {
                RuleInfo.Text = "核伤3:在轨头区域找零星单个的（3个点以上）";
            }
            else if (t.Contains("鱼鳞伤"))
            {
                RuleInfo.Text = "鱼鳞伤：在轨头区域找较长的鱼鳞伤（6个点以上）";
            }
            else if (t.Contains("轨底裂纹"))
            {
                RuleInfo.Text = "轨底裂纹：找轨底正八字";
            }
            else if (t.Contains("斜裂纹"))
            {
                RuleInfo.Text = "单边37度,并且轨底失波";
            }
            else if (t.Contains("月牙伤2"))
            {
                RuleInfo.Text = "焊缝底下出现的单边八字";
            }
            else if (t.Contains("核伤4"))
            {
                RuleInfo.Text = "多通道出波";
            }
            else if (t.Contains("轨面剥离"))
            {
                RuleInfo.Text = "剥离:轨头三个角度探头都有出波。（内70°，外70°、直70°）";
            }

        }

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!MainWindowViewModel.isBeiJIngOnly)
            {
                return;
            }

            // 获取当前的 TextBlock
            var sp = sender as Border;

            // 查找父 TreeViewItem
            var Grid01 = FindLogicalParent<Grid>(sp);
            var Grid02 = Grid01 == null ? null : FindLogicalParent<Grid>(Grid01);

            if (Grid02 != null)
            {
                // 在这里执行找到父元素后的操作
                var text = (Grid02.DataContext as ITreeNode)?.Name;
                if (text?.Contains("规则") ?? false)
                {
                    if (text.Contains("核伤1"))
                    {
                        RuleInfo.Text = "核伤1：在轨头区域找冒尖的，纵坐标最长（3个点以上）";

                    }
                    else if (text.Contains("核伤2"))
                    {
                        RuleInfo.Text = "核伤2：在一次波区域（2个点以上）";

                    }
                    else if (text.Contains("核伤3"))
                    {
                        RuleInfo.Text = "核伤3:在轨头区域找零星单个的（3个点以上）";
                    }
                    else if (text.Contains("鱼鳞伤"))
                    {
                        RuleInfo.Text = "鱼鳞伤：在轨头区域找较长的鱼鳞伤（6个点以上）";
                    }
                    else if (text.Contains("轨底裂纹"))
                    {
                        RuleInfo.Text = "轨底裂纹：找轨底正八字";
                    }
                    else if (text.Contains("斜裂纹"))
                    {
                        RuleInfo.Text = "单边37度,并且轨底失波";
                    }
                    else if (text.Contains("月牙伤2"))
                    {
                        RuleInfo.Text = "焊缝底下出现的单边八字";
                    }
                    else if (text.Contains("核伤4"))
                    {
                        RuleInfo.Text = "多通道出波";
                    }
                    else if (text.Contains("轨面剥离"))
                    {
                        RuleInfo.Text = "剥离:轨头三个角度探头都有出波。（内70°，外70°、直70°）";
                    }
                }
            }
            else
            {
                Console.WriteLine("未找到父元素");
            }
        }


        public static T? FindLogicalParent<T>(DependencyObject child) where T : DependencyObject
        {
            // 获取父元素
            DependencyObject parentObject = VisualTreeHelper .GetParent(child);
            

            // 如果没有父元素，则返回 null
            if (parentObject == null) return null;

            // 如果父元素是指定类型，则返回父元素
            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                // 递归查找父元素
                return FindLogicalParent<T>(parentObject);
            }
        }

     
    }
}
