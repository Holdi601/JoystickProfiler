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
    /// Interaktionslogik für UserCurveDCS.xaml
    /// </summary>
    public partial class UserCurveDCS : Window
    {
        List<double> curve;
        public UserCurveDCS(Bind b)
        {
            InitializeComponent();
            if (MainStructure.msave != null && MainStructure.msave.UsrCvW != null)
            {
                if (MainStructure.msave.UsrCvW.Top > 0) this.Top = MainStructure.msave.UsrCvW.Top;
                if (MainStructure.msave.UsrCvW.Left > 0) this.Left = MainStructure.msave.UsrCvW.Left;
                if (MainStructure.msave.UsrCvW.Width > 0) this.Width = MainStructure.msave.UsrCvW.Width;
                if (MainStructure.msave.UsrCvW.Height > 0) this.Height = MainStructure.msave.UsrCvW.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            if (b == null) Close();
            curve = b.Curvature;
            CancelBtn.Click += new RoutedEventHandler(closeThis);
            if (!(curve != null && curve.Count > 10))
            {
                curve = new List<double>();
                curve.Add(0.0);
                curve.Add(0.1);
                curve.Add(0.2);
                curve.Add(0.3);
                curve.Add(0.4);
                curve.Add(0.5);
                curve.Add(0.6);
                curve.Add(0.7);
                curve.Add(0.8);
                curve.Add(0.9);
                curve.Add(1.0);
            }
            tbcv1.Text = curve[0].ToString();
            tbcv2.Text = curve[1].ToString();
            tbcv3.Text = curve[2].ToString();
            tbcv4.Text = curve[3].ToString();
            tbcv5.Text = curve[4].ToString();
            tbcv6.Text = curve[5].ToString();
            tbcv7.Text = curve[6].ToString();
            tbcv8.Text = curve[7].ToString();
            tbcv9.Text = curve[8].ToString();
            tbcv10.Text = curve[9].ToString();
            tbcv11.Text = curve[10].ToString();
            SubmitBtn.Click += new RoutedEventHandler(submitCurve);
        }

        void closeThis(object sender, EventArgs e)
        {
            Close();
        }

        void submitCurve(object sender, EventArgs e)
        {
            bool? succ;
            double val;
            succ = double.TryParse(tbcv1.Text, out val);
            if (succ == true)
            {
                curve[0] = val;
            }
            else
            {
                MessageBox.Show("1st Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv2.Text, out val);
            if (succ == true)
            {
                curve[1] = val;
            }
            else
            {
                MessageBox.Show("2nd Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv3.Text, out val);
            if (succ == true)
            {
                curve[2] = val;
            }
            else
            {
                MessageBox.Show("3rd Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv4.Text, out val);
            if (succ == true)
            {
                curve[3] = val;
            }
            else
            {
                MessageBox.Show("4th Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv5.Text, out val);
            if (succ == true)
            {
                curve[4] = val;
            }
            else
            {
                MessageBox.Show("5th Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv6.Text, out val);
            if (succ == true)
            {
                curve[5] = val;
            }
            else
            {
                MessageBox.Show("6th Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv7.Text, out val);
            if (succ == true)
            {
                curve[6] = val;
            }
            else
            {
                MessageBox.Show("7th Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv8.Text, out val);
            if (succ == true)
            {
                curve[7] = val;
            }
            else
            {
                MessageBox.Show("8th Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv9.Text, out val);
            if (succ == true)
            {
                curve[8] = val;
            }
            else
            {
                MessageBox.Show("9th Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv10.Text, out val);
            if (succ == true)
            {
                curve[9] = val;
            }
            else
            {
                MessageBox.Show("10th Value is not a valid double");
                return;
            }
            succ = double.TryParse(tbcv11.Text, out val);
            if (succ == true)
            {
                curve[10] = val;
            }
            else
            {
                MessageBox.Show("10th Value is not a valid double");
                return;
            }
            Close();
        }
    }
}
