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
    /// Interaction logic for BindButtonSetting.xaml
    /// </summary>
    public partial class BindButtonSetting : Window
    {
        Bind bind;
        MainWindow mainw;
        public BindButtonSetting(Bind b, MainWindow mw)
        {
            bind = b;
            mainw = mw;
            InitializeComponent();
            this.Title = b.Rl.NAME + " Settings";
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            EditBtn.Click += new RoutedEventHandler(Edit);
            DuplicateBtn.Click += new RoutedEventHandler(Duplicate);
            DeleteBtn.Click += new RoutedEventHandler(Delete);
            this.Closing += new System.ComponentModel.CancelEventHandler(rfsh);
        }

        void Edit(object sender, EventArgs e)
        {
            mainw.EditRelationButton(bind.Rl);
            Close();
        }

        void Duplicate(object sender, EventArgs e)
        {
            mainw.duplicateRelation(bind.Rl);
            Close();
        }

        void Delete(object sender, EventArgs e)
        {
            mainw.DeleteRelationButton(bind.Rl);
            Close();
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        void rfsh(object sender, EventArgs e)
        {
            InternalDataManagement.ResyncRelations();
        }
    }
}
