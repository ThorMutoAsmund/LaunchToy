using LaunchToy.Impl;
using NAudio.Wave;
using System.Diagnostics;

namespace LaunchToy
{
    public class AssignmentsWaveProvider : IWaveProvider
    {
        public WaveFormat WaveFormat => waveFormat;

        private readonly WaveFormat waveFormat;
        private List<ScheduledAssignment> scheduledAssignments = new List<ScheduledAssignment>();

        private float[] floatBuffer = new float[512];

        public AssignmentsWaveProvider(WaveFormat waveFormat)
        {
            this.waveFormat = waveFormat;
        }

        public void StopGroup(Assignment assignment, int delayInSamples = 0)
        {
            if (!string.IsNullOrEmpty(assignment.Group))
            {
                UnscheduleGroupedAssignments(assignment.Group, delayInSamples);
            }
        }

        public void AddAssigment(Assignment assignment, int bpm, int delayInSamples = 0, int offsetInSamples = 0)
        {
            // If not polyphonic, remove others
            if (assignment.PlayMode != PlayMode.Poly)
            {
                UnscheduleMatchingAssignments(assignment, delayInSamples);
                if (!string.IsNullOrEmpty(assignment.Group))
                {
                    UnscheduleGroupedAssignments(assignment.Group, delayInSamples);
                }
            }

            this.scheduledAssignments.Add(new ScheduledAssignment(assignment, bpm, delayInSamples, offsetInSamples));
        }

        public void RemoveAssignmentIfGated(Assignment assignment)
        {
            if (assignment.Function != SpecialFunction.GroupStop && assignment.PlayMode == PlayMode.Gated)
            {
                RemoveMatchingAssignments(assignment);
            }
        }

        private void RemoveMatchingAssignments(Assignment assignment)
        {
            var toRemove = this.scheduledAssignments.Where(sa => sa.Assignment == assignment);
            if (toRemove.Any())
            {
                foreach (var sa in toRemove.ToArray())
                {
                    this.scheduledAssignments.Remove(sa);
                }
            }
        }

        private void RemoveGroupedAssignments(string group)
        {
            var toRemove = this.scheduledAssignments.Where(sa => sa.Assignment.Group == group);
            if (toRemove.Any())
            {
                foreach (var sa in toRemove.ToArray())
                {
                    this.scheduledAssignments.Remove(sa);
                }
            }
        }

        private void UnscheduleMatchingAssignments(Assignment assignment, int delayInSamples)
        {
            if (delayInSamples == 0)
            {
                RemoveMatchingAssignments(assignment);
                return;
            }

            var toUnschedule = this.scheduledAssignments.Where(sa => sa.Assignment == assignment);
            if (toUnschedule.Any())
            {
                RemoveOrUnschedule(toUnschedule.ToArray(), delayInSamples);
            }
        }

        private void UnscheduleGroupedAssignments(string group, int delayInSamples)
        {
            if (delayInSamples == 0)
            {
                RemoveGroupedAssignments(group);
                return;
            }

            var toUnschedule = this.scheduledAssignments.Where(sa => sa.Assignment.Group == group);
            if (toUnschedule.Any())
            {
                RemoveOrUnschedule(toUnschedule.ToArray(), delayInSamples);
            }
        }

        private void RemoveOrUnschedule(ScheduledAssignment[] items, int delayInSamples)
        { 
            if (items.Any())
            {
                foreach (var sa in items)
                {
                    if (sa.HasStarted)
                    {
                        sa.EndAfter(delayInSamples);
                    }
                    else
                    {
                        this.scheduledAssignments.Remove(sa);
                    }
                }
            }
        }

        public int Read(byte[] buffer, int offset, int count) // Typically  0 and 2048/4096
        {
            // 64 stereo samples buffer (1.3/1.4 ms buffer) 19-21 ms latency
            // 512 stereo samples buffer (10.6/11.6 ms buffer) 35-38 ms latency

            // 256 bytes buffer => 24 ms latency  (5 ms buffer)  14 - 19
            // 512 bytes buffer => 28 ms latency  (11 ms buffer) 6 - 17
            // 1024 bytes buffer => 54 ms latency  (23 ms buffer) 7 - 30
            // 2048 bytes buffer => 75 ms latency  (46 ms buffer) -16 - 30
            // Resize buffer if needed
            var floatCount = count / sizeof(float);
            Array.Resize(ref this.floatBuffer, floatCount);
            Array.Clear(floatBuffer, 0, floatCount);

            // Mix in samples
            var totalRead = 0;
            foreach (var scheduledAssignment in scheduledAssignments.ToArray())
            {
                var read = scheduledAssignment.Mix(floatBuffer, floatCount);
                if (read > totalRead)
                {
                    totalRead = read;
                }
                
                if (read != floatCount)
                {
                    if (scheduledAssignment.Assignment.PlayMode != PlayMode.Looped)
                    {
                        scheduledAssignments.Remove(scheduledAssignment);
                    }
                }
            }

            // Copy data created
            Buffer.BlockCopy(floatBuffer, 0, buffer, offset, totalRead * sizeof(float));
            return totalRead * sizeof(float);
        }

        //private static void asio_DataAvailable(object? sender, AsioAudioAvailableEventArgs e)
        //{
        //    byte[] buf = new byte[e.SamplesPerBuffer];
        //    for (int i = 0; i < e.InputBuffers.Length; i++)
        //    {
        //        //Marshal.Copy(e.InputBuffers[i], e.OutputBuffers, 0, e.InputBuffers.Length);
        //        //also tried to upper one but this way i also couldn't hear anything
        //        Marshal.Copy(e.InputBuffers[i], buf, 0, e.SamplesPerBuffer);
        //        Marshal.Copy(buf, 0, e.OutputBuffers[i], e.SamplesPerBuffer);
        //    }
        //    e.WrittenToOutputBuffers = true;
        //}



        //if (circularBuffer != null)
        //{
        //    num = circularBuffer.Read(buffer, offset, count);
        //}

        //if (ReadFully && num < count)
        //{
        //    Array.Clear(buffer, offset + num, count - num);
        //    num = count;
        //}

        //return count;


        //static float[] GenerateSineWave(int frequency, int sampleCount, int sampleRate)
        //{
        //    float[] wave = new float[sampleCount*2];
        //    for (int i = 0; i < sampleCount; i++)
        //    {
        //        wave[i * 2] = (float)(Math.Cos((2 * Math.PI * frequency * i) / sampleRate)); // 0.25 for volume scaling
        //        wave[i * 2 + 1] = wave[i * 2];
        //    }
        //    return wave;
        //}
    }
}
