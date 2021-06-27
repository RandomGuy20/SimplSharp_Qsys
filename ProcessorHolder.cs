using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    internal static class ProcessorHolder
    {
        public static Dictionary<string, ProcessorQsys> holder = new Dictionary<string, ProcessorQsys>();
    }
}