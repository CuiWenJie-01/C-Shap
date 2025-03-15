﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMaker.Models
{
    public class DamageFoldersInfo
    {
        public string FolderName { get; set; }
        public DateTime CreatTime { get; set; }
        public bool HasDamage { get; set; }
        public int PngCount { get; set; }
        public int DamagePngCount { get; set; }

        public bool HasDocx { get; set; }
        public bool HasMileage { get; set; }
        public override string ToString()
        {
            return FolderName;
        }

    }
}
