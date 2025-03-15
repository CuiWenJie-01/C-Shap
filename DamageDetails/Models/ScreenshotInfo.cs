using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMaker.Models
{
    public class ScreenshotInfo
    {
        public int ScreenshotOffset { get; set; }

        public int ScreenshotWidthPx { get; set; }
        public MoveDirection ScreenshotDirection { get; set; }

        public RailInfo RailWayInfo { get; set; }


    }
    public class RailInfo
    {

        //录入名称
        public string? RailwayName { get; set; }
        //串号
        public string? SerialNumber { get; set; }
        //仪器
        public string? Instruments { get; set; }
        //分析人
        public string? OperatorName { get; set; }
        //作业长度       
        public string? WorkLength { get; set; }
        //作业时间
        public string? WorkDate { get; set; }
        //正/站线
        public string? Line { get; set; }
        //工区
        public string? WorkAreas { get; set; }


    }

}
