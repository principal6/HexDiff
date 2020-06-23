using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace HexDiff
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        private static readonly int kVersionMajor = 1;
        private static readonly int kVersionMinor = 0;

        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main(string[] Arguments)
        {
            Console.WriteLine("HexDiff Version {0}.{1}", kVersionMajor, kVersionMinor);

            if (Arguments.Length > 0)
            {
                Console.WriteLine("> Using command-line arguments");

                for (int i = 0; i < Arguments.Length; ++i)
                {
                    Console.WriteLine(" Argument[{0}]: {1}", i, Arguments[i]);
                }

                Console.WriteLine("> Initializing the app");
            }

            FreeConsole();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
