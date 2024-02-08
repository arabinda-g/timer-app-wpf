using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Forms;
using Application = System.Windows.Application;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace Timer_App
{
    public partial class MainWindow : Window
    {
        // Define MAX_FONT_SIZE and MIN_FONT_SIZE here
        private const int MAX_FONT_SIZE = 100;
        private const int MIN_FONT_SIZE = 10;

        private DispatcherTimer timer;
        private NotifyIcon trayIcon;

        private bool isDarkTheme = false;
        private bool isTimeUp = false;
        private bool previousFlag = false;
        private System.TimeSpan defaultTargetTime;

        public MainWindow()
        {
            InitializeComponent();

            // Convert icon from resources and set it for the MainWindow
            this.Icon = IconToImageSource(Properties.Resources.HourglassIcon);
            defaultTargetTime = Properties.Settings.Default.TargetTime;

            LoadFontSize();
            InitializeTrayIcon();

            LoadWindowPosition();
            SetUpTimer();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            ApplyTheme();

            this.MouseDown += Window_MouseDown;
            this.Closed += Window_Closed;
        }

        private ImageSource IconToImageSource(Icon icon)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Save the icon to a memory stream in PNG format
                icon.Save(stream);

                // Create a BitmapDecoder from the memory stream
                BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                // Return the first frame of the decoded image as an ImageSource
                return decoder.Frames[0];
            }
        }

        private void LoadFontSize()
        {
            // Loading font size
            double fontSize = Properties.Settings.Default.FontSize;
            AdjustFontSizeAndWindowSize(fontSize);
        }

        private void AdjustFontSizeAndWindowSize(double fontSize)
        {
            // Set the font size
            timeText.FontSize = fontSize;

            // Calculate margin and padding in percentage
            timeText.Margin = new Thickness(0, -0.1 * fontSize, 0, 0);
            MainBorder.Padding = new Thickness(0, 0.01 * fontSize, 0, 0);

            // Adjust window size based on font size
            //this.Width = fontSize * 3;
            this.Height = fontSize;

            // Recalculate the window size based on the time
            if (isTimeUp)
            {
                this.Width = fontSize * 3;
            }
            else
            {
                this.Width = fontSize * 2.6;
            }

            // Save the font size to settings
            Properties.Settings.Default.FontSize = fontSize;
            Properties.Settings.Default.Save();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                var currentSize = timeText.FontSize;
                if (e.Delta > 0 && currentSize < MAX_FONT_SIZE)
                {
                    AdjustFontSizeAndWindowSize(currentSize + 1); // Increment font size
                }
                else if (e.Delta < 0 && currentSize > MIN_FONT_SIZE)
                {
                    AdjustFontSizeAndWindowSize(currentSize - 1); // Decrement font size
                }
            }
        }



        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                // Load icon from resources
                Icon = Properties.Resources.HourglassIcon,

                Visible = true
            };

            var contextMenu = new ContextMenuStrip();

            // Add options to change the theme
            contextMenu.Items.Add("Light theme", null, (sender, e) =>
            {
                isDarkTheme = false;
                ApplyTheme();

                // Save the theme to settings
                Properties.Settings.Default.Theme = "Light";
                Properties.Settings.Default.Save();

                // Set checkmark for the light theme
                ((ToolStripMenuItem)contextMenu.Items[0]).Checked = true;
                ((ToolStripMenuItem)contextMenu.Items[1]).Checked = false;
                ((ToolStripMenuItem)contextMenu.Items[2]).Checked = false;
            });
            contextMenu.Items.Add("Dark theme", null, (sender, e) =>
            {
                isDarkTheme = true;
                ApplyTheme();

                // Save the theme to settings
                Properties.Settings.Default.Theme = "Dark";
                Properties.Settings.Default.Save();

                // Set checkmark for the dark theme
                ((ToolStripMenuItem)contextMenu.Items[0]).Checked = false;
                ((ToolStripMenuItem)contextMenu.Items[1]).Checked = true;
                ((ToolStripMenuItem)contextMenu.Items[2]).Checked = false;
            });

            // Add option for system theme
            contextMenu.Items.Add("System theme", null, (sender, e) =>
            {
                isDarkTheme = IsDarkThemeEnabled();
                ApplyTheme();

                // Save the theme to settings
                Properties.Settings.Default.Theme = "System";
                Properties.Settings.Default.Save();

                // Set checkmark for the system theme
                ((ToolStripMenuItem)contextMenu.Items[0]).Checked = false;
                ((ToolStripMenuItem)contextMenu.Items[1]).Checked = false;
                ((ToolStripMenuItem)contextMenu.Items[2]).Checked = true;
            });

            // Add checkmark to the current theme
            if (Properties.Settings.Default.Theme == "Light")
            {
                ((ToolStripMenuItem)contextMenu.Items[0]).Checked = true;
            }
            else if (Properties.Settings.Default.Theme == "Dark")
            {
                ((ToolStripMenuItem)contextMenu.Items[1]).Checked = true;
            }
            else
            {
                ((ToolStripMenuItem)contextMenu.Items[2]).Checked = true;
            }

            // Add a separator
            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Change target time...", null, mnuChangeTargetTime_Click);
            contextMenu.Items.Add("Reset window position", null, ResetWindowPosition);
            contextMenu.Items.Add("Exit", null, ExitApplication);

            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.MouseClick += TrayIcon_MouseClick;
        }

        private void ResetWindowPosition(object sender, EventArgs e)
        {
            // Logic to reset window position
            this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2 + SystemParameters.WorkArea.Left;
            this.Top = (SystemParameters.WorkArea.Height - this.Height) / 2 + SystemParameters.WorkArea.Top;
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Current.Shutdown();
        }

        private void mnuChangeTargetTime_Click(object sender, EventArgs e)
        {
            var inputDialog = new InputDialog(defaultTargetTime.ToString(@"hh\:mm"));
            inputDialog.Icon = this.Icon;
            if (inputDialog.ShowDialog() == true)
            {
                // InputValue format: "00:00"
                var timeParts = inputDialog.InputValue.Split(':');

                // Convert TargetTime (System.TimeSpan)
                if (timeParts.Length == 2 && int.TryParse(timeParts[0], out int hours) && int.TryParse(timeParts[1], out int minutes))
                {
                    defaultTargetTime = new TimeSpan(hours, minutes, 0);
                    Properties.Settings.Default.TargetTime = new TimeSpan(hours, minutes, 0);
                    Properties.Settings.Default.Save();

                    // Restart the timer
                    timer.Stop();
                    SetUpTimer();
                }
                else
                {
                    System.Windows.MessageBox.Show("Invalid time format. Please use HH:MM format.", "Invalid Time", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Show the input dialog again
                    mnuChangeTargetTime_Click(sender, e);
                }
            }
        }

        private void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                ExitApplication(sender, e);
            }
        }





        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Middle)
            {
                ExitApplication(this, e);
            }
        }





        private void SetUpTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            var targetTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, defaultTargetTime.Hours, defaultTargetTime.Minutes, 0);

            if (currentTime.Hour >= defaultTargetTime.Hours)
            {
                // If the current time is after 10 PM, adjust the target time to today's date.
                isTimeUp = true;
                MainBorder.Background = new SolidColorBrush(Colors.Red);
                timeText.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                // Before 10 PM
                isTimeUp = false;
                if (isDarkTheme)
                {
                    MainBorder.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32)); // Dark theme background
                    timeText.Foreground = new SolidColorBrush(Colors.White); // Light text for dark background
                }
                else
                {
                    MainBorder.Background = new SolidColorBrush(Colors.WhiteSmoke); // Light theme background
                    timeText.Foreground = new SolidColorBrush(Colors.Black); // Dark text for light background
                }
            }

            TimeSpan timeSpan;
            string prefix;
            if (isTimeUp)
            {
                // Calculate the time since 10 PM
                timeSpan = currentTime - targetTime;
                prefix = "-";
            }
            else
            {
                // Calculate the time until 10 PM
                timeSpan = targetTime - currentTime;
                prefix = "";
            }

            // Ensure days are considered for the calculation if needed
            int totalMinutes = (int)timeSpan.TotalMinutes;
            int hours = totalMinutes / 60;
            int minutes = Math.Abs(totalMinutes % 60); // Use absolute value to avoid negative minutes

            timeText.Text = $"{prefix}{hours:D2}:{minutes:D2}";

            // If the time is up, resize the window
            if (previousFlag != isTimeUp)
            {
                previousFlag = isTimeUp;
                LoadFontSize();
            }
        }


        private void LoadWindowPosition()
        {
            // Check if it's the first time the app is running
            if (Properties.Settings.Default.WindowTop == 0 && Properties.Settings.Default.WindowLeft == 0)
            {
                // Center the window on the screen
                this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2 + SystemParameters.WorkArea.Left;
                this.Top = (SystemParameters.WorkArea.Height - this.Height) / 2 + SystemParameters.WorkArea.Top;
                return;
            }

            // Load window position from settings
            this.Top = Properties.Settings.Default.WindowTop;
            this.Left = Properties.Settings.Default.WindowLeft;
        }

        private void SaveWindowPosition()
        {
            // Save window position to settings
            Properties.Settings.Default.WindowTop = this.Top;
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.Save();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveWindowPosition();
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }

        private void ApplyTheme()
        {
            if (isTimeUp)
            {
                return;
            }

            if (isDarkTheme)
            {
                MainBorder.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32)); // Dark theme background
                timeText.Foreground = new SolidColorBrush(Colors.White); // Light text for dark background
            }
            else
            {
                MainBorder.Background = new SolidColorBrush(Colors.WhiteSmoke); // Light theme background
                timeText.Foreground = new SolidColorBrush(Colors.Black); // Dark text for light background
            }
        }

        private static bool IsDarkThemeEnabled()
        {
            const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string RegistryValueName = "AppsUseLightTheme";

            using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return false; // Default to light theme if the registry key is not found
                }

                int registryValue = (int)registryValueObject;
                return registryValue <= 0; // If the value is 0, dark theme is enabled. Otherwise, light theme is enabled.
            }
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                if (Properties.Settings.Default.Theme == "System")
                {
                    // Logic to check Windows theme and apply it to the app
                    isDarkTheme = IsDarkThemeEnabled();

                    ApplyTheme();
                }
            }
        }
    }
}
