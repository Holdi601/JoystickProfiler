using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;


namespace JoyPro
{
    public static class ScreenReader
    {
        public static Bitmap ScreenShot = null;
        public static Rectangle Bounds = Rectangle.Empty;
        public static void MakeScreenShot()
        {
            Bounds = Rectangle.Empty;
            foreach (Screen screen in Screen.AllScreens)
            {
                Bounds = Rectangle.Union(Bounds, screen.Bounds);
            }
            ScreenShot = new Bitmap(Bounds.Width, Bounds.Height);
            using (Graphics graphics = Graphics.FromImage(ScreenShot))
            {
                graphics.CopyFromScreen(Bounds.Left, Bounds.Top, 0, 0, ScreenShot.Size);
            }
            ScreenShot.Save(MainStructure.PROGPATH + "\\CurrentScreen.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public static Rectangle FindImageOnCurrentScreen(string pathToImg)
        {
            Rectangle result = Rectangle.Empty;
            MakeScreenShot();
            Bitmap queryImage = new Bitmap(pathToImg);
            for (int xStart = 0; xStart < ScreenShot.Width - queryImage.Width; xStart++)
            {
                for (int yStart = 0; yStart < ScreenShot.Height - queryImage.Height; yStart++)
                {
                    bool mismatch = false;
                    for (int y = 0; y < queryImage.Height; y++)
                    {
                        for (int x = 0; x < queryImage.Width; x++)
                        {
                            Color ScreenColor = ScreenShot.GetPixel(xStart + x, yStart + y);
                            Color QueryColor = queryImage.GetPixel(x, y);
                            if (ScreenColor != QueryColor)
                            {
                                mismatch = true;
                                break;
                            }
                            if (x == queryImage.Width - 1 && y == queryImage.Height - 1)
                            {
                                result = new Rectangle(xStart, yStart, queryImage.Width, queryImage.Height);
                                return result;
                            }
                        }
                        if (mismatch) break;
                    }
                }
            }
            return result;
        }

        public static Rectangle FindImageOnCurrentScreenBlackWhiteExtremeContrast(string pathToImg)
        {
            Rectangle result = Rectangle.Empty;

            // Ensure the screenshot is up-to-date
            MakeScreenShot();

            Bitmap queryImage = new Bitmap(pathToImg);

            // Pre-fetch dimensions to avoid cross-thread operations
            int screenWidth = ScreenShot.Width;
            int screenHeight = ScreenShot.Height;
            int queryWidth = queryImage.Width;
            int queryHeight = queryImage.Height;

            BitmapData screenData = ScreenShot.LockBits(new Rectangle(0, 0, screenWidth, screenHeight), ImageLockMode.ReadOnly, ScreenShot.PixelFormat);
            BitmapData queryData = queryImage.LockBits(new Rectangle(0, 0, queryWidth, queryHeight), ImageLockMode.ReadOnly, queryImage.PixelFormat);

            int bpp = Image.GetPixelFormatSize(ScreenShot.PixelFormat) / 8; // Bytes per pixel

            unsafe
            {
                byte* pScreen = (byte*)screenData.Scan0;
                byte* pQuery = (byte*)queryData.Scan0;

                Parallel.For(0, screenWidth - queryWidth, xStart =>
                {
                    for (int yStart = 0; yStart < screenHeight - queryHeight; yStart++)
                    {
                        bool mismatch = false;
                        for (int y = 0; y < queryHeight && !mismatch; y++)
                        {
                            for (int x = 0; x < queryWidth; x++)
                            {
                                byte* pScreenPixel = pScreen + (yStart + y) * screenData.Stride + (xStart + x) * bpp;
                                byte* pQueryPixel = pQuery + y * queryData.Stride + x * bpp;

                                byte sc_white = *pScreenPixel < 65 ? (byte)0 : *pScreenPixel < 128 ? (byte)1 : *pScreenPixel < 192 ? (byte)2 : (byte)3;
                                byte q_white = *pQueryPixel < 65 ? (byte)0 : *pQueryPixel < 128 ? (byte)1 : *pQueryPixel < 192 ? (byte)2 : (byte)3;

                                if (sc_white != q_white)
                                {
                                    mismatch = true;
                                    break;
                                }
                                if (x == queryWidth - 1 && y == queryHeight - 1)
                                {
                                    lock (queryImage)
                                    {
                                        if (result == Rectangle.Empty) // Prevent overwriting by other threads
                                        {
                                            result = new Rectangle(xStart, yStart, queryWidth, queryHeight);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            ScreenShot.UnlockBits(screenData);
            queryImage.UnlockBits(queryData);

            return result;
        }

        //public static Rectangle FindImageOnCurrentScreenBlackWhiteExtremeContrast(string pathToImg)
        //{
        //    Rectangle result = Rectangle.Empty;
        //    MakeScreenShot();
        //    Bitmap queryImage = new Bitmap(pathToImg);
        //    for (int xStart = 0; xStart < ScreenShot.Width - queryImage.Width; xStart++)
        //    {
        //        for (int yStart = 0; yStart < ScreenShot.Height - queryImage.Height; yStart++)
        //        {
        //            bool mismatch = false;
        //            for (int y = 0; y < queryImage.Height; y++)
        //            {
        //                for (int x = 0; x < queryImage.Width; x++)
        //                {
        //                    Color ScreenColor = ScreenShot.GetPixel(xStart + x, yStart + y);
        //                    byte sc_white= ScreenColor.R <65 ? (byte)0 : ScreenColor.R < 128 ? (byte)1 : ScreenColor.R < 192 ? (byte)2 : (byte)3;
        //                    Color QueryColor = queryImage.GetPixel(x, y);
        //                    byte q_white = QueryColor.R < 65 ? (byte)0 : QueryColor.R < 128 ? (byte)1 : QueryColor.R < 192 ? (byte)2 : (byte)3;
        //                    if (sc_white != q_white)
        //                    {
        //                        mismatch = true;
        //                        break;
        //                    }
        //                    if (x == queryImage.Width - 1 && y == queryImage.Height - 1)
        //                    {
        //                        result = new Rectangle(xStart, yStart, queryImage.Width, queryImage.Height);
        //                        return result;
        //                    }
        //                }
        //                if (mismatch) break;
        //            }
        //        }
        //    }
        //    return result;
        //}
    }


}
