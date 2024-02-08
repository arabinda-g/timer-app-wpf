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
        private const string SettingsPath = "windowPosition.json";

        public MainWindow()
        {
            InitializeComponent();

            LoadWindowPosition();
            SetUpTimer();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            ApplyTheme();
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
            if (currentTime > targetTime)
            {
                targetTime = targetTime.AddDays(1);
                this.Background = Brushes.Red;
            }
            else
            {
                this.Background = Brushes.Black; // Default background
            }

            var timeLeft = targetTime - currentTime;
            timeText.Text = (currentTime > targetTime ? "-" : "") + timeLeft.ToString(@"hh\:mm");
        }

        private void LoadWindowPosition()
        {
            // Logic to load window position from SettingsPath and set this.Top and this.Left
        }

        private void SaveWindowPosition()
        {
            // Logic to save window position to SettingsPath
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
