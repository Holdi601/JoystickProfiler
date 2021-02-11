using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public class DCSExportPlane
    {
        public Dictionary<string, DCSLuaInput> joystickConfig;
        public string plane;

        public DCSExportPlane()
        {
            joystickConfig = new Dictionary<string, DCSLuaInput>();
        }
    }
}
