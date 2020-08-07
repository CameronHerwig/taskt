using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace taskt.Core.Utilities.CommonUtilities
{
    public class ImageMethods
    {
        public static Bitmap Screenshot()
        {
            var screen = Screen.PrimaryScreen;
            var rect = screen.Bounds;
            Size size = new Size((int)(rect.Size.Width * 1), (int)(rect.Size.Height * 1));

            Bitmap bmpScreenshot = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(bmpScreenshot);
            g.CopyFromScreen(0, 0, 0, 0, size);

            return bmpScreenshot;
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        private enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
        }

        private static double GetScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            double ScreenScalingFactor = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }
    }
}
