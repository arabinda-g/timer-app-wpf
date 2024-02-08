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
        private bool isDragging = false;
        private Point startPoint;


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
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                ApplyTheme();
            }
        }
        private void OnBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                startPoint = e.GetPosition(this);
            }
        }

        private void OnBorderMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point mousePosition = e.GetPosition(this);
                double offsetX = mousePosition.X - startPoint.X;
                double offsetY = mousePosition.Y - startPoint.Y;

                this.Left += offsetX;
                this.Top += offsetY;

                startPoint = mousePosition;
            }
        }

        private void OnBorderMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
            }
        }
    }
}
