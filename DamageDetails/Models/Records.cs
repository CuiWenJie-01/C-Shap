using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMaker.Models
{
    public class Records
    {
        public record OcrData(string ImgFullPath, string MileageText);
    }
}
