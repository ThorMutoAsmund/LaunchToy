using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchToy
{
    public static class Constants
    {
        public static bool IsDebug = true;
        public static string AppName = "LaunchToy";
        public static string DefaultProjectName = "MyProject";
        public static string ProjectFileName = "project";
        public static int StandardBPM = 120;
        public static int MinBPM = 20;
        public static int MaxBPM = 1000;
        public static int StandardBeatsPerBar = 4;
        public static int MinBeatsPerBar = 1;
        public static int MaxBeatsPerBar = 16;
        public static double AcceptableCatchUpMs = 100.0;
    }
}
