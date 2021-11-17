using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SolidEdgeSpy
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            try
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new MainForm());
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }

            return 0;
        }
    }
}
