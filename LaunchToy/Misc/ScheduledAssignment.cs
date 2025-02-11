using LaunchToy.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchToy
{
    public class ScheduledAssignment
    {
        public Assignment Assignment { get; private set; }

        public bool HasStarted => this.delayInSamples == 0;
        private int bpm;
        private int length;
        private bool looping;
        private int offset = 0;
        private int delayInSamples = 0;
        public ScheduledAssignment(Assignment assignment, int bpm, int delayInSamples = 0, int offsetInSamples = 0)
        {
            this.Assignment = assignment;
            this.bpm = bpm;
            this.offset = offsetInSamples;
            this.delayInSamples = delayInSamples;
            this.length = assignment?.SampleData?.Length ?? 0;
            this.looping = assignment?.PlayMode == PlayMode.Looped;
        }

        public int Mix(float[] floatBuffer, int count, int bufferOffset = 0)
        {
            if (this.Assignment == null || this.Assignment.SampleData == null || this.offset >= this.length)
            {
                return 0;
            }

            // Regular play
            var totalRead = Math.Min(count, this.length - this.offset + this.delayInSamples);
            for (int i = this.delayInSamples; i < totalRead; i++)
            {
                floatBuffer[i] += this.Assignment.SampleData[i - this.delayInSamples + this.offset];
            }

            this.offset += Math.Max(0, totalRead - this.delayInSamples);

            // Additional samples due to looping?
            if (this.looping && totalRead != count)
            {
                var quantize = this.Assignment.Loop(44100, this.bpm, this.offset);
                this.offset = quantize.Offset;
                this.delayInSamples = quantize.Delay;
                
                return totalRead + Mix(floatBuffer, totalRead - count, totalRead);
            }

            this.delayInSamples = Math.Max(0, this.delayInSamples - count);

            return totalRead;
        }

        public void EndAfter(int delayInSamples)
        {
            this.length = Math.Min(this.length, this.offset + delayInSamples);
            this.looping = false;
        }
    }
}
