using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMaker.Models
{

    public enum DamageCategory
    {
        接头, // 0
        零度迟到波, // 1
        普通焊缝, // 2
        七十度螺孔回波, // 3
        轨形变换, // 4
        母材核伤, // 5
        焊缝核伤, // 6
        螺孔裂纹, // 7
        零度异常, // 8
        轨腰裂纹, // 9
        轨底裂纹_规则, // 10
        加固焊缝, // 11
        断面, // 12
        轨面剥离, // 13
        焊缝标记, // 14
        无伤标记, // 15
        焊缝且无伤标记, // 16
        轻伤标记, // 17
        轻伤发展, // 18
        重伤标志, // 19
        作业违规, // 20
        焊缝且无焊缝标记, // 21
        假像波, // 22
        鱼鳞伤_规则, // 23
        岔心, // 24
        核伤2_模型, // 25
        核伤3_规则, // 26
        核伤1_规则, // 27
        正常螺孔, // 28
        核伤2_模型_焊缝附近, // 29
        核伤2_规则, // 30
        核伤2_规则_焊缝附近, // 31
        水平裂纹_规则, // 32
        斜裂纹_规则, // 33
        月牙伤2_规则, // 34
        核伤4_规则, //35
        鱼鳞伤, // 36
        轨底裂纹, //37
        轨面剥离_规则, // 38
        未知异常,
    }

    public enum MoveDirection
    {
        Left,
        Right
    }


}
