using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchToy
{
    public static class MIDIConfig
    {
        private static MidiIn? midiIn = null;
        public static List<MidiInCapabilities> GetInputDevices()
        {
            var devices = new List<MidiInCapabilities>();
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                devices.Add(MidiIn.DeviceInfo(device));
            }

            return devices;
        }

        public static List<MidiOutCapabilities> GetOutputDevices()
        {
            var devices = new List<MidiOutCapabilities>();
            for (int device = 0; device < MidiOut.NumberOfDevices; device++)
            {
                devices.Add(MidiOut.DeviceInfo(device));
            }

            return devices;
        }

        public static void Enable(bool enable)
        {
            if (enable)
            {
                midiIn = new MidiIn(Env.MidiInDeviceIdx);
                midiIn.MessageReceived += midiIn_MessageReceived;
                midiIn.ErrorReceived += midiIn_ErrorReceived;
                midiIn.Start();
                Debug.WriteLine("MIDI ON");
            }
            else
            {

                if (midiIn != null)
                {
                    midiIn.MessageReceived -= midiIn_MessageReceived;
                    midiIn.ErrorReceived -= midiIn_ErrorReceived;
                    midiIn.Stop();
                    midiIn.Dispose();
                }
                midiIn = null;
                Debug.WriteLine("MIDI OFF");
            }
        }
 
        static void midiIn_ErrorReceived(object? sender, MidiInMessageEventArgs e)
        {
        }
 
        static void midiIn_MessageReceived(object? sender, MidiInMessageEventArgs e)
        {
            //AudioConfig.PlaySine();
            Env.OnMidiInReceived(e.MidiEvent);
        }
    }
}
