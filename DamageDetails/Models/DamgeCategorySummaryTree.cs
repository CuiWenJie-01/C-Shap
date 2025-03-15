using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DamageMaker.Models
{
    public interface ITreeNode
    {
        string Name { get; set; }
    }

    public class DamgeCategorySummaryTree:ITreeNode
    {
        public string Name { get; set; }
        public List<ITreeNode> Children { get; set; }
    }

    public class DamgeCategory : ITreeNode
    {
        public string Name { get; set; }
        public List<Details> Children { get; set; }
    }
    public class Details
    {
        public string FileName { get; set; }
        public int Count { get; set; }

        public float? weight { get; set; }

    }
}
