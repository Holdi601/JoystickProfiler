using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    [Serializable]
    public class WindowPos
    {
        public double Top;
        public double Left;
        public double Width;
        public double Height;

        public WindowPos()
        {
            Top = -1.0;
            Left = -1.0;
            Width = -1.0;
            Height = -1.0;
        }

        public override string ToString()
        {
            return Top.ToString(new CultureInfo("en-US")) + ";" + Left.ToString(new CultureInfo("en-US")) + ";" + Width.ToString(new CultureInfo("en-US")) + ";" + Height.ToString(new CultureInfo("en-US"));
        }

        public static WindowPos ToWindowPos(string line)
        {
            string[] parts = line.Split(';');
            if (parts.Length != 4) return null;
            WindowPos res = new WindowPos();
            res.Top = Convert.ToDouble(parts[0], new CultureInfo("en-US"));
            res.Left = Convert.ToDouble(parts[1], new CultureInfo("en-US"));
            res.Width = Convert.ToDouble(parts[2], new CultureInfo("en-US"));
            res.Height = Convert.ToDouble(parts[3], new CultureInfo("en-US"));
            return res;
        }
    }
}
