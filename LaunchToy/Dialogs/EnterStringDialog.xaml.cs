using LaunchToy.Impl;
using Microsoft.Win32;
using System.Windows;

namespace LaunchToy.Dialogs
{
    /// <summary>
    /// Interaction logic for EnterStringDialog.xaml
    /// </summary>
    public partial class EnterStringDialog : Window
    {
        private bool allowEmpty;
        private EnterStringDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
        }

        public static bool OpenDialog(string dialogTitle, string label, out string enteredString, string defaultString = "", bool allowEmpty = false)
        {
            return new EnterStringDialog()
            {
                Title = dialogTitle,
                Owner = Env.MainWindow
            }.InternalOpenDialog(label, out enteredString, defaultString, allowEmpty);
        }

        private bool InternalOpenDialog(string label, out string enteredString, string defaultString = "", bool allowEmpty = false)
        {
            this.allowEmpty = allowEmpty;
            this.stringLabel.Content = label;
            this.stringTextBox.Text = defaultString;

            this.okButton.IsEnabled = this.allowEmpty;

            if (ShowDialog() == true)
            {
                enteredString = this.stringTextBox.Text;
                return true;
            }

            enteredString = defaultString;

            return false;
        }

        private void stringTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!this.allowEmpty)
            {
                this.okButton.IsEnabled = !String.IsNullOrEmpty(this.stringTextBox.Text);
            }
        }
    }
}
