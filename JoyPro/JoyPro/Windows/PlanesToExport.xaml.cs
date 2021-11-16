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
        bool param;
        public PlanesToExport(ExportMode em, bool paramP)
        {
            InitializeComponent();
            exportMode = em;
            param = paramP;
            SetupGamesDropDown();
            CancelBtn.Click += new RoutedEventHandler(CloseThis);
            ExportBtn.Click += new RoutedEventHandler(Export);
        }

        void Export(object sender, EventArgs e)
        {
            Dictionary<string, List<string>> ActivePlanes = ActiveGamePlaneDict(GetActivePlanes());
            switch (exportMode)
            {
                case ExportMode.WriteCleanNotOverride: InternalDataManagement.WriteProfileCleanNotOverwriteLocal(param, ActivePlanes); break;
                case ExportMode.WriteCleanOverride: InternalDataManagement.WriteProfileCleanAndLoadedOverwritten(param,ActivePlanes); break;
                case ExportMode.WriteClean: InternalDataManagement.WriteProfileClean(param, ActivePlanes); break;
                case ExportMode.WriteCleanAdd: InternalDataManagement.WriteProfileCleanAndLoadedOverwrittenAndAdd(param,ActivePlanes); break;
            }
            Close();
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
                for (int j = 0; j < DBLogic.Planes.ElementAt(i).Value.Count; ++j)
                {
                    CheckBox pln = new CheckBox();
                    pln.Name = "plane";
                    string k = DBLogic.Planes.ElementAt(i).Key + ":" + DBLogic.Planes.ElementAt(i).Value[j];
                    pln.Content = k;
                    pln.IsChecked = InternalDataManagement.PlaneActivity[k];
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
                if (!(content == "ALL" || content == "NONE" || content.Contains(":ALL") || content.Contains(":NONE")) && element.IsChecked == true)
                {
                    result.Add(content);
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
