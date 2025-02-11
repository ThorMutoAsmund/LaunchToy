using NAudio.Midi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchToy.Impl
{
    public class Assignment
    {
        public string SampleId { get; set; } = "";
        public MidiCommandCode CommandCode { get; set; } = MidiCommandCode.NoteOn;
        public int MidiValue { get; set; } = 0;
        public SpecialFunction Function { get; set; } = SpecialFunction.None;
        public double SampleStart { get; set; } = 0D;
        public double SampleLength { get; set; } = 0D;
        public PlayMode PlayMode { get; set; } = PlayMode.Poly;
        public string Group { get; set; } = "";
        public QuantizationMethod Quantization { get; set; } = QuantizationMethod.None;
        [JsonIgnore] public float[]? SampleData { get; private set; } = null;

        public void Recalculate()
        {
            this.SampleData = null;

            EnsureCalculated();
        }


        public void EnsureCalculated()
        {
            if (Env.Project == null)
            {
                return;
            }

            if (this.SampleData == null)
            {
                if (!Env.Project.TryGetSampleById(this.SampleId, out var sample) || sample?.SampleData == null)
                {
                    this.SampleData = [];
                    return;
                }

                this.SampleData = new float[sample.SampleData.Length];

                Buffer.BlockCopy(sample.SampleData, 0, this.SampleData, 0, this.SampleData.Length * sizeof(float));
            }
        }

        public double GetQuantizeLenInMs(int bpm)
        {
            var beatLenMs = 60000.0 / bpm;
            switch (this.Quantization)
            {
                case QuantizationMethod.Q2:
                    beatLenMs = beatLenMs * 2; break;
                case QuantizationMethod.Q4:
                    beatLenMs = beatLenMs * 4; break;
                case QuantizationMethod.Q3:
                    beatLenMs = beatLenMs * 3; break;
                case QuantizationMethod.Q1_2:
                    beatLenMs = beatLenMs / 2.0; break;
                case QuantizationMethod.Q1_4:
                    beatLenMs = beatLenMs / 4.0; break;
                case QuantizationMethod.Q1_8:
                    beatLenMs = beatLenMs / 8.0; break;
                case QuantizationMethod.Q1_16:
                    beatLenMs = beatLenMs / 16.0; break;
                case QuantizationMethod.Q1_32:
                    beatLenMs = beatLenMs / 32.0; break;
                case QuantizationMethod.Q1_64:
                    beatLenMs = beatLenMs / 64.0; break;
                case QuantizationMethod.Q1_3:
                    beatLenMs = beatLenMs / 3.0; break;
                case QuantizationMethod.Q1_6:
                    beatLenMs = beatLenMs / 6.0; break;
                case QuantizationMethod.Q1_5:
                    beatLenMs = beatLenMs / 5.0; break;
            }

            return beatLenMs;
        }

        public (int Delay, int Offset) Quantize(int samplerate, int bpm, long ticksMs)
        {
            if (this.Quantization == QuantizationMethod.None || bpm < 1) 
            {
                return (0, 0); 
            }

            var qtLenInMs = GetQuantizeLenInMs(bpm);

            var offs = ticksMs % qtLenInMs;
            if (offs > Constants.AcceptableCatchUpMs)
            {
                return (2 * (int)((qtLenInMs - offs) * samplerate / 1000.0), 0);
            }
            else
            {
                return (0, 2 * (int)(offs * samplerate / 1000.0));
            }
        }

        public (int Delay, int Offset) Loop(int samplerate, int bpm, int offset)
        {
            var qtLenInMs = GetQuantizeLenInMs(bpm);
            var msPlayedByNow = (offset / 2) * 1000.0 / 44100.0;
            var offs = msPlayedByNow % qtLenInMs;
            if (offs > Constants.AcceptableCatchUpMs)
            {
                return (2 * (int)((qtLenInMs - offs) * samplerate / 1000.0), 0);
            }
            else
            {
                return (0, 2 * (int)(offs * samplerate / 1000.0));
            }
        }
    }
}
