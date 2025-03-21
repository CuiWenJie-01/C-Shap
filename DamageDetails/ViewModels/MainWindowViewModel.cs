﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DamageMaker.Automation;
using DamageMaker.Common;
using DamageMaker.Models;
using DamageMaker.Properties;
using DamageMaker.ViewModels;
using DamageMaker.Views;
using DamageMarker.Models;
using DamageMarker.Views;
using FlaUI.Core.WindowsAPI;
using HandyControl.Controls;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Xceed.Document.NET;
using Xceed.Words.NET;
using static DamageMaker.Models.Records;
using static System.Net.WebRequestMethods;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using File = System.IO.File;
using FormattedText = System.Windows.Media.FormattedText;
using MessageBox = HandyControl.Controls.MessageBox;
using Orientation = Xceed.Document.NET.Orientation;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace DamageMarker.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public static bool isBeiJIngOnly = false;
        bool isHideNormal = true;
        [ObservableProperty]
        bool isDistinctDamage = true;

        bool isDistinctErrorBoxed = false;
        [ObservableProperty]
        private string version;

        [ObservableProperty]
        private string inPath = Settings.Default.InPath;
        [ObservableProperty]
        private string outPath = Settings.Default.OutPath;
        [ObservableProperty]
        private string docxPath = Settings.Default.DocxPath;

        [ObservableProperty]

        private string yoloPath = Settings.Default?.YoloLocation;

        [ObservableProperty]
        private bool isShowAllBoxSelected;

        [ObservableProperty]
        private SetupContent setup = new SetupContent();
        public bool IsModifyResultJson { get; set; } = false;
        Stopwatch stopwatch = new Stopwatch();
        [ObservableProperty]
        int allImgCount = 0;
        [ObservableProperty]
        int singleImgCount = 0;
        int currentImgIndex;//当前图片在damageDatalist中的索引
        [ObservableProperty]
        ImgInfo selectedThumbnailImg;

        [ObservableProperty]
        private List<DamgeCategorySummaryTree> damageTree;

        [ObservableProperty]
        DamageFoldersInfo selectedFolder;
        public Sprite Cap; //截图窗口
        List<string> filterFiles;
        private ObservableCollection<DamageFoldersInfo> damageFolders =
            new ObservableCollection<DamageFoldersInfo>();
        public ObservableCollection<DamageFoldersInfo> DamageFolders
        {
            get => damageFolders;
            set => damageFolders = value;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ShowAllBoxSelectedCommand))]




        int stepIndex = 0;
        private int screenshotInterval; //截图时间间隔
        public int ScreenshotInterval
        {
            get { return screenshotInterval; }
            set
            {
                if (value != screenshotInterval)
                {
                    DamageMaker.Properties.Settings.Default.ScreenshotInterval = value;
                    SetProperty(ref screenshotInterval, value);
                }
            }
        }

        private int screenshotOffset; //截图偏移量
        public int ScreenshotOffset
        {
            get { return screenshotOffset; }
            set
            {

                if (value != screenshotOffset)
                {
                    DamageMaker.Properties.Settings.Default.ScreenshotOffset = value;
                    SetProperty(ref screenshotOffset, value);
                }
            }
        }

        private int mouseMovePixel; //截图偏移量
        public int MouseMovePixel
        {
            get { return mouseMovePixel; }
            set
            {

                if (value != mouseMovePixel)
                {
                    DamageMaker.Properties.Settings.Default.MouseMovePixel = value;
                    SetProperty(ref mouseMovePixel, value);
                }
            }
        }

        [ObservableProperty]
        Visibility progressVisibility = Visibility.Hidden;

        [ObservableProperty]
        private int currentProgress = 0;

        [ObservableProperty]
        private int totalProgress = 100;

        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private string[]? imgFiles; //所有未判伤图片

        [ObservableProperty]
        private bool isFilterThumbnail = false;

        [ObservableProperty]
        private bool isEnableThumbnail;

        public List<DamageData>? DamageDataList; //json探伤数据序列化后
        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        public static event Action<object, EventArgs>? ScreenShotPopuped;
        static readonly HttpClient client = new HttpClient();
        private bool isDataExist; //表明单个图片是否有对应的伤损数据

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ModifyImgCommand))]
        int selectedIndex;

        string ImgPath;

        [ObservableProperty]
        string imgFolderName;
        [ObservableProperty]
        string url = "http://127.0.0.1:3333/endpoint";

        [ObservableProperty]
        string parameter; // 发送给后端的参数
        ObservableCollection<ImgInfo> thumbnailImgInfos = new(); //缩略图的信息
        public ObservableCollection<ImgInfo> ThumbnailImgInfos
        {
            get => thumbnailImgInfos;
            set => thumbnailImgInfos = value;
        }
        ObservableCollection<ImgInfo> originalThumbnailImgInfos = new(); //原始缩略图的信息

        [ObservableProperty]
        string? jsonData;

        public float[][]? damagePoints;

        [ObservableProperty]
        BitmapFrame? imgSource;

        ObservableCollection<MenuItem> menuItems = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> MenuItems
        {
            get => menuItems;
            set => menuItems = value;
        }
        ObservableCollection<DamageDetails> detailsList = new ObservableCollection<DamageDetails>(); //单个图片伤损信息列表
        public ObservableCollection<DamageDetails> DetailsList
        {
            get => detailsList;
            set => detailsList = value;
        }
        public static ScreenshotInfo? NeedSavedInfo { get; set; } = null;
        string damageImgFolderName;

        public MainWindowViewModel()
        {
            Version = isBeiJIngOnly ? "V0.82B" : "V0.82";
            SelectedIndex = -1;
            ScreenshotOffset = DamageMaker.Properties.Settings.Default.ScreenshotOffset;
            ScreenshotInterval = DamageMaker.Properties.Settings.Default.ScreenshotInterval;
            MouseMovePixel = DamageMaker.Properties.Settings.Default.MouseMovePixel;
            client.Timeout = TimeSpan.FromSeconds(1200);
            //Parameter = ImgFolderName;



            foreach (var AppPath in DamageMaker.Properties.Settings.Default.AppLocation)
            {
                MenuItems.Add(
                    new MenuItem
                    {
                        Header = Path.GetFileName(AppPath),
                        CommandParameter = AppPath,
                        Command = ProcessStartCommand
                    }
                );
            }
            MenuItems.Add(new MenuItem
            {
                Header = "✚ 添加回放软件",
                CommandParameter = null,
                Command = SelectExeFileCommand
            });
            CheckAppStatusPeriodically();
            PlaybackWindow.ScreenshotStart += OnScreenShotStart;
            PlaybackWindow.ScreenshotFinished += OnScreenShotFinished;
        }

        [RelayCommand]
        void ScreenShotFunc()
        {
            ScreenShotPopuped?.Invoke(this, EventArgs.Empty);
            Cap = Sprite.Show(new Capturing());
        }

        public void UpdateImgSource(string imgPath)
        {

            // 使用 BitmapImage 加载图像，避免文件锁定
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 确保图像数据加载到内存中，避免文件锁定
            bitmapImage.UriSource = new Uri(imgPath, UriKind.Relative);
            bitmapImage.EndInit();
            //更新 ImgSource  
            ImgSource = BitmapFrame.Create(bitmapImage);
        }

        [RelayCommand]
        void ImgSelectionChanged()
        {

            if (SelectedIndex != -1)
            {
                DetailsList.Clear();
                ImgPath = ThumbnailImgInfos[SelectedIndex].Path;
                UpdateImgSource(ImgPath);
                SearchMileageText = Path.GetFileNameWithoutExtension(ImgPath).Split('_')[0].Trim();
                (damagePoints, currentImgIndex) = GetDamagePointsAndIndex(ImgPath);
                isDataExist = damagePoints == null ? false : true;
                ModifyImgCommand.NotifyCanExecuteChanged();
                //  ShowBoxSelectedCommand.NotifyCanExecuteChanged();
                if (isDataExist)
                {
                    InitDamageDetails(damagePoints);
                }
            }
            else
            {
                damagePoints = null;
                ImgPath = null;
                ImgSource = null;
            }
        }

        DamageFoldersList foldersListWindow;


        #region 打开历史数据后弹出的窗口
        [RelayCommand]
        async Task SelectedPath() //选择文件夹
        {
            DamageFolders.Clear();
            var imgPaths = Directory.GetDirectories(Settings.Default.InPath);
            if (imgPaths.Length == 0 || imgPaths == null)
            {
                MessageBox.Warning("没有找到历史数据,请先进行数据采集");
                return;
            }

            var docxPaths = Directory.GetFiles(Settings.Default.DocxPath).Select(x => Path.GetFileName(x));
            foreach (var path in imgPaths)
            {
                DamageFolders.Add(
                    new DamageFoldersInfo
                    {
                        FolderName = Path.GetFileName(path),
                        HasDamage = File.Exists(Path.Combine(path, "result.json")),
                        CreatTime = Directory.GetCreationTime(path),
                        PngCount = Directory.GetFiles(path, "*.png").Count(),
                        DamagePngCount =
                        Directory.Exists(path.InReplaceOutString())
                        ? Directory.GetFiles(path.InReplaceOutString(), "*.png").Count()
                        : 0,
                        HasDocx = docxPaths.Any(x => x.StartsWith(Path.GetFileName(path))),
                        HasMileage = File.Exists(Path.Combine(path, "OcrResult.json"))
                    }
                );
            }
            foldersListWindow = new DamageFoldersList();
            foldersListWindow.Owner = Application.Current.MainWindow;
            foldersListWindow.ShowDialog();


        }


        /// <summary>
        /// 双击文件夹后的操作
        /// </summary>
        /// <param name="pa">这个是双击文件夹的对象</param>
        /// <returns></returns>
        [RelayCommand]
        async Task DoubleClickDamageFolder(object pa)
        {
            if (pa != null)
            {
                var FolderName = (pa as DamageFoldersInfo).FolderName;
                foldersListWindow.Close();
                var FolderPath = Path.Combine(Path.GetFullPath(Settings.Default.InPath), FolderName);
                SwitchDamageFolder(FolderPath);
                await ShowThumbnails(FolderPath);
            }
            else
            {
                MessageBox.Warning("请双击文件夹");
            }
        }

        /// <summary>
        /// 切换伤损文件夹
        /// </summary>
        /// <param name="FolderPath">需要切换伤损文件夹的路径</param>
        private void SwitchDamageFolder(string FolderPath)
        {
            NeedSavedInfo = null;
            ClearImgAndData();
            if (IsModifyResultJson)
            {
                SaveJson<List<DamageData>>(DamageDataList, Path.Combine(Settings.Default.InPath, ImgFolderName), "result.json");
            }
            IsModifyResultJson = false;
            IsShowAllBoxSelected = false;
            NeedSavedInfo = DeserializeJson<ScreenshotInfo>(Path.Combine(FolderPath, "info.json"));
        }

        bool IsSelectedFolder() => SelectedFolder == null ? false : true;

        [RelayCommand(CanExecute = nameof(IsSelectedFolder))]
        void OpenDocx(DamageFoldersInfo d)
        {
            if (d.HasDocx)
            {
                StartProcess(
                    DamageMaker.Properties.Settings.Default.OfficeLocation,
                    Path.GetFullPath(
                        Path.Combine(Settings.Default.DocxPath, d.FolderName + "钢轨探伤检测报告.docx")
                    )
                );
            }
            else
            {
                MessageBox.Info("当前图片集没有报表文件,请选择当前图片集后点击导出报表按钮");
            }
        }
        #endregion

        [RelayCommand]
        void FilterImg() //点击数据筛选按钮
        {
            IsShowAllBoxSelected = false;
            if (filterFiles == null)
            {
                InitializeThumbnailImgInfos();
            }
            else
            {
                UpdataThumbnail();
            }
        }

        T? DeserializeJson<T>(string jsonFilePath)
        {
            if (File.Exists(jsonFilePath))
            {
                var jsonString = File.ReadAllText(jsonFilePath);
                return JsonSerializer.Deserialize<T>(jsonString);
            }
            else
            {

                return default(T);
            }
        }
        List<DamageData> MaskDamage(List<DamageData> data)
        {

            //var result = data.Select(x => new DamageData
            //{
            //    Url = x.Url,
            //    DamagePoint = x.DamagePoint.Where(x => x[4] != 0).ToArray()
            //}).Where(x => x.DamagePoint?.Length != 0).ToList();
            var result = data.Where(x => x.DamagePoint.All(y => FloatToBrush(y[4]) != Brushes.Green)).Where(x => x.DamagePoint?.Length != 0).ToList();

            return result;
        }
        float[][] MaskDamge(float[][] data)
        {
            return data.Where(x => x[4] != 0).ToArray();
        }



        // 修正 DistinctDamage 方法，避免变量名冲突
        List<DamageData> DistinctDamage(List<DamageData> data, string[] FileNames)
        {
            if (NeedSavedInfo == null)
            {
                return data;
            }

            var FirstImg = FileNames[0];
            var LastImg = FileNames[FileNames.Length - 1];

            if (NeedSavedInfo.ScreenshotDirection == MoveDirection.Left)
            {
                data = data.Select(d =>
                {
                    if (d.Url == FirstImg)
                    {
                        d.DamagePoint = d.DamagePoint.Where(dp => dp[0] >= NeedSavedInfo.ScreenshotOffset / 2).ToArray();
                    }
                    else if (d.Url == LastImg)
                    {
                        d.DamagePoint = d.DamagePoint.Where(dp => dp[0] < NeedSavedInfo.ScreenshotWidthPx - NeedSavedInfo.ScreenshotOffset / 2).ToArray();
                    }
                    else
                    {
                        d.DamagePoint = d.DamagePoint.Where(dp =>
                            dp[0] >= NeedSavedInfo.ScreenshotOffset / 2 &&
                            dp[0] < NeedSavedInfo.ScreenshotWidthPx - NeedSavedInfo.ScreenshotOffset / 2
                        ).ToArray();
                    }
                    return d;
                }).ToList();
            }
            else if (NeedSavedInfo.ScreenshotDirection == MoveDirection.Right)
            {
                data = data.Select(d =>
                {
                    if (d.Url == FirstImg)
                    {
                        d.DamagePoint = d.DamagePoint.Where(dp => dp[0] < NeedSavedInfo.ScreenshotWidthPx - NeedSavedInfo.ScreenshotOffset / 2).ToArray();
                    }
                    else if (d.Url == LastImg)
                    {
                        d.DamagePoint = d.DamagePoint.Where(dp => dp[0] >= NeedSavedInfo.ScreenshotOffset / 2).ToArray();
                    }
                    else
                    {
                        d.DamagePoint = d.DamagePoint.Where(dp =>
                            dp[0] >= NeedSavedInfo.ScreenshotOffset / 2 &&
                            dp[0] < NeedSavedInfo.ScreenshotWidthPx - NeedSavedInfo.ScreenshotOffset / 2
                        ).ToArray();
                    }
                    return d;
                }).ToList();
            }
            return data;
        }



        public async Task ShowThumbnails(string SelectedPath)
        {

            Parameter = SelectedPath;


            ImgFolderName = Path.GetFileName(SelectedPath);
            damageImgFolderName = SelectedPath.InReplaceOutString();

            if (!Directory.Exists(damageImgFolderName))
            {
                Directory.CreateDirectory(damageImgFolderName);
            }
            string jsonFilePath = Path.Combine(Parameter, "result.json"); //寻找json文件的路径
            if (File.Exists(jsonFilePath))
            {

                var task = Task.Run(() =>
                {
                    return Directory.GetFiles(Parameter, "*.png")
                        .OrderBy(File.GetCreationTime)
                        .ToArray();
                });
                imgFiles = await task.ContinueWith(
                    t => t.Result,
                    TaskScheduler.FromCurrentSynchronizationContext()
                );

                //去除重叠区域的伤损标记
                DamageDataList = DeserializeJson<List<DamageData>>(jsonFilePath);
                if (isDistinctDamage)
                {
                    DamageDataList = DistinctDamage(DamageDataList, imgFiles);
                }



                filterFiles = DamageDataList.Select(x => x.Url).ToList();


                filterFiles = filterFiles.OrderBy(File.GetCreationTime).ToList();
                AllImgCount = imgFiles.Length;
                SingleImgCount = filterFiles.Count;

                ModifyImgCommand.NotifyCanExecuteChanged();
                InitCategorySummary();
                IsEnableThumbnail = true;
                ExportReportCommand.NotifyCanExecuteChanged();
                GetJsonResultCommand.NotifyCanExecuteChanged();
                dispatcher.Invoke(
                    (Delegate)(
                        () =>
                        {
                            ThumbnailImgInfos.Clear();
                        }
                    )
                );

                UpdataThumbnail();

                SaveAllBoxSelectedImg(filterFiles, damageImgFolderName);
                StepIndex = 2;
            }
            else
            {
                ClearDamagePage();
                MessageBox.Warning("这个文件夹没有对应的损伤数据,请请求数据后重新打开该文件夹");
            }
        }



        private void ClearDamagePage()
        {
            AllImgCount = 0;
            SingleImgCount = 0;
            ThumbnailImgInfos.Clear();
            ImgSource = null;
            StepIndex = 1;
            IsEnableThumbnail = false;
            DamageDataList = null;
            GetJsonResultCommand.NotifyCanExecuteChanged();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Imgfiles"></param>
        /// <param name="SavePath"></param>
        /// <returns></returns>
        async Task SaveAllBoxSelectedImg(List<string> Imgfiles, [NotNull] string SavePath)
        {
            stopwatch.Restart();
            int saveImgCount = 0;
            await Task.Run(delegate
            {
                Parallel.ForEach((IEnumerable<string>)Imgfiles, (Action<string>)delegate (string imgFile)
                {
                    string fileName = Path.GetFileName(imgFile);
                    if (!File.Exists(Path.Combine(SavePath, fileName)))
                    {
                        BitmapFrame boxSelectedImg = BitmapFrame.Create(new Uri(imgFile));
                        float[][] item = GetDamagePointsAndIndex(imgFile).Item1;
                        //if (isHideNormal)
                        //{
                        //    item = MaskDamge(item);
                        //}

                        //后续查清楚item在那里为空,为什么,必须解决

                        //if (item?.Length != 0)
                        //{
                        Console.WriteLine($"绘制第{saveImgCount}图片{fileName}开始");
                        BitmapFrame image = BoxSelected(boxSelectedImg, item);
                        SaveBitmapToPng(Path.Combine(SavePath, fileName), image);
                        saveImgCount++;
                        Console.WriteLine($"绘制第{saveImgCount}图片{fileName}成功");
                        //  }
                    }
                });
            });

            Growl.Info($"图片伤损绘制{saveImgCount}张完成,耗时:" + stopwatch.Elapsed.TotalSeconds);

        }
        #region 单个明细

        [ObservableProperty]
        bool isHideNormalMarker;

        bool CanHideNormalMarker() => damagePoints != null;
        [RelayCommand(CanExecute = nameof(CanHideNormalMarker))]

        void HideNormalMarker()
        {
            DetailsList.Clear();
            InitDamageDetails(this.damagePoints);
        }

        private void InitDamageDetails([NotNull] float[][] damagePoints)
        {
            for (int i = 0; i < damagePoints.GetLength(0); i++)
            {
                if (IsHideNormalMarker == true)
                {
                    if (FloatToBrush(damagePoints[i][4]) != Brushes.Green)
                    {
                        DetailsList.Add(
                            new DamageDetails
                            {
                                Id = i,
                                MarkerColor = FloatToBrush(damagePoints[i][4]),
                                DamageCategory = FloatToCategory(damagePoints[i][4]) + $" {i} ",
                                Similarity = (damagePoints[i][5] * 100).ToString("0.0") + "%",
                            }
                        );
                    }
                }
                else
                {
                    DetailsList.Add(
                            new DamageDetails
                            {
                                Id = i,
                                MarkerColor = FloatToBrush(damagePoints[i][4]),
                                DamageCategory = FloatToCategory(damagePoints[i][4]) + $" {i} ",
                                Similarity = (damagePoints[i][5] * 100).ToString("0.0") + "%",
                            }
                        );
                }
            }
        }
        #endregion
        private async Task InitializeThumbnailImgInfos()
        {

            var task = Task.Run(() =>
            {
                return Directory.GetFiles(Parameter, "*.png")
                    .OrderBy(File.GetCreationTime)
                    .ToArray();
            });
            imgFiles = await task.ContinueWith(
                t => t.Result,
                TaskScheduler.FromCurrentSynchronizationContext()
            );

            //if (isHideNormal)
            //{
            //    filterFiles = MaskDamage(DamageDataList).Select(x => x.Url).ToList();
            //}
            //else
            //{
            filterFiles = DamageDataList.Select(x => x.Url).ToList();
            //}
            //filterFiles = MaskDamage(DamageDataList).Select(x => x.Url).ToList();
            filterFiles = filterFiles.OrderBy(File.GetCreationTime).ToList();
            AllImgCount = imgFiles.Length;
            SingleImgCount = filterFiles.Count;
            UpdataThumbnail();
            StepIndex = 2;
        }

        private async void UpdataThumbnail()
        {
            string[] ThumbnailFiles;
            if (IsFilterThumbnail)
            {
                ThumbnailFiles = filterFiles.ToArray();
            }
            else
            {
                ThumbnailFiles = imgFiles;
            }

            ThumbnailImgInfos.Clear();
            await dispatcher.InvokeAsync(
               () =>
               {
                   foreach (var file in ThumbnailFiles)
                   {
                       ThumbnailImgInfos.Add(
                           new ImgInfo { Path = file, Name = Path.GetFileName(file) }
                       );
                   }
                   Growl.Info("缩略图初始化完成");
               },
               DispatcherPriority.Background
           );
            ShowAllBoxSelectedCommand.NotifyCanExecuteChanged();

        }

        #region 发送请求获取result.json文件
        [RelayCommand(CanExecute = nameof(IsGetJsonResult))]
        public async Task GetJsonResult()
        {
            if (await IsPortInUse(3333))
            {

                if (Directory.Exists(Parameter.InReplaceOutString()))
                {
                    var files = Directory.GetFiles(Parameter.InReplaceOutString());
                    if (files.Length > 0)
                    {
                        Directory.Delete(Parameter.InReplaceOutString(), true);
                    }
                }

                ClearDamagePage();

                GetProgress(Url);
                string Result = await GetJsonFile(Url, Parameter, Parameter);
                MessageBox.Show(Result);
                StepIndex = 2;
                ShowThumbnails(this.Parameter);
            }
            else
            {
                MessageBox.Warning("当前还未启动探伤后台,请启动探伤后台后重试");
            }
        }

        /// <summary>
        /// 执行方法后在wpath处生成result.json文件
        /// </summary>
        /// <param name="url">发送请求地址</param>
        /// <param name="rpath">读取文件夹的位置</param>
        /// <param name="wpath">写入result.json文件的位置</param>
        /// <returns>保存result的路径</returns>
        async Task<string> GetJsonFile(string url, string rpath, string wpath)
        {
            var content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("rpath", rpath),
                    new KeyValuePair<string, string>("wpath", wpath)
                }
            );
            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var progressTask = GetProgress(url);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Error(ex.Message);
                throw;
            }
            finally
            {
                ProgressVisibility = Visibility.Hidden;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }

        private async Task GetProgress(string url)
        {
            CurrentProgress = 0;
            TotalProgress = 100;
            _cancellationTokenSource = new CancellationTokenSource();
            ProgressVisibility = Visibility.Visible;
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var content = new FormUrlEncodedContent(
                        new[] { new KeyValuePair<string, string>("get_progress", "true") }
                    );
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    var progressData = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ProgressData>(progressData);
                    var ProgressArr = result.progress.Split("/");
                    dispatcher.Invoke(() =>
                    {
                        CurrentProgress = int.Parse(ProgressArr[0]);
                        TotalProgress = int.Parse(ProgressArr[1]);
                    });
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Request canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                await Task.Delay(500, _cancellationTokenSource.Token);
            }
        }

        #endregion

        #region 展示详细图片的代码
        private (float[][]?, int) GetDamagePointsAndIndex(string ImgPath)
        {
            var DamageData = DamageDataList
                .Where(x => Path.GetFileName(x.Url) == Path.GetFileName(ImgPath)).FirstOrDefault();

            var result = DamageData?.DamagePoint;
            var index = DamageDataList.IndexOf(DamageData);
            return (result, index);
        }


        bool CanShowAllBoxSelected => ThumbnailImgInfos != null && ThumbnailImgInfos.Count != 0;
        [RelayCommand(CanExecute = nameof(CanShowAllBoxSelected))]
        public void ShowAllBoxSelected()
        {
            if (IsFilterThumbnail)
            {

                if (IsShowAllBoxSelected)
                {
                    foreach (var ThumbnailImgInfo in ThumbnailImgInfos)
                    {
                        ThumbnailImgInfo.Path = ThumbnailImgInfo.Path.InReplaceOutString();
                    };
                }
                else
                {
                    foreach (var ThumbnailImgInfo in ThumbnailImgInfos)
                    {
                        ThumbnailImgInfo.Path = ThumbnailImgInfo.Path.OutReplaceInString();
                    };

                }
            }
            else
            {
                foreach (var ThumbnailImgInfo in ThumbnailImgInfos)
                {
                    if (
                        filterFiles.Any(x =>
                            Path.GetFileName(x) == Path.GetFileName(ThumbnailImgInfo.Path)
                        )
                    )
                    {

                        if (IsShowAllBoxSelected)
                            ThumbnailImgInfo.Path = ThumbnailImgInfo.Path.InReplaceOutString();
                        else
                            ThumbnailImgInfo.Path = ThumbnailImgInfo.Path.OutReplaceInString();
                    }
                }
                ;
            }


            if (SelectedIndex != -1)
            {


                ImgPath = ThumbnailImgInfos[SelectedIndex].Path;
                UpdateImgSource(ImgPath);
            }
            StepIndex = 3;

        }

        private BitmapFrame BoxSelected(BitmapFrame boxSelectedImg, float[][] damagePoints)
        {

            int index = 0;
            // 获取Image控件中的源图片
            BitmapSource originalBitmap = (BitmapSource)boxSelectedImg;
            // 创建一个新的位图来存储绘制后的结果
            WriteableBitmap writableBitmap = new WriteableBitmap(originalBitmap);
            // 使用DrawingVisual来绘制
            DrawingVisual drawingVisual = new DrawingVisual();

            var drawingContext = drawingVisual.RenderOpen();
            // 将原图绘制到底层
            drawingContext.DrawImage(
                writableBitmap,
                new Rect(0, 0, writableBitmap.PixelWidth, writableBitmap.PixelHeight)
            );
            if (NeedSavedInfo != null && isDistinctDamage)
            {
                // 绘制两条红色的竖虚线
                Pen dashedPen = new Pen(Brushes.Red, 2)
                {
                    DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
                };
                var RightLineX = MainWindowViewModel.NeedSavedInfo.ScreenshotWidthPx - MainWindowViewModel.NeedSavedInfo.ScreenshotOffset / 2;
                var LeftLineX = MainWindowViewModel.NeedSavedInfo.ScreenshotOffset / 2;
                drawingContext.DrawLine(dashedPen, new Point(LeftLineX, 0), new Point(LeftLineX, writableBitmap.PixelHeight));
                drawingContext.DrawLine(dashedPen, new Point(RightLineX, 0), new Point(RightLineX, writableBitmap.PixelHeight));
            }




            foreach (var damagePoint in damagePoints)
            {
                float x = damagePoint[0],
                      y = damagePoint[1],
                      width = damagePoint[2];
                float height = damagePoint[3],
                      Similar = damagePoint[5] < 0.5 ? 0.5f : damagePoint[5];
                SolidColorBrush color = FloatToBrush(damagePoint[4]);
                string DamageCategory = FloatToCategory(damagePoint[4]);
                var boxBrush = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(
                        Convert.ToByte(255 * Similar),
                        color.Color.R,
                        color.Color.G,
                        color.Color.B
                    )
                );
                if (x > writableBitmap.Width || x < 0 || y > writableBitmap.Height || y < 0 || x + width > writableBitmap.Width || y + height > writableBitmap.Height)
                {
                    Growl.Info($"绘制图片{boxSelectedImg.Decoder.Frames.FirstOrDefault()}出错,");
                    Growl.Info($"第{index}个伤损超出范围,");

                    continue;

                }



                // 绘制一个框
                drawingContext.DrawRoundedRectangle(
                    Brushes.Transparent,
                    new Pen(boxBrush, 5),
                    new Rect(x, y, width, height),
                    width / 2 * (1 - Similar),
                    height / 2 * (1 - Similar)
                );
                drawingContext.DrawText(
                    new FormattedText(
                        DamageCategory + $" {index}",
                        CultureInfo.GetCultureInfo("zh-CN"),
                        System.Windows.FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        12,
                        System.Windows.Media.Brushes.White,
                        96
                    ),
                    new System.Windows.Point(
                        x - 30 <= 0 ? x + 30 : x - 30,
                        y - 30 <= 0 ? y + 30 : y - 30
                    )
                );
                index++;
            }

            drawingContext.Close();
            // 将DrawingVisual的内容转换为BitmapSource
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)writableBitmap.PixelWidth,
                (int)writableBitmap.PixelHeight,
                96,
                96,
                PixelFormats.Pbgra32
            );
            rtb.Render(drawingVisual);

            // 将结果转换为BitmapImage以显示
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

            }
            return BitmapFrame.Create((BitmapSource)bitmapImage);

        }

        public static string FloatToCategory(float category)
        {
            var Categorys = Enum.GetNames<DamageCategory>();
            for (int i = 0; i < Categorys.Length; i++)
            {
                if (i == category && (i == 10 || i == 23))
                {
                    // 如果是非北京的话,只显示10和23序号的规则 
                    return isBeiJIngOnly ? Categorys[i] : Categorys[i].Replace("规则", "").Replace("_", "");
                }
                else if (i == category)
                {
                    return Categorys[i];
                }


            }
            return "后端返回前端未录入的类型" + category;

            //return category switch
            //{
            //    0 => "接头",
            //    1 => "零度迟到波",
            //    2 => "焊缝",
            //    3 => "七十度螺孔回波",
            //    4 => "轨形变换",
            //    5 => "轨头核伤",
            //    6 => "焊缝核伤",
            //    7 => "螺孔裂纹",
            //    8 => "零度异常",
            //    9 => "轨腰裂纹",
            //    10 => isBeiJIngOnly ? "轨底裂纹(规则)" : "轨底裂纹",
            //    11 => "加固焊缝",
            //    12 => "断面",
            //    13 => "轨面剥离",
            //    14 => "焊缝标记",
            //    15 => "无伤标记",
            //    16 => "焊缝且无伤标记",
            //    17 => "轻伤标记",
            //    18 => "轻伤发展",
            //    19 => "重伤标志",
            //    20 => "作业违规",
            //    21 => "焊缝且无焊缝标记",
            //    22 => "假像波",
            //    23 => isBeiJIngOnly ? "鱼鳞伤(规则)" : "鱼鳞伤",
            //    24 => "岔心",
            //    25 => "核伤2(模型)",
            //    26 => "核伤3(规则)",
            //    27 => "核伤1(规则)",
            //    28 => "正常螺孔",
            //    29 => "核伤2（模型 - 焊缝附近）",
            //    30 => "核伤2（规则）",
            //    31 => "核伤2（规则 - 焊缝附近）",
            //    32 => "水平裂纹（规则）",
            //    33 => "斜裂纹（规则）",
            //    34 => "月牙伤2（规则）",
            //    35 => "核伤4(规则)",
            //    _ => "未知异常",
            //};



        }

        public static SolidColorBrush FloatToBrush(float category)
        {
            return category switch
            {
                0 or 1 or 2 or 3 or 4 or 11 or 12 or 24 or 22 or 28 => Brushes.Green,
                5 or 6 or 7 or 8 or 9 or 10 or 13 or 23 or 25 or 26 or 27 or 29 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or 38 => Brushes.Red,
                14 or 15 or 16 or 17 or 18 or 19 or 20 or 21 => Brushes.YellowGreen,
                _ => Brushes.Wheat,
            };
        }


        [RelayCommand]
        void ProcessStart(string exePath)
        {
            StartProcess(exePath);
            Growl.InfoGlobal("应用程序已成功启动!");
            ScreenShotFunc();
        }

        void StartProcess(string exePath, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo(exePath);
                //startInfo.WindowStyle = ProcessWindowStyle.Maximized;
                startInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
                if (arguments != null)
                {
                    startInfo.Arguments = $"\"{arguments}\"";
                }

                var p = Process.Start(startInfo);
                p.EnableRaisingEvents = true;

                if (p != null)
                {
                    p.Exited += OnStartedProcessExit;
                }
                else
                {
                    throw new Exception("无法启动");
                }
            }
            catch (Exception ex)
            {
                Growl.InfoGlobal($"无法启动应用程序: {ex.Message}");
            }
        }

        void StartProcess(string exePath)
        {
            StartProcess(exePath, null);
        }

        void OnStartedProcessExit(object? sender, EventArgs e)
        {
            if (Cap != null)
            {
                dispatcher.Invoke(() =>
                {
                    Cap.Close();
                });
            }
        }

        private bool IsDataExists() => isDataExist;
        private bool IsSelectedImg() => SelectedIndex != -1;

        bool IsGetJsonResult() => (Parameter != null) ? true : false;

        bool IsDamageDataListNotEmpty() => DamageDataList == null ? false : true;


        #endregion


        #region 导出报告代码
        [ObservableProperty]
        bool isImporting;

        DocX document;

        [RelayCommand(CanExecute = nameof(IsDamageDataListNotEmpty))]
        async Task ExportReport()
        {

            IsImporting = true;
            if ((string.IsNullOrEmpty(RailwayName) || string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(instruments) || string.IsNullOrEmpty(OperatorName)
                || string.IsNullOrEmpty(workLength) || string.IsNullOrEmpty(workDate) || string.IsNullOrEmpty(line) || string.IsNullOrEmpty(workAreas)) && NeedSavedInfo?.RailWayInfo == null)
            {
                string pattern = @"\d{4}-\d{2}-\d{2}";
                RailwayName = Regex.Replace(ImgFolderName, pattern, "").Trim();
                MessageBox.Success("请将信息输入完整后导出报表");
                new RailwayInfoInputWindow().ShowDialog();
            }

            try
            {
                var docxsPath = Settings.Default.DocxPath;
                await Task.Run(() =>
                {
                    document = DocX.Load("./Resources/钢轨探伤检测报告模板.docx");
                    document = Summarize(document);
                    var count = 0;
                    RepleaceInfo(document);
                    List<DamageData> datas;
                    if (isHideNormal)
                    {
                        datas = MaskDamage(DamageDataList);
                    }
                    else
                    {
                        datas = DamageDataList;
                    }


                    foreach (var damageData in datas)
                    {
                        document = InsertDocx(damageData, document);
                        if (count != 0)
                        {
                            document.InsertSectionPageBreak();
                        }
                        count++;
                    }
                    document.SaveAs(docxsPath + "\\" + ImgFolderName + "钢轨探伤检测报告");
                });
                IsImporting = false;
                MessageBox.Success(ImgFolderName + $"钢轨探伤检测报告 保存在{docxsPath}");


            }
            catch (Exception ex)
            {
                MessageBox.Error(ex.Message);
            }
            finally
            {
                document.Dispose();
            }
        }

        DocX InsertDocx(DamageData damageData, DocX document)
        {
            if (damageData == null)
            {
                throw new ArgumentNullException(nameof(damageData));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            // 注意 这里的路径是不能包含空格与特殊符号
            var imgPath = damageData.Url.InReplaceOutString();
            if (File.Exists(imgPath))
            {
                var imgF = new Bitmap(imgPath);
                int width = imgF.Width;
                int height = imgF.Height;
                document.PageLayout.Orientation = Orientation.Landscape;
                var img = document.AddImage(imgPath);
                var picture = img.CreatePicture(height * 0.36f, width * 0.36f);
                var p = document.InsertParagraph(Path.GetFileName(imgPath));
                p.FontSize(28).SpacingAfter(50).Alignment = Alignment.center;
                p.AppendPicture(picture);
                imgF.Dispose();
            }
            else
            {
                Console.WriteLine("该图片文件不存在: " + imgPath);
                throw new FileNotFoundException("图片文件不存在", imgPath);
            }

            var Paragraphs = document.Paragraphs;
            var frequency = GetColumn4(damageData.DamagePoint)
                .GroupBy(n => n)
                .ToDictionary(g => g.Key, g => g.Count());
            foreach (var item in frequency)
            {
                var p = document.InsertParagraph($"{FloatToCategory(item.Key)}共出现{item.Value} 次.");
                p.FontSize(12).Alignment = Alignment.center;
            }

            var t = document.AddTable(damageData.DamagePoint.GetLength(0) + 1, 4);
            t.Alignment = Alignment.center;
            t.Rows[0].Cells[0].InsertParagraph("编号").FontSize(10).Alignment = Alignment.center;
            t.Rows[0].Cells[1].InsertParagraph("缺陷类型").FontSize(10).Alignment = Alignment.center;
            t.Rows[0].Cells[2].InsertParagraph("相似度").FontSize(10).Alignment = Alignment.center;
            t.Rows[0].Cells[3].InsertParagraph("备注").FontSize(10).Alignment = Alignment.center;
            t.SetWidths(new float[] { 120f, 120f, 120f, 120f });

            for (int i = 0; i < damageData.DamagePoint.GetLength(0); i++)
            {
                t.Rows[i + 1].Cells[0].InsertParagraph($"{i}").FontSize(10).Alignment = Alignment.center;
                t.Rows[i + 1]
                    .Cells[1]
                    .InsertParagraph(FloatToCategory(damageData.DamagePoint[i][4]))
                    .FontSize(10)
                    .Alignment = Alignment.center;
                t.Rows[i + 1]
                    .Cells[2]
                    .InsertParagraph((damageData.DamagePoint[i][5] * 100).ToString("0.0") + "%")
                    .FontSize(10)
                    .Alignment = Alignment.center;
                t.Rows[i + 1].Cells[3].InsertParagraph("  ").FontSize(10).Alignment = Alignment.center;
            }

            document.InsertTable(t);
            return document;
        }

        private void RepleaceInfo(DocX document)
        {
            var Repstr = new StringReplaceTextOptions()
            {
                SearchValue = "2024年01月18日",
                NewValue = DateTime.Now.ToLocalTime().ToString()
            };
            document.ReplaceText(Repstr);

            var repRailwayName = new StringReplaceTextOptions()
            {
                SearchValue = "厚德",
                NewValue = this.RailwayName ?? NeedSavedInfo?.RailWayInfo.RailwayName ?? "未输入信息"
            };
            document.ReplaceText(repRailwayName);
            //替换铁轨起始里程
            //var repRailwayStartMileage = new StringReplaceTextOptions()
            //{
            //    SearchValue = "11111",
            //    NewValue = RailwayStartMileage ?? NeedSavedInfo?.RailWayInfo.RailwayStartMileage ?? "未输入信息"
            //};
            //document.ReplaceText(repRailwayStartMileage);
            var repSerialNumber = new StringReplaceTextOptions()
            {
                SearchValue = "11111",
                NewValue = this.serialNumber ?? NeedSavedInfo?.RailWayInfo.SerialNumber ?? "未输入信息"
            };
            document.ReplaceText(repSerialNumber);
            ////替换铁轨终止里程
            //var repRailwayEndMileage = new StringReplaceTextOptions()
            //{
            //    SearchValue = "22222",
            //    NewValue = RailwayEndMileage ?? NeedSavedInfo?.RailWayInfo.RailwayEndMileage ?? "未输入信息"
            //};
            //document.ReplaceText(repRailwayEndMileage);
            var repInstruments = new StringReplaceTextOptions()
            {
                SearchValue = "22222",
                NewValue = this.instruments ?? NeedSavedInfo?.RailWayInfo.Instruments ?? "未输入信息"
            };
            document.ReplaceText(repInstruments);
            //替换铁轨分析人
            var repRailwayOperator = new StringReplaceTextOptions()
            {
                SearchValue = "王五",
                NewValue = this.OperatorName ?? NeedSavedInfo?.RailWayInfo.OperatorName ?? "未输入信息"
            };
            document.ReplaceText(repRailwayOperator);
            ////替换铁轨探测时间
            //var repRailwayDetectTime = new StringReplaceTextOptions()
            //{
            //    SearchValue = "2020年09月28日",
            //    NewValue = DetectionTime ?? NeedSavedInfo?.RailWayInfo.DetectionTime ?? "未输入信息"
            //};
            //document.ReplaceText(repRailwayDetectTime);

            ///作业长度
            var repWorkLength = new StringReplaceTextOptions()
            {
                SearchValue = "33333",
                NewValue = this.workLength ?? NeedSavedInfo?.RailWayInfo.WorkLength ?? "未输入信息"
            };
            document.ReplaceText(repWorkLength);
            //作业时间
            var repWorkDate = new StringReplaceTextOptions()
            {
                SearchValue = "2025年03月14日",
                NewValue = this.workDate ?? NeedSavedInfo?.RailWayInfo.WorkDate ?? "未输入信息"
            };
            document.ReplaceText(repWorkDate);

            //正线/站线
            var repLine = new StringReplaceTextOptions()
            {
                SearchValue = "44444",
                NewValue = this.line ?? NeedSavedInfo?.RailWayInfo.Line ?? "未输入信息"
            };
            document.ReplaceText(repLine);

            //工区
            var repWorkAreas = new StringReplaceTextOptions()
            {
                SearchValue = "55555",
                NewValue = this.workAreas ?? NeedSavedInfo?.RailWayInfo.WorkAreas ?? "未输入信息"
            };
            document.ReplaceText(repWorkAreas);
        }



        DocX Summarize(DocX document)
        {
            document.InsertSectionPageBreak();
            document.InsertParagraph("伤损概述").FontSize(16);
            document
                .InsertParagraph(
                    ($"共计捕获{ThumbnailImgInfos.Count}张波形图,其中共有{DamageDataList.Count}张图检测出伤损")
                )
                .FontSize(12);
            //List<float> damageCategory = new List<float>();
            //foreach (var d in DamageDataList)
            //{
            //    var a = GetColumn4(d.DamagePoint);
            //    damageCategory.AddRange(a);
            //}
            //var frequency = damageCategory.GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            var frequency = GetCategoryAndCount(DamageDataList);

            foreach (var item in frequency)
            {
                var p = document.InsertParagraph($"{FloatToCategory(item.Value)}共出现{item.Key} 次.");
                p.FontSize(12);
            }
            var pie = document.AddChart<PieChart>();
            var damageSeries = new Series("伤损比例图");
            var frequencyList = frequency
                .Select(kv => new { Category = FloatToCategory(kv.Value), Count = kv.Key })
                .ToList();
            damageSeries.Bind(frequencyList, "Category", "Count");
            pie.AddSeries(damageSeries);
            document.InsertChart(pie);
            document.InsertSectionPageBreak();
            return document;
        }

        float[] GetColumn4(float[][] damageDatas)
        {
            return damageDatas.Select(x => x[4]).ToArray();
        }

        #endregion
        #region 判断判伤后台是否在线
        [ObservableProperty]
        Visibility stopPythonVisible;

        [ObservableProperty]
        SolidColorBrush portStatusColor;

        [ObservableProperty]
        string portStatusText;

        async Task CheckAppStatusPeriodically()
        {
            while (true)
            {

                IsDetectionAsynEnable =
                    (Is6MRunning = await JGT_6M.IsRunning()) ||
                    (Is8CRunning = await JGT_8C.IsRunning());

                if (IsDetectionAsynEnable == false)
                {
                    IsDetectionAsynChecked = false;
                }

                if (await IsPortInUse(3333))
                {
                    dispatcher.Invoke(() =>
                    {
                        PortStatusColor = Brushes.Green;
                        PortStatusText = "判伤后台正在运行";
                        StartPythonVisible = Visibility.Hidden;
                        StopPythonVisible = Visibility.Visible;
                    });
                }
                else
                {
                    dispatcher.Invoke(() =>
                    {
                        PortStatusColor = Brushes.Red;
                        PortStatusText = "判伤后台未在运行";
                        StartPythonVisible = Visibility.Visible;
                        StopPythonVisible = Visibility.Hidden;
                    });
                }
                await Task.Delay(1300);
            }
        }

        Task<bool> IsPortInUse(int portNumber)
        {
            return Task.Run(() =>
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
                return tcpListeners.Select(x => x.Port).Contains(portNumber);


            });
        }
        #endregion
        [ObservableProperty]
        Visibility startPythonVisible;

        [ObservableProperty]
        bool isStartPython;

        [RelayCommand]
        async Task StartPythonScript()
        {
#if WINDOWS
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "python", // 这里使用 "python" 表示使用系统环境变量中的 Python 解释器
                Arguments = YoloPath, // 传递 Python 脚本的路径作为参数
                WorkingDirectory = Path.GetDirectoryName(YoloPath),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                //var t1 = ReadStreamAsync(process.StandardOutput);

                await Task.Delay(10000);
                //  await process.WaitForExitAsync();


            }


#elif Linux

#endif



        }

        [RelayCommand]

        async Task StopPythonScript()
        {
            IsStartPython = false;
            await Task.Run(() =>
            {
                Process[] processes = Process.GetProcessesByName("python");
                foreach (var process in processes)
                {
                    process.Kill();
                }
            });
        }




        #region 伤损总结
        List<KeyValuePair<int, float>> GetCategoryAndCount(
            [NotNull] List<DamageData> damageDataListPara)
        {
            List<float> damageCategory = new List<float>();
            foreach (var d in damageDataListPara)
            {
                var a = GetColumn4(d.DamagePoint);
                damageCategory.AddRange(a);
            }
            var result = damageCategory
                .GroupBy(x => x)
                .OrderByDescending(x => x.Count())
                .Select(g => new KeyValuePair<int, float>(g.Count(), g.Key))
                .ToList();
            return result;
        }

        List<Details> GetCategorySummary(float CategoryIndex, List<DamageData> damageDataListPara, bool IsSortByArea = false)
        {
            var result = new List<Details>();

            foreach (DamageData damageData in damageDataListPara)
            {
                Details tempDetails = new();
                int count = 0;
                List<float> area = new();
                for (int i = 0; i < damageData.DamagePoint.GetLength(0); i++)
                {
                    if (damageData.DamagePoint[i][4] == CategoryIndex)
                    {
                        if (IsSortByArea)//根据不同类型来进行排序
                        {
                            if (CategoryIndex == 10 || CategoryIndex == 5)
                            {
                                area.Add(damageData.DamagePoint[i][2] * damageData.DamagePoint[i][3]);

                            }
                            else if (CategoryIndex == 23 || CategoryIndex == 25 || CategoryIndex == 26 || CategoryIndex == 27 || CategoryIndex == 34 || CategoryIndex == 35)
                            {
                                area.Add(damageData.DamagePoint[i][3]);
                            }
                        }
                        count++;
                        tempDetails = new Details()
                        {
                            FileName = Path.GetFileName(damageData.Url),
                            Count = count
                        };
                    }
                    if (
                        i == damageData.DamagePoint.GetLength(0) - 1
                        && tempDetails.FileName != null
                    )
                    {
                        if (IsSortByArea)
                        {
                            tempDetails.weight = area.Count > 0 ? area.Max() : 0;
                        }
                        result.Add(tempDetails);
                    }
                }
            }
            return result;
        }

        [RelayCommand]
        void LocationImg(string Fpath)
        {
            var item = ThumbnailImgInfos.Where(x => x.Name == Fpath).FirstOrDefault();
            if (item != null)
            {
                SelectedThumbnailImg = item;
            }
        }


        private void InitCategorySummary()
        {
            if (isBeiJIngOnly)
            {
                InitCategorySummaryForBeiJing();
            }
            else
            {
                InitCategorySummaryForNotBeiJing();
            }
        }

        [ObservableProperty]
        int displayedRulesCount = 10;
        [RelayCommand]
        void DisplayedRulesCountChanged()
        {
            InitCategorySummary();
        }

        private void InitCategorySummaryForBeiJing()
        {


            List<DamgeCategorySummaryTree> RootCategoryList = new List<DamgeCategorySummaryTree>();
            RootCategoryList.Add(new DamgeCategorySummaryTree() { Name = "正常标记", Children = new() });

            RootCategoryList.Add(new DamgeCategorySummaryTree()
            {
                Name = "伤损标记",
                Children = new(){
            new DamgeCategorySummaryTree
            {
                Name = "规则类",
                Children = new()
            },
            new DamgeCategorySummaryTree
            {
                Name = "模型类",
                Children = new List<ITreeNode> ()
            }
        }
            });



            RootCategoryList.Add(new DamgeCategorySummaryTree() { Name = "作业标记", Children = new() });

            List<DamageData> Datas = DamageDataList;
            var categoryAndCount = GetCategoryAndCount(Datas);

            foreach (string d in Enum.GetNames<DamageCategory>())
            {
                if (d.Contains("规则"))
                {
                    int index = Array.IndexOf(Enum.GetNames<DamageCategory>(), d);
                    if (!categoryAndCount.Any(x => x.Value == index))
                    {
                        (RootCategoryList[1].Children[0] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = d
                        });

                    }
                }
            }


            foreach (var x in categoryAndCount)
            {
                if (FloatToBrush(x.Value) == Brushes.Green)
                {
                    RootCategoryList[0].Children.Add(new DamgeCategory()
                    {
                        Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                        Children = GetCategorySummary(x.Value, DamageDataList)
                        .OrderByDescending(x => x.Count).ToList()
                    });
                }
                else if (FloatToBrush(x.Value) == Brushes.Red)
                {


                    if (x.Value == 10 || x.Value == 23 || x.Value == 26 || x.Value == 27 || x.Value == 38)
                    {
                        (RootCategoryList[1].Children[0] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {(x.Key > DisplayedRulesCount ? DisplayedRulesCount : x.Key)}次",
                            Children = GetCategorySummary(x.Value, DamageDataList, true)
                            .OrderByDescending(x => x.weight).Take(DisplayedRulesCount).ToList()
                        });
                    }
                    else if (x.Value == 30 || x.Value == 31 || x.Value == 32 || x.Value == 33 || x.Value == 34 || x.Value == 35)
                    {
                        (RootCategoryList[1].Children[0] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                            Children = GetCategorySummary(x.Value, DamageDataList)
                            .OrderByDescending(x => x.weight).ToList()
                        });
                    }
                    else if (x.Value == 5)
                    {
                        (RootCategoryList[1].Children[1] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                            Children = GetCategorySummary(x.Value, DamageDataList, true)
                            .OrderByDescending(x => x.weight).ToList()
                        });
                    }
                    else
                    {
                        (RootCategoryList[1].Children[1] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                            Children = GetCategorySummary(x.Value, DamageDataList)
                            .OrderByDescending(x => x.Count).ToList()
                        });
                    }
                }
                else if (FloatToBrush(x.Value) == Brushes.YellowGreen)
                {
                    RootCategoryList[2].Children.Add(new DamgeCategory()
                    {
                        Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                        Children = GetCategorySummary(x.Value, DamageDataList)
                        .OrderByDescending(x => x.Count).ToList()
                    });
                }
            }

            DamageTree = RootCategoryList;
        }

        private void InitCategorySummaryForNotBeiJing()
        {

            List<DamageData> Datas = DamageDataList;
            var categoryAndCount = GetCategoryAndCount(Datas);
            var 标记Sum = CalculateSum(categoryAndCount, 14, 15, 16, 17, 18, 19);


            List<DamgeCategorySummaryTree> RootCategoryList = new List<DamgeCategorySummaryTree>();
            RootCategoryList.Add(new DamgeCategorySummaryTree() { Name = "正常标记", Children = new() });
            RootCategoryList.Add(new DamgeCategorySummaryTree() { Name = "伤损标记", Children = new() });
            RootCategoryList.Add(new DamgeCategorySummaryTree() { Name = $"作业标记 总次数{标记Sum}", Children = new() });





            // 正常类里面细分
            var 焊缝Sum = CalculateSum(categoryAndCount, 2, 11, 22);
            var 轨头Sum = CalculateSum(categoryAndCount, 5, 6, 36, 13);
            var 核伤Sum = CalculateSum(categoryAndCount, 5, 6);
            var 轨腰Sum = CalculateSum(categoryAndCount, 7, 9);
            var 轨底Sum = CalculateSum(categoryAndCount, 37);

            RootCategoryList[0].Children.Add(new DamgeCategorySummaryTree()
            {
                Name = $"焊缝 总次数{焊缝Sum}",
                Children = new()
            });
            // 伤损类里面细分
            RootCategoryList[1].Children.Add(new DamgeCategorySummaryTree()
            {
                Name = $"轨头 总次数{轨头Sum}",
                Children = new()
                {
                    new DamgeCategorySummaryTree()
                    {
                        Name = $"核伤 总次数{核伤Sum}",
                        Children = new()
                    },

                }
            });
            RootCategoryList[1].Children.Add(new DamgeCategorySummaryTree()
            {
                Name = $"轨腰 总次数{轨腰Sum}",
                Children = new()
            });
            RootCategoryList[1].Children.Add(new DamgeCategorySummaryTree()
            {
                Name = $"轨底 总次数{轨底Sum}",
                Children = new()
            });



            foreach (var x in categoryAndCount)
            {
                //焊缝类归类
                if (FloatToBrush(x.Value) == Brushes.Green)
                {
                    if (x.Value == 2 || x.Value == 11 || x.Value == 22)
                    {
                        (RootCategoryList[0].Children[0] as DamgeCategorySummaryTree)
                            ?.Children.Add(new DamgeCategory()
                            {
                                Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                                Children = GetCategorySummary(x.Value, DamageDataList)
                                .OrderByDescending(x => x.Count).ToList()
                            });

                    }
                    else
                    {
                        RootCategoryList[0].Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                            Children = GetCategorySummary(x.Value, DamageDataList)
                        .OrderByDescending(x => x.Count).ToList()
                        });
                    }
                }
                else if (FloatToBrush(x.Value) == Brushes.Red)
                {

                    // 轨头大类
                    if (x.Value == 5 || x.Value == 6)
                    {
                        ((RootCategoryList[1].Children[0] as DamgeCategorySummaryTree)
                          ?.Children[0] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                          {
                              Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                              Children = GetCategorySummary(x.Value, DamageDataList)
                              .OrderByDescending(x => x.Count).ToList()
                          });


                    }
                    else if (x.Value == 36 || x.Value == 13)
                    {
                        (RootCategoryList[1].Children[0] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {(x.Key)}次",
                            Children = GetCategorySummary(x.Value, DamageDataList, true)
                            .OrderByDescending(x => x.weight).ToList()
                        });

                    }
                    //轨腰大类
                    else if (x.Value == 9 || x.Value == 7)
                    {
                        (RootCategoryList[1].Children[1] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                            Children = GetCategorySummary(x.Value, DamageDataList)
                            .OrderByDescending(x => x.Count).ToList()
                        });
                    }
                    //轨底大类
                    else if (x.Value == 37)
                    {
                        (RootCategoryList[1].Children[2] as DamgeCategorySummaryTree)?.Children.Add(new DamgeCategory()
                        {
                            Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                            Children = GetCategorySummary(x.Value, DamageDataList)
                            .OrderByDescending(x => x.Count).ToList()
                        });
                    }




                    //if (x.Value == 10 || x.Value == 23)
                    //{
                    //    RootCategoryList[1].Children.Add(new DamgeCategory()
                    //    {
                    //        Name = $"{FloatToCategory(x.Value)} {(x.Key > 10 ? 10 : x.Key)}次",
                    //        Children = GetCategorySummary(x.Value, DamageDataList, true)
                    //        .OrderByDescending(x => x.weight).Take(10).ToList()
                    //    });
                    //}
                    //else
                    //{
                    //    RootCategoryList[1].Children.Add(new DamgeCategory()
                    //    {
                    //        Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                    //        Children = GetCategorySummary(x.Value, DamageDataList)
                    //        .OrderByDescending(x => x.Count).ToList()
                    //    });
                    //}
                }
                else if (FloatToBrush(x.Value) == Brushes.YellowGreen)
                {
                    RootCategoryList[2].Children.Add(new DamgeCategory()
                    {
                        Name = $"{FloatToCategory(x.Value)} {x.Key}次",
                        Children = GetCategorySummary(x.Value, DamageDataList)
                        .OrderByDescending(x => x.Count).ToList()
                    });
                }
            }
            DamageTree = RootCategoryList;
        }

        private int CalculateSum(IEnumerable<KeyValuePair<int, float>> categoryAndCount, params int[] values)
        {
            return categoryAndCount
                .Where(x => values.Contains((int)x.Value))
                .Select(x => x.Key)
                .Sum();
        }
        #endregion

        SampleImg SampleWin;
        #region 修改当前图片
        [RelayCommand(CanExecute = nameof(IsSelectedImg))]
        void ModifyImg()
        {
            var sampleImg = BitmapFrame.Create(new Uri(ImgPath.OutReplaceInString(), UriKind.Relative));
            var vm = new SampleImgViewModel(
                sampleImg,
                sampleImg.Width,
                sampleImg.Height,
                DamageDataList,
                damagePoints?.ToList() ?? new List<float[]>(),
                currentImgIndex,
                ImgPath
            );
            SampleWin = new SampleImg(vm);
            SampleWin.Owner = Application.Current.MainWindow;
            SampleWin.ScreenShotTaken += SampleWin_ScreenShotTaken;
            SampleWin.Closed += ChildWindow_Closed;
            SampleWin.ShowDialog();
        }

        private async void SampleWin_ScreenShotTaken(object? sender, BitmapSource e)
        {
            if (e == null)
            {
                ImgSelectionChanged();
            }
            else if (e != null)
            {
                if (IsShowAllBoxSelected)//当显示所有框选按钮点击后才切换图片
                {

                    ImgSource = BitmapFrame.Create(e);
                }
                else
                {
                    ImgSource = BitmapFrame.Create(new Uri(ImgPath.OutReplaceInString(), UriKind.Relative));
                }
                var OutSavePath = ((sender as SampleImg).DataContext as SampleImgViewModel).OutImgPath;
                SaveBitmapToPng(OutSavePath, e);
                Growl.InfoGlobal($"{Path.GetFileName(ImgPath)}图片保存成功");
            }
        }

        private void SaveBitmapToPng(string path, BitmapSource image)
        {

            try
            {
                using var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Error($"文件保存异常,异常原因:{ex.Message}");
            }
        }


        private async void ChildWindow_Closed(object? sender, EventArgs e)
        {
            DetailsList.Clear();
            if (this.damagePoints != null && damagePoints.Length != 0)
            {
                InitDamageDetails(damagePoints);
            }
            else
            {
                InitCategorySummary();
            }
            //;
            //UpdataThumbnail();
            // 子窗口关闭后的逻辑处理
            if (currentImgIndex == -1)
            {
                (_, currentImgIndex) = GetDamagePointsAndIndex(ImgPath);
            }


            SampleWin.ScreenShotTaken -= SampleWin_ScreenShotTaken;
            SampleWin.Closed -= ChildWindow_Closed; // 取消订阅事件
            SampleWin = null; // 释放资源
        }
        [RelayCommand]
        void WinClosing()
        {
            ImgSource = null;
            ThumbnailImgInfos.Clear();
            ClearImgAndData();
            if (IsModifyResultJson)
            {
                SaveJson<List<DamageData>>(DamageDataList, Path.Combine(Settings.Default.InPath, ImgFolderName), "result.json");
            }
        }

        void SaveJson<T>(T jsonData, string jsonPath, string jsonName)
        {
            if (Directory.Exists(jsonPath))
            {
                if (jsonData != null && !string.IsNullOrEmpty(ImgFolderName))
                {
                    string jsonString = JsonSerializer.Serialize(jsonData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(Path.Combine(jsonPath, jsonName), jsonString);
                }

            }
            else
            {
                MessageBox.Error($"{jsonPath}路径不存在");
            }


        }
        void ClearImgAndData() //清除在out文件夹下面没有数据的图片还有相关图片
        {
            if (DamageDataList != null && !string.IsNullOrEmpty(ImgFolderName))
            {

                DamageDataList = DamageDataList//清理 damagePoint 为空的数据
                    .Where(x => x.DamagePoint.Length != 0 && x.DamagePoint != null)
                    .Select(x => x).ToList();

                var outImgPaths = Directory.GetFiles(Path.Combine(Settings.Default.OutPath, ImgFolderName), "*.png");
                var outImgFileNames = outImgPaths.Select(x => Path.GetFileName(x));
                var shouldBeDelateImgs = outImgFileNames.Except(DamageDataList.Select(x => Path.GetFileName(x.Url)));

                foreach (var img in shouldBeDelateImgs)
                {
                    File.Delete(Path.Combine(Settings.Default.OutPath, ImgFolderName, img));
                }
            }
        }
        #endregion
        #region 搜索ocr里程
        [ObservableProperty]
        bool isDetectionAsynEnable = false;
        [ObservableProperty]
        bool isDetectionAsynChecked = false;
        [ObservableProperty]
        bool is6MRunning = false;
        [ObservableProperty]
        bool is8CRunning = false;
        [ObservableProperty]
        string searchMileageText;

        ObservableCollection<OcrData> locationMileageImgs = new ObservableCollection<OcrData>();
        public ObservableCollection<OcrData> LocationMileageImgs { get => locationMileageImgs; set => locationMileageImgs = value; }

        [RelayCommand]
        private void SearchMileage(string mileage)
        {
            LocationMileageImgs.Clear();
            if (IsDetectionAsynChecked)
            {
                if (Is6MRunning && Is8CRunning)
                {
                    HandyControl.Controls.MessageBox.Warning("请不要同时打开6M和8C探伤软件的里程定位对话框");
                }
                else if (Is6MRunning)
                {
                    JGT_6M.LocationMileage(mileage);
                }
                else if (Is8CRunning)
                {
                    JGT_8C.LocationMileageAsync(mileage);
                }
            }
            string[] mils = mileage.Split(new string[2] { "km", "KM" }, StringSplitOptions.RemoveEmptyEntries);
            if (Parameter != null && File.Exists(Path.Combine(Parameter, "OcrResult.json")))
            {
                string km = mils.FirstOrDefault();
                string i = mils.LastOrDefault().Replace("m", "").Replace("M", "");
                string mileageFilePath = Path.Combine(Parameter, "OcrResult.json");
                List<Records.OcrData> ocrData = DeserializeJson<List<Records.OcrData>>(mileageFilePath);
                List<Records.OcrData> resutl = ocrData.Where((Records.OcrData x) => x.MileageText.Contains(km) && x.MileageText.Contains(i)).ToList();
                LocationMileageImgs = new ObservableCollection<Records.OcrData>(resutl.Select((Records.OcrData x) => new Records.OcrData(Path.GetFileName(x.ImgFullPath), x.MileageText)));
                if (LocationMileageImgs.Count != 0)
                {
                    SelectMileageWindow win = new SelectMileageWindow();
                    win.Owner = System.Windows.Application.Current.MainWindow;
                    win.ShowDialog();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Info("没有找到任何符合里程数的图片,请检查输入格式是否正确");
                }
            }
            else
            {
                HandyControl.Controls.MessageBox.Error("当前集合没有找到ocr里程文件");
            }
        }

        #endregion
        #region 铁轨信息输入
        [ObservableProperty]
        string railwayName;
        //串号
        [ObservableProperty]
        string serialNumber;
        //仪器
        [ObservableProperty]
        string instruments;
        //分析人
        [ObservableProperty]
        string operatorName;
        //作业长度
        [ObservableProperty]
        string workLength;
        //作业日期
        [ObservableProperty]
        string workDate;
        //正/站线
        [ObservableProperty]
        string line;
        //工区
        [ObservableProperty]
        string workAreas;

        [RelayCommand]
        void SaveRailWayInfo(HandyControl.Controls.Window win)
        {
            if ((!string.IsNullOrEmpty(RailwayName) && !string.IsNullOrEmpty(serialNumber) && !string.IsNullOrEmpty(instruments) && !string.IsNullOrEmpty(OperatorName)
                && !string.IsNullOrEmpty(workLength) && !string.IsNullOrEmpty(workDate) && !string.IsNullOrEmpty(line) && !string.IsNullOrEmpty(workAreas)))
            {
                MessageBox.Success("信息保存完成");
                win.Close();
            }
            else
            {
                MessageBox.Info("信息输入不完整,请填写完整信息");
            }

        }

        #endregion
        #region 截图事件发生与结束时处理函数
        public static string? FilePathIn;
        void OnScreenShotStart(object sender, EventArgs e)
        {

            //获取当前的月份与日期并转化为字符串
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            FilePathIn = Path.GetFullPath(Path.Combine(Settings.Default.InPath, RailwayName + dateStr));
            ImgFolderName = RailwayName + dateStr;

            if (!Directory.Exists(FilePathIn))
            {
                Directory.CreateDirectory(FilePathIn);
            }
            //var o = sender as PlaybackWindow;
            //NeedSavedInfo = NeedSavedInfo ?? new();
            //NeedSavedInfo.ScreenshotOffset = o?.MoveRepeatPx ?? 0;
            //NeedSavedInfo.ScreenshotDirection = o?.PlaybackDirection ?? MoveDirection.Left;
            //NeedSavedInfo.ScreenshotWidthPx = o?.ImgWidthPx ?? 0;

            //NeedSavedInfo.RailWayInfo = new RailInfo()
            //{
            //    RailwayName = RailwayName,
            //    RailwayStartMileage = RailwayStartMileage,
            //    RailwayEndMileage = RailwayEndMileage,
            //    OperatorName = OperatorName,
            //    DetectionTime = DetectionTime
            //};
            //SaveJson(NeedSavedInfo, Path.Combine(Settings.Default.InPath, ImgFolderName), "info.json");
        }
        async void OnScreenShotFinished(object sender, EventArgs e)
        {

            var o = sender as PlaybackWindow;
            NeedSavedInfo = NeedSavedInfo ?? new();
            NeedSavedInfo.ScreenshotOffset = o?.MoveRepeatPx ?? 0;
            NeedSavedInfo.ScreenshotDirection = o?.PlaybackDirection ?? MoveDirection.Left;
            NeedSavedInfo.ScreenshotWidthPx = o?.ImgWidthPx ?? 0;

            NeedSavedInfo.RailWayInfo = new RailInfo()
            {
                RailwayName = RailwayName,
                //RailwayStartMileage = RailwayStartMileage,
                SerialNumber = serialNumber,
                Instruments = instruments,
                OperatorName = OperatorName,
                WorkLength = workLength,
                WorkDate = workDate,
                Line = line,
                WorkAreas = workAreas
            };
            SaveJson(NeedSavedInfo, Path.Combine(Settings.Default.InPath, ImgFolderName), "info.json");






            var IsAnalyse = (MessageBox.Ask("当前数据已捕获完成,是否开始分析")) switch
            {
                MessageBoxResult.OK => true,
                _ => false
            };
            this.Parameter = FilePathIn;
            this.ImgFolderName = Path.GetFileName(FilePathIn) ?? "没有文件夹";

            if (IsAnalyse)
            {
                await this.GetJsonResult();
            }
            if (Cap != null)
            {
                dispatcher.Invoke(() =>
                {
                    Cap.Close();
                });
            }
        }

        #endregion

        [RelayCommand]
        private void SelectExeFile()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new();
            string selectedFilePath = null;
            openFileDialog.Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
            }
            HandyControl.Controls.MessageBox.Info((selectedFilePath == null) ? "当前没有选择任何程序" : ("当前选择程序的路径为" + selectedFilePath));
            if (selectedFilePath != null)
            {
                MenuItems.Insert(MenuItems.Count - 1, new MenuItem
                {
                    Header = Path.GetFileName(selectedFilePath),
                    CommandParameter = selectedFilePath,
                    Command = ProcessStartCommand
                });
                Settings.Default.AppLocation.Add(selectedFilePath);
                Settings.Default.Save();
            }
        }

        [RelayCommand]
        private void StartSnipaste()
        {

            StartProcess(Settings.Default.Snipaste);
            HandyControl.Controls.MessageBox.Info("Snipaste已启动,按F1按键截图 按F3按键贴图");
        }

        #region 系统设置
        [RelayCommand]
        void ChangeFilePath(string path)
        {
            var FolderDialog = new OpenFolderDialog();
            FolderDialog.Title = "请选择文件夹";
            FolderDialog.InitialDirectory = path;
            var result = FolderDialog.ShowDialog();
            if (!(result ?? false))
            {
                MessageBox.Warning("当前未选择任何文件夹,请重新选择文件夹");
                return;
            }

            if (path == InPath)
            {
                InPath = FolderDialog.FolderName;
                Settings.Default.InPath = FolderDialog.FolderName;

                MessageBox.Info("截图路径已经更改为" + FolderDialog.FolderName);

            }
            else if (path == OutPath)
            {
                OutPath = FolderDialog.FolderName;
                Settings.Default.OutPath = FolderDialog.FolderName;
                MessageBox.Info("伤损图片路径已经更改为" + FolderDialog.FolderName);
            }
            else if (path == DocxPath)
            {
                DocxPath = FolderDialog.FolderName;
                Settings.Default.DocxPath = FolderDialog.FolderName;
                MessageBox.Info("分析报告路径已经更改为" + FolderDialog.FolderName);
            }
            Settings.Default.Save();
        }

        #endregion

    }
}