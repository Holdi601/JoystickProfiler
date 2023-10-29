using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;


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
                            byte sc_white= ScreenColor.R <65 ? (byte)0 : ScreenColor.R < 128 ? (byte)1 : ScreenColor.R < 192 ? (byte)2 : (byte)3;
                            Color QueryColor = queryImage.GetPixel(x, y);
                            byte q_white = QueryColor.R < 65 ? (byte)0 : QueryColor.R < 128 ? (byte)1 : QueryColor.R < 192 ? (byte)2 : (byte)3;
                            if (sc_white != q_white)
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
    }


}
