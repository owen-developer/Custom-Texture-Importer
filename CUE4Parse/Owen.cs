﻿using System;
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
        public static List<string> Paths = new();
        public static List<long> Offsets = new();
        public static long FirstOffset = PullOffset();
        public static bool IsExporting = false;

        private static long PullOffset()
        {
            if (Offsets.Count == 0)
            {
                return 0;
            }
            else
            {
                return Offsets[0];
            }
        }
    }
}
