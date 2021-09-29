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
        public ReinstateBackup()
        {
            InitializeComponent();
            if (MainStructure.msave != null && MainStructure.msave.BackupW != null)
            {
                if (MainStructure.msave.BackupW.Top > 0) this.Top = MainStructure.msave.BackupW.Top;
                if (MainStructure.msave.BackupW.Left > 0) this.Left = MainStructure.msave.BackupW.Left;
                if (MainStructure.msave.BackupW.Width > 0) this.Width = MainStructure.msave.BackupW.Width;
                if (MainStructure.msave.BackupW.Height > 0) this.Height = MainStructure.msave.BackupW.Height;
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
            foreach(string game in MainStructure.Games)
            {
                GameSelection.Items.Add(game);
                if (!gameBUlist.ContainsKey(game))
                {
                    string path = "";
                    if (game == "DCS")
                    {
                        path = MainStructure.selectedInstancePath;
                    }
                    else if (game == "IL2Game")
                    {
                        path = MainStructure.IL2Instance;
                    }
                    gameBUlist.Add(game, MainStructure.GetPossibleFallbacksForInstance(path, game));
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
                inst = MainStructure.selectedInstancePath;
            else if ((string)GameSelection.SelectedItem == "IL2Game")
                inst = MainStructure.IL2Instance;
            else
                inst = "";
            MainStructure.RestoreInputsInInstance(inst, (string)ExistBUCB.SelectedItem, (string)GameSelection.SelectedItem);
            Close();
        }
    }
}
