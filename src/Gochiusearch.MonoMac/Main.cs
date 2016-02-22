using Mpga.Gochiusearch;
using System.Windows.Forms;
using System;

namespace Gochiusearch.MonoMac
{
    class MainClass
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NativeMainForm());
        }
    }
}

