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
            Console.WriteLine("Wait for all components to be closed.");
            Thread.Sleep(1000);
            if (args.Length > 2)
            {
                string xctbl = args[2].Replace(args[1], "");
                Process[] proc = Process.GetProcessesByName(xctbl);
                while (proc.Length > 0)
                {
                    foreach (Process p in proc) p.Kill();
                    Thread.Sleep(1000);
                    Console.WriteLine("Killing Processes...");
                    proc = Process.GetProcessesByName(xctbl);
                }
            }

            if (args.Length > 1)
            {
                string src = args[0];
                string dst = args[1];
                ZipFile zip1 = ZipFile.Read(src);
                Console.WriteLine("Starting extraction");
                foreach (ZipEntry e in zip1)
                {
                    try
                    {
                        e.Extract(dst, ExtractExistingFileAction.OverwriteSilently);
                        Console.WriteLine(e.FileName + " write successful");
                    }
                    catch(Exception es)
                    {
                        Console.WriteLine(dst + " write failed");
                        Console.WriteLine(es.Message);
                        Console.WriteLine(es.StackTrace);
                        Console.WriteLine(es.Source);
                        Console.WriteLine(es.HelpLink);
                    }
                    
                }
                Console.WriteLine("Installation done. Starting Process if given");

            }
            Console.WriteLine("Press any buttton to finish installation.");
            Console.ReadKey();
            if (args.Length > 2)
            {
                Console.WriteLine("Start Process: " + args[2]);
                Process.Start(args[2]);
            }
        }

    }
}
