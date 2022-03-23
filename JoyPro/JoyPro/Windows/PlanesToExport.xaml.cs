using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JoyPro
{
    /// <summary>
    /// Interaction logic for PlanesToExport.xaml
    /// </summary>
    /// 

    public enum ExportMode { WriteCleanNotOverride, WriteCleanOverride, WriteCleanAdd,WriteClean}
    public partial class PlanesToExport : Window
    {
        ExportMode exportMode;
        bool nukeDevices=false;
        public PlanesToExport(ExportMode em,bool nukeDev=false, Dictionary<string, List<string>> activeAirCraft = null, List<Bind> bindsToExport = null)
        {
            InitializeComponent();
            if (MainStructure.msave != null && MainStructure.msave._ExportWindow != null)
            {
                if (MainStructure.msave._ExportWindow.Top > 0) this.Top = MainStructure.msave._ExportWindow.Top;
                if (MainStructure.msave._ExportWindow.Left > 0) this.Left = MainStructure.msave._ExportWindow.Left;
                if (MainStructure.msave._ExportWindow.Width > 0) this.Width = MainStructure.msave._ExportWindow.Width;
                if (MainStructure.msave._ExportWindow.Height > 0) this.Height = MainStructure.msave._ExportWindow.Height;
            }
            exportMode = em;
            nukeDevices = nukeDev;
            SetupGamesDropDown();
            CancelBtn.Click += new RoutedEventHandler(CloseThis);
            ExportBtn.Click += new RoutedEventHandler(Export);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            if (bindsToExport != null) ExportActiveParameter(activeAirCraft, bindsToExport);
        }

        void ExportActiveParameter(Dictionary<string, List<string>> ActivePlanes, List<Bind> bindsInView=null)
        {
            switch (exportMode)
            {
                case ExportMode.WriteCleanNotOverride: InternalDataManagement.WriteProfileCleanNotOverwriteLocal(ActivePlanes, bindsInView); break;
                case ExportMode.WriteCleanOverride: InternalDataManagement.WriteProfileCleanAndLoadedOverwritten(ActivePlanes, bindsInView); break;
                case ExportMode.WriteClean: InternalDataManagement.WriteProfileClean(nukeDevices, ActivePlanes, bindsInView); break;
                case ExportMode.WriteCleanAdd: InternalDataManagement.WriteProfileCleanAndLoadedOverwrittenAndAdd(ActivePlanes, bindsInView); break;
            }
            Close();
        }

        void Export(object sender, EventArgs e)
        {
            Dictionary<string, List<string>> ActivePlanes = ActiveGamePlaneDict(GetActivePlanes());
            ExportActiveParameter(ActivePlanes);
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        private void SetupGamesDropDown()
        {
            GamePlaneBox.Items.Clear();
            CheckBox cbpAll = new CheckBox();
            cbpAll.Name = "ALL";
            cbpAll.Content = "ALL";
            cbpAll.IsChecked = false;
            cbpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
            GamePlaneBox.Items.Add(cbpAll);

            CheckBox cbpNone = new CheckBox();
            cbpNone.Name = "NONE";
            cbpNone.Content = "NONE";
            cbpNone.IsChecked = false;
            cbpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
            GamePlaneBox.Items.Add(cbpNone);

            for (int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                //REmove for later SC implementation                
                if (DBLogic.Planes.ElementAt(i).Key.ToLower().Contains("starcitizen")) continue;

                CheckBox cbgpAll = new CheckBox();
                cbgpAll.Name = "ALL";
                cbgpAll.Content = DBLogic.Planes.ElementAt(i).Key + ":" + "ALL";
                cbgpAll.IsChecked = false;
                cbgpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
                GamePlaneBox.Items.Add(cbgpAll);

                CheckBox cbgpNone = new CheckBox();
                cbgpNone.Name = "NONE";
                cbgpNone.Content = DBLogic.Planes.ElementAt(i).Key + ":" + "NONE";
                cbgpNone.IsChecked = false;
                cbgpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
                GamePlaneBox.Items.Add(cbgpNone);
            }

            for (int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                //REmove for later SC implementation                
                if (DBLogic.Planes.ElementAt(i).Key.ToLower().Contains("starcitizen")) continue;

                for (int j = 0; j < DBLogic.Planes.ElementAt(i).Value.Count; ++j)
                {
                    CheckBox pln = new CheckBox();
                    pln.Name = "plane";
                    string k = DBLogic.Planes.ElementAt(i).Key + ":" + DBLogic.Planes.ElementAt(i).Value[j];
                    pln.Content = k;
                    bool? state = MainStructure.msave.PlaneWasActiveLastTime(PlaneActivitySelection.Export, DBLogic.Planes.ElementAt(i).Key, DBLogic.Planes.ElementAt(i).Value[j]);
                    if (state == null) pln.IsChecked = true;
                    else pln.IsChecked = state;
                    pln.Click += new RoutedEventHandler(PlaneFilterChanged);
                    GamePlaneBox.Items.Add(pln);
                }
            }
        }

        private void PlaneFilterChanged(object sender, RoutedEventArgs e)
        {
            CheckBox sndr = (CheckBox)sender;
            if ((string)sndr.Content == "ALL")
            {
                for (int i = 0; i < GamePlaneBox.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamePlaneBox.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        element.IsChecked = true;
                    }
                }
            }
            else if ((string)sndr.Content == "NONE")
            {
                for (int i = 0; i < GamePlaneBox.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamePlaneBox.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        element.IsChecked = false;
                    }
                }
            }
            else if (((string)sndr.Content).Contains(":ALL"))
            {
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < GamePlaneBox.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamePlaneBox.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        string elementGame = ((string)element.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                        if (elementGame == game) element.IsChecked = true;
                    }
                }
            }
            else if (((string)sndr.Content).Contains(":NONE"))
            {
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < GamePlaneBox.Items.Count; ++i)
                {
                    CheckBox element = (CheckBox)GamePlaneBox.Items[i];
                    string cnt = (string)element.Content;
                    if (cnt == "ALL" || cnt == "NONE" || cnt.Contains(":ALL") || cnt.Contains(":NONE"))
                    {
                        element.IsChecked = false;
                    }
                    else
                    {
                        string elementGame = ((string)element.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                        if (elementGame == game) element.IsChecked = false;
                    }
                }
            }
            else
            {

            }
        }

        List<string> GetActivePlanes()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < GamePlaneBox.Items.Count; i++)
            {
                CheckBox element = (CheckBox)GamePlaneBox.Items[i];
                string content = (string)element.Content;
                if (!(content == "ALL" || content == "NONE" || content.Contains(":ALL") || content.Contains(":NONE")))
                {
                    string game = content.Substring(0, content.IndexOf(":"));
                    string plane = content.Substring(content.IndexOf(":") + 1);
                    bool state = element.IsChecked == true ? true : false;
                    MainStructure.msave.PlaneSetLastActivity(PlaneActivitySelection.Export, game, plane, state);
                    if (element.IsChecked == true)
                    {
                        result.Add(content);
                    }
                }
            }
            return result;
        }

        Dictionary<string, List<string>> ActiveGamePlaneDict(List<string> rawList)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            for (int i = 0; i < rawList.Count; i++)
            {
                string[] parts = rawList[i].Split(':');
                if (!result.ContainsKey(parts[0])) result.Add(parts[0], new List<string>());
                result[parts[0]].Add(parts[1]);
            }
            return result;
        }
    }


}
