using HandyControl.Controls;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using BitmapDecoder = Windows.Graphics.Imaging.BitmapDecoder;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DamageMaker.Common
{

    public static class MileageRecognition
    {
        private static OcrEngine? ocr;


        static MileageRecognition()
        {
            if (OcrEngine.IsLanguageSupported(new Language("en-US")))
            {
                ocr = OcrEngine.TryCreateFromLanguage(new Language("en-US"));
            }
            else
            {
                Growl.InfoGlobal("未找到英文ocr包,当前将使用系统默认语言ocr");
                var lag = OcrEngine.AvailableRecognizerLanguages.FirstOrDefault();
                if (lag != null)
                {
                    ocr = OcrEngine.TryCreateFromLanguage(lag);
                }
                else
                {
                    ocr = null;
                    MessageBox.Error("系统没有安装任何语言包,无法识别");
                }
            }
        }

        public static async Task<string> CropAndRecognizeMileage(string imgPath, float cropX)
        {
            using IRandomAccessStream stream = await FileRandomAccessStream.OpenAsync(imgPath, Windows.Storage.FileAccessMode.Read);
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            uint cropHeight = (uint)(softwareBitmap.PixelHeight * cropX);
            SoftwareBitmap croppedBitmap = await CropSoftwareBitmapAsync(softwareBitmap, 0, (int)(softwareBitmap.PixelHeight - cropHeight), softwareBitmap.PixelWidth, (int)cropHeight);
            var result = await ocr.RecognizeAsync(croppedBitmap);
            return result.Text.Replace("O", "0").Replace("o", "0").Replace("I", "1").Replace(" ", "");
        }
        public static async Task<string> CropAndRecognizeMileage(Bitmap imgBitmap, float cropX)
        {
            SoftwareBitmap softwareBitmap = await ConvertToSoftwareBitmapAsync(imgBitmap);
            uint cropHeight = (uint)(softwareBitmap.PixelHeight * cropX);
            SoftwareBitmap croppedBitmap = await CropSoftwareBitmapAsync(softwareBitmap, 0, (int)(softwareBitmap.PixelHeight - cropHeight), softwareBitmap.PixelWidth, (int)cropHeight);
            //await SaveDebugImg(croppedBitmap);

            var result = await ocr.RecognizeAsync(croppedBitmap);
            return result.Text.Replace("O", "0").Replace("o", "0").Replace("I", "1").Replace(" ", "");
        }

        public static async Task<string> CropAndRecognizeMileage(Bitmap imgBitmap, uint height)
        {
            SoftwareBitmap softwareBitmap = await ConvertToSoftwareBitmapAsync(imgBitmap);
            uint cropHeight = height;
            SoftwareBitmap croppedBitmap = await CropSoftwareBitmapAsync(softwareBitmap, 0, (int)(softwareBitmap.PixelHeight - cropHeight), softwareBitmap.PixelWidth, (int)cropHeight);

            //  await SaveDebugImg(croppedBitmap);

            var result = await ocr.RecognizeAsync(croppedBitmap);

            return result.Text.Replace("O", "0").Replace("o", "0").Replace("I", "1").Replace(" ", "");
        }

        private static async Task SaveDebugImg(SoftwareBitmap img)
        {
            using (var fileStream = new FileStream(@"C:\Users\19233\Pictures\" + Path.GetRandomFileName() + ".png", FileMode.Create))
            {
                Windows.Graphics.Imaging.BitmapEncoder encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, fileStream.AsRandomAccessStream());
                encoder.SetSoftwareBitmap(img);
                await encoder.FlushAsync();
            }
        }


        public static string? ExtractDistance(this string input)
        {
            string pattern = @"(?i)(\d+)km\+?(\d+\.?\d+)m";
            Match match = Regex.Match(input, pattern);

            if (match.Success && !string.IsNullOrEmpty(match.Groups[1].Value) && !string.IsNullOrEmpty(match.Groups[2].Value))
            {
                string beforeKm = match.Groups[1].Value;
                string afterKm = match.Groups[2].Value;
                return $"{beforeKm}KM{afterKm}M";
            }
            return null;
        }
        public static async Task<SoftwareBitmap> ConvertToSoftwareBitmapAsync(Bitmap bitmap)
        {
            // 创建一个内存流来保存 Bitmap 数据
            using (var memoryStream = new System.IO.MemoryStream())
            {
                // 将 Bitmap 保存为 PNG 格式到内存流中
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

                // 创建一个 InMemoryRandomAccessStream
                using (var randomAccessStream = new InMemoryRandomAccessStream())
                {
                    // 将内存流中的数据写入 InMemoryRandomAccessStream
                    using (var outputStream = randomAccessStream.GetOutputStreamAt(0))
                    {
                        var dataWriter = new DataWriter(outputStream);
                        var bytes = new byte[memoryStream.Length];
                        memoryStream.Read(bytes, 0, bytes.Length);
                        dataWriter.WriteBytes(bytes);
                        await dataWriter.StoreAsync();
                        await dataWriter.FlushAsync();
                    }

                    // 使用 BitmapDecoder 解码流中的图像
                    var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                    var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    return softwareBitmap;
                }
            }
        }


        private static async Task<SoftwareBitmap> CropSoftwareBitmapAsync(SoftwareBitmap sourceBitmap, int x, int y, int width, int height)
        {
            // 确保裁剪区域在源图像范围内
            if (x < 0 || y < 0 || width <= 0 || height <= 0 ||
                x + width > sourceBitmap.PixelWidth || y + height > sourceBitmap.PixelHeight)
            {
                throw new ArgumentException("裁剪区域超出图像范围");
            }
            // 创建一个内存流
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                // 创建一个 BitmapEncoder 并将 SoftwareBitmap 写入流
                Windows.Graphics.Imaging.BitmapEncoder encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.BmpEncoderId, stream);
                encoder.SetSoftwareBitmap(sourceBitmap);
                await encoder.FlushAsync();
                // 创建一个 BitmapDecoder 从流中读取图像
                Windows.Graphics.Imaging.BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                // 创建一个新的 SoftwareBitmap 对象，用于存储裁剪后的图像
                SoftwareBitmap croppedBitmap = new SoftwareBitmap(
                    BitmapPixelFormat.Bgra8,
                    width,
                    height,
                    BitmapAlphaMode.Premultiplied);
                // 裁剪图像
                BitmapTransform transform = new BitmapTransform
                {
                    Bounds = new BitmapBounds
                    {
                        X = (uint)x,
                        Y = (uint)y,
                        Width = (uint)width,
                        Height = (uint)height
                    },
                    InterpolationMode = BitmapInterpolationMode.Fant
                };
                // 获取裁剪后的像素数据
                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage);
                // 将裁剪后的像素数据写入新的 SoftwareBitmap 对象
                croppedBitmap.CopyFromBuffer(pixelData.DetachPixelData().AsBuffer());
                return croppedBitmap;
            }
        }

    }
}
