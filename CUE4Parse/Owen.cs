using CUE4Parse.UE4.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUE4Parse
{
    public static class Owen
    {
        public static string Path = string.Empty;
        public static int Partition = 0;
        public static uint TocIndex = 0;
        public static int FirstBlockIndex = 0;
        public static List<string> Paths = new();
        public static List<long> Offsets = new();
        public static IoStoreReader? Reader = null;

        public static bool IsExporting = false;
    }
}
