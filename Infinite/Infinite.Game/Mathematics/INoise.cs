using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite.Mathematics
{
    interface INoise
    {
        double Generate(long x, long y);
        double Generate(double x, double y);
        double Generate(long x, long y, long z);
        double Generate(double x, double y, double z);
    }
}
