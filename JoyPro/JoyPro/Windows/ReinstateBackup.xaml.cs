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
    /// Interaktionslogik für ReinstateBackup.xaml
    /// </summary>
    public partial class ReinstateBackup : Window
    {
        Dictionary<string, List<string>> gameBUlist;
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;


        public ReinstateBackup()
        {
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;

            if (MainStructure.msave != null && MainStructure.msave._BackupWindow != null)
            {
                if (MainStructure.msave._BackupWindow.Top > 0) this.Top = MainStructure.msave._BackupWindow.Top;
                if (MainStructure.msave._BackupWindow.Left > 0) this.Left = MainStructure.msave._BackupWindow.Left;
                if (MainStructure.msave._BackupWindow.Width > 0) this.Width = MainStructure.msave._BackupWindow.Width;
                if (MainStructure.msave._BackupWindow.Height > 0) this.Height = MainStructure.msave._BackupWindow.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            gameBUlist = new Dictionary<string, List<string>>();
            setupGames();
            GameSelection.SelectionChanged += new SelectionChangedEventHandler(setDropDown);
            CancelBtn.Click += new RoutedEventHandler(closeThis);
            ReinstateBtn.Click += new RoutedEventHandler(reinstiate);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
        }
        void setupGames()
        {
            foreach(string game in MiscGames.Games)
            {
                GameSelection.Items.Add(game);
                if (!gameBUlist.ContainsKey(game))
                {
                    string path = "";
                    if (game == "DCS")
                    {
                        path = MiscGames.DCSselectedInstancePath;
                    }
                    else if (game == "IL2Game")
                    {
                        path = MiscGames.IL2Instance;
                    }
                    gameBUlist.Add(game, MiscGames.GetPossibleFallbacksForInstance(path, game));
                }
            }
        }
        void setDropDown(object sender, EventArgs e)
        {
            if (gameBUlist.ContainsKey((string)GameSelection.SelectedItem))
                ExistBUCB.ItemsSource = gameBUlist[(string)GameSelection.SelectedItem];
        }

        void closeThis(object sender, EventArgs e)
        {
            Close();
        }

        void reinstiate(object sender, EventArgs e)
        {
            if (ExistBUCB.SelectedItem == null || ((string)ExistBUCB.SelectedItem).Length < 2)
            {
                MessageBox.Show("Not a valid item selected");
                return;
            }
            string inst = "";
            if ((string)GameSelection.SelectedItem == "DCS")
                inst = MiscGames.DCSselectedInstancePath;
            else if ((string)GameSelection.SelectedItem == "IL2Game")
                inst = MiscGames.IL2Instance;
            else
                inst = "";
            MiscGames.RestoreInputsInInstance(inst, (string)ExistBUCB.SelectedItem, (string)GameSelection.SelectedItem);
            Close();
        }
    }
}
