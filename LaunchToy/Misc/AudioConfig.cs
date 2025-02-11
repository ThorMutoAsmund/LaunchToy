using LaunchToy.Impl;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using System.Diagnostics;

namespace LaunchToy
{
    public static class AudioConfig
    {
        private static AsioOut? asioOut;
        private static AssignmentsWaveProvider? waveProvider;
        private static long baseTime = 0;
        private static bool isQuantizing = false;
        private static int sampleRate = 44100;
        public static List<string> GetOutputDevices()
        {
            return AsioOut.GetDriverNames().ToList();
            //return DirectSoundOut.Devices. GetDriverNames().ToList();
        }

        public static void Enable(bool enable)
        {
            if (asioOut != null)
            {
                asioOut.Stop();
                asioOut.Dispose();
            }

            if (!enable)
            {
                asioOut = null;
                waveProvider = null;
                isQuantizing = false;
                Debug.WriteLine("AUDIO OFF");
                return;
            }

            try
            {
                asioOut = new AsioOut(Env.ASIOOutDeviceName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //var outputChannels = asioOut.DriverOutputChannelCount;
            //asioOut.ChannelOffset = 2;

            waveProvider = new AssignmentsWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2));

            asioOut.Init(waveProvider);

            asioOut.Play();
            Debug.WriteLine("AUDIO ON");
        }


        //static float[] GenerateSineWave(int frequency, int seconds, int sampleRate)
        //{
        //    int sampleCount = seconds * sampleRate;
        //    float[] wave = new float[sampleCount];
        //    for (int i = 0; i < sampleCount; i++)
        //    {
        //        wave[i] = (float)(0.25 * Math.Sin((2 * Math.PI * frequency * i) / sampleRate)); // 0.25 for volume scaling
        //    }
        //    return wave;
        //}

        //public static void PlaySine()
        //{
        //    waveProvider!.playingSine = true;
        //}

        public static void AddAssigment(Assignment assignment, long tick)
        {
            if (waveProvider == null || Env.Project == null)
            {
                return;
            }

            if (assignment.Function == SpecialFunction.ArmToggle)
            {
                Env.ChangePower(true, Env.QuantizeState != QuantizeState.Waiting ? QuantizeState.Waiting : QuantizeState.Off);
                return;
            }

            if (isQuantizing)
            {
                var quantize = assignment.Quantize(sampleRate, Env.Project.BPM, tick - baseTime);
                if (assignment.Function == SpecialFunction.GroupStop)
                {
                    waveProvider.StopGroup(assignment, quantize.Delay);
                }
                else
                {
                    waveProvider.AddAssigment(assignment, Env.Project.BPM, quantize.Delay, quantize.Offset);
                }
            }
            else
            {
                waveProvider.AddAssigment(assignment, Env.Project.BPM);
            }            
        }

        public static void RemoveAssignmentIfGated(Assignment assignment)
        {
            if (waveProvider == null)
            {
                return;
            }

            waveProvider.RemoveAssignmentIfGated(assignment);
        }

        public static void StartQuantizing(int bpm, int beatsPerBar, long tick)
        {
            baseTime = tick;
            isQuantizing = true;
        }
    }
}
