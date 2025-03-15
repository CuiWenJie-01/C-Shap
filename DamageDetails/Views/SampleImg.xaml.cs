﻿using CommunityToolkit.Mvvm.Input;
using DamageMaker.Common;
using DamageMaker.Models;
using DamageMaker.ViewModels;
using DamageMarker.ViewModels;
using DamageMarker.Views;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;
using Windows.UI.Core;
using Path = System.IO.Path;
using Window = System.Windows.Window;

namespace DamageMaker.Views
{
    /// <summary>
    /// SampleImg.xaml 的交互逻辑
    /// </summary>
    public partial class SampleImg : Window
    {

        public event EventHandler<BitmapSource?> ScreenShotTaken;
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private bool drawFlag;
        private bool IsGetPos = false;

        private Shape insertShape;

        private Point startPosition;

        private Point endPosition;



        public static SampleImgViewModel SharedViewModel { get; set; }

        public SampleImg(SampleImgViewModel vm = null)
        {
            SharedViewModel = vm ?? new SampleImgViewModel();

            DataContext = SharedViewModel;
            this.InputBindings.Add(new KeyBinding(new RelayCommand(CloseWindow), Key.X, ModifierKeys.Alt));
            InitializeComponent();


        }
        private void CloseWindow()
        {
            this.Close();
        }

        private void canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            insertShape = new Rectangle
            {
                Fill = null,
                Stroke = Brushes.Red,
                StrokeThickness = 5.0
            };
            if (insertShape != null)
            {
                drawFlag = true;
                Canvas canvas = sender as Canvas;
                startPosition = e.GetPosition(canvas);
                insertShape.Opacity = 1.0;
                Canvas.SetLeft(insertShape, e.GetPosition(canvas).X);
                Canvas.SetTop(insertShape, e.GetPosition(canvas).Y);
                canvas.Children.Add(insertShape);
            }
        }

        private void canv_MouseMove(object sender, MouseEventArgs e)
        {


            if (drawFlag && insertShape != null)
            {
                Canvas relativeTo = sender as Canvas;
                if (e.GetPosition(relativeTo).X > startPosition.X)
                {
                    insertShape.Width = e.GetPosition(relativeTo).X - startPosition.X;
                }
                else
                {
                    insertShape.Width = startPosition.X - e.GetPosition(relativeTo).X;
                    Canvas.SetLeft(insertShape, e.GetPosition(relativeTo).X);
                }
                if (e.GetPosition(relativeTo).Y > startPosition.Y)
                {
                    insertShape.Height = e.GetPosition(relativeTo).Y - startPosition.Y;
                }
                else
                {
                    insertShape.Height = startPosition.Y - e.GetPosition(relativeTo).Y;
                    Canvas.SetTop(insertShape, e.GetPosition(relativeTo).Y);
                }

            }
        }



        private async void canv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //禁用canv里面的所有按钮          
            drawFlag = false;
            endPosition = e.GetPosition(sender as Canvas);
            if (startPosition.X == endPosition.X && startPosition.Y == endPosition.Y)
            {
                canv.Children.Remove(insertShape);
                return;
            }

            if (insertShape != null)
            {
                insertShape.Opacity = 1.0;
                var point = endPosition - startPosition;
                Debug.WriteLine(point.X);
                Debug.WriteLine(point.Y);
                var startOffset = point switch
                { { X: > 0, Y: > 0 } => new Vector(0, 0),
                    { X: < 0, Y: > 0 } => new Vector(point.X, 0),
                    { X: > 0, Y: < 0 } => new Vector(0, point.Y),
                    { X: < 0, Y: < 0 } => new Vector(point.X, point.Y),
                    _ => new Vector()
                };


                var dialog = Dialog.Show<BoxSelectedCategoryDialog>();

                var aa = await dialog.GetResultAsync<DamageCategory>();


                dialog.Initialize<SampleImgViewModel>(vm =>
                 {
                     if (!vm.IsNotSave)
                     {
                         vm.BoxedStack.Add(new BoxSelectedControl()
                         {
                             RectColor = MainWindowViewModel.FloatToBrush((float)aa),
                             RectX = (int)(startPosition.X + startOffset.X),
                             RectY = (int)(startPosition.Y + startOffset.Y),
                             RectWidth = Math.Abs((int)point.X),
                             RectHeight = Math.Abs((int)point.Y),
                             RectOpacity = 1,
                             ButtonContent = vm.Result.ToString()
                         });
                         vm.damagePoints.Add(new float[] { (int)(startPosition.X + startOffset.X), (int)(startPosition.Y + startOffset.Y), Math.Abs((int)point.X), Math.Abs((int)point.Y), (float)vm.Result, 1.0f });
                         vm.OutImgPath = vm.ImgFullPath.InReplaceOutString();
                     }
                 });
            }
            canv.Children.Remove(insertShape);
        }
        //private void HideAllButtons(Canvas canvas)
        //{
        //    foreach (var child in canvas.Children)
        //    {
        //        if (child is Button button)
        //        {
        //            button.Visibility = Visibility.Collapsed;
        //        }
        //        else if (child is Panel panel)
        //        {
        //            HideAllButtons(panel);
        //        }
        //    }
        //}

        //private void HideAllButtons(Panel panel)
        //{
        //    foreach (var child in panel.Children)
        //    {
        //        if (child is Button button)
        //        {
        //            button.Visibility = Visibility.Collapsed;
        //        }
        //        else if (child is Panel childPanel)
        //        {
        //            HideAllButtons(childPanel);
        //        }
        //    }
        //}





        private async void canv_MouseEnter(object sender, MouseEventArgs e)
        {
            IsGetPos = true;
            var c = sender as Canvas;
            while (IsGetPos)
            {

                await Dispatcher.InvokeAsync(() =>
                {
                    pos.Text = $"x轴坐标:{e.GetPosition(c).X}\n\r y轴坐标:{e.GetPosition(c).Y}";
                    System.Diagnostics.Trace.WriteLine("aa");
                });


                await Task.Delay(100); // 等待100毫秒
            }
        }

        private void canv_MouseLeave(object sender, MouseEventArgs e)
        {
            IsGetPos = false;
            canv.Children.Remove(insertShape);
        }
        private BitmapSource? SaveCanvasScreenshot()
        {
            // 获取 Canvas 控件
            var canv = this.FindName("canv") as Canvas;
            if (canv == null)
            {
                HandyControl.Controls.MessageBox.Show("Canvas 控件未找到");
                return null;
            }

            // 创建 RenderTargetBitmap
            var renderBitmap = new RenderTargetBitmap(
                (int)canv.ActualWidth,
                (int)canv.ActualHeight,
                96d,
                96d,
                PixelFormats.Pbgra32);

            // 渲染 Canvas 到 RenderTargetBitmap
            renderBitmap.Render(canv);
            var outImgPath = (OutPath.DataContext as SampleImgViewModel)?.OutImgPath;
            if (!string.IsNullOrEmpty(outImgPath))
            {
                return renderBitmap;
            }
            else { return null; }



        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var screenShot = SaveCanvasScreenshot();
            ScreenShotTaken?.Invoke(sender, screenShot);

        }

    }






}

