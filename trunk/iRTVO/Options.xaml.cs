﻿/*
 * Options.xaml.cs
 * 
 * Functionality of the options window.
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
// additional
using System.IO;
using Ini;

namespace iRTVO
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            this.Left = Properties.Settings.Default.OptionsLocationX;
            this.Top = Properties.Settings.Default.OptionsLocationY;
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            apply();
        }


        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            apply();
            this.Close();
        }

        private void apply()
        {

            saveOverlaySize();
            saveOverlayPos();
            
            ComboBoxItem cbi = (ComboBoxItem)comboBoxTheme.SelectedItem;
            Properties.Settings.Default.theme = cbi.Content.ToString();

            try
            {
                if (Int32.Parse(textBoxUpdateFreq.Text) > 0)
                    Properties.Settings.Default.countdownThreshold = Int32.Parse(textBoxCountdownTh.Text);
                else
                    MessageBox.Show("Countdown threshold needs to be larger than zero");
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Countdown threshold needs to be larger than zero");
            }

            try
            {
                if (Int32.Parse(textBoxUpdateFreq.Text) > 0)
                    Properties.Settings.Default.UpdateFrequency = Int32.Parse(textBoxUpdateFreq.Text);
                else
                    MessageBox.Show("Update frequency needs to be larger than zero");
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Update frequency needs to be larger than zero");
            }

            if (checkBoxShowBorders.IsChecked == true)
                Properties.Settings.Default.ShowBorders = true;
            else
                Properties.Settings.Default.ShowBorders = false;

            Properties.Settings.Default.webTimingUrl = textBoxWebTimingUrl.Text;
            Properties.Settings.Default.webTimingKey = textBoxWebTimingKey.Text;

            if (checkBoxWebTimingEnable.IsChecked == true)
                Properties.Settings.Default.webTimingEnable = true;
            else
                Properties.Settings.Default.webTimingEnable = false;

            try
            {
                if (Int32.Parse(textBoxWebTimingInterval.Text) > 0)
                    Properties.Settings.Default.webTimingInterval = Int32.Parse(textBoxWebTimingInterval.Text);
                else
                    MessageBox.Show("Web timing update interval needs to be larger than zero");
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Web timing update interval needs to be larger than zero");
            }

            if (checkBoxAOTMain.IsChecked == true)
                Properties.Settings.Default.AoTmain = true;
            else
                Properties.Settings.Default.AoTmain = false;

            if (checkBoxAOTControls.IsChecked == true)
                Properties.Settings.Default.AoTcontrols = true;
            else
                Properties.Settings.Default.AoTcontrols = false;

            if (checkBoxAOTLists.IsChecked == true)
                Properties.Settings.Default.AoTlists = true;
            else
                Properties.Settings.Default.AoTlists = false;

            try
            {
                if (Int32.Parse(textBoxRemoteServerPort.Text) > 0)
                    Properties.Settings.Default.remoteServerPort = Int32.Parse(textBoxRemoteServerPort.Text);
                else
                    MessageBox.Show("Remote server port must be an integer larger than zero");
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Remote server port must be an integer larger than zero");
            }

            try
            {
                if (Int32.Parse(textBoxRemoteClientPort.Text) > 0)
                    Properties.Settings.Default.remoteClientPort = Int32.Parse(textBoxRemoteClientPort.Text);
                else
                    MessageBox.Show("Remote client port must be an integer larger than zero");
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Remote client port must be an integer larger than zero");
            }

            Properties.Settings.Default.remoteClientIp = textBoxRemoteClientIp.Text;
            Properties.Settings.Default.remoteClientKey = textBoxRemoteClientKey.Text;
            Properties.Settings.Default.remoteServerKey = textBoxRemoteServerKey.Text;

            if (checkBoxRemoteClientAutoStart.IsChecked == true)
                Properties.Settings.Default.remoteClientAutostart = true;
            else
                Properties.Settings.Default.remoteClientAutostart = false;

            if (checkBoxRemoteServerAutoStart.IsChecked == true)
                Properties.Settings.Default.remoteServerAutostart = true;
            else
                Properties.Settings.Default.remoteServerAutostart = false;

            if (checkBoxEnableSC.IsChecked == true)
                Properties.Settings.Default.DriverListIncSC = true;
            else
                Properties.Settings.Default.DriverListIncSC = false;

            if (radioButtonSortName.IsChecked == true)
            {
                Properties.Settings.Default.DriverListSortName = true;
                Properties.Settings.Default.DriverListSortNumber = false;
            }
            else if (radioButtonSortNumber.IsChecked == true) 
            {
                Properties.Settings.Default.DriverListSortName = false;
                Properties.Settings.Default.DriverListSortNumber = true;
            }

            // save
            Properties.Settings.Default.Save();
            SharedData.refreshTheme = true;
        }

        private void saveOverlaySize()
        {
            int w = -1;
            int h = -1;
            try
            {
                w = Int32.Parse(textBoxSizeW.Text);
                h = Int32.Parse(textBoxSizeH.Text);
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Overlay size must be a number");
            }

            Properties.Settings.Default.OverlayWidth = w;
            Properties.Settings.Default.OverlayHeight = h;

        }

        private void saveOverlayPos()
        {
            int w = -1;
            int h = -1;
            try
            {
                w = Int32.Parse(textBoxPosX.Text);
                h = Int32.Parse(textBoxPosY.Text);
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Overlay position must be a number");
            }

            Properties.Settings.Default.OverlayLocationX = w;
            Properties.Settings.Default.OverlayLocationY = h;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxPosX.Text = Properties.Settings.Default.OverlayLocationX.ToString();
            textBoxPosY.Text = Properties.Settings.Default.OverlayLocationY.ToString();
            textBoxSizeW.Text = Properties.Settings.Default.OverlayWidth.ToString();
            textBoxSizeH.Text = Properties.Settings.Default.OverlayHeight.ToString();

            // get available themes
            IniFile settings;
            ComboBoxItem cboxitem;

            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\themes\\");
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) {
                if (File.Exists(Directory.GetCurrentDirectory() + "\\themes\\" + di.Name + "\\settings.ini"))
                {
                    settings = new IniFile(Directory.GetCurrentDirectory() + "\\themes\\" + di.Name + "\\settings.ini");
                    cboxitem = new ComboBoxItem();
                    cboxitem.Content = settings.IniReadValue("General", "name");
                    comboBoxTheme.Items.Add(cboxitem);
                }
            }

            comboBoxTheme.Text = Properties.Settings.Default.theme;
            settings = new IniFile(Directory.GetCurrentDirectory() + "\\themes\\" + Properties.Settings.Default.theme + "\\settings.ini");
            labelThemeAuthor.Content = "Author: " + settings.IniReadValue("General", "author");
            labelThemeSize.Content = "Original size: " + settings.IniReadValue("General", "width") + "x" + settings.IniReadValue("General", "height");

            // set countdown threshold
            textBoxCountdownTh.Text = Properties.Settings.Default.countdownThreshold.ToString();

            textBoxUpdateFreq.Text = Properties.Settings.Default.UpdateFrequency.ToString();

            if (Properties.Settings.Default.ShowBorders)
                checkBoxShowBorders.IsChecked = true;
            else
                checkBoxShowBorders.IsChecked = false;

            textBoxWebTimingUrl.Text = Properties.Settings.Default.webTimingUrl;
            textBoxWebTimingKey.Text = Properties.Settings.Default.webTimingKey;
            if (Properties.Settings.Default.webTimingEnable)
                checkBoxWebTimingEnable.IsChecked = true;
            else
                checkBoxWebTimingEnable.IsChecked = false;
            textBoxWebTimingInterval.Text = Properties.Settings.Default.webTimingInterval.ToString();

            if (Properties.Settings.Default.AoTmain)
                checkBoxAOTMain.IsChecked = true;
            else
                checkBoxAOTMain.IsChecked = false;

            if (Properties.Settings.Default.AoTcontrols)
                checkBoxAOTControls.IsChecked = true;
            else
                checkBoxAOTControls.IsChecked = false;

            if (Properties.Settings.Default.AoTlists)
                checkBoxAOTLists.IsChecked = true;
            else
                checkBoxAOTLists.IsChecked = false;

            // auto generate server key
            if (Properties.Settings.Default.remoteServerKey.Length < 1)
            {
                Random rnd = new Random();
                Properties.Settings.Default.remoteServerKey = rnd.Next(10000000, 99999999).ToString();
            }

            textBoxRemoteClientIp.Text = Properties.Settings.Default.remoteClientIp.ToString();
            textBoxRemoteClientKey.Text = Properties.Settings.Default.remoteClientKey.ToString();
            textBoxRemoteClientPort.Text = Properties.Settings.Default.remoteClientPort.ToString();

            textBoxRemoteServerKey.Text = Properties.Settings.Default.remoteServerKey.ToString();
            textBoxRemoteServerPort.Text = Properties.Settings.Default.remoteServerPort.ToString();

            if (Properties.Settings.Default.remoteClientAutostart)
                checkBoxRemoteClientAutoStart.IsChecked = true;
            else
                checkBoxRemoteClientAutoStart.IsChecked = false;

            if (Properties.Settings.Default.remoteServerAutostart)
                checkBoxRemoteServerAutoStart.IsChecked = true;
            else
                checkBoxRemoteServerAutoStart.IsChecked = false;

            if (Properties.Settings.Default.DriverListIncSC == true)
                checkBoxEnableSC.IsChecked = true;

            if (Properties.Settings.Default.DriverListSortName == true)
                radioButtonSortName.IsChecked = true;
            else if (Properties.Settings.Default.DriverListSortNumber == true)
                radioButtonSortNumber.IsChecked = true;

        }

        private void comboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IniFile settings;

            ComboBoxItem cbi = (ComboBoxItem)comboBoxTheme.SelectedItem;
            settings = new IniFile(Directory.GetCurrentDirectory() + "\\themes\\" + cbi.Content.ToString() + "\\settings.ini");
            labelThemeAuthor.Content = "Author: " + settings.IniReadValue("General", "author");
            labelThemeSize.Content = "Original size: " + settings.IniReadValue("General", "width") + "x" + settings.IniReadValue("General", "height");
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.OptionsLocationX = (int)this.Left;
            Properties.Settings.Default.OptionsLocationY = (int)this.Top;
            Properties.Settings.Default.Save();
        }

    }
}
