using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Text;
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
        List<string> assignedButtons;
        List<string> possibleAxBtn;
        string currentSelectedBtnAxis;
        string stick;
        string alias;
        string exportP;
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
            if(InternalDataMangement.JoystickAliases!=null&&
                InternalDataMangement.JoystickAliases.ContainsKey(joystick)&&
                InternalDataMangement.JoystickAliases[joystick].Length > 1)
            {
                alias = InternalDataMangement.JoystickAliases[joystick];
            }
            else
            {
                alias = joystick;
            }
            stick = joystick;
            exportP = exportpath;
            fontColor = BrushFromHex("#FF000000");
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            ColorBtn.Click += new RoutedEventHandler(OpenColorPicker);
            PopulateFontDropDown();
            if (filepath.EndsWith(".layout"))
            {
                openLayout(filepath);
            }
            else
            {
                initBitMaps(filepath);
            }
            List<string> rawPossibleBtns = JoystickReader.GetAllPossibleStickInputs();
            possibleAxBtn = new List<string>();
            possibleAxBtn.Add("Game");
            possibleAxBtn.Add("Plane");
            possibleAxBtn.Add("Joystick");
            for (int i = 0; i < rawPossibleBtns.Count; ++i) possibleAxBtn.Add(rawPossibleBtns[i]);
            List<string> modNames = new List<string>();
            foreach (KeyValuePair<string, Modifier> kvp in InternalDataMangement.AllModifiers) modNames.Add(kvp.Key);
            modNames.Sort();
            List<string> modNamesC = new List<string>();
            for(int i=0; i<modNames.Count; ++i)
            {
                if (i > 3) break;
                List<string> ModNamesCombined = getAllPossibleCombinationWithLength(modNames, i + 1);
                foreach (string s in ModNamesCombined) modNamesC.Add(s);
            }
            for(int i=0; i<modNamesC.Count; ++i)
            {
                for(int j=0; j<rawPossibleBtns.Count; ++j)
                {
                    possibleAxBtn.Add(modNamesC[i] + "+" + rawPossibleBtns[j]);
                }
            }
            assignedButtons = InternalDataMangement.GetButtonsAxisInUseForStick(stick);
            assignedButtons.Sort();
            assignedButtons.Add("Game");
            assignedButtons.Add("Plane");
            assignedButtons.Add("Joystick");
            PopulateButtonAxisList();
            textSize = Convert.ToInt32(TextSizeTB.Text);
            if(FontDropDown.SelectedIndex<0)
                FontDropDown.SelectedIndex = 0;
            ButtonsLB.SelectionChanged += new SelectionChangedEventHandler(SelectedButtonChanged);
            TextSizeTB.LostFocus += new RoutedEventHandler(textSizeChanged);
            TextSizeTB.KeyUp += new KeyEventHandler(textSizeEnterChange);
            FontDropDown.SelectionChanged += new SelectionChangedEventHandler(settingChanged);
            SaveLayoutBtn.Click += new RoutedEventHandler(saveLayout);
            ExportBtn.Click += new RoutedEventHandler(exportInputs);
            refreshImageToShow();
        }
        List<string> getAllPossibleCombinationWithLength(List<string> li, int leng)
        {
            if (leng > li.Count) return null;
            else if (leng == 0) return new List<string>();
            List<string> res = new List<string>();
            UInt16 max = (UInt16)Math.Pow(2, li.Count);
            for(UInt16 groups=1; groups<max; ++groups)
            {
                if (countSetBits(groups) == (UInt16)leng)
                {
                    string tempResult = "";
                    for(int i=0; i<16; ++i)
                    {
                        if ((groups & (1 << i)) != 0)
                        {
                            if (tempResult.Length == 0)
                            {
                                tempResult = li[i];
                            }
                            else
                            {
                                tempResult+="+"+li[i];
                            }
                        }
                    }
                    res.Add(tempResult);
                }
            }
            return res;
        }

        UInt16 countSetBits(UInt16 tester)
        {
            UInt16 j = 0;
            for (UInt16 i = 0; i < 16; ++i)
            {
                if ((tester & (1 << i))!=0)
                    ++j;
            }
            return j;
        }

        List<string> recursiveSetupNameList(List<string> li)
        {
            List<string> result = new List<string>();
            for(int i=0; i<li.Count; ++i)
            {
                string pref = li[i];
                for(int j=0; j<i; ++j)
                {
                    
                }
            }
            return result;
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
            if (succ == false)
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
            if((System.Windows.Controls.Label)ButtonsLB.SelectedItem!=null)
                currentSelectedBtnAxis = (string)((System.Windows.Controls.Label)ButtonsLB.SelectedItem).Content;
        }
        void PopulateButtonAxisList()
        {
            ButtonsLB.Items.Clear();
            for(int i=0; i< possibleAxBtn.Count; ++i)
            {
                System.Windows.Controls.Label btnLabel = new System.Windows.Controls.Label();
                btnLabel.Name = "btn";
                btnLabel.Content = possibleAxBtn[i];
                if (LabelLocations.ContainsKey(possibleAxBtn[i]))
                {
                    btnLabel.Foreground = Brushes.Black;
                }
                else if(assignedButtons.Contains(possibleAxBtn[i]))
                {
                    btnLabel.Foreground = Brushes.Red;
                }
                else
                {
                    btnLabel.Foreground = Brushes.Violet;
                }
                ButtonsLB.Items.Add(btnLabel);

            }
            if(possibleAxBtn != null&& possibleAxBtn.Count>0&&(currentSelectedBtnAxis==null||currentSelectedBtnAxis.Length<1))
                currentSelectedBtnAxis = possibleAxBtn[0];
        }
        void PopulateFontDropDown()
        {
            List<string> fonts = new List<string>();
            InstalledFontCollection installedFonts = new InstalledFontCollection();
            foreach (System.Drawing.FontFamily font in installedFonts.Families)
            {
                fonts.Add(font.Name);
            }
            FontDropDown.ItemsSource = fonts;
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
            uiElementImage.MouseRightButtonUp += new MouseButtonEventHandler(image_MouseRightButtonUp);
            sv.Content = uiElementImage;
        }

        void image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (LabelLocations.ContainsKey(currentSelectedBtnAxis))
            {
                LabelLocations.Remove(currentSelectedBtnAxis);
                refreshImageToShow();
                PopulateButtonAxisList();
            }
        }

        void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            PopulateButtonAxisList();
        }
        void openLayout(string filePath)
        {
            LayoutFile lf = MainStructure.ReadFromBinaryFile<LayoutFile>(filePath);
            backup = (System.Drawing.Bitmap)lf.backup.Clone();
            mainImg = (System.Drawing.Bitmap)lf.backup.Clone();
            fontColor = lf.ColorSCB;
            LabelLocations = lf.Positions;
            textSize = lf.Size;
            TextSizeTB.Text = textSize.ToString();
            int toSel = 0;
            for(int i=0; i<FontDropDown.Items.Count; ++i)
            {
                if ((string)FontDropDown.Items[i] == lf.Font) toSel = i;
            }
            FontDropDown.SelectedIndex = toSel;
        }
        void saveLayout(object sender, EventArgs e)
        {
            if (LabelLocations.Count < 1) return;
            LayoutFile lf = new LayoutFile();
            lf.backup = backup;
            lf.ColorSCB = fontColor;
            lf.Font = (string)FontDropDown.SelectedItem;
            lf.Joystick = stick;
            lf.Positions = LabelLocations;
            lf.Size = textSize;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Layout Files (*.layout)|*.layout|All filed (*.*)|*.*";
            saveFileDialog1.Title = "Save Joystick Layout";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            MainStructure.WriteToBinaryFile<LayoutFile>(filePath, lf);
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
        void exportInputs(object sender, EventArgs e)
        {
            if (LabelLocations.Count < 1)
            {
                MessageBox.Show("No Labels set. Set the labels where you want them before you can export");
                return;
            }
            foreach(KeyValuePair<string, List<string>> kvp in DBLogic.Planes)
            {
                if (!Directory.Exists(exportP + "\\" + kvp.Key))
                    Directory.CreateDirectory(exportP + "\\" + kvp.Key);

                for(int i=0; i < kvp.Value.Count; ++i)
                {
                    if (!Directory.Exists(exportP + "\\" + kvp.Key+"\\"+kvp.Value[i]))
                        Directory.CreateDirectory(exportP + "\\" + kvp.Key + "\\" + kvp.Value[i]);
                    System.Drawing.Bitmap export = (System.Drawing.Bitmap)backup.Clone();
                    System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(export);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    System.Drawing.Brush b = new System.Drawing.SolidBrush((System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(new System.Windows.Media.BrushConverter().ConvertToString(fontColor)));
                    foreach (KeyValuePair<string, Point> keys in LabelLocations)
                    {
                        string descriptor;
                        if (keys.Key == "Game")
                        {
                            descriptor = kvp.Key;
                        }else if (keys.Key == "Plane")
                        {
                            descriptor = kvp.Value[i];
                        }
                        else if (keys.Key == "Joystick")
                        {
                            descriptor = alias;
                        }
                        else
                        {
                            descriptor = InternalDataMangement.GetDescriptionForJoystickButtonGamePlane(stick, keys.Key, kvp.Key, kvp.Value[i]);
                        }
                        g.DrawString(descriptor, new System.Drawing.Font((string)FontDropDown.SelectedItem, textSize), b, Convert.ToSingle(keys.Value.X), Convert.ToSingle(keys.Value.Y));
                    }
                    g.Flush();
                    export.Save(exportP + "\\" + kvp.Key + "\\" + kvp.Value[i]+"\\"+ kvp.Value[i] + "_"+ alias + ".png", ImageFormat.Png);
                }
            }
            //One General Overview
            System.Drawing.Bitmap expMain = (System.Drawing.Bitmap)backup.Clone();
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(expMain);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            System.Drawing.Brush br = new System.Drawing.SolidBrush((System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(new System.Windows.Media.BrushConverter().ConvertToString(fontColor)));
            foreach (KeyValuePair<string, Point> keys in LabelLocations)
            {
                string descriptor;
                if (keys.Key == "Game")
                {
                    descriptor = "JoyPro";
                }
                else if (keys.Key == "Plane")
                {
                    descriptor = "JoyPro";
                }
                else if (keys.Key == "Joystick")
                {
                    descriptor = alias;
                }
                else
                {
                    descriptor = InternalDataMangement.GetRelationNameForJostickButton(stick, keys.Key);
                }
                gr.DrawString(descriptor, new System.Drawing.Font((string)FontDropDown.SelectedItem, textSize), br, Convert.ToSingle(keys.Value.X), Convert.ToSingle(keys.Value.Y));
            }
            gr.Flush();
            expMain.Save(exportP + "\\"+ alias + ".png", ImageFormat.Png);
            MessageBox.Show("Export looks successful.");
        }
    }
}
