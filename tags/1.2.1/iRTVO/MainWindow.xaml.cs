﻿/*
 * MainWindow.xaml.cs
 * 
 * Functionality of the MainWindow (program controls)
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
using System.Windows.Navigation;
using System.Windows.Shapes;

// additional
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Interop;

namespace iRTVO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Create overlay
        Window overlayWindow = new Overlay();

        // Create options
        Window options;

        // Create controls
        Window controlsWindow = new Controls();

        // Create lists
        Window listsWindow;

        // statusbar update timer
        DispatcherTimer statusBarUpdateTimer = new DispatcherTimer();

        // custom buttons
        StackPanel[] userButtonsRow;
        Button[] buttons;

        // web update wait
        Int16 webUpdateWait = 0;

        public MainWindow()
        {
            InitializeComponent();
            // set window position
            this.Left = Properties.Settings.Default.MainWindowLocationX;
            this.Top = Properties.Settings.Default.MainWindowLocationY;
            this.Width = Properties.Settings.Default.MainWindowWidth;
            this.Height = Properties.Settings.Default.MainWindowHeight;

            if (Properties.Settings.Default.AoTmain == true)
                this.Topmost = true;
            else
                this.Topmost = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            overlayWindow.Show();
            controlsWindow.Show();

            // start statusbar
            statusBarUpdateTimer.Tick += new EventHandler(updateStatusBar);
            statusBarUpdateTimer.Tick += new EventHandler(updateButtons);
            statusBarUpdateTimer.Tick += new EventHandler(checkWebUpdate);
            statusBarUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            statusBarUpdateTimer.Start();
        }
        
        // no focus
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private void updateButtons(object sender, EventArgs e)
        {
            if (SharedData.refreshButtons == true)
            {
                //userButtonsRows.Children.Clear();
                buttonStackPanel.Children.RemoveRange(1, buttonStackPanel.Children.Count-1);

                int rowCount = 0;
                for (int i = 0; i < SharedData.theme.buttons.Length; i++)
                {
                    if (SharedData.theme.buttons[i].row > rowCount)
                        rowCount = SharedData.theme.buttons[i].row;
                }

                userButtonsRow = new StackPanel[rowCount + 1];

                for (int i = 0; i < userButtonsRow.Length; i++)
                {
                    userButtonsRow[i] = new StackPanel();
                    buttonStackPanel.Children.Add(userButtonsRow[i]);
                }

                //RoutedEventArgs args;

                buttons = new Button[SharedData.theme.buttons.Length];
                for (int i = 0; i < SharedData.theme.buttons.Length; i++)
                {
                    //args = i;
                    buttons[i] = new Button();
                    buttons[i].Content = SharedData.theme.buttons[i].text;
                    buttons[i].Click += new RoutedEventHandler(HandleClick);
                    buttons[i].Name = "customButton" + i.ToString();
                    buttons[i].Margin = new Thickness(3);
                    userButtonsRow[SharedData.theme.buttons[i].row].Children.Add(buttons[i]);
                }

                SharedData.refreshButtons = false;
            }
            
            int sessions = 0;

            for (int i = 0; i < SharedData.Sessions.SessionList.Count; i++)
            {
                if (SharedData.Sessions.SessionList[i].Type != Sessions.SessionInfo.sessionType.invalid)
                    sessions++;
            }

            ComboBoxItem cboxitem;
            string selected = null;

            if (comboBoxSession.HasItems)
            {
                cboxitem = (ComboBoxItem)comboBoxSession.SelectedItem;
                selected = cboxitem.Content.ToString();
            }

            if (comboBoxSession.Items.Count != (sessions + 1))
            {
                comboBoxSession.Items.Clear();
                cboxitem = new ComboBoxItem();
                cboxitem.Content = "current";
                comboBoxSession.Items.Add(cboxitem);

                for (int i = 0; i < SharedData.Sessions.SessionList.Count; i++)
                {
                    if (SharedData.Sessions.SessionList[i].Type != Sessions.SessionInfo.sessionType.invalid)
                    {
                        cboxitem = new ComboBoxItem();
                        cboxitem.Content = i.ToString() + ": " + SharedData.Sessions.SessionList[i].Type.ToString();
                        comboBoxSession.Items.Add(cboxitem);
                    }
                }

                if(selected != null)
                    comboBoxSession.Text = selected;
                else
                    comboBoxSession.Text = "current";
            }
        }

        void HandleClick(object sender, RoutedEventArgs e)
        {
            Button button = new Button();

            try
            {
                button = (Button)sender;
            }
            finally
            {
                int buttonId = Int32.Parse(button.Name.Substring(12));
                for (int i = 0; i < SharedData.theme.buttons[buttonId].actions.Length; i++)
                {
                    Theme.ButtonActions action = (Theme.ButtonActions)i;
                    if (SharedData.theme.buttons[buttonId].actions[i] != null)
                    {
                        if (ClickAction(action, SharedData.theme.buttons[buttonId].actions[i]))
                            ClickAction(Theme.ButtonActions.hide, SharedData.theme.buttons[buttonId].actions[i]);
                    }
                }
            }
        }

        private Boolean ClickAction(Theme.ButtonActions action, string[] objects)
        {
            for (int j = 0; j < objects.Length; j++)
            {
                string[] split = objects[j].Split('-');
                switch (split[0])
                {
                    case "Overlay": // overlays
                        for (int k = 0; k < SharedData.theme.objects.Length; k++)
                        {
                            if (SharedData.theme.objects[k].name == split[1])
                            {

                                if (SharedData.theme.objects[k].dataset == Theme.dataset.standing && action == Theme.ButtonActions.show)
                                {
                                    SharedData.theme.objects[k].page++;
                                }

                                if (SharedData.lastPage[k] == true && SharedData.theme.objects[k].dataset == Theme.dataset.standing)
                                {
                                    SharedData.theme.objects[k].visible = setObjectVisibility(SharedData.theme.objects[k].visible, Theme.ButtonActions.hide);
                                    SharedData.theme.objects[k].page = -1;
                                    SharedData.lastPage[k] = false;
                                    return true;
                                }
                                else
                                {
                                    SharedData.theme.objects[k].visible = setObjectVisibility(SharedData.theme.objects[k].visible, action);
                                }
                            }
                        }
                        break;
                    case "Image": // images
                        for (int k = 0; k < SharedData.theme.images.Length; k++)
                        {
                            if (SharedData.theme.images[k].name == split[1])
                            {
                                SharedData.theme.images[k].visible = setObjectVisibility(SharedData.theme.images[k].visible, action);
                            }
                        }
                        break;
                    case "Ticker": // tickers
                        for (int k = 0; k < SharedData.theme.tickers.Length; k++)
                        {
                            if (SharedData.theme.tickers[k].name == split[1])
                            {
                                SharedData.theme.tickers[k].visible = setObjectVisibility(SharedData.theme.tickers[k].visible, action);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

        private Boolean setObjectVisibility(Boolean currentValue, Theme.ButtonActions action)
        {
            if (action == Theme.ButtonActions.hide)
                return false;
            else if (action == Theme.ButtonActions.show)
                return true;
            else if (action == Theme.ButtonActions.toggle)
            {
                if (currentValue == true)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        private string formatBytes(Int64 bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = 0;
            for (i = 0; (int)(bytes / 1024) > 0; i++, bytes /= 1024)
                dblSByte = bytes / 1024.0;
            return String.Format("{0:0.00} {1}", dblSByte, Suffix[i]);
        }

        private void updateStatusBar(object sender, EventArgs e)
        {
            if(SharedData.apiConnected) 
            {
                statusBarState.Text = "Sim: Running";
            }
            else 
            {
                statusBarState.Text = "Sim: No connection";
            }

            int count = SharedData.overlayFPSstack.Count() * 1000;
            float totaltime = 0;
            foreach (float frametime in SharedData.overlayFPSstack)
                totaltime += frametime;
            double fps = Math.Round(count / totaltime);
            SharedData.overlayFPSstack.Clear();
            statusBarFps.Text = fps.ToString() + " fps";

            count = SharedData.overlayEffectiveFPSstack.Count() * 1000;
            totaltime = 0;
            foreach (float frametime in SharedData.overlayEffectiveFPSstack)
                totaltime += frametime;
            double eff_fps = Math.Round(count / totaltime);
            SharedData.overlayEffectiveFPSstack.Clear();

            statusBarFps.ToolTip = string.Format("fps: {0}, effective fps: {1}",  fps, eff_fps);

            if (Properties.Settings.Default.webTimingEnable &&
                (SharedData.Sessions.CurrentSession.State != Sessions.SessionInfo.sessionState.invalid) &&
                SharedData.runOverlay)
            {
                statusBarWebTiming.Text = "Web: enabled";

                Brush textColor = System.Windows.SystemColors.WindowTextBrush;
                /*
                for (int i = 0; i < SharedData.webUpdateWait.Length; i++)
                {
                    if(SharedData.webUpdateWait[i] == true) {
                        textColor = System.Windows.Media.Brushes.Green;
                        break;
                    }
                }
                */
                if(SharedData.webError.Length > 0)
                    textColor = System.Windows.Media.Brushes.Red;

                statusBarWebTiming.Foreground = textColor;
            }
            else
            {
                statusBarWebTiming.Text = "Web: disabled";
            }

            if (SharedData.webError.Length > 0)
                statusBarWebTiming.ToolTip = string.Format("Error: {0}", SharedData.webError); 
            else
                statusBarWebTiming.ToolTip = string.Format("Out: {0}", formatBytes(SharedData.webBytes));

            if (comboBoxSession.SelectedItem != null)
            {
                ComboBoxItem cbi = (ComboBoxItem)comboBoxSession.SelectedItem;
                if (cbi.Content.ToString() == "current")
                {
                    SharedData.overlaySession = SharedData.Sessions.CurrentSession.Id;
                }
            }
            
        }

        private void checkWebUpdate(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.webTimingEnable &&
                (SharedData.Sessions.CurrentSession.State != Sessions.SessionInfo.sessionState.invalid) &&
                SharedData.runOverlay &&
                webUpdateWait > Properties.Settings.Default.webTimingInterval)
            {
                ThreadPool.QueueUserWorkItem(SharedData.web.postData);
                webUpdateWait = 0;
            }
            else
            {
                webUpdateWait++;
            }
        }


        private void CloseProgram()
        {
            SharedData.runApi = false;
            overlayWindow.Close();
            Application.Current.Shutdown(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseProgram();
        }

        private void setReplay()
        {
            SharedData.replayInProgress = true;
        }

        private void hideButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < SharedData.theme.objects.Length; i++)
                SharedData.theme.objects[i].visible = false;

            for (int i = 0; i < SharedData.theme.images.Length; i++)
                SharedData.theme.images[i].visible = false;

            for (int i = 0; i < SharedData.theme.tickers.Length; i++)
                SharedData.theme.tickers[i].visible = false;

            for (int i = 0; i < SharedData.theme.videos.Length; i++)
                SharedData.theme.videos[i].visible = false;
        }

        private void Main_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainWindowLocationX = (int)this.Left;
            Properties.Settings.Default.MainWindowLocationY = (int)this.Top;
            Properties.Settings.Default.Save();
        }

        private void bOptions_Click(object sender, RoutedEventArgs e)
        {
            if (options == null || options.IsVisible == false)
            {
                options = new Options();
                options.Show();
            }
            options.Activate();
        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.MainWindowWidth = (int)this.Width;
            Properties.Settings.Default.MainWindowHeight = (int)this.Height;
            Properties.Settings.Default.Save();
        }

        private void comboBoxSession_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxSession.Items.Count > 0)
            {
                ComboBoxItem cbi = (ComboBoxItem)comboBoxSession.SelectedItem;
                if (cbi.Content.ToString() == "current")
                    SharedData.overlaySession = SharedData.Sessions.CurrentSession.Id;
                else
                {
                    string[] split = cbi.Content.ToString().Split(':');
                    SharedData.overlaySession = Int32.Parse(split[0]);
                }
            }
        }

        private void bAbout_Click(object sender, RoutedEventArgs e)
        {
            Window about = new about();
            about.Show();
        }

        private void controlsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!controlsWindow.IsVisible)
            {
                controlsWindow = new Controls();
                controlsWindow.Show();
            }
            controlsWindow.Activate();
        }

        private void listsButton_Click(object sender, RoutedEventArgs e)
        {
            if (listsWindow == null || !listsWindow.IsVisible)
            {
                listsWindow = new Lists();
                listsWindow.Show();
            }
            listsWindow.Activate();
        }
    }
}
