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
        public static Dictionary<long, long> OffsetsAndLengths = new();
        public static List<long> TocOffsets = new();
        public static List<long> TocOffsets2 = new();
        public static List<string> Paths = new();
        public static Stack<long> Offsets = new();

        public static bool IsExporting = false;

        public static Stack<T> Reverse<T>(this Stack<T> stack)
        {
            var newStack = new Stack<T>();
            while (stack.Count > 0)
            {
                newStack.Push(stack.Pop());
            }
            return newStack;
        }
    }
}
