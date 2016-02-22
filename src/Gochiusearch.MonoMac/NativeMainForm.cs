using System;
using System.Linq;
using Mpga.Gochiusearch;
using System.Windows.Forms;
using MonoMac.Foundation;

namespace Gochiusearch.MonoMac
{
    public class NativeMainForm : MainForm
    {
        public NativeMainForm()
        {
        }

        public override void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var filesInPath = files.Select(x => NSUrl.FromFilename(x).Path).ToArray();

            e.Data.SetData("FileDrop", true, filesInPath.ToArray());

            base.Form1_DragDrop(sender, e);
        }
    }
}

