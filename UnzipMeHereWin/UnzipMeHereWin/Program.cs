using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Ionic.Zip;

namespace UnzipMeHereWin
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ARgs inc");
            for (int i = 0; i < args.Length; ++i)
                Console.WriteLine(args[i]);
            Thread.Sleep(5000);
            if (args.Length > 1)
            {
                string src = args[0];
                string dst = args[1];
                ZipFile zip1 = ZipFile.Read(src);
                foreach (ZipEntry e in zip1)
                {
                    try
                    {
                        e.Extract(dst, ExtractExistingFileAction.OverwriteSilently);
                    }
                    catch
                    {

                    }
                    
                }

            }
            if (args.Length > 2)
            {
                Process.Start(args[2]);
            }
        }

    }
}
