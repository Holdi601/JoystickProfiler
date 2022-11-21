using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JoyPro
{
    public enum Anchor { TOP_LEFT, TOP_CENTER, TOP_RIGHT, CENTER_LEFT, CENTER_CENTER, CENTER_RIGHT, BOTTOM_LEFT, BOTTOM_CENTER, BOTTOM_RIGHT}
    public struct Click
    {
        public int x;
        public int y;
        public Anchor Anchor;
        public int TimeoutMS;
    }
    public struct DCSDBClicks
    {
        public Click GearSymbol;
        public Click OptionsControls;
        public Click OptionsControlsPlaneDropDown;
        public Click OptionsControlsMakeHTML;
        public Click OptionsControlsClearAll;
        public Click OptionsControlsClearAllCheckAll;
        public Click OptionsControlsClearAllYes;

        public static DCSDBClicks GenerateDefault()
        {
            DCSDBClicks clicks = new DCSDBClicks();
            clicks.GearSymbol = new Click() { x=564, y=26, Anchor = Anchor.TOP_LEFT, TimeoutMS=2000 };
            clicks.OptionsControls = new Click() {x=-365, y=70, Anchor=Anchor.TOP_CENTER, TimeoutMS=2000 };
            clicks.OptionsControlsPlaneDropDown = new Click() { x = 70, y = 100, Anchor = Anchor.TOP_LEFT, TimeoutMS = 500 };
            clicks.OptionsControlsMakeHTML = new Click() { x = 310, y = -70, Anchor = Anchor.BOTTOM_CENTER, TimeoutMS = 1500 };
            clicks.OptionsControlsClearAll = new Click() { x = 305, y = 111, Anchor = Anchor.TOP_CENTER, TimeoutMS = 2000 };
            clicks.OptionsControlsClearAllCheckAll = new Click() { x = -182, y = -76, Anchor = Anchor.CENTER_CENTER, TimeoutMS = 2000 };
            clicks.OptionsControlsClearAllYes = new Click() { x=-58, y=169, Anchor= Anchor.CENTER_CENTER, TimeoutMS=60000 };
            return clicks;
        }

    }
}
