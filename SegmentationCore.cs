using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace R6Bot
{

    

        static class Program
    {
        public static readonly SegmentsForm Segments = new SegmentsForm();

        [STAThread]
        static void Main()
        {
            
            Application.Run(Segments);
        }

    }


}

