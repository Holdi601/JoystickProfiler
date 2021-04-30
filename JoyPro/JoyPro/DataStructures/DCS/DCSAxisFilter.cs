using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSAxisFilter
    {
        public List<double> curvature;
        public double deadzone;
        public bool inverted;
        public double saturationX;
        public double saturationY;
        public bool slider;

        public DCSAxisFilter()
        {
            curvature = new List<double>();
            deadzone = 0.0;
            inverted = false;
            saturationX = 1.0;
            saturationY = 1.0;
            slider = false;
        }

        public DCSAxisFilter Copy()
        {
            DCSAxisFilter result = new DCSAxisFilter();
            result.deadzone = deadzone;
            result.inverted = inverted;
            result.slider = slider;
            result.saturationX = saturationX;
            result.saturationY = saturationY;
            for(int i=0; i< curvature.Count; ++i)
            {
                result.curvature.Add(curvature[i]);
            }
            return result;
        }
    }
}
