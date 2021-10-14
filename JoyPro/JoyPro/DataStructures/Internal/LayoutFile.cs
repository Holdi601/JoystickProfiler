using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using Brushes = System.Drawing.Brushes;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace JoyPro
{
    [Serializable]
    public class LayoutFile
    {
        public string Joystick { get; set; }
        public string Font { get; set; }
        private byte r = 255, g=255, b=255, a=255;
        public SolidColorBrush ColorSCB
        {
            get
            {
               return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
            set
            {
                a = value.Color.A;
                r = value.Color.R;
                g = value.Color.G;
                b = value.Color.B;
            }
        }
        public Dictionary<string, System.Windows.Point> Positions;
        public int Size { get; set; }
        public System.Drawing.Bitmap backup;

        public LayoutFile()
        {
            Positions = new Dictionary<string, System.Windows.Point>();
        }
    }
}
