using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Timer_App
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public string InputValue { get; private set; }

        public InputDialog(string defaultValue = "")
        {
            InitializeComponent();

            // Set the TextBox text to the default value
            txtInput.Text = defaultValue;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtInput.Focus();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // Set the input value and close the window
            InputValue = txtInput.Text;
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Close the window without setting the input value
            DialogResult = false;
        }
    }
}
