using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common
{
    public struct Point2d
    {
        public int X { get; set; }
        public int Y { get; set; }
        public override string ToString() => $"(X:{X}, Y:{Y})";
    }
}
