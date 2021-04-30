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
        List<string> possibleBackups;
        public ReinstateBackup(List<string> possBackups)
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
            possibleBackups = possBackups;
            ExistBUCB.ItemsSource = possBackups;
            CancelBtn.Click += new RoutedEventHandler(closeThis);
            ReinstateBtn.Click += new RoutedEventHandler(reinstiate);
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
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
            MainStructure.RestoreInputsInInstance(MainStructure.selectedInstancePath, (string)ExistBUCB.SelectedItem);
            Close();
        }
    }
}
