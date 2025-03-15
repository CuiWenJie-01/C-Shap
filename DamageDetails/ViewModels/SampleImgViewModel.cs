using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DamageMaker.Common;
using DamageMaker.Models;
using DamageMaker.Views;
using DamageMarker.ViewModels;
using DamageMarker.Views;
using HandyControl.Controls;
using HandyControl.Tools.Extension;

namespace DamageMaker.ViewModels
{

    public partial class SampleImgViewModel
        : ObservableObject,
            IDialogResultable<DamageCategory>
    {

        internal int currentImgIndex = 0; //当前图片的索引
        private BoxSelectedControl boxSelecteObject; //当前选择的框
        public string ImgFullPath; //图片名称

        [ObservableProperty]
        int leftLineX = 0;
        [ObservableProperty]
        int rightLineX = 0;

        [ObservableProperty]
        ImageSource? imgSource;

        [ObservableProperty]
        double canvasWidth;

        [ObservableProperty]
        double canvasHeight;
        internal List<DamageData>? damageDataList { get; set; } //json探伤数据序列化后
        internal List<float[]>? damagePoints;

        ObservableCollection<BoxSelectedControl> boxedStack = new(); //当前图片所有的框框
        public ObservableCollection<BoxSelectedControl> BoxedStack
        {
            get => boxedStack;
            set => boxedStack = value;
        }
        [ObservableProperty]
        string outImgPath;

        #region 弹出对话框变量
        DamageCategory result;
        public DamageCategory Result
        {
            get => result;
            set => SetProperty(ref result, value);
        }
        public Action CloseAction { get; set; }

        //ConboBox的绑定
        [ObservableProperty]
        ObservableCollection<DamageCategory> categorys;
        public bool IsNotSave { get; set; } = false;

        #endregion


        public SampleImgViewModel()
        {
            if (MainWindowViewModel.NeedSavedInfo != null)
            {
                RightLineX = MainWindowViewModel.NeedSavedInfo.ScreenshotWidthPx - MainWindowViewModel.NeedSavedInfo.ScreenshotOffset / 2;
                LeftLineX = MainWindowViewModel.NeedSavedInfo.ScreenshotOffset / 2;
            }


            OutImgPath = "";
            var CategorysTemp = Enum.GetValues(typeof(DamageCategory))
                .Cast<DamageCategory>()
                .ToList();
            Categorys = new ObservableCollection<DamageCategory>(CategorysTemp);

        }
        [RelayCommand]
        private void SelectCategory(string indexString)
        {
            int index = int.Parse(indexString);
            if (index >= 0 && index < Categorys.Count)
            {
                Result = Categorys[index];
            }
        }



        public SampleImgViewModel(
            ImageSource Img,
            double Width,
            double Height,
            List<DamageData>? DamageData,
            List<float[]> DamagePoints,
            int cruuentImgIndex,
            string imgFullPath
        )
            : this()
        {
            this.currentImgIndex = cruuentImgIndex;
            canvasWidth = Width;
            canvasHeight = Height;
            ImgSource = Img;
            this.damageDataList = DamageData;
            this.damagePoints = DamagePoints;
            this.ImgFullPath = imgFullPath;
            int count = 0;
            //把面积大的放在下面,
            this.damagePoints = DamagePoints.OrderByDescending(x => x[2] * x[3]).ToList();
            foreach (var point in this.damagePoints)
            {
                boxedStack.Add(
                    new BoxSelectedControl()
                    {
                        RectColor = MainWindowViewModel.FloatToBrush(point[4]),
                        RectX = (int)point[0],
                        RectY = (int)point[1],
                        RectWidth = (int)point[2],
                        RectHeight = (int)point[3],
                        RectRadiusX = point[2] / 2 * (1 - point[5]),
                        RectRadiusY = point[3] / 2 * (1 - point[5]),
                        RectOpacity = point[5],
                        ButtonContent = MainWindowViewModel.FloatToCategory(point[4]) + " " + count,
                    }
                );
                count++;
            }
        }

        DamageCategory? CategoryDataResult { get; set; }
        [RelayCommand]
        async void OpenDialog(BoxSelectedControl O)
        {
            boxSelecteObject = O;
            if (!string.IsNullOrEmpty(O.ButtonContent))
            {
                var cate = Regex.Replace(O.ButtonContent, @"\d+$", "");//去除末尾的数字
                Result = (DamageCategory)Enum.Parse(typeof(DamageCategory), cate);
            }
            var dialog = Dialog.Show<BoxSelectedCategoryDialog>();

            CategoryDataResult = await dialog.GetResultAsync<DamageCategory>();//必须用这个语句才能注册close与result
        }

        [RelayCommand]
        void Close()
        {

            IsNotSave = true;
            CloseAction?.Invoke();
            boxSelecteObject = null;
        }


        [RelayCommand]
        void Modify()
        {
            IsNotSave = false;
            if (boxSelecteObject != null)//进入修改流程
            {
                boxSelecteObject.ButtonContent = Result.ToString();
                boxSelecteObject.RectColor = MainWindowViewModel.FloatToBrush((float)(int)Result);
                boxSelecteObject.RectColor = MainWindowViewModel.FloatToBrush((float)Result);
                damagePoints = damagePoints
               .Select(x =>
               {
                   if (x[0] == boxSelecteObject.RectX && x[1] == boxSelecteObject.RectY)
                   {
                       x[4] = (float)Result;
                       x[5] = 1;
                   }
                   return x;
               })
              .ToList();
            }
            if (string.IsNullOrEmpty(OutImgPath))
            {
                OutImgPath = this.ImgFullPath.InReplaceOutString();
            }
            boxSelecteObject = null; //防止新增框选的时候，误修改上一个对象
            CloseAction?.Invoke();

        }

        [RelayCommand]
        void Delete()
        {
            IsNotSave = true;
            var result = System.Windows.MessageBox.Show("确定删除吗 ??", "删除?", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes && boxSelecteObject != null)
            {
                BoxedStack.Remove(boxSelecteObject);
                damagePoints = damagePoints?.Where(x => !((int)x[0] == boxSelecteObject.RectX && (int)x[1] == boxSelecteObject.RectY))
                    .ToList();

                if (string.IsNullOrEmpty(OutImgPath))
                {
                    OutImgPath = this.ImgFullPath.InReplaceOutString();
                }
                CloseAction?.Invoke();
            }
            else if (boxSelecteObject == null)
            {
                HandyControl.Controls.MessageBox.Warning("当前没有选中任何对象");
            }
        }
        [RelayCommand]
        void DeleteAll()
        {
            IsNotSave = true;
            var result = System.Windows.MessageBox.Show("确定删除吗 ??", "删除?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                BoxedStack.Clear();
                damagePoints = new List<float[]>();
                if (string.IsNullOrEmpty(OutImgPath))
                {
                    OutImgPath = this.ImgFullPath.InReplaceOutString();
                }
                CloseAction?.Invoke();
            }
        }



        [RelayCommand]
        void SampleClosing()
        {
            if (currentImgIndex != -1)//如果等于-1 则代表这个图片以前没有任何数据
            {
                if (!Utilities.AreArraysEqual<float>(MainWindow.MainVm.DamageDataList[currentImgIndex].DamagePoint, damagePoints.ToArray()))
                {
                    MainWindow.MainVm.DamageDataList[currentImgIndex].DamagePoint = damagePoints.ToArray();
                    MainWindow.MainVm.damagePoints = damagePoints.ToArray();
                    MainWindow.MainVm.IsModifyResultJson = true;
                }
            }
            else
            {
                if (damagePoints.Count > 0)
                {
                    MainWindow.MainVm.DamageDataList.Add(new DamageData() { Url = this.ImgFullPath, DamagePoint = damagePoints.ToArray() });
                    MainWindow.MainVm.damagePoints = damagePoints.ToArray();
                    MainWindow.MainVm.IsModifyResultJson = true;

                }
            }


        }

    }
}
