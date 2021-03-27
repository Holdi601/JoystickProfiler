using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Threading;
using System.Diagnostics;

namespace UnzipMeHereWin
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(5000);
            if (args.Length > 1)
            {
                string src = args[0];
                string dst = args[1];
                ZipFile.ExtractToDirectory(src, dst);
            }
            if (args.Length > 2)
            {
                Process.Start(args[2]);
            }
        }
    }
}
