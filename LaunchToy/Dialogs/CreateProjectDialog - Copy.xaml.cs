using LaunchToy.Impl;
using Microsoft.Win32;
using System.Windows;

namespace LaunchToy.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateProjectDialog.xaml
    /// </summary>
    public partial class CreateProjectDialog : Window
    {

        private CreateProjectDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
        }

        public static void OpenDialog(string defaultProjectName, string initialLocation)
        {
            new CreateProjectDialog()
            {
                Owner = Env.MainWindow
            }.InternalOpenDialog(defaultProjectName, initialLocation);
        }

        private void InternalOpenDialog(string defaultProjectName, string initialLocation)
        { 
            int cnt = 1;
            string _defaultProjectName = defaultProjectName;
            while (System.IO.Directory.Exists(System.IO.Path.Combine(initialLocation, _defaultProjectName)))
            {
                _defaultProjectName = defaultProjectName + cnt++;
            }

            this.locationTextBox.Text = initialLocation;
            this.projectNameTextBox.Text = _defaultProjectName;

            if (ShowDialog() == true)
            {
                if (!System.IO.Directory.Exists(this.locationTextBox.Text))
                {
                    MessageBox.Show("Selected project folder not found");
                    return;
                }
                
                Env.DefaultLocation = this.locationTextBox.Text;

                var projectPath = System.IO.Path.Combine(this.locationTextBox.Text, this.projectNameTextBox.Text);

                if (System.IO.Directory.Exists(projectPath))
                {
                    MessageBox.Show("Project already exists");
                    return;
                }

                System.IO.Directory.CreateDirectory(projectPath);

                Project.CreateNew(projectPath, this.projectNameTextBox.Text);
            }
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                InitialDirectory = this.locationTextBox.Text
            };

            if (folderDialog.ShowDialog() == true)
            {
                this.locationTextBox.Text = folderDialog.FolderName;
            }
        }
    }
}
