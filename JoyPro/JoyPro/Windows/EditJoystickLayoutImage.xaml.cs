using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für EditJoystickLayoutImage.xaml
    /// </summary>
    public partial class EditJoystickLayoutImage : Window
    {
        string currentSelectedBtnAxis;
        string stick;
        string export;
        System.Drawing.Bitmap backup;
        System.Drawing.Bitmap mainImg;
        Image uiElementImage;
        SolidColorBrush fontColor;
        int textSize;
        Dictionary<string, Point> LabelLocations;
        public EditJoystickLayoutImage(string joystick, string filepath, string exportpath)
        {
            InitializeComponent();
            currentSelectedBtnAxis = "";
            LabelLocations = new Dictionary<string, Point>();
            stick = joystick;
            export = exportpath;
            fontColor = BrushFromHex("#FF000000");
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            ColorBtn.Click += new RoutedEventHandler(OpenColorPicker);
            PopulateButtonAxisList();
            PopulateFontDropDown();
            initBitMaps(filepath);
            textSize = Convert.ToInt32(TextSizeTB.Text);
            FontDropDown.SelectedIndex = 0;
            ButtonsLB.SelectionChanged += new SelectionChangedEventHandler(SelectedButtonChanged);
            TextSizeTB.LostFocus += new RoutedEventHandler(textSizeChanged);
            TextSizeTB.KeyUp += new KeyEventHandler(textSizeEnterChange);
            FontDropDown.SelectionChanged += new SelectionChangedEventHandler(settingChanged);
            refreshImageToShow();
        }
        void textSizeEnterChange(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textSizeChanged(sender, e);
            }
        }
        void textSizeChanged(object sender, EventArgs e)
        {
            bool succ = false;
            int num = -1;
            succ = int.TryParse(TextSizeTB.Text, out num);
            if (succ = false)
            {
                MessageBox.Show("Not a valid integer as text size");
                TextSizeTB.Text = textSize.ToString();
                return;
            }
            textSize = num;
            refreshImageToShow();
        }
        void initBitMaps(string path)
        {
            Uri fileUri = new Uri(path);
            backup = new System.Drawing.Bitmap(path);
            mainImg = new System.Drawing.Bitmap(path);
        }
        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }
        void SelectedButtonChanged(object sender, EventArgs e)
        {
            currentSelectedBtnAxis = (string)ButtonsLB.SelectedItem;
        }
        void PopulateButtonAxisList()
        {
            List<string> assignedButtons = InternalDataMangement.GetButtonsAxisInUseForStick(stick);
            assignedButtons.Sort();
            ButtonsLB.ItemsSource = assignedButtons;
            ButtonsLB.SelectedIndex = 0;
            currentSelectedBtnAxis = (string)ButtonsLB.SelectedItem;
        }
        void PopulateFontDropDown()
        {
            FontDropDown.Items.Add("Arial");
            FontDropDown.Items.Add("Calibri");
        }
        SolidColorBrush BrushFromHex(string hexColorString)
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hexColorString));
        }
        void OpenColorPicker(object sender, EventArgs e)
        {
            ColorDialog dig = new ColorDialog();
            if (dig.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fontColor = new SolidColorBrush(Color.FromArgb(dig.Color.A, dig.Color.R, dig.Color.G, dig.Color.B));
            }

            refreshImageToShow();
        }
        void refreshImageToShow()
        {
            mainImg = (System.Drawing.Bitmap)backup.Clone();
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(mainImg);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            System.Drawing.Brush b = new System.Drawing.SolidBrush((System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(new System.Windows.Media.BrushConverter().ConvertToString(fontColor)));
            foreach (KeyValuePair<string, Point> kvp in LabelLocations)
            {
                g.DrawString(kvp.Key, new System.Drawing.Font((string)FontDropDown.SelectedItem, textSize), b, Convert.ToSingle(kvp.Value.X), Convert.ToSingle(kvp.Value.Y));
            }
            g.Flush();

            uiElementImage = new Image();
            uiElementImage.Source = ConverBitmapToBitmapImage(mainImg);
            uiElementImage.Stretch = Stretch.Uniform;
            uiElementImage.MouseLeftButtonUp += new MouseButtonEventHandler(image_MouseLeftButtonUp);
            sv.Content = uiElementImage;
        }
        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(uiElementImage);
            AddLabelToImage(pos);
        }
        void settingChanged(object sender, EventArgs e)
        {
            refreshImageToShow();
        }
        void AddLabelToImage(Point pos)
        {
            double scaleFactor = mainImg.Height / uiElementImage.ActualHeight ;
            Point newPos = new Point(pos.X * scaleFactor, pos.Y * scaleFactor);

            if (LabelLocations.ContainsKey(currentSelectedBtnAxis))
                LabelLocations[currentSelectedBtnAxis] = newPos;
            else
                LabelLocations.Add(currentSelectedBtnAxis, newPos);
            refreshImageToShow();
        }

        private BitmapImage ConverBitmapToBitmapImage(System.Drawing.Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, ImageFormat.Png);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}
