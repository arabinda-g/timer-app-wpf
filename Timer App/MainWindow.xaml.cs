using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Windows.Media;

namespace Timer_App
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            LoadWindowPosition();
            SetUpTimer();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            ApplyTheme();

            this.MouseDown += Window_MouseDown;
            this.Closed += Window_Closed;
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
            if (currentTime >= targetTime)
            {
                targetTime = currentTime < targetTime ? targetTime : targetTime.AddDays(1);
                this.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                this.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32)); // Default background for dark mode
            }

            var timeLeft = (currentTime >= targetTime ? currentTime - targetTime : targetTime - currentTime);
            timeText.Text = (currentTime >= targetTime ? "-" : "") + timeLeft.ToString(@"hh\:mm");
        }

        private void LoadWindowPosition()
        {
            // Logic to load window position from SettingsPath and set this.Top and this.Left

            // Load window position from settings
            this.Top = Properties.Settings.Default.WindowTop;
            this.Left = Properties.Settings.Default.WindowLeft;
        }

        private void SaveWindowPosition()
        {
            // Logic to save window position to SettingsPath
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
            var isDarkTheme = IsDarkThemeEnabled();

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
