using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.Extensions;
using OpenCvSharp;
using DamageMaker.Models;
using Xceed.Document.NET;
using System.Windows.Forms;

namespace DamageMaker.Common
{
  public static class ImgProcessing
    {
       internal  struct MatchInfo
        {
         internal  float similarity;
          internal  int Xpos;           
        }
        public static int GetImgOffset(Bitmap bitmap1,Bitmap bitmap2,bool LeftContrast )
        {
         
          

            Mat img1 = BitmapConverter.ToMat(bitmap1);
            Mat img2 = BitmapConverter.ToMat(bitmap2);

            // 获取第一张图片的宽度和第二张图片的高度
            int width = 300;
            int height = img2.Height;

            // 从第一张图片中截取指定区域
            Rect roi;
            if (LeftContrast == true)
            {
             roi = new Rect(0, 0, width, height);
            }
            else
            {
                roi = new Rect(img1.Width - width, 0, width, height);
            }

            Mat img1Cropped = new Mat(img1, roi);

            // 转换为灰度图像
            Mat grayImg1Cropped = new Mat();
            Mat grayImg2 = new Mat();
            Cv2.CvtColor(img1Cropped, grayImg1Cropped, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(img2, grayImg2, ColorConversionCodes.BGR2GRAY);

            // 膨胀操作
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            Cv2.Dilate(grayImg1Cropped, grayImg1Cropped, kernel);
            Cv2.Dilate(grayImg2, grayImg2, kernel);


            // 模板匹配
            Mat result = new Mat();
            Cv2.MatchTemplate(grayImg2, grayImg1Cropped, result, TemplateMatchModes.SqDiffNormed);

            // 输出匹配值的统计信息
            double minVal, maxVal;
            OpenCvSharp.Point minLoc, maxLoc;
            Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);
            Console.WriteLine($"最小匹配值: {minVal}");
            Console.WriteLine($"最大匹配值: {maxVal}");

            // 设置相似度阈值
            double threshold = minVal + (maxVal - minVal) * 0.1; // 选择一个合适的阈值

            var m=new MatchInfo();
            m.similarity = 1;
            m.Xpos = 1;

            // 遍历结果矩阵，找到所有匹配区域
            for (int y = 0; y < result.Rows; y++)
            {
                for (int x = 0; x < result.Cols; x++)
                {
                    if (result.At<float>(y, x) <= threshold) // SqDiff 模式下，值越小越匹配
                    {
                        // 输出匹配位置和相似度
                        Console.WriteLine($"匹配位置: ({x}, {y})");
                        Console.WriteLine($"相似度: {result.At<float>(y, x)}");
                        if (result.At<float>(y, x) < m.similarity)
                        {
                            m.similarity = result.At<float>(y, x);
                            m.Xpos = x;
                        }

                     ////   在原图上绘制匹配区域
                     //  Rect matchRect = new Rect(x, y, grayImg1Cropped.Width, grayImg1Cropped.Height);
                     //   Cv2.Rectangle(img2, matchRect, Scalar.Red, 2);
                    }
                }
            }
           // 显示结果
            //Cv2.ImShow("Matched Image", img2);
            //Cv2.WaitKey(0);
            //Cv2.DestroyAllWindows();
            if (m.similarity == 1)
            {
                return -1;
            }
            else
            {
                if (LeftContrast)
                {
                return m.Xpos;
                }
                else
                {
                    return img1.Width - width- m.Xpos;
                }
            }
        } 

    }
}
