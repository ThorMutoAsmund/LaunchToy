using LaunchToy.Impl;
using Microsoft.Win32;
using NAudio.Midi;
using System.Windows;

namespace LaunchToy.Dialogs
{
    /// <summary>
    /// Interaction logic for MIDIDevicesDialog.xaml
    /// </summary>
    public partial class MIDIDevicesDialog : Window
    {
        private MIDIDevicesDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
        }

        public static void OpenDialog()
        {
            var dialog = new MIDIDevicesDialog()
            {
                Owner = Env.MainWindow
            };

            dialog.midiInputsListBox.Items.Clear();
            var selectedItem = -1;
            var idx = 0;
            foreach (var device in MIDIConfig.GetInputDevices())
            {
                dialog.midiInputsListBox.Items.Add(new ListViewItemContainer<int>(idx, device.ProductName));
                if (Env.MidiInDeviceIdx == idx)
                {
                    selectedItem = idx;
                }
                idx++;
            }

            dialog.midiInputsListBox.SelectedIndex = selectedItem;

            dialog.midiOutputsListBox.Items.Clear();
            selectedItem = -1;
            idx = 0;
            foreach (var device in MIDIConfig.GetOutputDevices())
            {
                dialog.midiOutputsListBox.Items.Add(new ListViewItemContainer<int>(idx, device.ProductName));
                if (Env.MidiOutDeviceIdx == idx)
                {
                    selectedItem = idx;
                }
                idx++;
            }

            dialog.midiOutputsListBox.SelectedIndex = selectedItem;

            if (dialog.ShowDialog() == true)
            {
                if (dialog.midiInputsListBox.SelectedItem is ListViewItemContainer<int> inputContainer)
                {
                    Env.MidiInDeviceIdx.Set(inputContainer.Value);
                }
                if (dialog.midiOutputsListBox.SelectedItem is ListViewItemContainer<int> outputContainer)
                {
                    Env.MidiOutDeviceIdx.Set(outputContainer.Value);
                }
            }
        }
    }
}
