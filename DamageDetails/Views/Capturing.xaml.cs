using DamageMarker.Views;
using HandyControl.Controls;
using System.Windows;
using Windows.UI.Popups;
using static DamageMarker.App;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DamageMaker.Views
{
    /// <summary>
    /// Capturing.xaml 的交互逻辑
    /// </summary>
    public partial class Capturing
    {

        PlaybackWindow? PbWin;
        RailwayInfoInputWindow RailWin;
        public static event Action<object, EventArgs>? RailWayInfoInputed;
        public Capturing()
        {
            InitializeComponent();
        }
        void ToNewSelectionWindow_Click(object sender, RoutedEventArgs e)
        {
            var b = HandyControl.Controls.MessageBox.Ask("在智能回放前,请您确认是否点击拼图按钮并将点大小改为4以提高识别率") switch
            {
                MessageBoxResult.OK => true,
                _ => false,

            };
            if (b)
            {
                new NewSelectionWindow().Show();
            }

        }
        void ToPlaybackWindow_Click(object sender, RoutedEventArgs e)
        {
            if (RailWin == null)
            {
                MessageBox.Info("智能回放前请先输入铁轨信息");
                RailWin = new RailwayInfoInputWindow();
                RailWayInfoInputed?.Invoke(sender, e);
            }


            PbWin = PlaybackWindow.GetInstance((mouseEndX >= mouseStartX) ? mouseStartX : mouseEndX, (mouseEndY >= mouseStartY) ? mouseStartY : mouseEndY, Math.Abs(mouseEndX - mouseStartX), Math.Abs(mouseEndY - mouseStartY));

            PbWin.Show();
            var b = PbWin.Visibility;

        }

        private void LeftMove(object sender, RoutedEventArgs e)
        {
            if (PbWin?.Activate() ?? false)
            {
                PbWin.ToLeftPlayback();
            }
            else
            {
                HandyControl.Controls.MessageBox.Error("请框选标记出现后点击此按钮");
            }
        }

        //private void StopMove(object sender, RoutedEventArgs e)
        //{
        //    if (PbWin != null)
        //    {
        //        PbWin?.StopPlayback();
        //    }
        //    else
        //    {
        //        HandyControl.Controls.MessageBox.Error("请框选后点击智能回放");
        //    }
        //}

        private void RightMove(object sender, RoutedEventArgs e)
        {
            if (PbWin?.Activate() ?? false)
            {
                PbWin.ToRightPlayback();
            }
            else
            {
                HandyControl.Controls.MessageBox.Error("请框选标记出现后点击此按钮");
            }
        }

        private void InputInfo(object sender, RoutedEventArgs e)
        {
            RailWin = new();
            RailWayInfoInputed?.Invoke(sender, e);
        }
    }
}