using LaunchToy.Impl;
using Microsoft.Win32;
using System.Windows;

namespace LaunchToy.Dialogs
{
    /// <summary>
    /// Interaction logic for EditAssignmentDialog.xaml
    /// </summary>
    public partial class EditAssignmentDialog : Window
    {
        private string lastSelectedGroup = "";
        private bool isUpdatingGroupComboBox;

        private EditAssignmentDialog()
        {
            InitializeComponent();

            this.okButton.Click += (sender, e) => this.DialogResult = true;
        }

        public static bool OpenDialog(Assignment assignment)
        {
            if (Env.Project == null)
            {
                return false;
            }

            return new EditAssignmentDialog()
            {
                Owner = Env.MainWindow
            }.InternalOpenDialog(Env.Project, assignment);
        }

        private bool InternalOpenDialog(Project project, Assignment assignment)
        {
            Sample? sample = null;

            switch (assignment.Function)
            {
                case SpecialFunction.GroupStop:
                    this.sampleTextBox.Text = "(stop)";
                    break;
                case SpecialFunction.ArmToggle:
                    this.sampleTextBox.Text = "(arm toggle)";
                    break;
                default:
                    if (project.TryGetSampleById(assignment.SampleId, out sample))
                    {
                        this.sampleTextBox.Text = sample?.Path;
                    }
                    else
                    {
                        this.sampleTextBox.Text = "(sample not found)";
                        this.okButton.IsEnabled = false;
                    }
                    break;
            }


            this.commandCodeComboBox.SelectedIndex = assignment.CommandCode == NAudio.Midi.MidiCommandCode.ControlChange ? 1 : 0;

            if (assignment.CommandCode == NAudio.Midi.MidiCommandCode.ControlChange)
            {
                this.midiValueTextBox.Text = assignment.MidiValue.ToString();
            }
            else
            {
                this.midiValueTextBox.Text = assignment.MidiValue.ToString();
            }
            
            if (assignment.Function != SpecialFunction.None)
            {
                this.functionLabel.Content = "Function";
                this.playModeComboBox.Visibility = Visibility.Hidden;
                this.playModeLabel.Visibility = Visibility.Hidden;
                this.quantizeComboBox.Visibility = Visibility.Hidden;
                this.quantizeLabel.Visibility = Visibility.Hidden;
                if (assignment.Function == SpecialFunction.ArmToggle)
                {
                    this.groupComboBox.Visibility = Visibility.Hidden;
                    this.groupLabel.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                this.playModeComboBox.SelectedIndex = (int)assignment.PlayMode;
                this.quantizeComboBox.SelectedIndex = (int)assignment.Quantization;
            }

            PopulateGroupCombobox(project, assignment.Group);

            this.lastSelectedGroup = assignment.Group;

            if (ShowDialog() == true)
            {
                assignment.CommandCode = this.commandCodeComboBox.SelectedIndex == 1 ? NAudio.Midi.MidiCommandCode.ControlChange : NAudio.Midi.MidiCommandCode.NoteOn;
                assignment.MidiValue = Int32.Parse(this.midiValueTextBox.Text);
                if (assignment.Function == SpecialFunction.None)
                {
                    assignment.PlayMode = (PlayMode)this.playModeComboBox.SelectedIndex;
                    assignment.Quantization = (QuantizationMethod)this.quantizeComboBox.SelectedIndex;
                }
                if (assignment.Function != SpecialFunction.ArmToggle)
                {
                    assignment.Group = this.lastSelectedGroup;
                }

                assignment.Recalculate();

                return true;
            }

            return false;
        }

        private void PopulateGroupCombobox(Project project, string currentGroup, int setIndex = -1)
        {
            this.isUpdatingGroupComboBox = true;
            this.groupComboBox.Items.Clear();

            var selectedIndex = setIndex == -1 ? 0 : setIndex;
            this.groupComboBox.Items.Add("(none)");
            var idx = 0;
            foreach (var group in project.Groups)
            {
                this.groupComboBox.Items.Add(group);
                idx++;
                if (setIndex == -1 && group == currentGroup)
                {
                    selectedIndex = idx;
                }
            }
            this.groupComboBox.Items.Add("(create new)");

            this.groupComboBox.SelectedIndex = selectedIndex;
            this.isUpdatingGroupComboBox = false;
        }

        private void groupComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Env.Project == null)// || this.isUpdatingGroupComboBox)
            {
                return;
            }

            // Create new?
            if (this.groupComboBox.SelectedIndex == 0)
            {
                this.lastSelectedGroup = "";
            }
            else if (this.groupComboBox.SelectedIndex > 0 && this.groupComboBox.SelectedIndex == this.groupComboBox.Items.Count - 1)
            {
                if (EnterStringDialog.OpenDialog("Enter group name", "Group name", out var enteredString))
                {
                    if (Project.AddGroup(enteredString))
                    {
                        this.lastSelectedGroup = enteredString;
                    }
                }

                PopulateGroupCombobox(Env.Project, lastSelectedGroup);
            }
            else
            {
                this.lastSelectedGroup = (string)this.groupComboBox.SelectedItem;
            }
        }
    }
}
