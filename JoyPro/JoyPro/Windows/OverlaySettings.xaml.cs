using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace JoyPro
{
    /// <summary>
    /// Interaction logic for OverlaySettings.xaml
    /// </summary>
    public partial class OverlaySettings : Window
    {
        public OverlaySettings()
        {
            InitializeComponent();
            if (MainStructure.msave != null && MainStructure.msave.OverlaySW != null)
            {
                if (MainStructure.msave.OverlaySW.Top > 0) this.Top = MainStructure.msave.OverlaySW.Top;
                if (MainStructure.msave.OverlaySW.Left > 0) this.Left = MainStructure.msave.OverlaySW.Left;
                if (MainStructure.msave.OverlaySW.Width > 0) this.Width = MainStructure.msave.OverlaySW.Width;
                if (MainStructure.msave.OverlaySW.Height > 0) this.Height = MainStructure.msave.OverlaySW.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            HeighTF.Text = MainStructure.msave.OvlH.ToString();
            WidthTF.Text = MainStructure.msave.OvlW.ToString();
            TextSizeTF.Text = MainStructure.msave.OvlTxtS.ToString();
            ElementsToShowTF.Text = MainStructure.msave.OvlElementsToShow.ToString();
            PollTimeTF.Text = MainStructure.msave.OvlPollTime.ToString();
            TimeAliveTF.Text = MainStructure.msave.TextTimeAlive.ToString();
            List<string> fonts = new List<string>();
            InstalledFontCollection installedFonts = new InstalledFontCollection();
            int fontSelectIndex = -1;
            int i = 0;
            foreach (System.Drawing.FontFamily font in installedFonts.Families)
            {
                fonts.Add(font.Name);
                if(font.Name==MainStructure.msave.Font)fontSelectIndex= i;
                i++;
            }
            FontDropdown.ItemsSource = fonts;
            FadeButtonsCB.IsChecked = MainStructure.msave.OvlFade;
            ChangeButtonModeCB.IsChecked = MainStructure.msave.OvlBtnChangeMode;
            if(fontSelectIndex>=0)FontDropdown.SelectedIndex = fontSelectIndex;
            ColorPickerBtn.Click += new RoutedEventHandler(OpenColorPicker);
            WidthTF.LostFocus += new RoutedEventHandler(WidthLostFocus);
            HeighTF.LostFocus += new RoutedEventHandler(HeightLostFocus);
            TextSizeTF.LostFocus += new RoutedEventHandler(TextSizeLostFocus);
            ElementsToShowTF.LostFocus += new RoutedEventHandler(ElementsToShowLostFocus);
            PollTimeTF.LostFocus += new RoutedEventHandler(PollTimeLostFocus);
            FontDropdown.SelectionChanged += new SelectionChangedEventHandler(FontSelectionChanged);
            FadeButtonsCB.Click += new RoutedEventHandler(FadeButtonsChanged);
            ChangeButtonModeCB.Click += new RoutedEventHandler(ButtonnModeChanged);
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            InstallScriptBtn.Click += new RoutedEventHandler(InstallDCSScript);
            TimeAliveTF.LostFocus += new RoutedEventHandler(TimeAliveLostFocus);
        }
        void OpenColorPicker(object sender, EventArgs e)
        {
            ColorDialog dig = new ColorDialog();
            if (dig.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MainStructure.msave.ColorSCB = new SolidColorBrush(Color.FromArgb(dig.Color.A, dig.Color.R, dig.Color.G, dig.Color.B));
            }
        }

        void WidthLostFocus(object sender, EventArgs e)
        {
            if(!int.TryParse(WidthTF.Text, out MainStructure.msave.OvlW))
            {
                System.Windows.MessageBox.Show("Not a Valid integer for Width");
            }
            else
            {
                if (MainStructure.msave.OvlW < 1)
                {
                    MainStructure.msave.OvlW = MetaSave.default_OvlW;
                    WidthTF.Text = MetaSave.default_OvlW.ToString();
                    System.Windows.MessageBox.Show("Needs a positive integer for Width to Show");
                }
            }
        }

        void HeightLostFocus(object sender, EventArgs e)
        {
            if (!int.TryParse(HeighTF.Text, out MainStructure.msave.OvlH))
            {
                System.Windows.MessageBox.Show("Not a Valid integer for Height");
            }
            else
            {
                if (MainStructure.msave.OvlH < 1)
                {
                    MainStructure.msave.OvlH = MetaSave.default_OvlH;
                    HeighTF.Text = MetaSave.default_OvlH.ToString();
                    System.Windows.MessageBox.Show("Needs a positive integer for Height to Show");
                }
            }
        }

        void TextSizeLostFocus(object sender, EventArgs e)
        {
            if (!int.TryParse(TextSizeTF.Text, out MainStructure.msave.OvlTxtS))
            {
                System.Windows.MessageBox.Show("Not a Valid integer for Text Size");
            }
            else
            {
                if (MainStructure.msave.OvlTxtS < 1)
                {
                    MainStructure.msave.OvlTxtS = MetaSave.default_OvlTxtS;
                    TextSizeTF.Text = MetaSave.default_OvlTxtS.ToString();
                    System.Windows.MessageBox.Show("Needs a positive integer for Text Size to Show");
                }
            }
        }

        void ElementsToShowLostFocus(object sender, EventArgs e)
        {
            if (!int.TryParse(ElementsToShowTF.Text, out MainStructure.msave.OvlElementsToShow))
            {
                System.Windows.MessageBox.Show("Not a Valid integer for Elements to Show");
            }
            else
            {
                if (MainStructure.msave.OvlElementsToShow < 1)
                {
                    MainStructure.msave.OvlElementsToShow = MetaSave.default_OvlElementsToShow;
                    ElementsToShowTF.Text = MetaSave.default_OvlElementsToShow.ToString();
                    System.Windows.MessageBox.Show("Needs a positive integer for Elements to Show");
                }
            }
        }

        void PollTimeLostFocus(object sender, EventArgs e)
        {
            if (!int.TryParse(PollTimeTF.Text, out MainStructure.msave.OvlPollTime))
            {
                System.Windows.MessageBox.Show("Not a Valid integer for Poll Time to Show");
            }
            else
            {
                if (MainStructure.msave.OvlPollTime < 1)
                {
                    MainStructure.msave.OvlPollTime = MetaSave.default_OvlPollTime;
                    PollTimeTF.Text = MetaSave.default_OvlPollTime.ToString();
                    System.Windows.MessageBox.Show("Needs a positive integer for Poll Time to Show");
                }
            }
        }

        void TimeAliveLostFocus(object sender, EventArgs e)
        {
            if (!int.TryParse(TimeAliveTF.Text, out MainStructure.msave.TextTimeAlive))
            {
                System.Windows.MessageBox.Show("Not a Valid integer for Poll Time to Show");
            }
            else
            {
                if (MainStructure.msave.TextTimeAlive < 1)
                {
                    MainStructure.msave.TextTimeAlive = MetaSave.default_TextAlive;
                    TimeAliveTF.Text = MetaSave.default_TextAlive.ToString();
                    System.Windows.MessageBox.Show("Needs a positive integer for Text Alive to Show");
                }
            }
        }

        void FontSelectionChanged(object sender, EventArgs e)
        {
            MainStructure.msave.Font = (string)FontDropdown.SelectedItem;
        }

        void FadeButtonsChanged(object sender, EventArgs e)
        {
            if (FadeButtonsCB.IsChecked == true) MainStructure.msave.OvlFade = true;
            else MainStructure.msave.OvlFade = false;
        }

        void ButtonnModeChanged(object sender, EventArgs e)
        {
            if (ChangeButtonModeCB.IsChecked == true) MainStructure.msave.OvlBtnChangeMode = true;
            else MainStructure.msave.OvlBtnChangeMode = false;
            
        }

        void CloseThis(object sender, EventArgs e)
        {
            Close();
        }

        void InstallDCSScript(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(MainStructure.PROGPATH + "\\Tools\\DCSLuaSocket\\Addative.lua");
            string contentAddative = sr.ReadToEnd();
            sr.Close();
            sr.Dispose();
            string selectedPath;
            if (MainStructure.msave.DCSInstaceOverride != null && Directory.Exists(MainStructure.msave.DCSInstaceOverride))
            {
                selectedPath=MainStructure.msave.DCSInstaceOverride;
            }
            else
            {
                selectedPath = MiscGames.DCSselectedInstancePath;
            }
            string luaContent="";
            if(File.Exists(selectedPath+ "\\Scripts\\Export.lua"))
            {
                StreamReader srExp = new StreamReader(selectedPath + "\\Scripts\\Export.lua");
                luaContent = srExp.ReadToEnd();
                srExp.Close();
                srExp.Dispose();
            }
            if (!luaContent.Contains(contentAddative))
            {
                try
                {
                    luaContent = luaContent + "\r\n" + contentAddative;
                    StreamWriter sw = new StreamWriter(selectedPath + "\\Scripts\\Export.lua");
                    sw.WriteLine(luaContent);
                    sw.Close();
                    sw.Dispose();
                    System.Windows.MessageBox.Show("Script was installed successfully");
                }catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
                
            }
            else
            {
                System.Windows.MessageBox.Show("Script was already installed");
            }
        }
    }
}
