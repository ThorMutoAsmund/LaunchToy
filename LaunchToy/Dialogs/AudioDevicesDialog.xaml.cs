using LaunchToy.Impl;
using Microsoft.Win32;
using NAudio.Midi;
using System.Windows;

namespace LaunchToy.Dialogs
{
    /// <summary>
    /// Interaction logic for AudioDevicesDialog.xaml
    /// </summary>
    public partial class AudioDevicesDialog : Window
    {
        private AudioDevicesDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
        }

        public static void OpenDialog()
        {
            new AudioDevicesDialog()
            {
                Owner = Env.MainWindow
            }.InternalOpenDialog();
        }

        private void InternalOpenDialog()
        { 
            this.asioOutputsListBox.Items.Clear();
            var selectedItem = -1;
            var idx = 0;
            foreach (var device in AudioConfig.GetOutputDevices())
            {
                this.asioOutputsListBox.Items.Add(new ListViewItemContainer<string>(device, device));
                if (Env.ASIOOutDeviceName == device)
                {
                    selectedItem = idx;
                }
                idx++;
            }

            this.asioOutputsListBox.SelectedIndex = selectedItem;

            if (ShowDialog() == true)
            {
                if (this.asioOutputsListBox.SelectedItem is ListViewItemContainer<string> outputContainer)
                {
                    Env.ASIOOutDeviceName.Set(outputContainer.Value);
                }
            }
        }
    }
}
