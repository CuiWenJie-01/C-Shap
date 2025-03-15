using DamageMaker.Properties;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace DamageMarker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = null;

        public static double mouseStartX { get; set; }
        public static double mouseStartY { get; set; }
        public static double mouseEndX { get; set; }
        public static double mouseEndY { get; set; }
        public static double mouseStartX1 { get; set; }
        public static double mouseStartY1 { get; set; }
        public static double mouseEndX1 { get; set; }
        public static double mouseEndY1 { get; set; }

        App()
        {
            Settings.Default.InPath = Path.GetFullPath(Settings.Default.InPath);
            Settings.Default.OutPath = Path.GetFullPath(Settings.Default.OutPath);
            Settings.Default.DocxPath = Path.GetFullPath(Settings.Default.DocxPath);

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!File.Exists(Settings.Default.DocxPath))
            {
                Directory.CreateDirectory(Settings.Default.DocxPath);
            }
            if (!File.Exists(Settings.Default.InPath))
            {
                Directory.CreateDirectory($"{Settings.Default.InPath}");
            }
            if (!File.Exists(Settings.Default.OutPath))
            {
                Directory.CreateDirectory($"{Settings.Default.OutPath}");
            }
            if (!File.Exists("./Logs"))
            {
                Directory.CreateDirectory("./Logs");
            }

            const string mutexName = "YourUniqueMutexName";
            bool createdNew;

            mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // 如果程序已经在运行，则关闭新实例
                MessageBox.Show("程序已经在运行中。");
                Application.Current.Shutdown();
                return;
            }


            if (!System.Diagnostics.Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                DispatcherUnhandledException += OnDispatcherUnhandledException;
            }
        }

        #region 关于异常捕获处理
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException((Exception)e.ExceptionObject);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            HandyControl.Controls.MessageBox.Error(e.Exception.Message);
            e.Handled = true; // 标记异常已处理
        }
        private void LogException(Exception ex)
        {
            string logFilePath = $"./Logs/error.log";
            using (
                StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"[{DateTime.Now}] \n\rUnhandled Exception:");
                writer.WriteLine("--------------------------------------------------");
                writer.WriteLine(ex.StackTrace);
                writer.WriteLine("--------------------------------------------------");
            }
        }

        #endregion

        protected override void OnExit(ExitEventArgs e)
        {
            DamageMaker.Properties.Settings.Default.Save();

            base.OnExit(e);
        }
    }
}
