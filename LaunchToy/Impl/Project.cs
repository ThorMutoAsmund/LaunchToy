using NAudio.Midi;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace LaunchToy.Impl
{
    public class Project
    {
        [JsonIgnore] public string ProjectFilePath => Path.Combine(this.projectPath, $"{Constants.ProjectFileName}.json");
        [JsonIgnore] public string ProjectPath => projectPath;
        public int Version { get; set; } = 0;
        public string Title { get; set; } = "";
        public List<Sample> Samples { get; set; } = new List<Sample>();
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        public List<string> Groups { get; set; } = new List<string>();
        public int BPM { get; set; } = Constants.StandardBPM;
        public int BeatsPerBar { get; set; } = Constants.StandardBeatsPerBar;

        private string projectPath = "";

        public static void CreateNew(string projectPath, string title)
        {
            Env.Project = new Project()
            {
                projectPath = projectPath,
                Title = title,
            };

            Project.Save();

            //Env.AddRecentFile(projectPath);
        }

        public static void Close()
        {
            if (Env.Project == null)
            {
                return;
            }

            Env.Project = null;
            Env.OnProjectChanged(ProjectChangedAction.Closed);
        }

        public static void Save()
        {
            if (Env.Project == null)
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                ProjectSerializer.ToFile(Env.Project);
                Env.OnProjectChanged(ProjectChangedAction.Saved);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }


        public static void Open(string projectPath)
        {
            if (!Directory.Exists(projectPath))
            {
                MessageBox.Show("File not found", "Project Open Error");
                return;
            }

            Project.Close();

            try
            {
                var projectSerializer = ProjectSerializer.FromFile(new Project() { projectPath = projectPath }.ProjectFilePath);
                Env.Project = projectSerializer?.Project;

                if (Env.Project == null)
                {
                    MessageBox.Show("Deserialization error", "Project Open Error");
                    return;
                }

                Env.Project.projectPath = projectPath;
                
                EnsureSamplesLoaded();
                EnsureAssignmentsCaculated();

                Env.OnProjectChanged(ProjectChangedAction.Opened);

                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating or opening default: {ex.Message}", "Project Open Error");

                return;
            }
        }

        public static void AddSamples(string[] samplePaths)
        {
            if (Env.Project == null)
            {
                return;
            }

            var samplesAdded = false;
            foreach (var samplePath in samplePaths)
            {
                // Just skip those that do not exist somehow
                if (File.Exists(samplePath))
                {
                    var sampleName = Path.GetFileName(samplePath);
                    var destinationPath = Path.Combine(Env.Project.projectPath, sampleName);
                    if (!File.Exists(destinationPath))
                    {
                        File.Copy(samplePath, destinationPath);
                    }

                    if (!Env.Project.Samples.Any(s => s.Path == sampleName))
                    {
                        Env.Project.Samples.Add(new Sample()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Path = sampleName,
                        });
                        samplesAdded = true;
                    }
                }
            }

            if (samplesAdded)
            {
                EnsureSamplesLoaded();

                Env.OnSamplesChanged();
                Env.OnDirtyChanged(true);
            }
        }

        public static void DeleteSamples(IEnumerable<Sample> samples)
        {
            if (Env.Project == null)
            {
                return;
            }

            foreach (var sample in samples)
            {
                Env.Project.Samples.Remove(sample);
                Env.Project.Assignments.RemoveAll(a => a.SampleId == sample.Id);
            }

            Env.OnSamplesChanged();
            Env.OnAssignmentsChanged();
            Env.OnDirtyChanged(true);
        }

        public static void AddAssignment(Sample sample, MidiCommandCode comandCode, int midiValue)
        {
            if (Env.Project == null)
            {
                return;
            }

            if (Env.Project.TryGetAssignment(comandCode, midiValue, out _))
            {
                if (MessageBox.Show($"This will replace the current assignment. Are you sure?", "Replace Assignment", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return;
                }
                Project.RemoveAssigments(comandCode, midiValue);
            }

            var assignment = new Assignment()
            {
                SampleId = sample.Id,
                CommandCode = comandCode,
                MidiValue = midiValue
            };
            Env.Project.Assignments.Add(assignment);

            assignment.Recalculate();

            Env.OnAssignmentsChanged();
            Env.OnDirtyChanged(true);
        }

        public static void AddSpecialFunction(MidiCommandCode comandCode, int midiValue, SpecialFunction specialFunction)
        {
            if (Env.Project == null)
            {
                return;
            }

            var assignment = new Assignment()
            {
                CommandCode = comandCode,
                MidiValue = midiValue,
                Function = specialFunction
            };
            Env.Project.Assignments.Add(assignment);

            assignment.Recalculate();

            Env.OnAssignmentsChanged();
            Env.OnDirtyChanged(true);
        }

        public static void RemoveAssigments(MidiCommandCode commandCode, int midiValue)
        {
            if (Env.Project == null)
            {
                return;
            }

            Env.Project.Assignments.RemoveAll(a => a.CommandCode == commandCode && a.MidiValue == midiValue);
            Env.OnAssignmentsChanged();
            Env.OnDirtyChanged(true);
        }

        public static bool AddGroup(string group)
        {
            if (Env.Project == null)
            {
                return false;
            }

            if (Env.Project.Groups.Contains(group))
            {
                MessageBox.Show("Error", "Group already exists");

                return false;
            }

            Env.Project.Groups.Add(group);
            Env.OnDirtyChanged(true);

            return true;
        }

        public static void EnsureSamplesLoaded()
        {
            if (Env.Project == null)
            {
                return;
            }

            foreach (var sample in Env.Project.Samples)
            {
                sample.EnsureLoaded();
            }
        }

        public static void EnsureAssignmentsCaculated()
        {
            if (Env.Project == null)
            {
                return;
            }

            foreach (var assignment in Env.Project.Assignments)
            {
                assignment.EnsureCalculated();
            }
        }

        public bool TryGetSampleById(string sampleId, out Sample? sample)
        {
            sample = this.Samples.FirstOrDefault(s => s.Id == sampleId);

            return sample != null;
        }

        public bool TryGetAssignment(MidiCommandCode commandCode, int midiValue, out Assignment? assignment)
        {
            assignment = this.Assignments.FirstOrDefault(a => a.CommandCode == commandCode && a.MidiValue == midiValue);
            
            return assignment != null;  
        }
    }
}
