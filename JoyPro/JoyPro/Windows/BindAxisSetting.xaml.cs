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
    /// Interaction logic for BindAxisSetting.xaml
    /// </summary>
    public partial class BindAxisSetting : Window
    {
        Bind bind;
        MainWindow mainw;
        public BindAxisSetting(Bind b, MainWindow mw)
        {
            bind = b;
            mainw = mw;
            InitializeComponent();
            this.Title = b.Rl.NAME + " Settings";
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            EditBtn.Click += new RoutedEventHandler(Edit);
            DuplicateBtn.Click += new RoutedEventHandler(Duplicate);
            DeleteBtn.Click += new RoutedEventHandler(Delete);
            DeadzoneTB.LostFocus += new RoutedEventHandler(DeadzoneChanged);
            SatXTB.LostFocus+= new RoutedEventHandler(SaturationXChanged);
            SatYTB.LostFocus+=new RoutedEventHandler(SaturationYChanged);
            CurvTB.LostFocus+=new RoutedEventHandler(CurvChanged);
            InvertedCB.Click+= new RoutedEventHandler(InvertedChanged);
            SliderCB.Click+= new RoutedEventHandler(SliderChanged);
            UserCVCB.Click+=new RoutedEventHandler(UserCurveChanged);
            UserCurveBtn.Click += new RoutedEventHandler(openUserCurve);
            this.Closing += new System.ComponentModel.CancelEventHandler(rfsh);
            DeadzoneTB.Text = b.Deadzone.ToString();
            SatXTB.Text = b.SaturationX.ToString();
            SatYTB.Text = b.SaturationY.ToString();
            CurvTB.Text = b.Curvature[0].ToString();
            SliderCB.IsChecked = b.Slider;
            InvertedCB.IsChecked = b.Inverted;
            if (b.Curvature.Count > 1)
            {
                UserCVCB.IsChecked = true;
                CurvTB.Visibility = Visibility.Hidden;
                UserCurveBtn.Visibility = Visibility.Visible;
            }
            else
            {
                UserCVCB.IsChecked = false;
                CurvTB.Visibility = Visibility.Visible;
                UserCurveBtn.Visibility = Visibility.Hidden;
            }
        }

        void openUserCurve(object sender, EventArgs e)
        {
            mainw.changeUserCurve(bind);
        }
        void DeadzoneChanged(object sender, EventArgs e)
        {
            double val;
            if (!double.TryParse(DeadzoneTB.Text, out val))
            {
                MessageBox.Show("Not a valid double for Deadzone");
                return;
            }
            bind.Deadzone = val;
        }

        void SaturationXChanged(object sender, EventArgs e)
        {
            double val;
            if (!double.TryParse(SatXTB.Text, out val))
            {
                MessageBox.Show("Not a valid double for Saturation X");
                return;
            }
            bind.SaturationX = val;
        }

        void SaturationYChanged(object sender, EventArgs e)
        {
            double val;
            if (!double.TryParse(SatYTB.Text, out val))
            {
                MessageBox.Show("Not a valid double for Saturation Y");
                return;
            }
            bind.SaturationY = val;
        }

        void CurvChanged(object sender, EventArgs e)
        {
            double val;
            if(!double.TryParse(CurvTB.Text, out val))
            {
                MessageBox.Show("Not a valid double for Curvature");
                return;
            }
            bind.Curvature[0] = val;
        }

        void InvertedChanged(object sender, EventArgs e)
        {
            bind.Inverted=InvertedCB.IsChecked;
        }

        void SliderChanged(object sender, EventArgs e)
        {
            bind.Slider = SliderCB.IsChecked;
        }
        void UserCurveChanged(object sender, EventArgs e)
        {
            if (UserCVCB.IsChecked == true)
            {
                bind.GenerateDefaultUserCurve();
                CurvTB.Visibility = Visibility.Hidden;
                UserCurveBtn.Visibility = Visibility.Visible;
            }
            else
            {
                bind.Curvature = new List<double>() { 0.0 };
                CurvTB.Visibility = Visibility.Visible;
                UserCurveBtn.Visibility = Visibility.Hidden;
            }
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
            MainStructure.OverlayWorker.SetButtonMapping();
            InternalDataManagement.ResyncRelations();
        }
    }
}
