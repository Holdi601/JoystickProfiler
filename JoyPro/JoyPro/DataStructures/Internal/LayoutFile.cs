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

namespace JoyPro.Internal
{
    [Serializable]
    public class LayoutFile
    {
        public string Joystick { get; set; }
        public string Font { get; set; }
        public SolidColorBrush Color { get; set; }
        public Dictionary<string, Point> Positions;
        public int Size { get; set; }
        public System.Drawing.Bitmap backup;

        public LayoutFile()
        {
            Positions = new Dictionary<string, Point>();
        }
    }
}
