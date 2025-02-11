using LaunchToy.Impl;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;


namespace LaunchToy.UserControls
{
    /// <summary>
    /// Interaction logic for SampleListUserControl.xaml
    /// </summary>
    public partial class SampleListUserControl : UserControl
    {
        private Point dragStartPoint;

        public SampleListUserControl()
        {
            InitializeComponent();

            Env.ProjectChanged += Env_ProjectChanged;
            Env.SamplesChanged += Env_SamplesChanged;
        }

        private void Env_ProjectChanged(ProjectChangedAction action)
        {
            if (action == ProjectChangedAction.Closed)
            {
                this.listView.Items.Clear();
            }
            else if (action == ProjectChangedAction.Opened)
            {
                Env_SamplesChanged();
            }
        }

        private void Env_SamplesChanged()
        {
            this.listView.Items.Clear();

            if (Env.Project != null)
            {
                foreach (var sample in Env.Project!.Samples)
                {
                    this.listView.Items.Add(sample);
                }
            }
        }

        private void listView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.dragStartPoint = e.GetPosition(null);
        }

        private void listView_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && this.listView.SelectedItem != null)
            {
                // Get the current mouse position
                var mousePos = e.GetPosition(null);
                var diff = this.dragStartPoint - mousePos;

                var sample = this.listView.SelectedItem as Sample;
                if (sample !=  null)
                {
                    this.listView.CheckDragDrop(diff, sample, DragDropKey.Sample);
                }
            }
        }

        private void deleteSamplesMenu_Click(object sender, RoutedEventArgs e)
        {
            var numberOfFilesToDelete = this.listView.SelectedItems.Count;
            if (MessageBox.Show($"Are you sure you want to delete these {numberOfFilesToDelete} samples(s)?", "Changes Made", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                return;
            }

            var samplesToRemove = new List<Sample>();
            foreach (var item in this.listView.SelectedItems)
            {
                if (item is Sample sample)
                {
                    samplesToRemove.Add(sample);
                }
            }

            Project.DeleteSamples(samplesToRemove);
        }
    }
}
