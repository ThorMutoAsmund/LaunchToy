using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaunchToy.Impl;
using NAudio.Midi;

namespace LaunchToy
{
    public static class Env
    {
        public static string DefaultLocation = ""; 
        public static MainWindow? MainWindow;
        public static Project? Project { get; set; }
        public static ButtonLayout ButtonLayout { get; set; } = ButtonLayout.Drum;
        public static bool IsRunning { get; set; } = false;
        public static QuantizeState QuantizeState { get; set; } = QuantizeState.Off;
        public static int LayoutOffset { get; set; } = 9;
        public static int LayoutRowsCount = 24; 
        public static bool IsProjectLoaded => Project != null;
        public static AppSettingsValueInt MidiInDeviceIdx = new AppSettingsValueInt("MidiInDeviceIdx", -1);
        public static AppSettingsValueInt MidiOutDeviceIdx = new AppSettingsValueInt("MidiOutDeviceIdx", -1);
        public static AppSettingsValueString ASIOOutDeviceName = new AppSettingsValueString("ASIOOutDeviceIdx", "");

        public static long TickCount => Environment.TickCount64;

        public static event Action? ApplicationEnded;
        public static event Action<bool>? DirtyChanged;
        public static event Action<ProjectChangedAction>? ProjectChanged;
        public static event Action? SamplesChanged;
        public static event Action? AssignmentsChanged;
        public static event Action? LayoutChanged;
        public static event Action<MidiEvent>? MidiInReceived;
        public static event Action? PowerChanged;
        public static bool HasChanges;

        public static void OnApplicationEnded()
        {
            ApplicationEnded?.Invoke();
        }

        public static void OnProjectChanged(ProjectChangedAction action)
        {
            ProjectChanged?.Invoke(action);
            Env.OnDirtyChanged(false);
        }

        public static void OnDirtyChanged(bool dirty)
        {
            if (dirty != HasChanges)
            {
                HasChanges = dirty;
                DirtyChanged?.Invoke(HasChanges);
            }
        }

        public static void OnSamplesChanged()
        {
            SamplesChanged?.Invoke();
        }

        public static void OnAssignmentsChanged()
        {
            AssignmentsChanged?.Invoke();
        }

        public static void OnLayoutChanged()
        {
            LayoutChanged?.Invoke();
        }

        public static void OnMidiInReceived(MidiEvent midiEvent)
        {
            MidiInReceived?.Invoke(midiEvent);  
        }

        public static void ChangePower(bool isRuning, QuantizeState quantizeState)
        {
            Env.IsRunning = isRuning;
            Env.QuantizeState = quantizeState;

            PowerChanged?.Invoke();
        }
    }
}
