using LaunchToy.Impl;
using System.IO;

namespace LaunchToy
{
    public static class YTDownloader
    {
        private const string YTVideoPrefix = "https://www.youtube.com/watch?v=";
        public static void EnsureDownloaded(Project project, Sample sample)
        {
            /*
            // Check if already downloaded
            if (File.Exists(project.GetSampleFullPath(sample)))
            {
                return;
            }

            var arguments = $"{YTVideoPrefix}{sample.YTId} -x --audio-format wav --audio-quality 0 -o {project.GetSampleRelativePath(sample)}";

            var proc = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "External\\yt-dlp.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            Console.WriteLine($"Downloading file {YTVideoPrefix}{sample.YTId}...");

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string? line = proc.StandardOutput.ReadToEnd();
                if (line != null)
                {
                    //Console.WriteLine(line);
                }
            }

            while (!proc.StandardError.EndOfStream)
            {
                string? line = proc.StandardError.ReadToEnd();
                if (line != null)
                {
                    //Console.WriteLine(line);
                }
            }
            */
        }
    }
}
