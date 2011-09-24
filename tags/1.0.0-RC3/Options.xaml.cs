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

            try
            {
                if (Int32.Parse(textBoxTickerSpeed.Text) > 0)
                    Properties.Settings.Default.TickerSpeed = Int32.Parse(textBoxTickerSpeed.Text);
                else
                    MessageBox.Show("Ticker speed needs to be larger than zero");
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Ticker speed needs to be larger than zero");
            }

            if (checkBoxShowBorders.IsChecked == true)
                Properties.Settings.Default.ShowBorders = true;
            else
                Properties.Settings.Default.ShowBorders = false;

            if (radioButtonSpeedKph.IsChecked == true)
                Properties.Settings.Default.speedUnit = 0;
            else
                Properties.Settings.Default.speedUnit = 1;

            // save
            Properties.Settings.Default.Save();
            SharedData.requestRefresh = true;
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
                MessageBox.Show("Overlay size needs to be larger than one");
            }

            if (w >= 0 && h >= 0)
            {

                Properties.Settings.Default.OverlayWidth = w;
                Properties.Settings.Default.OverlayHeight = h;
            }
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
                MessageBox.Show("Overlay position needs to be larger than one");
            }

            if (w >= 0 && h >= 0)
            {

                Properties.Settings.Default.OverlayLocationX = w;
                Properties.Settings.Default.OverlayLocationY = h;
            }
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

            textBoxTickerSpeed.Text = Properties.Settings.Default.TickerSpeed.ToString();

            if (Properties.Settings.Default.ShowBorders)
                checkBoxShowBorders.IsChecked = true;
            else
                checkBoxShowBorders.IsChecked = false;

            if (Properties.Settings.Default.speedUnit == 0)
                radioButtonSpeedKph.IsChecked = true;
            else
                radioButtonSpeedMph.IsChecked = true;
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