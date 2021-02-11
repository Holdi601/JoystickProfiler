using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSAxisFilter
    {
        public List<double> curviture;
        public double deadzone;
        public bool inverted;
        public double saturationX;
        public double saturationY;
        public bool slider;

        public DCSAxisFilter()
        {
            curviture = new List<double>();
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
            for(int i=0; i<curviture.Count; ++i)
            {
                result.curviture.Add(curviture[i]);
            }
            return result;
        }
    }
}
