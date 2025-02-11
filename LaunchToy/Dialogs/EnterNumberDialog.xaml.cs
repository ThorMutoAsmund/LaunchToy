using LaunchToy.Impl;
using Microsoft.Win32;
using System.Windows;

namespace LaunchToy.Dialogs
{
    /// <summary>
    /// Interaction logic for EnterNumberDialog.xaml
    /// </summary>
    public partial class EnterNumberDialog : Window
    {
        private int validatedValue;
        private int? minValule = null;
        private int? maxValule = null;

        private EnterNumberDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
        }

        public static bool OpenDialog(string dialogTitle, string label, out int enteredValue, int defaultValue = 0, int? minValue = null, int? maxValue = null)
        {
            return new EnterNumberDialog()
            {
                Title = dialogTitle,
                Owner = Env.MainWindow
            }.InternalOpenDialog(label, out enteredValue, defaultValue, minValue, maxValue);
        }

        private bool InternalOpenDialog(string label, out int enteredValue, int defaultValue = 0, int? minValue = null, int? maxValue = null)
        {
            this.stringLabel.Content = label;
            this.titleLabel.Content = this.Title;
            this.valueTextBox.Text = defaultValue.ToString();

            this.validatedValue = defaultValue;
            this.minValule = minValue;
            this.maxValule = maxValue;

            if (ShowDialog() == true)
            {
                enteredValue = this.validatedValue;
                return true;
            }

            enteredValue = defaultValue;

            return false;
        }

        private void valueTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var isValueOk = Int32.TryParse(this.valueTextBox.Text, out this.validatedValue);
            this.okButton.IsEnabled = isValueOk && 
                (this.minValule == null || this.validatedValue >= this.minValule.Value) &&
                (this.maxValule == null || this.validatedValue <= this.maxValule.Value);
        }
    }
}
