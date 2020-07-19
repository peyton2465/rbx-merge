using System;
using System.Collections.Generic;
using System.Text;

namespace rbx_merge
{
    public static class Logs
    {
        public static void Output(string format, params object[] args)
            => Console.WriteLine("[*] {0}", string.Format(format, args));

        public static void Info(string format, params object[] args)
            => Console.WriteLine("[!] {0}", string.Format(format, args));
    }
}
