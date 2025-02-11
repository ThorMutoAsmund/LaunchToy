using LaunchToy.Dialogs;
using LaunchToy.Impl;
using LaunchToy.UserControls;
using Microsoft.Win32;
using NAudio.Midi;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LaunchToy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string combinedTitle = Constants.AppName;
        private List<NovationButton> buttons = new List<NovationButton>();

        public MainWindow()
        {
            InitializeComponent();

            Env.MainWindow = this;
            Env.DefaultLocation = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var testLocation = System.IO.Path.Combine(Env.DefaultLocation, Constants.AppName);
            if (System.IO.Directory.Exists(testLocation))
            {
                Env.DefaultLocation = testLocation;
            }
            Env.DirtyChanged += Env_DirtyChanged;
            Env.ProjectChanged += Env_ProjectChanged;
            Env.MidiInReceived += Env_MidiInReceived;
            Env.PowerChanged += Env_PowerChanged;

            Env_ProjectChanged(ProjectChangedAction.Closed);

            this.Title = "LaunchToy";

            var topRowSpecialFunction = new ControlButtonSpecialFunction[] { ControlButtonSpecialFunction.PageUp,ControlButtonSpecialFunction.PageDown,
                ControlButtonSpecialFunction.RowUp, ControlButtonSpecialFunction.RowDown
            };
            var topRowCC = new NovationControlChange[] { NovationControlChange.None, NovationControlChange.None, NovationControlChange.None, NovationControlChange.None,
                NovationControlChange.Session, NovationControlChange.Note, NovationControlChange.Device, NovationControlChange.User
            };
            var bottomRowCC = new NovationControlChange[] { NovationControlChange.Record_Arm, NovationControlChange.Track_Select, NovationControlChange.Mute, NovationControlChange.Solo,
                NovationControlChange.Volume, NovationControlChange.Pan, NovationControlChange.Sends, NovationControlChange.Stop_Clip
            };
            var leftColumnCC = new NovationControlChange[] { NovationControlChange.Shift, NovationControlChange.Click, NovationControlChange.Undo, NovationControlChange.Delete,
                NovationControlChange.Quantise, NovationControlChange.Duplicate, NovationControlChange.Double, NovationControlChange.Record
            };
            var rightColumnCC = new NovationControlChange[] { NovationControlChange.Row0, NovationControlChange.Row1, NovationControlChange.Row2, NovationControlChange.Row3,
                NovationControlChange.Row4, NovationControlChange.Row5, NovationControlChange.Row6, NovationControlChange.Row7
            };

            for (int y = 0; y < 8; ++y)
            {
                for (int x = 0; x < 8; ++x)
                {
                    if (y == 0 || y == 7)
                    {
                        var cb = new CtrlButton();
                        cb.Width = 50;
                        cb.Height = 50;
                        cb.HorizontalAlignment = HorizontalAlignment.Center;
                        cb.VerticalAlignment= VerticalAlignment.Center;
                        cb.NovationControlChange = y == 0 ? topRowCC[x] : bottomRowCC[x];
                        cb.SpecialFunction = y == 0 && x < 4 ? topRowSpecialFunction[x] : ControlButtonSpecialFunction.None;
                        Grid.SetRow(cb, y == 0 ? 2 : 11);
                        Grid.SetColumn(cb, 3 + x);
                        mainGrid.Children.Add(cb);
                        this.buttons.Add(cb);
                    }
                    if (x == 0 || x == 7)
                    {
                        var cb = new CtrlButton();
                        cb.Width = 50;
                        cb.Height = 50;
                        cb.HorizontalAlignment = HorizontalAlignment.Center;
                        cb.VerticalAlignment = VerticalAlignment.Center;
                        cb.NovationControlChange = x == 0 ? leftColumnCC[y] : rightColumnCC[y];
                        Grid.SetRow(cb, 3 + y);
                        Grid.SetColumn(cb, x == 0 ? 2 : 11);
                        mainGrid.Children.Add(cb);
                        this.buttons.Add(cb);
                    }
                    var pb = new PadButton();
                    pb.Width = 50;
                    pb.Height = 50;
                    pb.HorizontalAlignment = HorizontalAlignment.Center;
                    pb.VerticalAlignment = VerticalAlignment.Center;
                    pb.ButtonIndex = ((7 - y) << 4) + x;
                    Grid.SetRow(pb, 3 + y);
                    Grid.SetColumn(pb, 3 + x);
                    mainGrid.Children.Add(pb);
                    this.buttons.Add(pb);
                }
            }

            if (Constants.IsDebug)
            {
                var defaultProjectPath = System.IO.Path.Combine(Env.DefaultLocation, Constants.DefaultProjectName);
                if (System.IO.Directory.Exists(defaultProjectPath))
                {
                    Project.Open(defaultProjectPath);
                }
            }
        }

        private void Env_MidiInReceived(MidiEvent midiEvent)
        {
            if (Env.Project == null || !Env.IsRunning)
            {
                return;
            }

            // Get midi value
            var isNoteOff = false;
            var midiValue = 0;
            switch (midiEvent)
            {
                case ControlChangeEvent controlChangeEvent when controlChangeEvent.ControllerValue > 0:
                    midiValue = (int)controlChangeEvent.Controller; break;
                case ControlChangeEvent controlChangeEvent when controlChangeEvent.ControllerValue == 0:
                    isNoteOff = true;
                    midiValue = (int)controlChangeEvent.Controller; break;

                case NoteOnEvent noteOnEvent when noteOnEvent.Velocity > 0 :
                    midiValue = noteOnEvent.NoteNumber; break;
                case NoteEvent noteEvent when noteEvent.Velocity == 0:
                    isNoteOff = true;
                    midiValue = noteEvent.NoteNumber; 
                    break;
                default:
                    return;
            }

            var tick = Env.TickCount;
            
            var hlButton = this.buttons.FirstOrDefault(b => b.CommandCode == midiEvent.CommandCode && b.GetMidiValue() == midiValue);
            if (hlButton != null)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new System.Windows.Threading.DispatcherOperationCallback(delegate
                    {
                        var assignment = Env.Project.Assignments.FirstOrDefault(a => a.CommandCode == midiEvent.CommandCode && a.MidiValue == midiValue);

                        // Play sample
                        if (assignment != null)
                        {
                            if (assignment.Function == SpecialFunction.None && isNoteOff && Env.QuantizeState == QuantizeState.Waiting)
                            {
                                AudioConfig.StartQuantizing(Env.Project.BPM, Env.Project.BeatsPerBar, tick);
                                Env.ChangePower(Env.IsRunning, QuantizeState.Active);
                            }

                            if (!isNoteOff)
                            {
                                AudioConfig.AddAssigment(assignment, tick);
                            }
                            else
                            {
                                AudioConfig.RemoveAssignmentIfGated(assignment);
                            }
                        }

                        hlButton.Flash();

                        // update your GUI here    
                        return null;
                    }), null);
            }
        }

        private void Env_DirtyChanged(bool dirty)
        {
            this.Title = $"{this.combinedTitle}{(dirty ? " *" : string.Empty)}";
        }
 
        private void Env_ProjectChanged(ProjectChangedAction action)
        {
            this.combinedTitle = $"{Constants.AppName}{(Env.IsProjectLoaded ? $" - {Env.Project?.Title}" : string.Empty)}";

            this.FileOpenMenu.IsEnabled = true;
            this.FileSaveMenu.IsEnabled = Env.IsProjectLoaded;
            this.FileCloseMenu.IsEnabled = Env.IsProjectLoaded;
            this.SamplesImportMenu.IsEnabled = Env.IsProjectLoaded;
            this.SamplesImportFromYoutubeMenu.IsEnabled = Env.IsProjectLoaded;

            foreach (var item in this.setupContextMenu.Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.IsEnabled = Env.IsProjectLoaded;
                }
            }

            Env_DirtyChanged(false);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !StandardDialogs.ConfirmChangesMade();

            if (!e.Cancel)
            {
                MIDIConfig.Enable(false);
                AudioConfig.Enable(false);
                Env.OnApplicationEnded();
            }
        }

        private void setupContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (Env.Project != null)
            {
                this.bpmMenu.Header = $"BPM: {Env.Project.BPM}";
            }
        }

        private void FileNewMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!StandardDialogs.ConfirmChangesMade())
            {
                return;
            }

            CreateProjectDialog.OpenDialog(Constants.DefaultProjectName, Env.DefaultLocation);
        }

        private void FileOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!StandardDialogs.ConfirmChangesMade())
            {
                return;
            }

            var folderDialog = new OpenFolderDialog
            {
                InitialDirectory = Env.DefaultLocation
            };

            if (folderDialog.ShowDialog() == true)
            {
                Project.Open(folderDialog.FolderName);
            }
        }

        private void FileCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!StandardDialogs.ConfirmChangesMade())
            {
                return;
            }

            Project.Close();
        }

        private void FileSaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (Env.HasChanges)
            {
                Project.Save();
            }
        }

        private void FileQuitMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SamplesImportMenu_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Env.DefaultLocation,
                Multiselect = true,
                Filter = "Audio files (*.wav)|*.wav|(*.mp3)|*.mp3"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames != null || openFileDialog.FileName != null)
                {
                    Project.AddSamples(openFileDialog.FileNames ?? [openFileDialog.FileName]);
                }
            }
        }

        private void SamplesImportFromYoutubeMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented");
        }

        private void midiDevicesMenu_Click(object sender, RoutedEventArgs e)
        {
            MIDIDevicesDialog.OpenDialog();
        }

        private void audioDevicesMenu_Click(object sender, RoutedEventArgs e)
        {
            AudioDevicesDialog.OpenDialog();
        }

        private void HelpAboutMenu_Click(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            MessageBox.Show($"{Constants.AppName} {version}");
        }

        private void bpmMenu_Click(object sender, RoutedEventArgs e)
        {
            if (Env.Project == null)
            {
                return;
            }

            if (EnterNumberDialog.OpenDialog("Change Project BPM", "Project BPM", out var enteredBPM, Env.Project.BPM, Constants.MinBPM, Constants.MaxBPM))
            {
                Env.Project.BPM = enteredBPM;
                Env.OnDirtyChanged(true);
            }
        }

        private void projectSettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            ProjectSettingsDialog.OpenDialog();
        }

        private void drumModeMenu_Click(object sender, RoutedEventArgs e)
        {
            Env.ButtonLayout = ButtonLayout.Drum;
            Env.OnLayoutChanged();
        }

        private void noteModeMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented");
            //Env.ButtonLayout = ButtonLayout.Note;
            //Env.OnLayoutChanged();
        }


        private void powerButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var envWasRunning = Env.IsRunning;

            if (e.ClickCount == 1)
            {
                if (Env.QuantizeState != QuantizeState.Off)
                {
                    Env.ChangePower(false, QuantizeState.Off);
                }
                else
                {
                    Env.ChangePower(!Env.IsRunning, QuantizeState.Off);
                }
            }
            else if (e.ClickCount == 2)
            {
                if (Env.QuantizeState == QuantizeState.Off)
                {
                    Env.ChangePower(true, QuantizeState.Waiting);
                }
                else
                {
                    Env.ChangePower(false, QuantizeState.Off);
                }
            }

            if (envWasRunning != Env.IsRunning)
            {
                MIDIConfig.Enable(Env.IsRunning);
                AudioConfig.Enable(Env.IsRunning);
            }
        }

        private void Env_PowerChanged()
        {
            switch (Env.QuantizeState)
            {
                case QuantizeState.Off:
                    this.powerButtonOverlayBrush.Color = Env.IsRunning ? Color.FromArgb(0x80, 0x00, 0xa9, 0x00) : Color.FromArgb(0xa0, 0x00, 0x00, 0x00);
                    break;
                case QuantizeState.Waiting:
                    this.powerButtonOverlayBrush.Color = Color.FromArgb(0x80, 0xa9, 0x00, 0x00);
                    break;
                case QuantizeState.Active:
                    this.powerButtonOverlayBrush.Color = Color.FromArgb(0xa0, 0x00, 0x00, 0xd9);
                    break;
            }
        }

        private void setupButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectSettingsDialog.OpenDialog();
        }
    }
}