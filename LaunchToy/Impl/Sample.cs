using NAudio.Wave;
using Newtonsoft.Json;
using System.IO;

namespace LaunchToy.Impl
{
    public class Sample
    {
        public string Id { get; set; } = "";
        public string Path { get; set; } = "";

        [JsonIgnore] public float[]? SampleData { get; private set; } = null;
        
        public override string ToString()
        {
            return System.IO.Path.GetFileNameWithoutExtension(this.Path);
        }

        public void EnsureLoaded()
        {
            if (Env.Project == null)
            {
                return;
            }

            if (this.SampleData == null)
            {
                var fullPath = System.IO.Path.Combine(Env.Project.ProjectPath, this.Path);
                if (!File.Exists(fullPath))
                {
                    this.SampleData = [];
                    return;
                }

                this.SampleData = LoadWaveFileToFloatArray(fullPath);
            }
        }

        static float[] LoadWaveFileToFloatArray(string filePath)
        {
            using var audioFileReader = new AudioFileReader(filePath);
            var sampleProvider = audioFileReader.ToSampleProvider();
            int totalSamples = (int)(audioFileReader.Length / (audioFileReader.WaveFormat.BitsPerSample / 8));
            float[] buffer = new float[totalSamples];
            int samplesRead = sampleProvider.Read(buffer, 0, buffer.Length);

            // Optionally trim unused buffer (if fewer samples are read)
            //if (samplesRead < buffer.Length)
            //{
            //    Array.Resize(ref buffer, samplesRead);
            //}

            return buffer;
        }
    }
}
