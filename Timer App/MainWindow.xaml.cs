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

        public MainWindow()
        {
            InitializeComponent();

            // Convert icon from resources and set it for the MainWindow
            this.Icon = IconToImageSource(Properties.Resources.HourglassIcon);

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
            // Example loading font size (You'll need to implement the actual loading logic)
            double fontSize = Properties.Settings.Default.FontSize;
            AdjustFontSizeAndWindowSize(fontSize);
        }

        private void AdjustFontSizeAndWindowSize(double fontSize)
        {
            // Set the font size
            timeText.FontSize = fontSize;

            // Adjust window size based on font size
            this.Width = fontSize * 4; // Example calculation, adjust as necessary
            this.Height = fontSize * 2; // Example calculation, adjust as necessary

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
                if (e.Delta > 0 && currentSize < MAX_FONT_SIZE) // Define MAX_FONT_SIZE
                {
                    AdjustFontSizeAndWindowSize(currentSize + 1); // Increment font size
                }
                else if (e.Delta < 0 && currentSize > MIN_FONT_SIZE) // Define MIN_FONT_SIZE
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
            var targetTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 22, 0, 0);

            if (currentTime.Hour >= 22)
            {
                // If the current time is after 10 PM, adjust the target time to today's date.
                isTimeUp = true;
                this.Background = new SolidColorBrush(Colors.Red);
                timeText.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                // Before 10 PM
                isTimeUp = false;
                if (isDarkTheme)
                {
                    this.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32)); // Dark theme background
                    timeText.Foreground = new SolidColorBrush(Colors.White); // Light text for dark background
                }
                else
                {
                    this.Background = new SolidColorBrush(Colors.WhiteSmoke); // Light theme background
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
        }


        private void LoadWindowPosition()
        {
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
            // Logic to check Windows theme and apply it to the app
            isDarkTheme = IsDarkThemeEnabled();

            if (isTimeUp)
            {
                return;
            }

            if (isDarkTheme)
            {
                this.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32)); // Dark theme background
                timeText.Foreground = new SolidColorBrush(Colors.White); // Light text for dark background
            }
            else
            {
                this.Background = new SolidColorBrush(Colors.WhiteSmoke); // Light theme background
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
                ApplyTheme();
            }
        }
    }
}
