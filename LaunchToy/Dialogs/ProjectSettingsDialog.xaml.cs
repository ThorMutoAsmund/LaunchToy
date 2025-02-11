using LaunchToy.Impl;
using Microsoft.Win32;
using System.Windows;

namespace LaunchToy.Dialogs
{
    /// <summary>
    /// Interaction logic for ProjectSettingsDialog.xaml
    /// </summary>
    public partial class ProjectSettingsDialog : Window
    {
        private int validatedBPM;
        private int validatedBeatsPerBar;
        private ProjectSettingsDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
        }

        public static void OpenDialog()
        {
            if (Env.Project == null)
            {
                return;
            }

            new ProjectSettingsDialog()
            {
                Owner = Env.MainWindow
            }.InternalOpenDialog(Env.Project);
        }

        private void InternalOpenDialog(Project project)
        { 
            this.projectNameTextBox.Text = project.Title;
            this.bpmTextBox.Text = project.BPM.ToString();
            this.beatsPerBarTextBox.Text = project.BeatsPerBar.ToString();
            this.validatedBPM = project.BPM;
            if (ShowDialog() == true)
            {
                project.BPM = this.validatedBPM;
                project.BeatsPerBar = this.validatedBeatsPerBar;
                
                Env.OnDirtyChanged(true);
            }
        }

        private void numberTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var bpmOK = Int32.TryParse(this.bpmTextBox.Text, out this.validatedBPM);
            var beatsPerBarOK = Int32.TryParse(this.beatsPerBarTextBox.Text, out this.validatedBeatsPerBar);
            
            this.okButton.IsEnabled = bpmOK && beatsPerBarOK &&
                this.validatedBPM >= Constants.MinBPM &&
                this.validatedBPM <= Constants.MaxBPM &&
                this.validatedBeatsPerBar >= Constants.MinBeatsPerBar &&
                this.validatedBeatsPerBar <= Constants.MaxBeatsPerBar;
        }
    }
}
